using System.Collections;
using UnityEngine;

public class DynamicEyeBlinker : MonoBehaviour
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

        public bool IsValid =>
            avatarHead != null &&
            avatarEyelashes != null;
    }

    public AvatarBlinkTarget avatarA = new AvatarBlinkTarget();
    public AvatarBlinkTarget avatarB = new AvatarBlinkTarget();

    [Header("Properties")]
    public Vector2 blinkIntervalRange = new Vector2(3f, 7f);
    public float blinkHold = 0.06f;
    public float blinkOpenSeconds = 0.03f;
    public float blinkCloseSeconds = 0.1f;

    // ★ NEW: Dynamically update both blink targets
    public void SetActiveAvatars(GameObject participant, GameObject interlocutor)
    {
        StopAllBlinking();

        SetupTargetFromAvatar(avatarA, participant);
        SetupTargetFromAvatar(avatarB, interlocutor);

        StartBlinking();
    }

    private void SetupTargetFromAvatar(AvatarBlinkTarget target, GameObject avatar)
    {
        if (avatar == null)
        {
            target.avatarHead = null;
            target.avatarEyelashes = null;
            return;
        }

        // Automatically search for required skinned meshes
        target.avatarHead = avatar.GetComponentInChildren<SkinnedMeshRenderer>(true);
        target.avatarEyelashes = FindEyelashesRenderer(avatar);

        Init(target);
    }

    private SkinnedMeshRenderer FindEyelashesRenderer(GameObject root)
    {
        var skinnedMeshes = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var sm in skinnedMeshes)
        {
            if (sm.sharedMesh.GetBlendShapeIndex("eyeBlinkLeft") >= 0 ||
                sm.sharedMesh.GetBlendShapeIndex("eyeBlinkRight") >= 0)
                return sm;
        }
        return null;
    }

    private void Init(AvatarBlinkTarget avatar)
    {
        if (!avatar.IsValid) return;

        avatar.leftHead = GetBlendshapeIndex(avatar.avatarHead, "eyeBlinkLeft");
        avatar.rightHead = GetBlendshapeIndex(avatar.avatarHead, "eyeBlinkRight");

        avatar.leftLashes = GetBlendshapeIndex(avatar.avatarEyelashes, "eyeBlinkLeft");
        avatar.rightLashes = GetBlendshapeIndex(avatar.avatarEyelashes, "eyeBlinkRight");
    }

    private int GetBlendshapeIndex(SkinnedMeshRenderer mesh, string name)
    {
        if (!mesh) return -1;
        return mesh.sharedMesh.GetBlendShapeIndex(name);
    }

    // ★ Start blinking for both avatars
    private void StartBlinking()
    {
        if (avatarA.IsValid)
            avatarA.blinkRoutine = StartCoroutine(BlinkRoutine(avatarA));

        if (avatarB.IsValid)
            avatarB.blinkRoutine = StartCoroutine(BlinkRoutine(avatarB));
    }

    private void StopAllBlinking()
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
            float wait = Random.Range(blinkIntervalRange.x, blinkIntervalRange.y);
            yield return new WaitForSeconds(wait);

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
        if (!avatar.IsValid) return;

        if (avatar.leftHead >= 0) avatar.avatarHead.SetBlendShapeWeight(avatar.leftHead, weight);
        if (avatar.rightHead >= 0) avatar.avatarHead.SetBlendShapeWeight(avatar.rightHead, weight);

        if (avatar.leftLashes >= 0) avatar.avatarEyelashes.SetBlendShapeWeight(avatar.leftLashes, weight);
        if (avatar.rightLashes >= 0) avatar.avatarEyelashes.SetBlendShapeWeight(avatar.rightLashes, weight);
    }
}
