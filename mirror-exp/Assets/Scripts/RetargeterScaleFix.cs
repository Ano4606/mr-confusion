using UnityEngine;
using Meta.XR.Movement.Retargeting;

/// <summary>
/// Workaround: CharacterRetargeter.Setup() sets localScale = Vector3.zero when ApplyScale is true,
/// and only restores it once body tracking data is valid. If the avatar starts inactive and is
/// activated mid-session, tracking may take several frames to become valid, leaving the avatar invisible.
///
/// This script restores scale to Vector3.one each frame until the retargeter reports it is valid.
/// Once RetargeterValid is true, the retargeter manages scale itself and this script disables itself.
/// </summary>
[RequireComponent(typeof(CharacterRetargeter))]
public class RetargeterScaleFix : MonoBehaviour
{
    private CharacterRetargeter _retargeter;

    void Awake()
    {
        _retargeter = GetComponent<CharacterRetargeter>();
    }

    void LateUpdate()
    {
        if (_retargeter == null) { enabled = false; return; }

        // Retargeter is valid and managing scale itself — stop interfering.
        if (_retargeter.RetargeterValid)
        {
            enabled = false;
            return;
        }

        // Retargeter zeroed the scale in Setup() but hasn't restored it yet — keep avatar visible.
        if (transform.localScale == Vector3.zero)
            transform.localScale = Vector3.one;
    }
}
