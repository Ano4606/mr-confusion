using System.Collections;
using UnityEngine;

public class MoveToReferenceAfterDelay : MonoBehaviour
{
    [SerializeField] private Transform referencePosition;
    [SerializeField] private float delay = 1f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);

        if (referencePosition != null)
        {
            transform.position = new Vector3(referencePosition.position.x, 0.0f, referencePosition.position.z);
            //transform.rotation = referencePosition.rotation;
        }
        else
        {
            Debug.LogWarning("No reference position assigned.", this);
        }
    }
}