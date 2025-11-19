using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    public GameObject ConversationPhase;
    public GameObject EmbodiementPhase;
    public ConversationManager conversationManager;
   // public GameObject mirror;

    void Start()
    {
        // ❌ Nothing starts automatically anymore.
        // The phases remain hidden until you call BeginExperiment() manually.
        EmbodiementPhase.SetActive(false);
        ConversationPhase.SetActive(false);
    }

    // 👍 Call this from a button, script, or event when you WANT the experiment to start
    public void BeginExperiment()
    {
        StartTraining();
    }

    void StartTraining()
    {
     //   mirror.SetActive(true);
        EmbodiementPhase.SetActive(true);
        ConversationPhase.SetActive(false);
    }

    public void EndTrainingAndStartTrials()
    {
        EmbodiementPhase.SetActive(false);
        StartCoroutine(WaitAndStartTask());
    }

    private IEnumerator WaitAndStartTask()
    {
        yield return new WaitForSeconds(2f);
        ConversationPhase.SetActive(true);
        conversationManager.StartTask(); 
    }
}


// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class ExperimentManager : MonoBehaviour
// {
//     public GameObject ConversationPhase;
//     public GameObject EmbodiementPhase;
//     public ConversationManager conversationManager;

//     void Start()
//     {
//         StartTraining();
//     }

//     void StartTraining()
//     {
//         EmbodiementPhase.SetActive(true);
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
