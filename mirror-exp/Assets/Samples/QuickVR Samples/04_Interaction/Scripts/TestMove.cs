using QuickVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMove : MonoBehaviour
{

    public bool _move = false;
    public float _moveSpeed = 2;

    // Start is called before the first frame update
    protected virtual void OnEnable()
    {
        //QuickVRManager.OnPostCameraUpdate += UpdateMove;
    }

    // Update is called once per frame

    private void Update()
    {
        UpdateMove();    
    }

    void UpdateMove()
    {
        if (_move)
        {
            //transform.Translate(Vector3.forward * _moveSpeed * Time.deltaTime, Space.Self);
            transform.Rotate(Vector3.up, 10 * Time.deltaTime, Space.Self);
        }
    }
}
