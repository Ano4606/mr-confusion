using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarBlinkController : MonoBehaviour
{
    [SerializeField] private List<SkinnedMeshRenderer> skinnedMeshRenderers;
    [SerializeField] private SOBlinkBlendShape blendShape;
    [SerializeField] private float blinkSpeed = 0.1f;
    [SerializeField] private float minBlinkInterval = 2f;
    [SerializeField] private float maxBlinkInterval = 5f;

    private readonly List<int> blendShapeIndices = new();
    private Coroutine blinkRoutine;

    private void Awake()
    {
        CacheRendererIfNeeded();
        BuildBlendShapeIndexCache();
    }

    private void OnEnable()
    {
        // In case Awake ran before the mesh was ready (some setups), ensure cache is valid.
        CacheRendererIfNeeded();
        if (blendShapeIndices.Count == 0) BuildBlendShapeIndexCache();

        ResetEyes();

        if (blendShapeIndices.Count > 0 && blinkRoutine == null)
            blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private void OnDisable()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        // Prevent getting stuck half-blinked
        ResetEyes();
    }

    private void CacheRendererIfNeeded()
    {
        for(int i = 0; i<skinnedMeshRenderers.Count; i++){
 			if (skinnedMeshRenderers[i] == null && transform.childCount > 0)
            	skinnedMeshRenderers[i] = transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
		}
       
    }

    private void BuildBlendShapeIndexCache()
    {
        blendShapeIndices.Clear();

        if (skinnedMeshRenderers == null || blendShape == null)
            return;

        foreach (var name in blendShape.EyesBlendshapeName)
        {
			for(int i = 0; i<skinnedMeshRenderers.Count; i++){
           		int idx = skinnedMeshRenderers[i].sharedMesh.GetBlendShapeIndex(name);
            	if (idx == -1) Debug.LogError($"Blend shape '{name}' not found on {skinnedMeshRenderers[i].name}");
            	else blendShapeIndices.Add(idx);
			}

        }
    }

    private void ResetEyes()
    {
		for(int i = 0; i<skinnedMeshRenderers.Count; i++){
        	if (skinnedMeshRenderers[i] == null) return;
        	foreach (int idx in blendShapeIndices)
            	skinnedMeshRenderers[i].SetBlendShapeWeight(idx, 0f);
		}

    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            float interval = Random.Range(minBlinkInterval, maxBlinkInterval);

            // If you want blinking even when timeScale == 0, use WaitForSecondsRealtime instead.
            yield return new WaitForSeconds(interval);

            yield return Blink();
        }
    }

    private IEnumerator Blink()
    {
		for(int i = 0; i<skinnedMeshRenderers.Count; i++){
       // Close
        float t = 0f;
        while (t < blinkSpeed)
        {
            t += Time.deltaTime;
            float w = Mathf.Lerp(0f, 100f, t / blinkSpeed);
            foreach (int idx in blendShapeIndices) skinnedMeshRenderers[i].SetBlendShapeWeight(idx, w);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        // Open
        t = 0f;
        while (t < blinkSpeed)
        {
            t += Time.deltaTime;
            float w = Mathf.Lerp(100f, 0f, t / blinkSpeed);
            foreach (int idx in blendShapeIndices) skinnedMeshRenderers[i].SetBlendShapeWeight(idx, w);
            yield return null;
        }
		}


    }
}
