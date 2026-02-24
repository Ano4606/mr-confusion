using System;
using UnityEngine;
using UnityEngine.UI;

public class ChoosePhaseAvatarManager : MonoBehaviour
{
    public GameObject choosingAvatarPhase;
    public GameObject mirror;

    public Toggle[] avatarToggles;
    public string[] avatarNames;

    private bool hasChosen = false;

    public event Action<string> OnAvatarChosen;

    void Start()
    {
        // First, ensure all toggles start unchecked WITHOUT triggering events
        for (int i = 0; i < avatarToggles.Length; i++)
        {
            avatarToggles[i].SetIsOnWithoutNotify(false);
        }

        // Then add listeners after setting initial state
        for (int i = 0; i < avatarToggles.Length; i++)
        {
            int index = i;
            avatarToggles[i].onValueChanged.AddListener(
                (on) => { if (on) SelectAvatar(index); }
            );
        }
    }

    public void Show()
    {
        choosingAvatarPhase.SetActive(true);
        mirror.SetActive(false);
        hasChosen = false;
        
        // Reset all toggles when showing the panel WITHOUT triggering events
        for (int i = 0; i < avatarToggles.Length; i++)
        {
            avatarToggles[i].SetIsOnWithoutNotify(false);
        }
    }

    private void SelectAvatar(int index)
    {
        if (hasChosen) return;
        
        if (index < 0 || index >= avatarNames.Length)
        {
            Debug.LogError($"Avatar index {index} out of bounds!");
            return;
        }
        
        hasChosen = true;

        string avatarName = avatarNames[index];
        OnAvatarChosen?.Invoke(avatarName);

        choosingAvatarPhase.SetActive(false);
		mirror.SetActive(true);
    }
}
