using System.Collections;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class AvatarWallPlacer : MonoBehaviour
{
    [SerializeField] private GameObject chooseAvatar;
    [SerializeField] private Vector3 localOffset = Vector3.zero;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => MRUK.Instance != null
                                     && MRUK.Instance.GetCurrentRoom() != null);
        PlaceOnWindow();
    }

    private void PlaceOnWindow()
    {
        var room = MRUK.Instance.GetCurrentRoom();

        MRUKAnchor window = null;
        foreach (var anchor in room.Anchors)
        {
            if (anchor.HasLabel("WINDOW_FRAME"))
            {
                window = anchor;
                break;
            }
        }

        if (window == null)
        {
            Debug.LogWarning("[AvatarWallPlacer] No Window anchor found in room.", this);
            return;
        }

        if (chooseAvatar == null)
        {
            Debug.LogWarning("[AvatarWallPlacer] ChooseAvatar is not assigned.", this);
            return;
        }

        // Parent and place ChooseAvatar at the window anchor
        chooseAvatar.transform.SetParent(window.transform);
        chooseAvatar.transform.localPosition = localOffset;
        chooseAvatar.transform.localRotation = Quaternion.identity;

        Debug.Log("[AvatarWallPlacer] ChooseAvatar placed on Window anchor.");
    }
}