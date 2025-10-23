using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;


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

    [Header("UI")]
    public TextMeshProUGUI lineText;

    [Header("Settings")]
    public float lineDuration = 50f;       // Duration per line
    public string Participant = "P01";    // Can set via UI

    [Header("Audio")]
    public AudioSource AudioInterlocutor;       // Assign in Inspector
    public AudioSource AudioAvatar;       // Assign in Inspector

    private List<ConversationLine> conversation = new List<ConversationLine>();
    private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

    public void StartTask()
    {
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
        TextAsset csvFile = Resources.Load<TextAsset>("rando");
        if (csvFile == null)
        {
            Debug.LogError("CSV file not found in Resources!");
            return;
        }

        string[] lines = csvFile.text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);

        conversation.Clear();

        for (int i = 1; i < lines.Length; i++) // skip header
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            if (values.Length < 6) continue; // need 6 columns

            // Trim whitespace and remove surrounding quotes
            for (int j = 0; j < values.Length; j++)
                values[j] = values[j].Trim().Trim('"');

            if (!values[0].Equals(participant, System.StringComparison.OrdinalIgnoreCase)) continue;

            if (!int.TryParse(values[2], out int lineNumber)) continue;

            conversation.Add(new ConversationLine
            {
                Line = lineNumber,
                Speaker = values[3],
                Word = values[4],
                AudioFile = values[5] // Column from R script
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
else if (line.Speaker.Equals("A", StringComparison.OrdinalIgnoreCase))
{
    sourceToUse = AudioAvatar;
    displayText = "...";
}
else
{
    sourceToUse = null;
    displayText = $"{line.Speaker}: {line.Word}";
}

lineText.text = displayText;

float waitTime = 0f;

// Play audio if available
if (sourceToUse != null && !string.IsNullOrEmpty(line.AudioFile) && !line.AudioFile.Equals("NA", StringComparison.OrdinalIgnoreCase))
{
    string clipKey = line.AudioFile.Trim();               // Trim whitespace
    clipKey = Path.GetFileNameWithoutExtension(clipKey);  // Remove extension if present

    // Case-insensitive search in clip cache
    AudioClip clip = null;
    foreach (var kvp in clipCache)
    {
        if (kvp.Key.Equals(clipKey, StringComparison.OrdinalIgnoreCase))
        {
            clip = kvp.Value;
            break;
        }
    }

    if (clip != null)
    {
        sourceToUse.clip = clip;
        sourceToUse.Stop();  // Ensure previous audio stops
        sourceToUse.Play();
        waitTime = clip.length;
    }
    else
    {
        Debug.LogWarning($"[ConversationManager] Audio clip not found: '{line.AudioFile}' for line {line.Line}");
    }
}

// If no audio, wait a fixed duration (or dynamically based on text)
if (waitTime <= 0f)
    waitTime = Mathf.Max(2f, line.Word.Length * 0.2f); // Dynamic wait time based on text length

yield return new WaitForSeconds(waitTime);

    }

    lineText.text = "Conversation terminée !";
    Debug.Log("Conversation complète !");
}


}
