using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    public GameObject ConversationPhase;
    public GameObject EmbodiementPhase;
    public ConversationManager conversationManager;

    void Start()
    {
        StartTraining();
    }

    void StartTraining()
    {
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
