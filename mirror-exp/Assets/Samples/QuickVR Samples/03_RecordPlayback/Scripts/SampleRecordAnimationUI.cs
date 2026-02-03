using System.Collections;

using UnityEngine;
using TMPro;

namespace QuickVR.Samples.RecordAnimation
{

    public class SampleRecordAnimationUI : QuickMonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public string _savedAnimName = "test";

        [Header("General Settings")]
        public float _guiDistance = 2.5f;

        /// <summary>
        /// The gui will update its orientation once the angle formed by the forward of the target avatar and the forward of 
        /// the gui is greater than _guiDeadAngle. 
        /// </summary>
        public float _guiDeadAngle = 45.0f;

        [Header("Animation UI")]

        public TMP_Text _info = null;
        public float _infoLifeTime = 3;

        public QuickUIButton _buttonRecordStop = null;
        public QuickUIButton _buttonPlayback = null;
        public QuickUIButton _buttonExportToAnim = null;
        public QuickUIButton _buttonExportToJSON = null;
        public QuickUIButton _buttonLoadFromJSON = null;

        //[Space]
        //The QuickAnimationPlayer component of the Avatar we want to record/playback. 
        public QuickAnimationPlayer _animationPlayerSrc
        {
            get
            {
                return QuickSingletonManager._vrManager.GetAnimatorSource().GetOrCreateComponent<QuickAnimationPlayer>();
            }
            
        }
        public QuickAnimationPlayer _animationPlayerDst = null;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Coroutine _coUpdateShowInfo = null;

        protected bool _show = true;

        protected CanvasGroup _canvasGroup = null;

        protected Coroutine _coUpdatePosition = null;

        protected Animator _animator
        {
            get
            {
                if (!m_Animator)
                {
                    m_Animator = QuickSingletonManager._vrManager.GetAnimatorSource();
                }

                return m_Animator;
            }
        }

        protected Animator m_Animator = null;
        protected Coroutine _coUpdateTargetFwd = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            QuickXRSettings._interactionSettings._rightHandInteractors._uiRayEnabled = true;

            _buttonRecordStop.OnDown += ButtonRecordStop_Down;
            _buttonPlayback.OnDown += ButtonPlayback_Down;
            _buttonExportToAnim.OnDown += ButtonExportToAnim_Down;
            _buttonExportToJSON.OnDown += ButtonExportToJSON_Down;
            _buttonLoadFromJSON.OnDown += ButtonLoadFromJSON_Down;
        }

        protected virtual void OnDestroy()
        {
            if (_buttonRecordStop)
            {
                _buttonRecordStop.OnDown -= ButtonRecordStop_Down;
            }
            
            if (_buttonPlayback)
            {
                _buttonPlayback.OnDown -= ButtonPlayback_Down;
            }

            if (_buttonExportToAnim)
            {
                _buttonExportToAnim.OnDown += ButtonExportToAnim_Down;
            }

            if (_buttonExportToJSON)
            {
                _buttonExportToJSON.OnDown -= ButtonExportToJSON_Down;
            }

            if (_buttonLoadFromJSON)
            {
                _buttonLoadFromJSON.OnDown -= ButtonLoadFromJSON_Down;
            }
        }

        protected virtual void OnEnable()
        {
            _coUpdatePosition = StartCoroutine(CoUpdatePosition());
        }

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
            _coUpdatePosition = null;
            _coUpdateTargetFwd = null;
        }

        protected virtual void Start()
        {
            _canvasGroup = gameObject.GetOrCreateComponent<CanvasGroup>();
        }

        [ButtonMethod]
        public void ButtonRecordStop_Down()
        {
            if (_animationPlayerSrc.IsRecording())
            {
                _animationPlayerSrc.StopRecording();
            }
            else
            {
                _animationPlayerSrc.Record();
            }

            UpdateStateButtonRecordStop();
        }

        private void ButtonPlayback_Down()
        {
            _animationPlayerDst.Play(_animationPlayerSrc.GetRecordedAnimation());
        }

        [ButtonMethod]
        public virtual void ButtonExportToAnim_Down()
        {
            ShowInfo($"Saving animation to {_savedAnimName}.anim");
            QuickAnimationUtils.SaveToAnim($"{_savedAnimName}.anim", _animationPlayerSrc.GetRecordedAnimation());
            ShowInfo($"Animation saved to {_savedAnimName}.anim");
        }

        protected virtual void ButtonExportToJSON_Down()
        {
            ShowInfo($"Saving animation to {_savedAnimName}.json");
            QuickAnimationUtils.SaveToJson($"{_savedAnimName}.json", _animationPlayerSrc.GetRecordedAnimation());
            ShowInfo($"Animation saved to {_savedAnimName}.json");
        }

        protected virtual void ButtonLoadFromJSON_Down()
        {
            ShowInfo($"Loading animation from {_savedAnimName}.json");
            QuickAnimation animation = QuickAnimationUtils.FromJson(System.IO.File.ReadAllText($"{_savedAnimName}.json"), _animationPlayerDst.GetComponent<Animator>());
            _animationPlayerDst.Play(animation);
        }

        protected virtual void ShowInfo(string text)
        {
            if (_info  != null)
            {
                if (_coUpdateShowInfo != null)
                {
                    StopCoroutine(_coUpdateShowInfo);
                }

                _coUpdateShowInfo = StartCoroutine(CoUpdateShowInfo());

                _info.text = text;

            }
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            if (InputManagerVR.GetButtonDown(InputManagerVR.ButtonCodes.LeftPrimaryPress) || InputManagerKeyboard.GetKeyDown(UnityEngine.InputSystem.Key.X))
            {
                _show = !_show;

                _canvasGroup.alpha = _show ? 1 : 0;
                _canvasGroup.blocksRaycasts = _show;
                _canvasGroup.interactable = _show;

                QuickXRSettings._interactionSettings._rightHandInteractors._uiRayEnabled = _show;
            }
        }

        protected virtual IEnumerator CoUpdateShowInfo()
        {
            yield return new WaitForSeconds(_infoLifeTime);

            if (_info != null)
            {
                _info.text = string.Empty;
            }
        }

        protected virtual IEnumerator CoUpdatePosition()
        {
            while (true)
            {
                if (_animator)
                {
                    if ((Vector3.Angle(transform.forward, _animator.transform.forward) > 45) && _coUpdateTargetFwd == null)
                    {
                        _coUpdateTargetFwd = StartCoroutine(CoUpdateTargetForward());
                    }

                    Vector3 offset = transform.forward * _guiDistance + _animator.transform.up;
                    transform.position = _animator.transform.position + offset;
                }

                yield return null;
            }
        }

        protected virtual IEnumerator CoUpdateTargetForward()
        {
            while (Vector3.Angle(transform.forward, _animator.transform.forward) > 1)
            {
                transform.forward = Vector3.Lerp(transform.forward, _animator.transform.forward, Time.deltaTime);

                yield return null;
            }

            _coUpdateTargetFwd = null;
        }

        protected virtual void UpdateStateButtonRecordStop()
        {
            var bText = _buttonRecordStop.GetComponentInChildren<TextMeshProUGUI>();
            if (_animationPlayerSrc.IsRecording())
            {
                bText.text = "Stop Recording";
                ShowInfo("Recording Start");
            }
            else
            {
                bText.text = "Recording";
                ShowInfo("Recording Stop");
            }
        }

        #endregion

    }

}


