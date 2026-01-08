using UnityEngine;
using System.Collections.Generic;

public class AvatarManager : MonoBehaviour
{
        [Header("Participant Avatars (6)")]
    public List<GameObject> participantObjects;

    [Header("Interlocutor Avatars (6)")]
    public List<GameObject> interlocutorObjects;

    [HideInInspector] public GameObject ActiveParticipantAvatar;
    [HideInInspector] public GameObject ActiveInterlocutorAvatar;

    private Dictionary<string, GameObject> participantLookup;
    private Dictionary<string, GameObject> interlocutorLookup;
    public DynamicEyeBlinker blinker;


    void Awake()
    {
        participantLookup = new Dictionary<string, GameObject>();
        interlocutorLookup = new Dictionary<string, GameObject>();

        foreach (var obj in participantObjects)
            participantLookup[obj.name] = obj;

        foreach (var obj in interlocutorObjects)
            interlocutorLookup[obj.name] = obj;
    }

    public void SelectParticipant(string participantName)
    {
        // --- Activate participant avatar ---
        foreach (var obj in participantObjects)
            obj.SetActive(false);

        if (participantLookup.TryGetValue(participantName, out GameObject chosen))
        {
            chosen.SetActive(true);
            ActiveParticipantAvatar = chosen;
            //ApplyUniqueMaterials(chosen);
        }
        else
        {
            Debug.LogError("Participant not found: " + participantName);
            return;
        }

        // --- Determine corresponding interlocutor ---
        string interlocutorName = ChooseInterlocutor(participantName);

        // --- Activate interlocutor ---
        foreach (var obj in interlocutorObjects)
            obj.SetActive(false);

        if (interlocutorLookup.TryGetValue(interlocutorName, out GameObject inter))
        {
            inter.SetActive(true);
            ActiveInterlocutorAvatar = inter;
        }
        else
        {
            Debug.LogError("Interlocutor not found: " + interlocutorName);
        }

        if (blinker != null)
            blinker.SetActiveAvatars(ActiveParticipantAvatar, ActiveInterlocutorAvatar);

    }

    private string ChooseInterlocutor(string p)
    {
        switch (p)
        {
            case "participant-black-female":  return "interlocutor-black-male";
            case "participant-white-female":  return "interlocutor-white-male";
            case "participant-asian-female":  return "interlocutor-asian-male";

            case "participant-black-male":    return "interlocutor-black-female";
            case "participant-white-male":    return "interlocutor-white-female";
            case "participant-asian-male":    return "interlocutor-asian-female";
        }

        Debug.LogWarning("No interlocutor match for: " + p);
        return "";
    }

    // private void ApplyUniqueMaterials(GameObject avatar)
    // {
    //     Renderer[] renderers = avatar.GetComponentsInChildren<Renderer>();

    //     foreach (Renderer r in renderers)
    //         r.material = new Material(r.material);
    // }
}
