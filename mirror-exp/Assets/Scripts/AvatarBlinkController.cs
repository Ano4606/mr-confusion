using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarBlinkController : MonoBehaviour
{
    [SerializeField] private List<SkinnedMeshRenderer> skinnedMeshRenderers;
    [SerializeField] private SOBlinkBlendShape blendShape;

    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 0.1f;
    [SerializeField] private float minBlinkInterval = 2f;
    [SerializeField] private float maxBlinkInterval = 5f;

    private List<List<int>> leftIndicesPerRenderer = new();
    private List<List<int>> rightIndicesPerRenderer = new();

    private Coroutine blinkRoutine;

    private void Awake()
    {
        CacheRendererIfNeeded();
        BuildBlendShapeIndexCache();
    }

    private void OnEnable()
    {
        CacheRendererIfNeeded();

        if (leftIndicesPerRenderer.Count == 0)
            BuildBlendShapeIndexCache();

        ResetEyes();

        if (blinkRoutine == null)
            blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private void OnDisable()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        ResetEyes();
    }

    private void CacheRendererIfNeeded()
    {
        for (int i = 0; i < skinnedMeshRenderers.Count; i++)
        {
            if (skinnedMeshRenderers[i] == null && transform.childCount > 0)
                skinnedMeshRenderers[i] = transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
        }
    }

    private void BuildBlendShapeIndexCache()
    {
        leftIndicesPerRenderer.Clear();
        rightIndicesPerRenderer.Clear();

        if (blendShape == null)
        {
            Debug.LogError("BlendShape ScriptableObject is missing!");
            return;
        }

        foreach (var smr in skinnedMeshRenderers)
        {
            List<int> leftIndices = new();
            List<int> rightIndices = new();

            if (smr == null || smr.sharedMesh == null)
            {
                leftIndicesPerRenderer.Add(leftIndices);
                rightIndicesPerRenderer.Add(rightIndices);
                continue;
            }

            // LEFT EYE
            foreach (var name in blendShape.LeftEyeBlendshapeNames)
            {
                int idx = smr.sharedMesh.GetBlendShapeIndex(name);
                if (idx == -1)
                    Debug.LogWarning($"Left '{name}' not found on {smr.name}");
                else
                    leftIndices.Add(idx);
            }

            // RIGHT EYE
            foreach (var name in blendShape.RightEyeBlendshapeNames)
            {
                int idx = smr.sharedMesh.GetBlendShapeIndex(name);
                if (idx == -1)
                    Debug.LogWarning($"Right '{name}' not found on {smr.name}");
                else
                    rightIndices.Add(idx);
            }

            leftIndicesPerRenderer.Add(leftIndices);
            rightIndicesPerRenderer.Add(rightIndices);
        }
    }

    private void ResetEyes()
    {
        ApplyBlinkWeight(0f);
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            float interval = Random.Range(minBlinkInterval, maxBlinkInterval);
            yield return new WaitForSeconds(interval);
            yield return Blink();
        }
    }

    private IEnumerator Blink()
    {
        // CLOSE
        float t = 0f;
        while (t < blinkSpeed)
        {
            t += Time.deltaTime;
            float w = Mathf.Lerp(0f, 100f, t / blinkSpeed);

            ApplyBlinkWeight(w);
            yield return null;
        }

        yield return new WaitForSeconds(0.05f);

        // OPEN
        t = 0f;
        while (t < blinkSpeed)
        {
            t += Time.deltaTime;
            float w = Mathf.Lerp(100f, 0f, t / blinkSpeed);

            ApplyBlinkWeight(w);
            yield return null;
        }
    }

    private void ApplyBlinkWeight(float weight)
    {
        for (int i = 0; i < skinnedMeshRenderers.Count; i++)
        {
            var smr = skinnedMeshRenderers[i];
            if (smr == null) continue;

            // LEFT
            foreach (int idx in leftIndicesPerRenderer[i])
                smr.SetBlendShapeWeight(idx, weight);

            // RIGHT
            foreach (int idx in rightIndicesPerRenderer[i])
                smr.SetBlendShapeWeight(idx, weight);
        }
    }
}