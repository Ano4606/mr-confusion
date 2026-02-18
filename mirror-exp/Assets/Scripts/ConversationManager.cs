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
    
    [Header("Microphone Settings")]
    [Tooltip("Maximum time for player to speak (in seconds)")]
    public float playerSpeakingTime = 10f;
    
    [Tooltip("Calculate time based on text length (0.2s per character)")]
    public bool useTextBasedDuration = false;
    
    [Header("Gender & Group")]
    private string participantGender; // "female" or "male"
    private int groupNumber; // 1, 2, or 3

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
    public void SetGenderAndGroup(string gender, int group)
    {
        participantGender = gender.ToLower();
        groupNumber = group;
        Debug.Log($"ConversationManager: Gender set to {participantGender}, Group set to {groupNumber}");
    }

    public void StartTask()
    {
        if(avatarLipsync == null){
            avatarLipsync = this.GetComponent<OVRLipSyncContext>();
        }

        // Validate gender and group are set
        if (string.IsNullOrEmpty(participantGender))
        {
            Debug.LogError("ConversationManager: Gender not set! Call SetGenderAndGroup() before StartTask()");
            participantGender = "female"; // fallback
        }

        if (groupNumber == 0)
        {
            Debug.LogError("ConversationManager: Group number not set! Call SetGenderAndGroup() before StartTask()");
            groupNumber = 1; // fallback
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
        // Determine the correct audio path based on gender and group
        string audioPath = $"conversation-audio/{participantGender}-participant/Group{groupNumber}";

        Debug.Log($"[ConversationManager] Loading audio clips from: Resources/{audioPath}");
        Debug.Log($"[ConversationManager] Gender: {participantGender}, Group: {groupNumber}");

        AudioClip[] clips = Resources.LoadAll<AudioClip>(audioPath);

        Debug.Log($"[ConversationManager] Found {clips.Length} clips at path: {audioPath}");

        if (clips.Length == 0)
        {
            Debug.LogWarning($"[ConversationManager] No audio clips found at path: {audioPath}. Falling back to default.");
            clips = Resources.LoadAll<AudioClip>("conversation-audio");
            Debug.Log($"[ConversationManager] Fallback found {clips.Length} clips");
        }

        // Count by type
        int iCount = 0;
        int saCount = 0;
        int otherCount = 0;

        foreach (var clip in clips)
        {
            clipCache[clip.name] = clip;
            
            if (clip.name.StartsWith("I_"))
                iCount++;
            else if (clip.name.StartsWith("SA_"))
                saCount++;
            else
                otherCount++;
        }

        Debug.Log($"[ConversationManager] === AUDIO LOADING SUMMARY ===");
        Debug.Log($"[ConversationManager] Total clips loaded: {clips.Length}");
        Debug.Log($"[ConversationManager] Interlocutor (I_*): {iCount} clips");
        Debug.Log($"[ConversationManager] Self-Avatar (SA_*): {saCount} clips");
        Debug.Log($"[ConversationManager] Other: {otherCount} clips");
        Debug.Log($"[ConversationManager] Total in cache: {clipCache.Count}");
        
        if (saCount == 0 && iCount > 0)
        {
            Debug.LogError($"[ConversationManager] ⚠ WARNING: No SA clips loaded but {iCount} I clips found!");
            Debug.LogError($"[ConversationManager] Self-Avatar audio will NOT play during conversation!");
        }
        else if (saCount > 0 && iCount > 0)
        {
            Debug.Log($"[ConversationManager] ✓ Both I and SA audio loaded successfully!");
        }
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

        Debug.Log($"[CSV] Loading conversation for participant: {participant}");

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // Parse CSV line properly handling quoted fields with commas
            string[] values = ParseCSVLine(line);
            
            if (values.Length < 6)
            {
                Debug.LogWarning($"[CSV] Line {i} has only {values.Length} columns, expected 6. Skipping.");
                continue;
            }

            // Clean up quotes from values
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
            
            // Debug first few lines to verify parsing
            if (conversation.Count <= 3)
            {
                Debug.Log($"[CSV] Line {lineNumber}: Speaker={values[3]}, Text='{values[4].Substring(0, Mathf.Min(30, values[4].Length))}...', Audio={values[5]}");
            }
        }

        conversation = conversation.OrderBy(c => c.Line).ToList();
        Debug.Log($"[CSV] Total lines loaded for {participant}: {conversation.Count}");
    }

    // Properly parse CSV line handling quoted fields with commas
    private string[] ParseCSVLine(string line)
    {
        var result = new System.Collections.Generic.List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                // Toggle quote state
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                // End of field
                result.Add(currentField);
                currentField = "";
            }
            else
            {
                // Add character to current field
                currentField += c;
            }
        }

        // Add the last field
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
        if (line.Speaker.Equals("I", StringComparison.OrdinalIgnoreCase))
        {
            sourceToUse = AudioInterlocutor;
            displayText = "...";
        }
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
            
            if (avatarLipsync != null)
                avatarLipsync.audioLoopback = false;
        }
        else if (line.Speaker.Equals("P", StringComparison.OrdinalIgnoreCase))
        {
            sourceToUse = AudioAvatar;
            displayText = $"{line.Speaker}: {line.Word}";
            
            if (avatarLipsync != null)
                avatarLipsync.audioLoopback = false;
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
            if (sourceToUse != null && Microphone.devices.Length > 0)
            {
                string micName = Microphone.devices[0];
                int sampleRate = 44100;

                Debug.Log($"[Microphone] Starting recording from: {micName}");

                // Store original volume and mute state
                float originalVolume = sourceToUse.volume;
                bool originalMute = sourceToUse.mute;

                // MUTE the AudioSource so you don't hear yourself
                sourceToUse.mute = true;

                // Calculate recording duration
                float recordingDuration;
                if (useTextBasedDuration)
                {
                    // Use text length to calculate duration (0.2s per character, max playerSpeakingTime)
                    recordingDuration = Mathf.Min(playerSpeakingTime, line.Word.Length * 0.2f);
                }
                else
                {
                    // Use fixed duration from Inspector
                    recordingDuration = playerSpeakingTime;
                }

                // Start microphone with enough buffer for the recording
                int recordingLength = Mathf.CeilToInt(recordingDuration) + 1;
                AudioClip micClip = Microphone.Start(micName, true, recordingLength, sampleRate);
                sourceToUse.clip = micClip;

                while (!(Microphone.GetPosition(micName) > 0))
                    yield return null;

                // Play for lip sync (but muted so you don't hear it)
                sourceToUse.Play();

                Debug.Log($"[Microphone] Recording for {recordingDuration:F1} seconds (muted)");

                // Wait for the duration
                yield return new WaitForSeconds(recordingDuration);

                Microphone.End(micName);
                sourceToUse.Stop();
                sourceToUse.loop = false;

                // Restore original volume and mute state
                sourceToUse.volume = originalVolume;
                sourceToUse.mute = originalMute;

                Debug.Log($"[Microphone] Recording ended");
            }
            else
            {
                Debug.LogWarning("No microphone detected or AudioSource is null!");
                yield return new WaitForSeconds(2f);
            }
            continue;
        }

        // --- NORMAL AUDIO FILE LOGIC (SA / I / others) ---
        if (sourceToUse != null && !string.IsNullOrEmpty(line.AudioFile) && !line.AudioFile.Equals("NA", StringComparison.OrdinalIgnoreCase))
        {
            string clipKey = Path.GetFileNameWithoutExtension(line.AudioFile.Trim());
            Debug.Log($"[Audio] Looking for clip: '{clipKey}' for speaker: {line.Speaker}");
            
            if (!clipCache.TryGetValue(clipKey, out AudioClip clip))
            {
                // fallback case-insensitive search
                clip = clipCache.FirstOrDefault(kvp => kvp.Key.Equals(clipKey, StringComparison.OrdinalIgnoreCase)).Value;
            }

            if (clip != null)
            {
                Debug.Log($"[Audio] Playing clip: {clip.name} on {sourceToUse.gameObject.name}, Volume: {sourceToUse.volume}, Mute: {sourceToUse.mute}");
                sourceToUse.clip = clip;
                sourceToUse.Stop();
                sourceToUse.Play();
                waitTime = clip.length;
                
                // Check if audio is actually playing
                if (sourceToUse.isPlaying)
                {
                    Debug.Log($"[Audio] ✓ Audio is playing");
                }
                else
                {
                    Debug.LogWarning($"[Audio] ✗ Audio failed to play!");
                }
            }
            else
            {
                Debug.LogWarning($"[Audio] ✗ Audio clip not found: '{line.AudioFile}' (searched for: '{clipKey}')");
                Debug.Log($"[Audio] Available clips in cache: {string.Join(", ", clipCache.Keys)}");
            }
        }
        else
        {
            if (sourceToUse == null)
                Debug.LogWarning($"[Audio] AudioSource is null for speaker: {line.Speaker}");
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


