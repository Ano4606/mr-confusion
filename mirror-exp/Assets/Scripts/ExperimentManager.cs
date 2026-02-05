using UnityEngine;

    public class ExperimentManager : MonoBehaviour
    {
        [Header("Phase Managers")]
        public ChoosePhaseAvatarManager choosePhase;
        public EmbodimentManager embodimentPhase;
        public ConversationManager conversationPhase;

        [Header("Global References")]
        public AvatarManager avatarManager;
        public GameObject mirror;
        
        public TMPro.TextMeshProUGUI debugText;

        private void Awake()
        {
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

            // Start the first phase
            if (choosePhase != null)
            {
                choosePhase.Show();
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
                    conversationPhase.BindToAvatar(avatarManager.ActiveParticipantAvatar);
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


