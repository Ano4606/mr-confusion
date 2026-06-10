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
    public string Participant = "";
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
    private string groupNumber;
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

    public void SetGenderAndGroup(string gender, string group)
    {
        participantGender = gender.ToLower();
        
        // Only set group if it's not empty - otherwise let CSV extraction handle it
        if (!string.IsNullOrEmpty(group))
        {
            groupNumber = group;
        }
    }
    
    public void SetParticipantID(string participantID)
    {
        Participant = participantID;
        Debug.Log($"ConversationManager: Participant ID set to {participantID}");
    }

    public void StartTask()
    {
        if (string.IsNullOrEmpty(participantGender))
            participantGender = "female";

        // Ensure no leftover state from embodiment phase
        if (AudioAvatar != null)
        {
            AudioAvatar.Stop();
            AudioAvatar.clip  = null;
            AudioAvatar.loop  = false;
            AudioAvatar.mute  = false;
        }
        if (Microphone.devices.Length > 0)
            Microphone.End(Microphone.devices[0]);

        // Load conversation FIRST to extract group number from CSV
        LoadConversation(Participant);
        
        // Set default group if still empty after loading
        if (string.IsNullOrEmpty(groupNumber))
        {
            lineText.text = "No group found!";
            groupNumber = "";
        }

        // Now preload audio with the correct group number
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
            
            // Extract group number from CSV (column 1) on first matching line
            if (string.IsNullOrEmpty(groupNumber) && values.Length > 1)
            {
                // Extract just the number from "Group1", "Group2", etc.
                string groupValue = values[1];
                if (groupValue.StartsWith("Group", StringComparison.OrdinalIgnoreCase))
                {
                    groupNumber = groupValue.Substring(5); // Extract number after "Group"
                    Debug.Log($"Group number extracted from CSV for {participant}: {groupNumber}");
                    
                    // Update ParticipantIDManager with the group number
                    if (ParticipantIDManager.Instance != null)
                    {
                        ParticipantIDManager.Instance.SetGroupNumber(groupNumber);
                    }
                }
            }
            
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

    IEnumerator PulseText(float scaleFactor = 1.4f, float duration = 0.4f)
    {
        if (lineText == null) yield break;

        RectTransform rt = lineText.GetComponent<RectTransform>();
        Vector3 originalScale = rt.localScale;
        Vector3 targetScale = originalScale * scaleFactor;

        float half = duration / 2f;

        // Scale up
        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.Lerp(originalScale, targetScale, t / half);
            yield return null;
        }

        // Scale back down
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.Lerp(targetScale, originalScale, t / half);
            yield return null;
        }

        rt.localScale = originalScale;
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
                {
                    avatarLipsync.audioSource = AudioAvatar;
                    avatarLipsync.audioLoopback = true;
                }

                // Disable retargeter so animation can take over body movement
                if (retargeter != null)
                    retargeter.enabled = false;

                if (selfAvatarAnimator != null)
                {
                    selfAvatarAnimator.enabled = true;
                    selfAvatarAnimator.Play("participant-talking", 0, 0f);
                }
            }
            else if (line.Speaker.Equals("P", StringComparison.OrdinalIgnoreCase))
            {
                sourceToUse = AudioParticipant;
                displayText = line.Text;
                if (avatarLipsync != null)
                    avatarLipsync.audioLoopback = false;
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
                    lineText.text = displayText;

                yield return StartCoroutine(PulseText());
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

            if (line.Speaker.Equals("SA", StringComparison.OrdinalIgnoreCase))
            {
                // Return body control to the retargeter and go back to idle
                if (selfAvatarAnimator != null)
                    selfAvatarAnimator.Play("participant-talking", 0, 0f);

                if (retargeter != null)
                    retargeter.enabled = true;
            }
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
