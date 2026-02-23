using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicEyeBlinker : MonoBehaviour
{
    [System.Serializable]
    public class AvatarBlinkTarget
    {
        public SkinnedMeshRenderer avatarHead;
        public SkinnedMeshRenderer avatarEyelashes;

        // Eye bones for rotation (required by MetaPerson)
        public Transform leftEyeBone;
        public Transform rightEyeBone;

        [HideInInspector] public int leftHead, rightHead;
        [HideInInspector] public int leftLashes, rightLashes;
        [HideInInspector] public List<int> additionalBlendShapeIndices = new List<int>();
        [HideInInspector] public Coroutine blinkRoutine;

        // Eye movement blendshape indices (MetaPerson naming)
        [HideInInspector] public int idx_upL, idx_upR;
        [HideInInspector] public int idx_downL, idx_downR;
        [HideInInspector] public int idx_outL, idx_outR;   // outer corner (used for horizontal)
        [HideInInspector] public int idx_inL, idx_inR;     // inner corner (used for horizontal)

        // Same indices for eyelashes mesh
        [HideInInspector] public int lash_upL, lash_upR;
        [HideInInspector] public int lash_downL, lash_downR;
        [HideInInspector] public int lash_outL, lash_outR;
        [HideInInspector] public int lash_inL, lash_inR;

        // Eye movement runtime state
        [HideInInspector] public Vector2 currentGaze;
        [HideInInspector] public Vector2 targetGaze;
        [HideInInspector] public Vector2 microOffset;
        [HideInInspector] public float nextSaccadeTime;
        [HideInInspector] public bool isBlinking;

        public bool IsValid => avatarHead != null && avatarEyelashes != null;
    }

    public AvatarBlinkTarget avatarA = new AvatarBlinkTarget();
    public AvatarBlinkTarget avatarB = new AvatarBlinkTarget();

    // ─────────────────────────────────────────
    [Header("Blink Properties")]
    public Vector2 blinkIntervalRange = new Vector2(3f, 7f);
    public float blinkHold            = 0.06f;
    public float blinkOpenSeconds     = 0.03f;
    public float blinkCloseSeconds    = 0.1f;

    [Header("Blend Shape Names — Blink")]
    public string[] additionalBlendShapeNames = new string[] { };

    // ─────────────────────────────────────────
    [Header("Eye Movement — Saccade")]
    public float saccadeInterval         = 2.5f;
    public float saccadeIntervalVariance = 1.5f;
    public float saccadeSpeed            = 12f;
    public float maxHorizontal           = 8f;    // degrees (reduced from 15)
    public float maxVertical             = 6f;    // degrees (reduced from 10)

    [Header("Eye Movement — Microsaccade")]
    public float microSaccadeStrength = 0.8f;     // reduced from 1.5
    public float microSaccadeSpeed    = 30f;

    [Header("Eye Movement — Look-At (optional)")]
    [Tooltip("Assign the VR camera transform to make avatars track the player's gaze")]
    public Transform lookAtTarget;

    // Max bone rotation angles per MetaPerson docs
    private const float MAX_VERT_ANGLE  = 23f;
    private const float MAX_HORIZ_ANGLE_OUT = 45f;  // outer eye at extreme
    private const float MAX_HORIZ_ANGLE_IN  = 25f;  // inner eye at extreme

    // ─────────────────────────────────────────

    public void SetActiveAvatars(GameObject participant, GameObject interlocutor)
    {
        StopAllBlinking();
        SetupTargetFromAvatar(avatarA, participant);
        SetupTargetFromAvatar(avatarB, interlocutor);
        StartBlinking();
    }

    private void Start()
    {
        if (avatarA.IsValid) { Init(avatarA); avatarA.blinkRoutine = StartCoroutine(BlinkRoutine(avatarA)); ScheduleNextSaccade(avatarA); }
        if (avatarB.IsValid) { Init(avatarB); avatarB.blinkRoutine = StartCoroutine(BlinkRoutine(avatarB)); ScheduleNextSaccade(avatarB); }
    }

    private void Update()
    {
        UpdateEyeMovement(avatarA);
        UpdateEyeMovement(avatarB);
    }

    // ─────────────────────────────────────────
    // SETUP
    // ─────────────────────────────────────────

    private void SetupTargetFromAvatar(AvatarBlinkTarget target, GameObject avatar)
    {
        if (avatar == null) { target.avatarHead = null; target.avatarEyelashes = null; return; }

        // Find all skinned meshes with blink blend shapes
        var allMeshes = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        SkinnedMeshRenderer headMesh = null;
        SkinnedMeshRenderer lashesMesh = null;

        foreach (var sm in allMeshes)
        {
            if (sm.sharedMesh == null) continue;

            bool hasBlinkShapes = sm.sharedMesh.GetBlendShapeIndex("eyeBlinkLeft") >= 0 ||
                                  sm.sharedMesh.GetBlendShapeIndex("eyeBlinkRight") >= 0;

            if (hasBlinkShapes)
            {
                string nameLower = sm.name.ToLower();
                // Check if this is the eyelashes mesh
                if (nameLower.Contains("lash") || nameLower.Contains("eyelash"))
                {
                    lashesMesh = sm;
                }
                else
                {
                    // This is likely the head/face mesh
                    headMesh = sm;
                }
            }
        }

        // Assign meshes (fallback to same mesh if only one found)
        target.avatarHead = headMesh ?? lashesMesh;
        target.avatarEyelashes = lashesMesh ?? headMesh;

        if (target.avatarHead != null)
            Debug.Log($"Avatar head mesh: {target.avatarHead.name}");
        if (target.avatarEyelashes != null)
            Debug.Log($"Avatar eyelashes mesh: {target.avatarEyelashes.name}");

        // Auto-find eye bones by name
        target.leftEyeBone  = FindBone(avatar, "LeftEye");
        target.rightEyeBone = FindBone(avatar, "RightEye");

        Init(target);
        ScheduleNextSaccade(target);
    }

    private Transform FindBone(GameObject root, string boneName)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == boneName) return t;
        return null;
    }

    private void Init(AvatarBlinkTarget a)
    {
        if (!a.IsValid) return;

        // Blink
        a.leftHead    = GetBS(a.avatarHead,      "eyeBlinkLeft");
        a.rightHead   = GetBS(a.avatarHead,      "eyeBlinkRight");
        a.leftLashes  = GetBS(a.avatarEyelashes, "eyeBlinkLeft");
        a.rightLashes = GetBS(a.avatarEyelashes, "eyeBlinkRight");

        a.additionalBlendShapeIndices.Clear();
        foreach (string name in additionalBlendShapeNames)
        {
            int idx = GetBS(a.avatarHead, name);
            if (idx >= 0) a.additionalBlendShapeIndices.Add(idx);
            else Debug.LogWarning($"Blend shape '{name}' not found on {a.avatarHead.name}");
        }

        // Eye movement — AvatarHead (official MetaPerson blendshape names)
        a.idx_upL   = GetBS(a.avatarHead, "eyeLookUpLeft");
        a.idx_upR   = GetBS(a.avatarHead, "eyeLookUpRight");
        a.idx_downL = GetBS(a.avatarHead, "eyeLookDownLeft");
        a.idx_downR = GetBS(a.avatarHead, "eyeLookDownRight");
        a.idx_outL  = GetBS(a.avatarHead, "eyeLookOutLeft");   // left eye looks left
        a.idx_outR  = GetBS(a.avatarHead, "eyeLookOutRight");  // right eye looks right
        a.idx_inL   = GetBS(a.avatarHead, "eyeLookInLeft");    // left eye looks right
        a.idx_inR   = GetBS(a.avatarHead, "eyeLookInRight");   // right eye looks left

        // Eye movement — AvatarEyelashes (same blendshapes must be set on lashes too)
        a.lash_upL   = GetBS(a.avatarEyelashes, "eyeLookUpLeft");
        a.lash_upR   = GetBS(a.avatarEyelashes, "eyeLookUpRight");
        a.lash_downL = GetBS(a.avatarEyelashes, "eyeLookDownLeft");
        a.lash_downR = GetBS(a.avatarEyelashes, "eyeLookDownRight");
        a.lash_outL  = GetBS(a.avatarEyelashes, "eyeLookOutLeft");
        a.lash_outR  = GetBS(a.avatarEyelashes, "eyeLookOutRight");
        a.lash_inL   = GetBS(a.avatarEyelashes, "eyeLookInLeft");
        a.lash_inR   = GetBS(a.avatarEyelashes, "eyeLookInRight");
    }

    private int GetBS(SkinnedMeshRenderer mesh, string name)
    {
        if (!mesh) return -1;
        return mesh.sharedMesh.GetBlendShapeIndex(name);
    }

    // ─────────────────────────────────────────
    // BLINK
    // ─────────────────────────────────────────

    private void StartBlinking()
    {
        if (avatarA.IsValid) avatarA.blinkRoutine = StartCoroutine(BlinkRoutine(avatarA));
        if (avatarB.IsValid) avatarB.blinkRoutine = StartCoroutine(BlinkRoutine(avatarB));
    }

    private void StopAllBlinking()
    {
        StopRoutine(avatarA);
        StopRoutine(avatarB);
    }

    private void StopRoutine(AvatarBlinkTarget avatar)
    {
        if (avatar.blinkRoutine != null) { StopCoroutine(avatar.blinkRoutine); avatar.blinkRoutine = null; }
        ApplyBlink(avatar, 0);
    }

    private IEnumerator BlinkRoutine(AvatarBlinkTarget avatar)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(blinkIntervalRange.x, blinkIntervalRange.y));
            avatar.isBlinking = true;
            yield return AnimateBlend(avatar, 0, 1, blinkCloseSeconds);
            yield return new WaitForSeconds(blinkHold);
            yield return AnimateBlend(avatar, 1, 0, blinkOpenSeconds);
            avatar.isBlinking = false;
        }
    }

    private IEnumerator AnimateBlend(AvatarBlinkTarget avatar, float start, float end, float duration)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            ApplyBlink(avatar, Mathf.Lerp(start, end, t) * 100f);
            yield return null;
        }
        ApplyBlink(avatar, end * 100f);
    }

    private void ApplyBlink(AvatarBlinkTarget avatar, float weight)
    {
        if (!avatar.IsValid) return;
        if (avatar.leftHead  >= 0) avatar.avatarHead.SetBlendShapeWeight(avatar.leftHead,  weight);
        if (avatar.rightHead >= 0) avatar.avatarHead.SetBlendShapeWeight(avatar.rightHead, weight);
        if (avatar.leftLashes  >= 0) avatar.avatarEyelashes.SetBlendShapeWeight(avatar.leftLashes,  weight);
        if (avatar.rightLashes >= 0) avatar.avatarEyelashes.SetBlendShapeWeight(avatar.rightLashes, weight);
        foreach (int idx in avatar.additionalBlendShapeIndices)
            avatar.avatarHead.SetBlendShapeWeight(idx, weight);
    }

    // ─────────────────────────────────────────
    // EYE MOVEMENT
    // ─────────────────────────────────────────

    private void UpdateEyeMovement(AvatarBlinkTarget avatar)
    {
        if (!avatar.IsValid) return;
        HandleSaccade(avatar);
        HandleMicroSaccade(avatar);
        ApplyGaze(avatar);
    }

    private void HandleSaccade(AvatarBlinkTarget avatar)
    {
        if (lookAtTarget != null)
        {
            Vector3 dir   = lookAtTarget.position - avatar.avatarHead.transform.position;
            Vector3 local = avatar.avatarHead.transform.InverseTransformDirection(dir.normalized);
            avatar.targetGaze = new Vector2(
                Mathf.Clamp(local.x * Mathf.Rad2Deg * 2f, -maxHorizontal, maxHorizontal),
                Mathf.Clamp(local.y * Mathf.Rad2Deg * 2f, -maxVertical,   maxVertical)
            );
        }
        else if (Time.time >= avatar.nextSaccadeTime)
        {
            // Center-biased random target
            float h = (Random.Range(-maxHorizontal, maxHorizontal) + Random.Range(-maxHorizontal, maxHorizontal)) * 0.5f;
            float v = (Random.Range(-maxVertical,   maxVertical)   + Random.Range(-maxVertical,   maxVertical))   * 0.5f;
            avatar.targetGaze = new Vector2(h, v);
            ScheduleNextSaccade(avatar);
        }

        float speed = avatar.isBlinking ? saccadeSpeed * 0.2f : saccadeSpeed;
        avatar.currentGaze = Vector2.Lerp(avatar.currentGaze, avatar.targetGaze, Time.deltaTime * speed);
    }

    private void HandleMicroSaccade(AvatarBlinkTarget avatar)
    {
        if (avatar.isBlinking)
        {
            avatar.microOffset = Vector2.Lerp(avatar.microOffset, Vector2.zero, Time.deltaTime * microSaccadeSpeed);
            return;
        }
        float seed = avatar == avatarA ? 0f : 100f;
        Vector2 microTarget = new Vector2(
            (Mathf.PerlinNoise(Time.time * 0.8f + seed, 0f) * 2f - 1f) * microSaccadeStrength,
            (Mathf.PerlinNoise(0f, Time.time * 0.8f + seed) * 2f - 1f) * microSaccadeStrength
        );
        avatar.microOffset = Vector2.Lerp(avatar.microOffset, microTarget, Time.deltaTime * microSaccadeSpeed);
    }

    private void ApplyGaze(AvatarBlinkTarget avatar)
    {
        Vector2 gaze = avatar.currentGaze + avatar.microOffset;

        // ── Blendshape weights (0-100) ──────────────────────────────────────
        float upVal   = Mathf.Clamp( gaze.y / maxVertical   * 100f, 0f, 100f);
        float downVal = Mathf.Clamp(-gaze.y / maxVertical   * 100f, 0f, 100f);

        // Horizontal: MetaPerson uses Out/In per eye, NOT simple left/right
        // gaze.x > 0 = looking right: OutRight + InLeft active
        // gaze.x < 0 = looking left:  OutLeft  + InRight active
        float lookRightVal = Mathf.Clamp( gaze.x / maxHorizontal * 100f, 0f, 100f);
        float lookLeftVal  = Mathf.Clamp(-gaze.x / maxHorizontal * 100f, 0f, 100f);

        // Head blendshapes
        SetBS(avatar.avatarHead, avatar.idx_upL,   upVal);
        SetBS(avatar.avatarHead, avatar.idx_upR,   upVal);
        SetBS(avatar.avatarHead, avatar.idx_downL, downVal);
        SetBS(avatar.avatarHead, avatar.idx_downR, downVal);
        SetBS(avatar.avatarHead, avatar.idx_outL,  lookLeftVal);   // left eye outer = looking left
        SetBS(avatar.avatarHead, avatar.idx_outR,  lookRightVal);  // right eye outer = looking right
        SetBS(avatar.avatarHead, avatar.idx_inL,   lookRightVal);  // left eye inner = looking right
        SetBS(avatar.avatarHead, avatar.idx_inR,   lookLeftVal);   // right eye inner = looking left

        // Eyelashes blendshapes (mirror exactly)
        SetBS(avatar.avatarEyelashes, avatar.lash_upL,   upVal);
        SetBS(avatar.avatarEyelashes, avatar.lash_upR,   upVal);
        SetBS(avatar.avatarEyelashes, avatar.lash_downL, downVal);
        SetBS(avatar.avatarEyelashes, avatar.lash_downR, downVal);
        SetBS(avatar.avatarEyelashes, avatar.lash_outL,  lookLeftVal);
        SetBS(avatar.avatarEyelashes, avatar.lash_outR,  lookRightVal);
        SetBS(avatar.avatarEyelashes, avatar.lash_inL,   lookRightVal);
        SetBS(avatar.avatarEyelashes, avatar.lash_inR,   lookLeftVal);

        // ── Eye bone rotation ───────────────────────────────────────────────
        ApplyEyeBoneRotation(avatar.leftEyeBone,  avatar.rightEyeBone, gaze);
    }

    private void ApplyEyeBoneRotation(Transform leftEye, Transform rightEye, Vector2 gaze)
    {
        if (leftEye == null && rightEye == null) return;

        float t = Mathf.Abs(gaze.x) / maxHorizontal;

        // Vertical angle: symmetric, ±23° max
        float xAngle = -(gaze.y / maxVertical) * MAX_VERT_ANGLE;

        // Horizontal: asymmetric per eye (Out = 45°, In = 25°)
        float leftYAngle, rightYAngle;
        if (gaze.x < 0) // looking left
        {
            leftYAngle  = -Mathf.Lerp(0, MAX_HORIZ_ANGLE_OUT, t);  // left eye outer
            rightYAngle = -Mathf.Lerp(0, MAX_HORIZ_ANGLE_IN,  t);  // right eye inner
        }
        else             // looking right
        {
            leftYAngle  =  Mathf.Lerp(0, MAX_HORIZ_ANGLE_IN,  t);  // left eye inner
            rightYAngle =  Mathf.Lerp(0, MAX_HORIZ_ANGLE_OUT, t);  // right eye outer
        }

        if (leftEye  != null) leftEye.localRotation  = Quaternion.Euler(xAngle, leftYAngle,  0f);
        if (rightEye != null) rightEye.localRotation = Quaternion.Euler(xAngle, rightYAngle, 0f);
    }

    private void SetBS(SkinnedMeshRenderer mesh, int index, float value)
    {
        if (index >= 0) mesh.SetBlendShapeWeight(index, value);
    }

    private void ScheduleNextSaccade(AvatarBlinkTarget target)
    {
        target.nextSaccadeTime = Time.time + saccadeInterval + Random.Range(-saccadeIntervalVariance, saccadeIntervalVariance);
    }
}