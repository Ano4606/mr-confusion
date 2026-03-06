using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ParticipantNumberSelector : MonoBehaviour
{
    [Header("UI References")]
    public GameObject selectorPanel;

    public TextMeshProUGUI displayText;
    public Toggle confirmButton;
    public Toggle deleteButton;

    
    [Header("Number Toggles")]
    public Toggle[] numberToggles; // Toggles for digits
    public int[] digitValues; // Corresponding digit values (e.g., [1,2,3,4,5,6,7,8,9,0])
    
    [Header("Settings")]
    public int maxDigits = 3; // Max participant number (e.g., P999)
    
    private string currentNumber = "";
    private bool hasSelected = false;
    
    public event Action<int> OnNumberConfirmed;
    
    void Start()
    {
        // Validate arrays match
        if (numberToggles.Length != digitValues.Length)
        {
            Debug.LogError("Number toggles and digit values arrays must be the same length!");
            return;
        }
        
        // Initialize all toggles as off without triggering events
        for (int i = 0; i < numberToggles.Length; i++)
        {
            numberToggles[i].SetIsOnWithoutNotify(false);
        }
        
        // Setup number toggles with their corresponding digit values
        for (int i = 0; i < numberToggles.Length; i++)
        {
            int digit = digitValues[i]; // Use the mapped digit value
            numberToggles[i].onValueChanged.AddListener((isOn) => 
            {
                if (isOn) AddDigit(digit);
            });
        }
        
        if (confirmButton != null)
        {
            confirmButton.onValueChanged.AddListener((isOn) => 
            {
                if (isOn) ConfirmNumber();
            });
        }
        
        if (deleteButton != null)
        {
            deleteButton.onValueChanged.AddListener((isOn) => 
            {
                if (isOn)
                {
                    Debug.Log("Delete toggle activated");
                    DeleteLastDigit();
                }
            });
        }
        else
        {
            Debug.LogWarning("Delete button is not assigned in Inspector!");
        }
        
        UpdateDisplay();
    }
    
    public void Show()
    {
        selectorPanel.SetActive(true);
        Clear();
    }
    
    private void AddDigit(int digit)
    {
        if (currentNumber.Length < maxDigits)
        {
            currentNumber += digit.ToString();
            UpdateDisplay();
            
            // Reset all toggles after selection
            ResetToggles();
        }
    }
    
    private void Clear()
    {
        currentNumber = "";
        hasSelected = false;
        ResetToggles();
        UpdateDisplay();
    }
    
    private void DeleteLastDigit()
    {
        Debug.Log($"DeleteLastDigit called. Current number: '{currentNumber}'");
        
        if (currentNumber.Length > 0)
        {
            currentNumber = currentNumber.Substring(0, currentNumber.Length - 1);
            Debug.Log($"After delete: '{currentNumber}'");
            UpdateDisplay();
        }
        else
        {
            Debug.Log("Nothing to delete - current number is empty");
        }
        
        // Reset delete toggle
        if (deleteButton != null)
        {
            deleteButton.SetIsOnWithoutNotify(false);
        }
    }
    
    private void ResetToggles()
    {
        for (int i = 0; i < numberToggles.Length; i++)
        {
            numberToggles[i].SetIsOnWithoutNotify(false);
        }
    }
    
    private void UpdateDisplay()
    {
        displayText.text = string.IsNullOrEmpty(currentNumber) ? "---" : currentNumber;
        confirmButton.interactable = !string.IsNullOrEmpty(currentNumber);
    }
    
    private void ConfirmNumber()
    {
        if (int.TryParse(currentNumber, out int participantNum))
        {
            string participantID = $"P{participantNum}";
            
            OnNumberConfirmed?.Invoke(participantNum);
            
            // Update ParticipantIDManager if it exists
            if (ParticipantIDManager.Instance != null)
            {
                ParticipantIDManager.Instance.SetParticipantID(participantID);
            }
            
            selectorPanel.SetActive(false);
        }
    }
}
