using UnityEngine;
using System.Collections;
using Meta.XR.MRUtilityKit;

public class MirrorLayerSetup : MonoBehaviour
{
    [SerializeField] private GameObject _effectMeshObject;

    private IEnumerator Start()
    {
        Debug.Log("Waiting for MRUK...");

        // Wait until MRUK.Instance exists
        yield return new WaitUntil(() => MRUK.Instance != null);

        Debug.Log("MRUK found!");

        MRUK.Instance.SceneLoadedEvent.AddListener(OnSceneLoaded);

        if (MRUK.Instance.IsInitialized)
        {
            Debug.Log("MRUK already initialized");
            OnSceneLoaded();
        }
    }

    private void OnSceneLoaded()
    {
        Debug.Log("OnSceneLoaded fired!");

        MRUKRoom room = MRUK.Instance.GetCurrentRoom();

        if (room == null)
        {
            Debug.LogError("Room is NULL!");
            return;
        }

        Debug.Log($"Room found: {room.gameObject.name}");

        int mirrorOnly = LayerMask.NameToLayer("MirrorOnly");

        if (mirrorOnly == -1)
        {
            Debug.LogError("Layer 'MirrorOnly' does not exist! Add it in Project Settings > Tags and Layers");
            return;
        }

        SetLayerRecursively(room.gameObject, mirrorOnly);
        Debug.Log("Layers set successfully!");

        if (_effectMeshObject != null)
            SetLayerRecursively(_effectMeshObject, mirrorOnly);
    }

    private void OnDestroy()
    {
        if (MRUK.Instance != null)
            MRUK.Instance.SceneLoadedEvent.RemoveListener(OnSceneLoaded);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}