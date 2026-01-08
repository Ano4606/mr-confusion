using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    public ChoosePhaseAvatarManager choosePhase;
    public EmbodimentManager embodimentPhase;
    public ConversationManager conversationPhase;

    public AvatarManager avatarManager;

    public GameObject mirror;

    private void Start()
    {
        // Subscribe to phase events
        choosePhase.OnAvatarChosen += HandleAvatarChosen;
        embodimentPhase.OnEmbodimentFinished += HandleEmbodimentFinished;

        // Start the first phase
        choosePhase.Show();
    }

    private void HandleAvatarChosen(string avatarName)
    {
    Debug.Log("Avatar chosen: " + avatarName);

    // Activate participant + interlocutor avatars
    avatarManager.SelectParticipant(avatarName);

    // Bind conversation to the active participant avatar
    conversationPhase.BindToAvatar(avatarManager.ActiveParticipantAvatar);

    // Show mirror & start embodiment
    mirror.SetActive(true);
    embodimentPhase.gameObject.SetActive(true);
    embodimentPhase.PlayInstruction();
    }


    // private void HandleAvatarChosen(string avatarName)
    // {
    //     mirror.SetActive(true);

    //     // Start embodiment training
    //     embodimentPhase.gameObject.SetActive(true);
    //     embodimentPhase.PlayInstruction();
    // }

    private void HandleEmbodimentFinished()
    {
        embodimentPhase.gameObject.SetActive(false);

        // Start Conversation
        conversationPhase.gameObject.SetActive(true);
        conversationPhase.StartTask();
    }
}


// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class ExperimentManager : MonoBehaviour
// {
//     public GameObject ConversationPhase;
//     public GameObject EmbodiementPhase;
//     public GameObject ChoosePhase;

//     public ChoosingAvatarPhaseManager choosingAvatarPhaseManager;

//     public EmbodimentManager embodimentmanager;

//     public ConversationManager conversationManager;
//    public GameObject mirror;


// public void Start()
//     {
        
//     choosingAvatarPhaseManager.StartChoosing();

//     }

//     public void BeginExperiment()
//     {
//         StartTraining();
//     }

//     void StartTraining()
//     {
//         mirror.SetActive(true);
//         EmbodiementPhase.SetActive(true);
//         embodimentmanager.PlayInstruction();
//         ConversationPhase.SetActive(false);
//     }

//     public void EndTrainingAndStartTrials()
//     {
//         EmbodiementPhase.SetActive(false);
//         StartCoroutine(WaitAndStartTask());
//     }

//     private IEnumerator WaitAndStartTask()
//     {
//         yield return new WaitForSeconds(2f);
//         ConversationPhase.SetActive(true);
//         conversationManager.StartTask(); 
//     }
// }

