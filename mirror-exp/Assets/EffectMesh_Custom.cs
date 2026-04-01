using Meta.XR.MRUtilityKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorMaterialRemapper : MonoBehaviour
{
    [SerializeField] private EffectMesh effectMesh;

    [Tooltip("Seconds to wait after start before remapping, to let EffectMesh finish building.")]
    [SerializeField] private float delay = 2f;

    [SerializeField] private List<LabelMaterialPair> materialMappings = new();

    [System.Serializable]
    public class LabelMaterialPair
    {
        public MRUKAnchor.SceneLabels label;
        public Material material;
    }

    void Start()
    {
        if (effectMesh == null)
            effectMesh = GetComponent<EffectMesh>();

        StartCoroutine(RemapAfterDelay());
    }

    private IEnumerator RemapAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        RemapMaterials();
    }

    public void RemapMaterials()
    {
        // The key IS the MRUKAnchor — no need to search for it on the GameObject
        foreach (var kvp in effectMesh.EffectMeshObjects)
        {
            MRUKAnchor anchor       = kvp.Key;
            EffectMesh.EffectMeshObject emo = kvp.Value;

            if (emo.effectMeshGO == null) continue;

            var renderer = emo.effectMeshGO.GetComponent<MeshRenderer>();
            if (renderer == null) continue;

            // Walk the mapping list and apply the first label match
            foreach (var pair in materialMappings)
            {
                if (anchor.HasAnyLabel(pair.label))
                {
                    renderer.material = pair.material;
                    break; // remove break if you want last match to win instead
                }
            }
        }
    }
}