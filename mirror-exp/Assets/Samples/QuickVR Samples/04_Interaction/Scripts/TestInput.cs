using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace QuickVR.SampleInteraction
{

    public class TestInput : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void LateUpdate()
        {
            InputManager.GetButton("Action1");
            InputManager.GetButtonDown("Action1");
            InputManager.GetButtonUp("Action1");

            //Debug.Log(InputManagerVR.GetButton(InputManagerVR.ButtonCodes.RightTriggerPress));
            //Debug.Log(InputManagerHands.GetButton(InputManagerHands.ButtonCode.RightIndexPinch));
            if (InputManagerHands.GetButtonDown(InputManagerHands.ButtonCode.LeftIndexPinch))
            {
                Debug.Log("LeftIndex PINCH!!!");
            }

            if (InputManagerHands.GetButtonDown(InputManagerHands.ButtonCode.RightIndexPinch))
            {
                Debug.Log("RightIndex PINCH!!!");
            }

            //Debug.Log(InputManagerHands.GetButton(InputManagerHands.ButtonCode.LeftIndexPinch));
        }
    }

}


