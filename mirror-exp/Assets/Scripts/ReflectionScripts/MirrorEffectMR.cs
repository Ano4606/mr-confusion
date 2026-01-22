using Meta.XR.MRUtilityKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MirrorEffectMR : MonoBehaviour
{
    [SerializeField]
    private EffectMesh effectMesh;
    private bool hasBeenInitialized = false;

    [SerializeField]
    private Material wallMaterial;
    // Start is called before the first frame update
    void Start()
    {
        if(effectMesh == null)
        {
            effectMesh = GetComponent<EffectMesh>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!hasBeenInitialized)
        {
            if (effectMesh.EffectMeshObjects.Count > 0)
            {
                hasBeenInitialized = true;
                StartCoroutine(SetupEffectMeshes());
            }
        }
    }

    private IEnumerator SetupEffectMeshes()
    {
        Debug.Log("Setting up the meshes");
        yield return new WaitForEndOfFrame();
        foreach (var mrkuanchor in effectMesh.EffectMeshObjects.Keys)
        {
            Debug.Log("mrkuanchor.Label = " + mrkuanchor.Label);
            Debug.Log("mrkuanchor.Label = " + mrkuanchor.Label);
            if (mrkuanchor.Label == MRUKAnchor.SceneLabels.WALL_FACE)
            {
                GameObject effectMeshGO = effectMesh.EffectMeshObjects[mrkuanchor].effectMeshGO.gameObject;

                effectMeshGO.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.On;
                effectMeshGO.GetComponent<MeshRenderer>().receiveShadows = true;
                effectMeshGO.GetComponent<MeshRenderer>().material = wallMaterial;
                effectMeshGO.gameObject.layer = LayerMask.NameToLayer("MirrorOnly");
            }

            else
            {
                effectMesh.EffectMeshObjects[mrkuanchor].effectMeshGO.gameObject.SetActive(false);
            }
        }
    }
}
