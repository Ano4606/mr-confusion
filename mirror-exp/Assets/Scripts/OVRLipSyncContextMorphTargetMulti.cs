using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[DisallowMultipleComponent]
public class OVRLipSyncContextMorphTargetMulti : MonoBehaviour
{
    [Serializable]
    public class Target
    {
        [Tooltip("Skinned Mesh Renderer target to be driven by Oculus Lipsync")]
        public SkinnedMeshRenderer renderer;

        [Tooltip("If true, the script will scan blendshape names and auto-map visemes on Start/OnValidate.")]
        public bool autoMap = true;

        [Tooltip("If true, writes mapping results to the Console.")]
        public bool logMapping = false;

        [Tooltip("Blendshape index to trigger for each viseme (-1 = unassigned). Length must be OVRLipSync.VisemeCount.")]
        public int[] visemeToBlendShape = CreateDefaultVisemeMap();

        [Tooltip("Blendshape index to trigger for laughter (-1 = unassigned).")]
        public int laughterBlendShape = -1;
    }

    [Header("Targets")]
    public List<Target> targets = new List<Target>();

    [Header("Laughter")]
    [Range(0.0f, 1.0f)]
    public float laughterThreshold = 0.5f;

    [Range(0.0f, 3.0f)]
    public float laughterMultiplier = 1.5f;

    [Header("Smoothing")]
    [Range(1, 100)]
    public int smoothAmount = 70;

    [Header("Optional Viseme Test Keys")]
    public bool enableVisemeTestKeys = false;
    public KeyCode[] visemeTestKeys =
    {
        KeyCode.BackQuote, KeyCode.Tab, KeyCode.Q, KeyCode.W, KeyCode.E,
        KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I,
        KeyCode.O, KeyCode.P, KeyCode.LeftBracket, KeyCode.RightBracket, KeyCode.Backslash,
    };

    public KeyCode laughterKey = KeyCode.CapsLock;

    private OVRLipSyncContextBase lipsyncContext;

    // Canonical OVR viseme names in order (index -> name)
    private static readonly string[] VisemeNames =
    {
        "sil","PP","FF","TH","DD","kk","CH","SS","nn","RR","aa","E","ih","oh","ou"
    };

    private void Start()
    {
        lipsyncContext = GetComponent<OVRLipSyncContextBase>();
        if (lipsyncContext == null)
        {
            Debug.LogError($"{nameof(OVRLipSyncContextMorphTargetMulti)}: No OVRLipSyncContextBase found on this object.");
            enabled = false;
            return;
        }

        lipsyncContext.Smoothing = smoothAmount;

        // Auto-map all targets (if requested)
        for (int t = 0; t < targets.Count; t++)
        {
            if (targets[t] != null && targets[t].autoMap)
                AutoMapTarget(targets[t]);
        }
    }

    private void Update()
    {
        if (lipsyncContext == null) return;

        // Keep smoothing in sync
        if (smoothAmount != lipsyncContext.Smoothing)
            lipsyncContext.Smoothing = smoothAmount;

        var frame = lipsyncContext.GetCurrentPhonemeFrame();
        if (frame == null) return;

        // Drive all renderers
        for (int t = 0; t < targets.Count; t++)
        {
            var target = targets[t];
            if (target?.renderer == null) continue;

            ApplyVisemes(target, frame);
            ApplyLaughter(target, frame);
        }

        // Optional test keys
        if (enableVisemeTestKeys)
            CheckForKeys();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // In-editor auto-map for convenience
        if (targets == null) return;
        for (int t = 0; t < targets.Count; t++)
        {
            if (targets[t] != null && targets[t].autoMap)
                AutoMapTarget(targets[t]);
        }
    }
#endif

    private void ApplyVisemes(Target target, OVRLipSync.Frame frame)
    {
        int[] map = target.visemeToBlendShape;
        if (map == null || map.Length != OVRLipSync.VisemeCount) return;

        for (int i = 0; i < OVRLipSync.VisemeCount; i++)
        {
            int bs = map[i];
            if (bs < 0) continue;

            target.renderer.SetBlendShapeWeight(bs, frame.Visemes[i] * 100f);
        }
    }

    private void ApplyLaughter(Target target, OVRLipSync.Frame frame)
    {
        int bs = target.laughterBlendShape;
        if (bs < 0) return;

        float laughterScore = frame.laughterScore;

        // Threshold then re-map to [0,1]
        laughterScore = laughterScore < laughterThreshold ? 0.0f : laughterScore - laughterThreshold;
        laughterScore = Mathf.Min(laughterScore * laughterMultiplier, 1.0f);
        laughterScore *= 1.0f / Mathf.Max(laughterThreshold, 0.0001f);

        target.renderer.SetBlendShapeWeight(bs, laughterScore * 100f);
    }

