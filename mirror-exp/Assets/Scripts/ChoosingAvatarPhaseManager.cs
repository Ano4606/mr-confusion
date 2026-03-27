using System;
using UnityEngine;
using UnityEngine.UI;

public class ChoosePhaseAvatarManager : MonoBehaviour
{
    [Header("Panel to hide on confirm")]
    public GameObject panelToHide;

    public Toggle[] avatarToggles;
    public string[] avatarNames;

    public event Action<string> OnAvatarChosen;

    void Start()
    {
        for (int i = 0; i < avatarToggles.Length; i++)
        {
            avatarToggles[i].SetIsOnWithoutNotify(false);
            int index = i;
            avatarToggles[i].onValueChanged.AddListener(isOn => { if (isOn) SelectAvatar(index); });
        }
    }

    public void Show()
    {
        (panelToHide != null ? panelToHide : gameObject).SetActive(true);
        foreach (var t in avatarToggles)
            t.SetIsOnWithoutNotify(false);
    }

    private void SelectAvatar(int index)
    {
        if (index < 0 || index >= avatarNames.Length) return;
        OnAvatarChosen?.Invoke(avatarNames[index]);
        (panelToHide != null ? panelToHide : gameObject).SetActive(false);
    }
}
