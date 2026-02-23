using System.Collections;
using UnityEngine;

public class EyeBlinkerTwoAvatarsAsync : MonoBehaviour
{
    [System.Serializable]
    public class AvatarBlinkTarget
    {
        public SkinnedMeshRenderer avatarHead;
        public SkinnedMeshRenderer avatarEyelashes;

        [HideInInspector] public int leftHead;
        [HideInInspector] public int rightHead;
        [HideInInspector] public int leftLashes;
        [HideInInspector] public int rightLashes;

        [HideInInspector] public Coroutine blinkRoutine;
    }

    public AvatarBlinkTarget avatarA;
    public AvatarBlinkTarget avatarB;

    [Header("Properties")]
    public Vector2 blinkIntervalRange = new Vector2(3f, 7f);
    public float blinkHold = 0.06f;
    public float blinkOpenSeconds = 0.03f;
    public float blinkCloseSeconds = 0.1f;

    private void Awake()
    {
        Init(avatarA);
        Init(avatarB);
    }

    private void Init(AvatarBlinkTarget avatar)
    {
        avatar.leftHead   = GetBlendshapeIndex(avatar.avatarHead,     "eyeBlinkLeft");
        avatar.rightHead  = GetBlendshapeIndex(avatar.avatarHead,     "eyeBlinkRight");
        avatar.leftLashes = GetBlendshapeIndex(avatar.avatarEyelashes,"eyeBlinkLeft");
        avatar.rightLashes= GetBlendshapeIndex(avatar.avatarEyelashes,"eyeBlinkRight");
    }

    private int GetBlendshapeIndex(SkinnedMeshRenderer mesh, string name)
    {
        if (!mesh) return -1;
        return mesh.sharedMesh.GetBlendShapeIndex(name);
    }

    private void OnEnable()
    {
        avatarA.blinkRoutine = StartCoroutine(BlinkRoutine(avatarA));
        avatarB.blinkRoutine = StartCoroutine(BlinkRoutine(avatarB));
    }

    private void OnDisable()
    {
        StopRoutine(avatarA);
        StopRoutine(avatarB);
    }

    private void StopRoutine(AvatarBlinkTarget avatar)
    {
        if (avatar.blinkRoutine != null)
        {
            StopCoroutine(avatar.blinkRoutine);
            avatar.blinkRoutine = null;
        }
        ApplyBlendshape(avatar, 0);
    }

    private IEnumerator BlinkRoutine(AvatarBlinkTarget avatar)
    {
        while (true)
        {
            // Wait random time
            float wait = Random.Range(blinkIntervalRange.x, blinkIntervalRange.y);
            yield return new WaitForSeconds(wait);

            // Close
            yield return AnimateBlend(avatar, 0, 1, blinkCloseSeconds);
            yield return new WaitForSeconds(blinkHold);
            yield return AnimateBlend(avatar, 1, 0, blinkOpenSeconds);
        }
    }

    private IEnumerator AnimateBlend(AvatarBlinkTarget avatar, float start, float end, float duration)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            float weight = Mathf.Lerp(start, end, t) * 100f;
            ApplyBlendshape(avatar, weight);
            yield return null;
        }
        ApplyBlendshape(avatar, end * 100);
    }

    private void ApplyBlendshape(AvatarBlinkTarget avatar, float weight)
    {
        if (!avatar.avatarHead) return;

        if (avatar.leftHead   >= 0) avatar.avatarHead.SetBlendShapeWeight(avatar.leftHead, weight);
        if (avatar.rightHead  >= 0) avatar.avatarHead.SetBlendShapeWeight(avatar.rightHead, weight);
        
        if (avatar.avatarEyelashes != null)
        {
            if (avatar.leftLashes >= 0) avatar.avatarEyelashes.SetBlendShapeWeight(avatar.leftLashes, weight);
            if (avatar.rightLashes>= 0) avatar.avatarEyelashes.SetBlendShapeWeight(avatar.rightLashes, weight);
        }
    }
}
