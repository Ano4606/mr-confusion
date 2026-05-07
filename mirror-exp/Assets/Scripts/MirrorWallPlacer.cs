using System.Collections;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class MirrorWallPlacer : MonoBehaviour
{
    [SerializeField] private float mirrorOffset = 0.01f;

    private IEnumerator Start()
    {
        // Wait until MRUK room is loaded
        yield return new WaitUntil(() => MRUK.Instance != null 
                                     && MRUK.Instance.GetCurrentRoom() != null);

        PlaceOnWallArt();
    }

    private void PlaceOnWallArt()
    {
        var room = MRUK.Instance.GetCurrentRoom();

        // Find Wall Art anchor
        MRUKAnchor wallArt = null;
        foreach (var anchor in room.Anchors)
        {
            if (anchor.HasLabel("WALL_ART"))
            {
                wallArt = anchor;
                break;
            }
        }

        if (wallArt == null)
        {
            Debug.LogWarning("[MirrorPlacer] No Wall Art anchor found in room.", this);
            return;
        }

        // // Attach mirror as child of the Wall Art anchor
        // transform.SetParent(wallArt.transform);

        // // Reset local position/rotation then nudge off the wall
        // transform.localPosition = new Vector3(0, 0, -mirrorOffset);
        // transform.localRotation = Quaternion.identity;

        // Debug.Log("[MirrorPlacer] Mirror attached to Wall Art anchor.");

        transform.SetParent(wallArt.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}