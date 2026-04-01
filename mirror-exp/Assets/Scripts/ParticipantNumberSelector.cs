using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ParticipantNumberSelector : MonoBehaviour
{
    [Header("Panel to hide on confirm")]
    public GameObject panelToHide;

    [Header("UI References")]
    public TextMeshProUGUI displayText;
    public Toggle confirmButton;
    public Toggle deleteButton;

    [Header("Number Toggles")]
    public Toggle[] numberToggles;
    public int[] digitValues;

    [Header("Settings")]
    public int maxDigits = 3;

    public event Action<string> OnConfirmed; // fires "P42" etc.

    private string currentNumber = "";

    void Start()
    {
        for (int i = 0; i < numberToggles.Length; i++)
        {
            numberToggles[i].SetIsOnWithoutNotify(false);
            int digit = digitValues[i];
            numberToggles[i].onValueChanged.AddListener(isOn => { if (isOn) AddDigit(digit); });
        }

        confirmButton.SetIsOnWithoutNotify(false);
        confirmButton.onValueChanged.AddListener(isOn => { if (isOn) Confirm(); });

        deleteButton.SetIsOnWithoutNotify(false);
        deleteButton.onValueChanged.AddListener(isOn => { if (isOn) DeleteLast(); });

        UpdateDisplay();
    }

    private void AddDigit(int digit)
    {
        if (currentNumber.Length >= maxDigits) return;
        currentNumber += digit.ToString();
        ResetNumberToggles();
        UpdateDisplay();
    }

    private void DeleteLast()
    {
        if (currentNumber.Length > 0)
            currentNumber = currentNumber.Substring(0, currentNumber.Length - 1);
        deleteButton.SetIsOnWithoutNotify(false);
        UpdateDisplay();
    }

    private void Confirm()
    {
        if (string.IsNullOrEmpty(currentNumber)) return;

        string participantID = $"{currentNumber}";

        if (ParticipantIDManager.Instance != null)
            ParticipantIDManager.Instance.SetParticipantID(participantID);

        OnConfirmed?.Invoke(participantID);
        (panelToHide != null ? panelToHide : gameObject).SetActive(false);
    }

    private void ResetNumberToggles()
    {
        foreach (var t in numberToggles)
            t.SetIsOnWithoutNotify(false);
    }

    private void UpdateDisplay()
    {
        displayText.text = string.IsNullOrEmpty(currentNumber) ? "---" : currentNumber;
        confirmButton.interactable = !string.IsNullOrEmpty(currentNumber);
        confirmButton.SetIsOnWithoutNotify(false);
    }
}
