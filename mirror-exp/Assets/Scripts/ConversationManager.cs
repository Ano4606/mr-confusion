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
    [System.Serializable]
    public class ConversationLine
    {
        public int Line;
        public string Speaker;
        public string Word;
        public string AudioFile;
    }

    public event Action OnConversationFinished;

    [Header("UI")]
    public TextMeshProUGUI lineText;

    [Header("Settings")]
    public float lineDuration = 50f;
    public string Participant = "P01";
    
    [Header("Player Speaking Settings")]
    public float playerSpeakingTime = 10f;
    public bool useTextBasedDuration = false;

    [Header("Gender & Group")]
    private string participantGender;
    private int groupNumber;

    [Header("Audio")]
    public AudioSource AudioInterlocutor;
    public AudioSource AudioAvatar;
    public AudioSource AudioParticipantMicrophone; // Separate source for participant's mic input

    [Header("Avatar Control")]
    public CharacterRetargeter retargeter;
    public Animator selfAvatarAnimator;

    [Header("Lip Sync")]
    public OVRLipSyncContext avatarLipsync;
    public OVRLipSyncContext interlocutorLipsync;

    private List<ConversationLine> conversation = new List<ConversationLine>();
    private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

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
        
        // Create a separate AudioSource for microphone input
        //AudioParticipantMicrophone = avatar.AddComponent<AudioSource>();
        AudioParticipantMicrophone.playOnAwake = false;
        AudioParticipantMicrophone.loop = false;
        AudioParticipantMicrophone.spatialBlend = 0f; // 2D audio
        
        Debug.Log("[ConversationManager] Participant avatar bound successfully with separate mic AudioSource");
    }

    public void BindToInterlocutor(GameObject interlocutor)
    {
        if (interlocutor == null)
        {
            Debug.LogError("[ConversationManager] Interlocutor GameObject is null!");
            return;
        }

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

        Debug.Log($"[ConversationManager] Interlocutor bound: {interlocutor.name}");
    }

    public void SetGenderAndGroup(string gender, int group)
    {
        participantGender = gender.ToLower();
        groupNumber = group;
        Debug.Log($"ConversationManager: Gender={participantGender}, Group={groupNumber}");
    }

    public void StartTask()
    {
        if (string.IsNullOrEmpty(participantGender))
        {
            Debug.LogError("Gender not set! Using fallback.");
            participantGender = "female";
        }

        if (groupNumber == 0)
        {
            Debug.LogError("Group not set! Using fallback.");
            groupNumber = 1;
        }

        // Check microphone availability
        Debug.Log("=== MICROPHONE DIAGNOSTIC ===");
        if (Microphone.devices.Length > 0)
        {
            Debug.Log($"[Microphone] {Microphone.devices.Length} device(s) detected:");
            for (int i = 0; i < Microphone.devices.Length; i++)
            {
                Debug.Log($"[Microphone] Device {i}: {Microphone.devices[i]}");
            }
            
            if (lineText != null)
            {
                lineText.text = $"Microphone detected: {Microphone.devices[0]}";
            }
        }
        else
        {
            Debug.LogError("[Microphone] NO MICROPHONE DETECTED!");
            if (lineText != null)
            {
                lineText.text = "WARNING: No microphone detected!";
            }
        }
        Debug.Log("============================");

        LoadConversation(Participant);
        PreloadAudioClips();

        if (conversation.Count > 0)
            StartCoroutine(RunConversation());
        else
            lineText.text = "No conversation lines found!";
    }

    void PreloadAudioClips()
    {
        string audioPath = $"conversation-audio/{participantGender}-participant/Group{groupNumber}";
        Debug.Log($"[Audio] Loading from: Resources/{audioPath}");

        AudioClip[] clips = Resources.LoadAll<AudioClip>(audioPath);
        
        if (clips.Length == 0)
        {
            Debug.LogWarning($"No clips found at {audioPath}, trying fallback");
            clips = Resources.LoadAll<AudioClip>("conversation-audio");
        }

        foreach (var clip in clips)
        {
            clipCache[clip.name] = clip;
        }

        Debug.Log($"[Audio] Loaded {clipCache.Count} clips");
    }

    void LoadConversation(string participant)
    {
        TextAsset csvFile = Resources.Load<TextAsset>("rando");
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
                Word = values[4],
                AudioFile = values[5]
            });
        }

        conversation = conversation.OrderBy(c => c.Line).ToList();
        Debug.Log($"[CSV] Loaded {conversation.Count} lines for {participant}");
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

            // Determine speaker
                ///// I speaker

            if (line.Speaker.Equals("I", StringComparison.OrdinalIgnoreCase))
            {
                sourceToUse = AudioInterlocutor;
                displayText = "...";
            }
                ///// SA speaker

            else if (line.Speaker.Equals("SA", StringComparison.OrdinalIgnoreCase))
            {
                sourceToUse = AudioAvatar;
                displayText = "...";

                if (retargeter != null)
                    retargeter.enabled = true;

                if (selfAvatarAnimator != null)
                {
                    selfAvatarAnimator.Play("Standing", 0, 0f);
                    selfAvatarAnimator.Update(0f);
                }
            }
                ///// SA speaker

            else if (line.Speaker.Equals("P", StringComparison.OrdinalIgnoreCase))
            {
                sourceToUse = AudioAvatar;
                displayText = $"{line.Speaker}: {line.Word}";
            }
            else
            {
                displayText = $"{line.Speaker}: {line.Word}";
            }

            lineText.text = displayText;
            float waitTime = 0f;

            // --- MICROPHONE MODE (Player speaking) ---
            if (line.Speaker.Equals("P", StringComparison.OrdinalIgnoreCase))
            {

                if (AudioParticipantMicrophone != null && Microphone.devices.Length > 0)
                {
                    string micName = Microphone.devices[0];
                    Debug.Log($"[Microphone] Starting recording from: {micName}");

                    // Start microphone recording
                    AudioClip micClip = Microphone.Start(micName, true, 20, 44100);
                    
                    // Wait for microphone to start
                    int timeout = 0;
                    while (Microphone.GetPosition(micName) <= 0 && timeout < 100)
                    {
                        timeout++;
                        yield return new WaitForSeconds(0.01f);
                    }

                    if (timeout >= 100)
                    {
                        Debug.LogError("[Microphone] Failed to start!");
                        if (lineText != null)
                        {
                            lineText.text = "⚠️ Microphone failed to start!";
                        }
                        Microphone.End(micName);
                        yield return new WaitForSeconds(2f);
                        continue;
                    }

                    Debug.Log("[Microphone] Recording started");

                    // Assign microphone clip to the SEPARATE microphone AudioSource
                    AudioParticipantMicrophone.clip = micClip;
                    AudioParticipantMicrophone.loop = true;
                    AudioParticipantMicrophone.mute = true; // Mute so user doesn't hear themselves
                    AudioParticipantMicrophone.Play();
                    
                    // Temporarily switch OVRLipSync to use the microphone AudioSource
                    if (avatarLipsync != null)
                    {
                        avatarLipsync.audioSource = AudioParticipantMicrophone;
                        Debug.Log("[LipSync] Switched to microphone AudioSource");
                    }

                    // Calculate duration
                    float duration = useTextBasedDuration 
                        ? Mathf.Min(playerSpeakingTime, line.Word.Length * 0.2f) 
                        : playerSpeakingTime;

                    // Wait for speaking duration
                    yield return new WaitForSeconds(duration);

                    // Stop microphone and audio source
                    Microphone.End(micName);
                    AudioParticipantMicrophone.Stop();
                    AudioParticipantMicrophone.mute = false;
                    AudioParticipantMicrophone.loop = false;
                    
                    // Switch OVRLipSync back to the main AudioSource
                    if (avatarLipsync != null)
                    {
                        avatarLipsync.audioSource = AudioAvatar;
                        Debug.Log("[LipSync] Switched back to main AudioSource");
                    }
                    
                    Debug.Log("[Microphone] Recording ended");
                }
                else
                {
                    Debug.LogWarning("[Microphone] No microphone detected or AudioSource is null!");
                    yield return new WaitForSeconds(2f);
                }
                continue;
            }

            // --- AUDIO FILE PLAYBACK (SA / I) ---
            if (sourceToUse != null && !string.IsNullOrEmpty(line.AudioFile) && 
                !line.AudioFile.Equals("NA", StringComparison.OrdinalIgnoreCase))
            {
                string clipKey = Path.GetFileNameWithoutExtension(line.AudioFile.Trim());

                if (!clipCache.TryGetValue(clipKey, out AudioClip clip))
                {
                    clip = clipCache.FirstOrDefault(kvp => 
                        kvp.Key.Equals(clipKey, StringComparison.OrdinalIgnoreCase)).Value;
                }

                if (clip != null)
                {
                    sourceToUse.clip = clip;
                    sourceToUse.Play();
                    waitTime = clip.length;
                }
                else
                {
                    Debug.LogWarning($"[Audio] Clip not found: {clipKey}");
                }
            }

            if (waitTime <= 0f)
                waitTime = Mathf.Max(2f, line.Word.Length * 0.2f);

            yield return new WaitForSeconds(waitTime);

            if (line.Speaker.Equals("SA", StringComparison.OrdinalIgnoreCase) && retargeter != null)
            {
                retargeter.enabled = true;
            }
        }

        lineText.text = "Conversation terminée !";
        Debug.Log("Conversation complete!");
        OnConversationFinished?.Invoke();
    }
}
