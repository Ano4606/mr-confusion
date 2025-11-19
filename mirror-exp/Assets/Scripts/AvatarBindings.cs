using UnityEngine;
using Meta.XR.Movement.Retargeting;

public class AvatarBindings : MonoBehaviour
{
    public Animator animator;              
    public CharacterRetargeter retargeter; 
    public OVRLipSyncContext lipSync;      
    public AudioSource voiceSource;        

    void Awake()
{
    if (animator == null)
        animator = GetComponentInChildren<Animator>(true);

    if (retargeter == null)
        retargeter = GetComponentInChildren<CharacterRetargeter>(true);

    if (lipSync == null)
        lipSync = GetComponentInChildren<OVRLipSyncContext>(true);

    if (voiceSource == null)
        voiceSource = GetComponentInChildren<AudioSource>(true);
}

}

