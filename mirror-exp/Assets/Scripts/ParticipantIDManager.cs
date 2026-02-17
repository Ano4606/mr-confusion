using System;
using UnityEngine;

public class ParticipantIDManager : MonoBehaviour
{
    [Header("Inspector Settings")]
    [Tooltip("Enter participant ID (e.g., P01, P02, P03)")]
    public string participantID = "P01";
    
    [Tooltip("Group number (1-3) - determines which audio folder to use")]
    [Range(1, 3)]
    public int groupNumber = 1;

    public static ParticipantIDManager Instance { get; private set; }
    
    public string ParticipantID => participantID;
    public int GroupNumber => groupNumber;
    
    public event Action<string, int> OnParticipantIDConfirmed;

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
        
        // Validate on awake
        ValidateSettings();
    }

    void Start()
    {
        // Automatically confirm the inspector settings
        ConfirmSettings();
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrEmpty(participantID))
        {
            Debug.LogWarning("ParticipantID is empty! Using default 'P01'");
            participantID = "P01";
        }
        
        if (groupNumber < 1 || groupNumber > 3)
        {
            Debug.LogWarning($"Group number {groupNumber} is out of range! Using default 1");
            groupNumber = 1;
        }
    }

    private void ConfirmSettings()
    {
        Debug.Log($"Participant ID: {participantID}, Group: {groupNumber}");
        OnParticipantIDConfirmed?.Invoke(participantID, groupNumber);
    }

    // Optional: Call this if you want to update settings at runtime
    public void UpdateSettings(string newParticipantID, int newGroupNumber)
    {
        participantID = newParticipantID;
        groupNumber = Mathf.Clamp(newGroupNumber, 1, 3);
        
        Debug.Log($"Settings updated - Participant ID: {participantID}, Group: {groupNumber}");
        OnParticipantIDConfirmed?.Invoke(participantID, groupNumber);
    }
}
