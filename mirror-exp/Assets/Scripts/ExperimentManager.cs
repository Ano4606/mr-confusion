using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    public GameObject ConversationPhase;
    public GameObject EmbodiementPhase;

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
        ConversationPhase.SetActive(true);
    }
}
