using System;
using UnityEngine;
using UnityEngine.UI;

public class ChoosePhaseAvatarManager : MonoBehaviour
{
    public GameObject panel;
    public GameObject handsMenu;
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
        panel.SetActive(true);
        handsMenu.SetActive(true);
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

        panel.SetActive(false);
        handsMenu.SetActive(false);
		mirror.SetActive(true);
    }
}


// public class ChoosePhaseAvatarManager : MonoBehaviour
// {

// public GameObject mirror;

// [Header("UI Panels")]
// public GameObject choosingPanel;
// public GameObject embodimentPhase;
// public GameObject conversationPhase;


// [Header("Managers")]
// public AvatarManager avatarManager;
// public ExperimentManager experimentManager;
// public ConversationManager conversationManager;

// public GameObject handsMenu; // Activate and desactivate the hands for the menu 

// [Header("Avatar Toggles")]
// public Toggle[] avatarToggles; // Assign all 6 toggles in inspector
// public string[] avatarNames;    // Same order as toggles

// private bool hasChosen = false;

// void StartChoosing()
// {
//     choosingPanel.SetActive(true);
//     handsMenu.SetActive(true);
//     mirror.SetActive(false);

//     // Add listener for each toggle
//     for (int i = 0; i < avatarToggles.Length; i++)
//     {
//         int index = i; // Local copy for closure
//         avatarToggles[i].onValueChanged.AddListener((isOn) => OnAvatarToggleChanged(isOn, index));
//     }
// }

// private void OnAvatarToggleChanged(bool isOn, int index)
// {
//     if (!isOn) return;        // Only act when toggle is switched ON
//     if (hasChosen) return;    // Prevent multiple selections

//     hasChosen = true;
//     string chosenAvatarName = avatarNames[index];
//     ChooseAvatar(chosenAvatarName);
// }

// private void ChooseAvatar(string participantName)
// {
//     // Tell AvatarManager which avatar to activate
//     avatarManager.SelectParticipant(participantName);

//     // Bind conversation to active participant
//     if (avatarManager.ActiveParticipantAvatar != null)
//     {
//         conversationManager.BindToAvatar(avatarManager.ActiveParticipantAvatar);
//     }

//     // Switch UI panels
//     handsMenu.SetActive(false);
//     choosingPanel.SetActive(false);

//     // Begin experiment
//     experimentManager.BeginExperiment();
// }

// }
