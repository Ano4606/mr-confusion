using UnityEngine;

public class ChoosingPhaseManager : MonoBehaviour
{
    public GameObject choosingPanel;             
    public AvatarManager avatarManager;           
    public ExperimentManager experimentManager;
    public ConversationManager conversationManager;

    public GameObject EmbodiementPhase;
    private bool hasChosen = false;

    void Start()
    {
   //     ChooseAvatar("participant-white-male");
        choosingPanel.SetActive(true);
        EmbodiementPhase.SetActive(false);
    }
// public void TestSelectWhiteMale()
// {
//     ChooseAvatar("participant-white-male");
// }

    public void ChooseAvatar(string participantName)
    {
        if (hasChosen) return;
        hasChosen = true;

        avatarManager.SelectParticipant(participantName);

        if (avatarManager.ActiveParticipantAvatar != null)
        {
            conversationManager.BindToAvatar(avatarManager.ActiveParticipantAvatar);
        }

        choosingPanel.SetActive(false);
        EmbodiementPhase.SetActive(true);

        experimentManager.BeginExperiment();
    }
}
