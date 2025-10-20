using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Linq;

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
    public float lineDuration = 3f;       // Duration per line
    public string Participant = "P01";    // Can set via UI

    [Header("Audio")]
    public AudioSource AudioInterlocutor;       // Assign in Inspector
    public AudioSource AudioAvatar;       // Assign in Inspector

    private List<ConversationLine> conversation = new List<ConversationLine>();
    private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

    void Start()
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
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio");
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
            string displayText;

            // Decide which AudioSource to use
            AudioSource sourceToUse = null;

            if (line.Speaker.Equals("I", System.StringComparison.OrdinalIgnoreCase))
            {
                displayText = "..."; // Or whatever you want for this speaker
                sourceToUse = AudioInterlocutor;
            }
            else if (line.Speaker.Equals("A", System.StringComparison.OrdinalIgnoreCase))
            {
                displayText = "...";
                sourceToUse = AudioAvatar;
            }
            else
            {
                displayText = $"{line.Speaker}: {line.Word}";
            }

            lineText.text = displayText;

            // Play the audio if available
            if (sourceToUse != null && !string.IsNullOrEmpty(line.AudioFile))
            {
                if (clipCache.TryGetValue(Path.GetFileNameWithoutExtension(line.AudioFile), out AudioClip clip))
                {
                    sourceToUse.PlayOneShot(clip);
                }
            }

            yield return new WaitForSeconds(lineDuration);
        }

        lineText.text = "Conversation finished!";
        Debug.Log("Conversation complete!");
    }

}
