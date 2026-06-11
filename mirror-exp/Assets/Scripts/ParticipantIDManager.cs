using System;
using System.IO;
using UnityEngine;

public class ParticipantIDManager : MonoBehaviour
{
    [Header("Debug Participant ID and Group")]
    public string participantID = "";
    
    public string groupNumber = "";

    public static ParticipantIDManager Instance { get; private set; }
    
    public string ParticipantID => participantID;
    public string GroupNumber => groupNumber;
    
    public event Action<string, string> OnParticipantIDConfirmed;
    
    private bool hasConfirmed = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Only auto-confirm if participantID is already set in inspector
        if (!string.IsNullOrEmpty(participantID))
        {
            ValidateSettings();
            ConfirmSettings();
        }
        else
        {
            Debug.Log("Waiting for participant ID from ParticipantNumberSelector...");
        }
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrEmpty(participantID))
        {
            Debug.LogWarning("ParticipantID is empty!");
        }
        
        if (!string.IsNullOrEmpty(groupNumber))
        {
            if (int.TryParse(groupNumber, out int groupNum))
            {
                if (groupNum < 1 || groupNum > 3)
                {
                    Debug.LogWarning($"Group number {groupNumber} is out of range (1-3)!");
                }
            }
            else
            {
                Debug.LogWarning($"Group number '{groupNumber}' is not a valid number!");
            }
        }
    }

    private void ConfirmSettings()
    {
        if (!hasConfirmed)
        {
            hasConfirmed = true;
            Debug.Log($"Participant ID: {participantID}, Group: {groupNumber}");
            OnParticipantIDConfirmed?.Invoke(participantID, groupNumber);
            SaveToCSV();
        }
    }

    // Optional: Call this if you want to update settings at runtime
    public void UpdateSettings(string newParticipantID, string newGroupNumber)
    {
        participantID = newParticipantID;
        groupNumber = newGroupNumber;
        
        hasConfirmed = false;
        Debug.Log($"Settings updated - Participant ID: {participantID}, Group: {groupNumber}");
        OnParticipantIDConfirmed?.Invoke(participantID, groupNumber);
    }
    
    // Set participant ID from ParticipantNumberSelector
    public void SetParticipantID(string newParticipantID)
    {
        participantID = newParticipantID;
        hasConfirmed = false;
        Debug.Log($"Participant ID set to: {participantID}, Group: {groupNumber}");
        ConfirmSettings();
    }
    
    // Set group number from selector or other source
    public void SetGroupNumber(string newGroupNumber)
    {
        groupNumber = newGroupNumber;
        Debug.Log($"Group number set to: {groupNumber}");
    }

    private void SaveToCSV()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string folder = "/storage/emulated/0/Download";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
#else
        string folder = Application.persistentDataPath;
#endif
        string filePath = Path.Combine(folder, "participants.csv");

        try
        {
            bool fileExists = File.Exists(filePath);
            using (StreamWriter writer = new StreamWriter(filePath, append: true))
            {
                if (!fileExists)
                    writer.WriteLine("ParticipantID,GroupNumber,DateTime");

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                writer.WriteLine($"{participantID},{groupNumber},{timestamp}");
            }
            Debug.Log($"Saved to CSV: {filePath}");
        }
        catch (Exception e)
        {
            // Fallback to persistentDataPath if external write fails
            Debug.LogWarning($"Could not write to {folder}, falling back. Error: {e.Message}");
            string fallbackPath = Path.Combine(Application.persistentDataPath, "participants.csv");
            bool exists = File.Exists(fallbackPath);
            using (StreamWriter writer = new StreamWriter(fallbackPath, append: true))
            {
                if (!exists)
                    writer.WriteLine("ParticipantID,GroupNumber,DateTime");

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                writer.WriteLine($"{participantID},{groupNumber},{timestamp}");
            }
            Debug.Log($"Saved to fallback CSV: {fallbackPath}");
        }
    }
}
