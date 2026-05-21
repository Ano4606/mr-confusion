using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    [Header("Panels / Phases")]
    public ParticipantNumberSelector numberSelector;
    public ChoosePhaseAvatarManager choosePhase;
    public EmbodimentManager embodimentPhase;
    public ConversationManager conversationPhase;

    [Header("Global References")]
    public AvatarManager avatarManager;
    public ParticipantIDManager participantIDManager;
    public GameObject mirror;

    [Header("Debug")]
    public bool debugMode = false;
    public string debugAvatarName = "participant-black-female";
    public bool skipEmbodiment = true;
    public TMPro.TextMeshProUGUI debugText;

    private void Awake()
    {
        if (numberSelector != null)
            numberSelector.OnConfirmed += OnParticipantConfirmed;

        if (choosePhase != null)
            choosePhase.OnAvatarChosen += OnAvatarChosen;

        if (embodimentPhase != null)
            embodimentPhase.OnEmbodimentFinished += OnEmbodimentFinished;
    }

    private void OnDestroy()
    {
        if (numberSelector != null)
            numberSelector.OnConfirmed -= OnParticipantConfirmed;

        if (choosePhase != null)
            choosePhase.OnAvatarChosen -= OnAvatarChosen;

        if (embodimentPhase != null)
            embodimentPhase.OnEmbodimentFinished -= OnEmbodimentFinished;
    }

    private void Start()
    {
        if (mirror != null) mirror.SetActive(false);
        if (embodimentPhase != null) embodimentPhase.gameObject.SetActive(false);
        if (conversationPhase != null) conversationPhase.gameObject.SetActive(false);
        if (choosePhase != null) choosePhase.gameObject.SetActive(false);

        if (debugMode)
        {
            OnAvatarChosen(debugAvatarName);
            if (skipEmbodiment) OnEmbodimentFinished();
        }
        else
        {
            // Start with participant number panel
            if (numberSelector != null)
                numberSelector.gameObject.SetActive(true);
        }
    }

    // Step 1: participant number confirmed → show avatar selection
    private void OnParticipantConfirmed(string participantID)
    {
        Log($"Participant confirmed: {participantID}");
        if (choosePhase != null) choosePhase.Show();
    }

    // Step 2: avatar chosen → bind everything, show mirror, start embodiment
    private void OnAvatarChosen(string avatarName)
    {
        Log($"Avatar chosen: {avatarName}");

        if (avatarManager != null)
        {
            avatarManager.SelectParticipant(avatarName);

            if (conversationPhase != null)
            {
                conversationPhase.BindToAvatar(avatarManager.ActiveParticipantAvatar);
                conversationPhase.BindToInterlocutor(avatarManager.ActiveInterlocutorAvatar);

                if (participantIDManager != null)
                {
                    conversationPhase.SetParticipantID(participantIDManager.ParticipantID);
                    conversationPhase.SetGenderAndGroup(avatarManager.SelectedGender, participantIDManager.GroupNumber);
                }
            }
        }

        if (mirror != null) mirror.SetActive(true);

        if (embodimentPhase != null)
        {
            embodimentPhase.gameObject.SetActive(true);

            // Give embodiment phase the selected avatar so it can drive lipsync
            if (avatarManager != null)
            {
                embodimentPhase.BindToAvatar(avatarManager.ActiveParticipantAvatar);
                embodimentPhase.BindToInterlocutor(avatarManager.ActiveInterlocutorAvatar);

                // Force avatar to face the mirror before embodiment starts
                FaceAvatarTowardMirror(avatarManager.ActiveParticipantAvatar);
            }

            embodimentPhase.PlayInstruction();
        }
    }

    // Step 3: embodiment done → start conversation
    private void OnEmbodimentFinished()
    {
        if (embodimentPhase != null) embodimentPhase.gameObject.SetActive(false);
        if (conversationPhase != null)
        {
            // Force avatar to face the mirror before conversation starts
            if (avatarManager != null)
                FaceAvatarTowardMirror(avatarManager.ActiveParticipantAvatar);

            conversationPhase.gameObject.SetActive(true);
            conversationPhase.StartTask();
        }
    }

    /// <summary>
    /// Rotates the participant avatar's root transform to face the mirror,
    /// keeping the Y-axis upright (horizontal rotation only).
    /// </summary>
    private void FaceAvatarTowardMirror(GameObject avatar)
    {
        if (avatar == null || mirror == null) return;

        // The mirror's forward points out from its surface into the room.
        // The avatar should face that same direction (toward the mirror surface).
        Vector3 mirrorForward = mirror.transform.forward;
        mirrorForward.y = 0f;

        if (mirrorForward.sqrMagnitude < 0.0001f) return;

        avatar.transform.rotation = Quaternion.LookRotation(-mirrorForward.normalized);
        Log($"Avatar '{avatar.name}' rotated to face mirror.");
    }

    private void Log(string msg)
    {
        Debug.Log($"[ExperimentManager] {msg}");
        if (debugText != null) debugText.text = msg;
    }
}
