using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR.SampleInteraction
{

    public class SampleInteractionStage : QuickStageBase
    {

        #region PROTECTED ATTRIBUTES

        protected Canvas _gui = null;
        protected QuickKeyboard _keyboard = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _gui = GetComponentInChildren<Canvas>();

            if (_gui)
            {
                _gui.gameObject.SetActive(false);
            }

            _keyboard = QuickSingletonManager.GetInstance<QuickKeyboard>();
            _keyboard.Enable(true);
            _keyboard.OnSubmit += _keyboard_OnSubmit;
        }

        private void _keyboard_OnSubmit(string text)
        {
            Debug.Log(text);
        }

        public override void Init()
        {
            if (_gui)
            {
                _gui.gameObject.SetActive(true);
            }

            base.Init();
        }

        #endregion

    }

}


