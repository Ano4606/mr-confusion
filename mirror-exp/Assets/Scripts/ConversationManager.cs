using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;
using Meta.XR.Movement.Retargeting; // Correct namespace for CharacterRetargeter

public class ConversationManager : MonoBehaviour
{
    [System.Serializable]
    public class ConversationLine
    {
        public int Line;
        public string Speaker;
        public string Word;
        public string AudioFile; // Name of audio clip without extension
    }
// added
    public event Action OnConversationFinished;


    [Header("UI")]
    public TextMeshProUGUI lineText;

    [Header("Settings")]
    public float lineDuration = 50f;
    public string Participant = "P01";

    [Header("Audio")]
    public AudioSource AudioInterlocutor;
    public AudioSource AudioAvatar;

    [Header("Avatar Control")]
    public CharacterRetargeter retargeter; // Meta XR Movement SDK retargeter
    public Animator selfAvatarAnimator;    // Animator for Mixamo speaking animation

    private List<ConversationLine> conversation = new List<ConversationLine>();
    private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

    public OVRLipSyncContext avatarLipsync; //OVRLipSynccontext inside the avatar
    public void BindToAvatar(GameObject avatar)
    {
    AvatarBindings bindings = avatar.GetComponent<AvatarBindings>();

            if (bindings == null)
            {
                Debug.LogError("Avatar does not have AvatarBindings component!");
                return;
            }

    selfAvatarAnimator = bindings.animator;
    retargeter = bindings.retargeter;
    avatarLipsync = bindings.lipSync;
    AudioAvatar = bindings.voiceSource;
    
    }
    public void StartTask()
    {
        if(avatarLipsync == null){
            avatarLipsync = this.GetComponent<OVRLipSyncContext>();
        }

        LoadConversation(Participant);
        PreloadAudioClips();

        if (conversation.Count > 0)
            StartCoroutine(RunConversation());
        else
            lineText.text = "No conversation lines found!";
    }

    void PreloadAudioClips()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("conversation-audio");
        foreach (var clip in clips)
            clipCache[clip.name] = clip;

        Debug.Log($"Preloaded {clipCache.Count} audio clips.");
    }

    void LoadConversation(string participant)
    {
        TextAsset csvFile = Resources.Load<TextAsset>("rando_balanced");
        if (csvFile == null)
        {
            Debug.LogError("CSV file not found in Resources!");
            return;
        }

        string[] lines = csvFile.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        conversation.Clear();

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 6) continue;

            for (int j = 0; j < values.Length; j++)
                values[j] = values[j].Trim().Trim('"');

            if (!values[0].Equals(participant, StringComparison.OrdinalIgnoreCase)) continue;
            if (!int.TryParse(values[2], out int lineNumber)) continue;

            conversation.Add(new ConversationLine
            {
                Line = lineNumber,
                Speaker = values[3],
                Word = values[4],
                AudioFile = values[5]
            });
        }

        conversation = conversation.OrderBy(c => c.Line).ToList();
        Debug.Log($"Total lines loaded for {participant}: {conversation.Count}");
    }

    IEnumerator RunConversation()
{
    foreach (var line in conversation)
    {
        AudioSource sourceToUse = null;
        string displayText = "";

        // Determine speaker
        if (line.Speaker.Equals("I", StringComparison.OrdinalIgnoreCase))
        {
            sourceToUse = AudioInterlocutor;
            displayText = "...";
        }
        else if (line.Speaker.Equals("SA", StringComparison.OrdinalIgnoreCase))
        {
            sourceToUse = AudioAvatar;
            displayText = "...";

            retargeter.enabled = false;
            // Force the Animator to play from the Entry state
            selfAvatarAnimator.Play("Standing", 0, 0f); 
            selfAvatarAnimator.Update(0f); // Optional: forces immediate update
            avatarLipsync.audioLoopback = false;


        }
        else if (line.Speaker.Equals("P", StringComparison.OrdinalIgnoreCase))
        {
            sourceToUse = AudioAvatar;
            displayText = $"{line.Speaker}: {line.Word}";
            avatarLipsync.audioLoopback = true;

        }
        else
        {
            displayText = $"{line.Speaker}: {line.Word}";
        }

        lineText.text = displayText;
        float waitTime = 0f;

        // --- MICROPHONE MODE ---
        if (line.Speaker.Equals("P", StringComparison.OrdinalIgnoreCase))
        {
            if (Microphone.devices.Length > 0)
            {
                string micName = Microphone.devices[0];
                int sampleRate = 44100;

                AudioClip micClip = Microphone.Start(micName, true, 5, sampleRate);
                sourceToUse.clip = micClip;

                while (!(Microphone.GetPosition(micName) > 0))
                    yield return null;

                sourceToUse.Play(); // realtime monitoring

                // Wait for the duration of the line
                yield return new WaitForSeconds(Mathf.Min(5f, line.Word.Length * 0.2f));

                Microphone.End(micName);
                sourceToUse.loop = false;
            }
            else
            {
                Debug.LogWarning("No microphone detected!");
                yield return new WaitForSeconds(2f);
            }
            continue;
        }

        // --- NORMAL AUDIO FILE LOGIC (SA / I / others) ---
        if (sourceToUse != null && !string.IsNullOrEmpty(line.AudioFile) && !line.AudioFile.Equals("NA", StringComparison.OrdinalIgnoreCase))
        {
            string clipKey = Path.GetFileNameWithoutExtension(line.AudioFile.Trim());
            if (!clipCache.TryGetValue(clipKey, out AudioClip clip))
            {
                // fallback case-insensitive search
                clip = clipCache.FirstOrDefault(kvp => kvp.Key.Equals(clipKey, StringComparison.OrdinalIgnoreCase)).Value;
            }

            if (clip != null)
            {
                sourceToUse.clip = clip;
                sourceToUse.Stop();
                sourceToUse.Play();
                waitTime = clip.length;
            }
            else
            {
                Debug.LogWarning($"Audio clip not found: '{line.AudioFile}'");
            }
        }

        if (waitTime <= 0f)
            waitTime = Mathf.Max(2f, line.Word.Length * 0.2f);

        yield return new WaitForSeconds(waitTime);

        // Re-enable retargeter after SA finishes
        if (line.Speaker.Equals("SA", StringComparison.OrdinalIgnoreCase) && retargeter != null)
        {
            retargeter.enabled = true;
        }
    }

    // lineText.text = "Conversation terminée !";
    // Debug.Log("Conversation complete!");

    lineText.text = "Conversation terminée !";
Debug.Log("Conversation complete!");

OnConversationFinished?.Invoke();

}

}


