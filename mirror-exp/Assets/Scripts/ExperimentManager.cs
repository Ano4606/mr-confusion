using UnityEngine;

    public class ExperimentManager : MonoBehaviour
    {
        [Header("Debug Mode")]
        [Tooltip("Skip avatar selection and use debug avatar directly")]
        public bool debugMode = false;
        
        [Tooltip("Avatar to use in debug mode (e.g., participant-black-female)")]
        public string debugAvatarName = "participant-black-female";
        
        [Tooltip("Skip embodiment phase in debug mode")]
        public bool skipEmbodiment = true;
        
        [Header("Phase Managers")]
        public ParticipantIDManager participantIDManager;
        public ChoosePhaseAvatarManager choosePhase;
        public EmbodimentManager embodimentPhase;
        public ConversationManager conversationPhase;

        [Header("Global References")]
        public AvatarManager avatarManager;
        public GameObject mirror;
        
        public TMPro.TextMeshProUGUI debugText;

        private void Awake()
        {
            // Subscribe to participant ID manager
            if (participantIDManager != null)
            {
                participantIDManager.OnParticipantIDConfirmed += HandleParticipantIDConfirmed;
            }
            
            // Subscribe here to ensure we don't miss the event
            if (choosePhase != null)
            {
                choosePhase.OnAvatarChosen += HandleAvatarChosen;
            }
        
            if (embodimentPhase != null)
            {
                embodimentPhase.OnEmbodimentFinished += HandleEmbodimentFinished;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            if (participantIDManager != null)
            {
                participantIDManager.OnParticipantIDConfirmed -= HandleParticipantIDConfirmed;
            }
            
            if (choosePhase != null)
            {
                choosePhase.OnAvatarChosen -= HandleAvatarChosen;
            }
        
            if (embodimentPhase != null)
            {
                embodimentPhase.OnEmbodimentFinished -= HandleEmbodimentFinished;
            }
        }

        private void Start()
        {
            // Hide all phases except choosing at startup
            if (mirror != null)
                mirror.SetActive(false);
            
            if (embodimentPhase != null)
                embodimentPhase.gameObject.SetActive(false);
            
            if (conversationPhase != null)
                conversationPhase.gameObject.SetActive(false);

            // DEBUG MODE: Skip avatar selection
            if (debugMode)
            {
                Debug.Log($"[DEBUG MODE] Using avatar: {debugAvatarName}");
                StartDebugMode();
            }
            else
            {
                // Normal mode: Start the avatar selection phase
                // ParticipantIDManager will auto-confirm its inspector settings
                if (choosePhase != null)
                {
                    choosePhase.Show();
                }
            }
        }

        private void StartDebugMode()
        {
            // Simulate avatar selection
            HandleAvatarChosen(debugAvatarName);
            
            // Skip embodiment if requested
            if (skipEmbodiment)
            {
                Debug.Log("[DEBUG MODE] Skipping embodiment phase");
                HandleEmbodimentFinished();
            }
        }

        private void HandleAvatarChosen(string avatarName)
        {
            if (debugText != null) debugText.text = "Step 1: Event Received for " + avatarName;

            if (avatarManager != null)
            {
                avatarManager.SelectParticipant(avatarName);
                if (debugText != null) debugText.text = "Step 2: Avatar Manager Done";
        
                if (conversationPhase != null)
                {
                    // Bind participant avatar
                    conversationPhase.BindToAvatar(avatarManager.ActiveParticipantAvatar);
                    
                    // Bind interlocutor avatar
                    conversationPhase.BindToInterlocutor(avatarManager.ActiveInterlocutorAvatar);
                    
                    // Set gender and group for conversation audio
                    if (participantIDManager != null)
                    {
                        conversationPhase.SetGenderAndGroup(
                            avatarManager.SelectedGender, 
                            participantIDManager.GroupNumber
                        );
                    }
                    
                    if (debugText != null) debugText.text = "Step 3: Conversation Bound";
                }
            }

            if (mirror != null) mirror.SetActive(true);
    
            if (embodimentPhase != null)
            {
                if (debugText != null) debugText.text = "Step 4: Starting Embodiment";
                embodimentPhase.gameObject.SetActive(true);
                embodimentPhase.PlayInstruction();
            }
        }

        private void HandleParticipantIDConfirmed(string participantID, int groupNumber)
        {
            Debug.Log($"ExperimentManager: Using Participant ID '{participantID}' with Group {groupNumber}");
            // Settings are already confirmed, no additional action needed
        }

        private void HandleEmbodimentFinished()
        {
            if (embodimentPhase != null) embodimentPhase.gameObject.SetActive(false);

            // Start Conversation
            if (conversationPhase != null)
            {
                conversationPhase.gameObject.SetActive(true);
                conversationPhase.StartTask();
            }
        }
    }


