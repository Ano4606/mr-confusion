using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using QuickVR.Interaction;

namespace QuickVR.SampleInteraction
{

    public class TestInteractionUI : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        [Header("General Settings")]
        public float _guiDistance = 2.5f;
        public float _guiDistanceTolerance = 0.1f;

        /// <summary>
        /// The gui will update its orientation once the angle formed by the forward of the target avatar and the forward of 
        /// the gui is greater than _guiDeadAngle. 
        /// </summary>
        public float _guiDeadAngle = 45.0f;

        [Header("Locomotion")]
        public Toggle _toggleDirectMove = null;
        public Toggle _toggleControllersMove = null;
        public Toggle _toggleTeleportLeftHand = null;
        public Toggle _toggleTeleportRightHand = null;
        public Toggle _toggleWalkInPlace = null;

        [Header("Interaction")]
        public Toggle _toggleGrabDirectLeftHand = null;
        public Toggle _toggleGrabDirectRightHand = null;
        public Toggle _toggleGrabRayLeftHand = null;
        public Toggle _toggleGrabRayRightHand = null;
        public Toggle _toggleUIRayLeftHand = null;
        public Toggle _toggleUIRayRightHand = null;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected bool _show = true;

        protected CanvasGroup _canvasGroup = null;

        protected Animator _animator
        {
            get
            {
                return QuickSingletonManager._vrManager.GetAnimatorTarget();
            }
        }

        protected Vector3 _targetPosition = Vector3.zero;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Start()
        {
            _canvasGroup = gameObject.GetOrCreateComponent<CanvasGroup>();

            _toggleDirectMove.isOn = QuickXRSettings._locomotionSettings._directMoveEnabled;
            _toggleControllersMove.isOn = QuickXRSettings._locomotionSettings._controllersMoveEnabled;
            _toggleWalkInPlace.isOn = QuickXRSettings._locomotionSettings._walkInPlaceEnabled;
            _toggleUIRayRightHand.isOn = true;
            //_toggleUIRayLeftHand.isOn = true;   
        }

        #endregion

        #region GET AND SET

        protected virtual Vector3 ComputeTargetPosition()
        {
            return _animator.transform.position + _animator.transform.forward * _guiDistance + _animator.transform.up;
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            if (_animator)
            {
                Vector3 targetPos = transform.position;
                Quaternion targetRot = transform.rotation;
                bool openCloseUI = InputManagerVR.GetButtonDown(InputManagerVR.ButtonCodes.LeftPrimaryPress) || InputManagerKeyboard.GetKeyDown(UnityEngine.InputSystem.Key.X);
                Vector3 v = Vector3.ProjectOnPlane(transform.position - _animator.transform.position, _animator.transform.up);
                float d2 = v.sqrMagnitude;
                if
                (
                    openCloseUI ||
                    Mathf.Abs(d2 - (_guiDistance * _guiDistance)) > (_guiDistanceTolerance * _guiDistanceTolerance) ||
                    Vector3.Angle(_animator.transform.forward, v) > _guiDeadAngle
                )
                {
                    targetPos = ComputeTargetPosition();
                    targetRot = _animator.transform.rotation;

                    if (openCloseUI)
                    {
                        _show = !_show;

                        _canvasGroup.alpha = _show ? 1 : 0;
                        _canvasGroup.blocksRaycasts = _show;
                        _canvasGroup.interactable = _show;

                        if (_show)
                        {
                            //By default, enable the ui interactor for the right hand. 
                            _toggleUIRayRightHand.isOn = true;
                        }
                    }
                }

                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime);
            }

            if (_show)
            {
                //Locomotion
                QuickXRSettings._locomotionSettings._directMoveEnabled = _toggleDirectMove.isOn;
                QuickXRSettings._locomotionSettings._controllersMoveEnabled = _toggleControllersMove.isOn;
                QuickXRSettings._locomotionSettings._walkInPlaceEnabled = _toggleWalkInPlace.isOn;
                QuickXRSettings._locomotionSettings._teleportLeftHandEnabled = _toggleTeleportLeftHand.isOn;
                QuickXRSettings._locomotionSettings._teleportRightHandEnabled = _toggleTeleportRightHand.isOn;

                //Left Hand Interaction
                QuickXRSettings._interactionSettings._leftHandInteractors._grabDirectEnabled = _toggleGrabDirectLeftHand.isOn;
                QuickXRSettings._interactionSettings._leftHandInteractors._grabRayEnabled = _toggleGrabRayLeftHand.isOn;
                QuickXRSettings._interactionSettings._leftHandInteractors._uiRayEnabled = _toggleUIRayLeftHand.isOn;

                QuickXRSettings._interactionSettings._rightHandInteractors._grabDirectEnabled = _toggleGrabDirectRightHand.isOn;
                QuickXRSettings._interactionSettings._rightHandInteractors._grabRayEnabled = _toggleGrabRayRightHand.isOn;
                QuickXRSettings._interactionSettings._rightHandInteractors._uiRayEnabled = _toggleUIRayRightHand.isOn;
            }
        }

        #endregion

    }

}


