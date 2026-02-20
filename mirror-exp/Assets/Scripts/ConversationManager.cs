using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;
using Meta.XR.Movement.Retargeting;

public class ConversationManager : MonoBehaviour
{
    [Serializable]
    public class ConversationLine
    {
        public int Line;
        public string Speaker;
        public string Text;
        public string AudioFile;
    }

    public event Action OnConversationFinished;

    [Header("UI")]
    public TextMeshProUGUI lineText;

    [Header("Settings")]
    public string Participant = "P01";
    public float playerSpeakingTime = 10f;
    public bool useTextBasedDuration = false;

    [Header("Audio")]
    public AudioSource AudioInterlocutor;
    public AudioSource AudioAvatar;
    public AudioSource AudioParticipant;
    
    [Header("Avatar Control")]
    public CharacterRetargeter retargeter;
    public Animator selfAvatarAnimator;

    [Header("Lip Sync")]
    public OVRLipSyncContext avatarLipsync;
    public OVRLipSyncContext interlocutorLipsync;

    private string participantGender;
    private int groupNumber;
    private List<ConversationLine> conversation = new List<ConversationLine>();
    private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

    public void BindToAvatar(GameObject avatar)
    {
        if (avatar == null) return;

        AvatarBindings bindings = avatar.GetComponent<AvatarBindings>();
        if (bindings == null) return;

        selfAvatarAnimator = bindings.animator;
        retargeter = bindings.retargeter;
        avatarLipsync = bindings.lipSync;
        AudioAvatar = bindings.voiceSource;
    }

    public void BindToInterlocutor(GameObject interlocutor)
    {
        if (interlocutor == null) return;

        AvatarBindings bindings = interlocutor.GetComponent<AvatarBindings>();
        if (bindings != null)
        {
            interlocutorLipsync = bindings.lipSync;
            AudioInterlocutor = bindings.voiceSource;
        }
        else
        {
            interlocutorLipsync = interlocutor.GetComponentInChildren<OVRLipSyncContext>(true);
            AudioInterlocutor = interlocutor.GetComponentInChildren<AudioSource>(true);
        }
    }

    public void SetGenderAndGroup(string gender, int group)
    {
        participantGender = gender.ToLower();
        groupNumber = group;
    }

    public void StartTask()
    {
        if (string.IsNullOrEmpty(participantGender))
            participantGender = "female";

        if (groupNumber == 0)
            groupNumber = 1;

        LoadConversation(Participant);
        PreloadAudioClips();

        if (conversation.Count > 0)
            StartCoroutine(RunConversation());
        else if (lineText != null)
            lineText.text = "No conversation lines found!";
    }

    void PreloadAudioClips()
    {
        string audioPath = $"conversation-audio/{participantGender}-participant/Group{groupNumber}";
        AudioClip[] clips = Resources.LoadAll<AudioClip>(audioPath);
        
        if (clips.Length == 0)
            clips = Resources.LoadAll<AudioClip>("conversation-audio");

        foreach (var clip in clips)
            clipCache[clip.name] = clip;
    }

    void LoadConversation(string participant)
    {
        TextAsset csvFile = Resources.Load<TextAsset>("rando");
        if (csvFile == null) return;

        string[] lines = csvFile.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        conversation.Clear();

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = ParseCSVLine(line);
            if (values.Length < 6) continue;

            for (int j = 0; j < values.Length; j++)
                values[j] = values[j].Trim().Trim('"');

            if (!values[0].Equals(participant, StringComparison.OrdinalIgnoreCase)) continue;
            if (!int.TryParse(values[2], out int lineNumber)) continue;

            conversation.Add(new ConversationLine
            {
                Line = lineNumber,
                Speaker = values[3],
                Text = values[4],
                AudioFile = values[5]
            });
        }

        conversation = conversation.OrderBy(c => c.Line).ToList();
    }

    private string[] ParseCSVLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }

        result.Add(currentField);
        return result.ToArray();
    }

    IEnumerator RunConversation()
    {
        foreach (var line in conversation)
        {
            AudioSource sourceToUse = null;
            string displayText = "";

            if (line.Speaker.Equals("I", StringComparison.OrdinalIgnoreCase))
            {
                sourceToUse = AudioInterlocutor;
                displayText = "...";
            }
            else if (line.Speaker.Equals("SA", StringComparison.OrdinalIgnoreCase))
            {
                sourceToUse = AudioAvatar;
                displayText = "...";


                if (avatarLipsync != null)
                    avatarLipsync.audioSource = AudioAvatar;
                    avatarLipsync.audioLoopback = true;



                if (retargeter != null)
                    retargeter.enabled = true;

                if (selfAvatarAnimator != null)
                {
                    selfAvatarAnimator.Play("Standing", 0, 0f);
                    selfAvatarAnimator.Update(0f);
                }
            }
            else if (line.Speaker.Equals("P", StringComparison.OrdinalIgnoreCase))
            {
                sourceToUse = AudioParticipant;
                displayText = $"{line.Speaker}: {line.Text}";

            }
            else
            {
                displayText = $"{line.Speaker}: {line.Text}";
            }

            if (lineText != null)
                lineText.text = displayText;

            float waitTime = 0f;

            if (line.Speaker.Equals("P", StringComparison.OrdinalIgnoreCase))
            
            {
                if (lineText != null)
                    lineText.text = displayText; // ← affiche le texte AVANT de parler

                yield return StartCoroutine(HandlePlayerSpeaking(sourceToUse, line));
                continue;

            }


            if (sourceToUse != null && !string.IsNullOrEmpty(line.AudioFile) && 
                !line.AudioFile.Equals("NA", StringComparison.OrdinalIgnoreCase))
            {
                string clipKey = Path.GetFileNameWithoutExtension(line.AudioFile.Trim());

                if (!clipCache.TryGetValue(clipKey, out AudioClip clip))
                    clip = clipCache.FirstOrDefault(kvp => kvp.Key.Equals(clipKey, StringComparison.OrdinalIgnoreCase)).Value;

                if (clip != null)
                {
                    sourceToUse.clip = clip;
                    sourceToUse.Play();
                    waitTime = clip.length;
                }
            }

            if (waitTime <= 0f)
                waitTime = Mathf.Max(2f, line.Text.Length * 0.2f);

            yield return new WaitForSeconds(waitTime);

            if (line.Speaker.Equals("SA", StringComparison.OrdinalIgnoreCase) && retargeter != null)
                retargeter.enabled = true;
        }

        if (lineText != null)
            lineText.text = "Conversation terminée";
        
        OnConversationFinished?.Invoke();
    }

    IEnumerator HandlePlayerSpeaking(AudioSource source, ConversationLine line)
{
    if (Microphone.devices.Length > 0)
    {
        string micName = Microphone.devices[0];

        AudioClip micClip = Microphone.Start(micName, true, 20, 44100);

        while (!(Microphone.GetPosition(micName) > 0))
            yield return null;

        // Use AudioAvatar directly so lipsync source never changes
        AudioAvatar.clip = micClip;
        AudioAvatar.mute = false; // mute so the player doesn't hear themselves
        AudioAvatar.Play();


        float duration = useTextBasedDuration 
            ? Mathf.Min(playerSpeakingTime, line.Text.Length * 0.2f) 
            : playerSpeakingTime;

        yield return new WaitForSeconds(duration);

        Microphone.End(micName);
        AudioAvatar.Stop();
        AudioAvatar.mute = false;
        AudioAvatar.clip = null;
    }
    else
    {
        yield return new WaitForSeconds(2f);
    }
}
}
