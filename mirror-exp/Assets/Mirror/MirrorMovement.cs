using UnityEngine;

/// <summary>
/// Mirrors the player head across a reflective transform for realistic mirror effect.
/// </summary>
public class MirrorMovement : MonoBehaviour
{
    [Header("Mirror Settings")]
    [Tooltip("Transform representing the mirror surface")]
    [SerializeField] private Transform mirror;

    public Transform playerHead;


    void Update()
    {
        // Compute local position relative to mirror
        Vector3 localPos = mirror.InverseTransformPoint(playerHead.position);

        // Reflect position across mirror (invert Z)
        Vector3 reflectedPos = new Vector3(localPos.x, localPos.y, -localPos.z);
        transform.position = mirror.TransformPoint(reflectedPos);

        // Compute look-at point rather than direct look to avoid inversion issues
        Vector3 localLook = new Vector3(-localPos.x, localPos.y, localPos.z);
        Vector3 worldLook = mirror.TransformPoint(localLook);
        transform.LookAt(worldLook);
    }
}