    private void CheckForKeys()
    {
        // Visemes
        for (int i = 0; i < OVRLipSync.VisemeCount && i < visemeTestKeys.Length; i++)
        {
            if (Input.GetKeyDown(visemeTestKeys[i])) lipsyncContext.SetVisemeBlend(i, 100);
            if (Input.GetKeyUp(visemeTestKeys[i])) lipsyncContext.SetVisemeBlend(i, 0);
        }

        // Laughter (OVR uses index VisemeCount for laughter in some samples)
        if (Input.GetKeyDown(laughterKey)) lipsyncContext.SetVisemeBlend(OVRLipSync.VisemeCount, 100);
        if (Input.GetKeyUp(laughterKey)) lipsyncContext.SetVisemeBlend(OVRLipSync.VisemeCount, 0);
    }

    private static void AutoMapTarget(Target target)
    {
        if (target.renderer == null) return;

        Mesh mesh = target.renderer.sharedMesh;
        if (mesh == null) return;

        // Ensure map array is correct size
        if (target.visemeToBlendShape == null || target.visemeToBlendShape.Length != OVRLipSync.VisemeCount)
            target.visemeToBlendShape = CreateBlankVisemeMap();

        // Reset to -1 before mapping (so removed blendshapes don’t keep stale indices)
        for (int i = 0; i < target.visemeToBlendShape.Length; i++)
            target.visemeToBlendShape[i] = -1;

        target.laughterBlendShape = -1;

        // Build lookup of normalized blendshape name -> index
        var nameToIndex = new Dictionary<string, int>(mesh.blendShapeCount);
        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            string n = mesh.GetBlendShapeName(i);
            if (string.IsNullOrEmpty(n)) continue;
            nameToIndex[Normalize(n)] = i;
        }

        // Map visemes
        for (int v = 0; v < OVRLipSync.VisemeCount; v++)
        {
            string canonical = VisemeNames[v];

            // Common variants people use in rigs:
            // viseme_PP, VisemePP, v_PP, mouth_PP, etc.
            // We normalize and try a few sensible forms.
            string[] candidates =
            {
                canonical,
                "viseme" + canonical,
                "viseme_" + canonical,
                "v" + canonical,
                "v_" + canonical,
                "mouth" + canonical,
                "mouth_" + canonical,
            };

            int found = -1;
            for (int c = 0; c < candidates.Length && found < 0; c++)
            {
                if (nameToIndex.TryGetValue(Normalize(candidates[c]), out found))
                    break;
            }

            // Special case: some rigs use IH uppercase
            if (found < 0 && canonical == "ih")
            {
                if (nameToIndex.TryGetValue(Normalize("IH"), out found)) { }
                else if (nameToIndex.TryGetValue(Normalize("viseme_IH"), out found)) { }
            }

            target.visemeToBlendShape[v] = found;
        }

        // Map laughter (best-effort)
        // Look for any blendshape containing "laugh" or "laughter"
        int laughIndex = -1;
        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            string n = mesh.GetBlendShapeName(i);
            if (string.IsNullOrEmpty(n)) continue;
            string nn = n.ToLowerInvariant();
            if (nn.Contains("laughter") || nn.Contains("laugh"))
            {
                laughIndex = i;
                break;
            }
        }
        target.laughterBlendShape = laughIndex;

        if (target.logMapping)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[{nameof(OVRLipSyncContextMorphTargetMulti)}] Auto-map for {target.renderer.name}:");
            for (int v = 0; v < OVRLipSync.VisemeCount; v++)
                sb.AppendLine($"  {v,2} {VisemeNames[v],3} -> {target.visemeToBlendShape[v]}");
            sb.AppendLine($"  laughter -> {target.laughterBlendShape}");
            Debug.Log(sb.ToString(), target.renderer);
        }
    }

    private static string Normalize(string s)
    {
        // Lowercase and keep only letters/digits so:
        // "viseme_PP", "Viseme PP", "VISeme-pp" all match.
        if (string.IsNullOrEmpty(s)) return string.Empty;

        var sb = new StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            char ch = char.ToLowerInvariant(s[i]);
            if (char.IsLetterOrDigit(ch))
                sb.Append(ch);
        }
        return sb.ToString();
    }

    private static int[] CreateBlankVisemeMap()
    {
        var arr = new int[OVRLipSync.VisemeCount];
        for (int i = 0; i < arr.Length; i++) arr[i] = -1;
        return arr;
    }

    private static int[] CreateDefaultVisemeMap()
    {
        // Default matches the original single-renderer script style (0..VisemeCount-1),
        // but your mesh likely needs name-based mapping, so this gets overwritten by AutoMap.
        var arr = new int[OVRLipSync.VisemeCount];
        for (int i = 0; i < arr.Length; i++) arr[i] = i;
        return arr;
    }
}
