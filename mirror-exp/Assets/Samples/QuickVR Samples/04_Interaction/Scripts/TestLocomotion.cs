using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR.SampleInteraction
{

    public class TestLocomotion : QuickMonoBehaviour
    {

        public Animator _targetAnimator1 = null;
        public Animator _targetAnimator2 = null;

        public TestMove _vehicle = null;

        // Start is called before the first frame update
        //protected virtual IEnumerator Start()
        //{
        //    yield return new WaitForSeconds(0.5f);

        //    SetTargetAvatar();
        //}


        [ButtonMethod]
        public virtual void SetTargetAvatar1()
        {
            QuickSingletonManager._vrManager.SetAnimatorTarget(_targetAnimator1);
        }

        [ButtonMethod]
        public virtual void SetTargetAvatar2()
        {
            QuickSingletonManager._vrManager.SetAnimatorTarget(_targetAnimator2);
        }

        [ButtonMethod]
        public virtual void GetVehicleIn()
        {
            if (_vehicle != null)
            {
                var animator = QuickSingletonManager._vrManager.GetAnimatorSource();
                animator.transform.parent = _vehicle.transform;
                animator.transform.ResetTransformation();

                _vehicle._move = true;
            }
        }

        [ButtonMethod]
        public virtual void GetVehicleOut() 
        { 
            if (_vehicle != null)
            {
                _vehicle._move = false;

                var animator = QuickSingletonManager._vrManager.GetAnimatorSource();
                animator.transform.parent = null;
            }
        }

    }

}


