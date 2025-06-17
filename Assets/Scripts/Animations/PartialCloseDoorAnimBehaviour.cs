using System.Collections.Generic;
using UnityEngine;

namespace VARLab.PublicHealth
{
    public class PartialCloseDoorAnimBehaviour : MonoBehaviour
    {
        //Set all animator controllers and parameters on the inspector
        [Header("Partial Close Anim Behaviour")]
        [Tooltip("Animator controllers for the door behaviour when entering")] public AnimBehaviour PartialCloseAnimBehaviour;

        //Set all animator controllers and parameters on the inspector
        [Header("Partial Open Anim Behaviour")]
        [Tooltip("Animator controllers for the door behaviour when exiting")] public AnimBehaviour PartialOpenAnimBehaviour;


        //For double doors, OpenFromOutside euqals rotate on positive Y axis for the left door
        private List<string> _openTriggers = new List<string> { "Open", "OpenFromInside", "OpenFromOutside", "PartialOpen" };

        public void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                PlayDoorAnimation(PartialCloseAnimBehaviour);
            }
        }
        public void OnTriggerExit(Collider collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                Vector3 relativePosition = gameObject.transform.InverseTransformPoint(collision.gameObject.transform.position);

                PlayDoorAnimation(PartialOpenAnimBehaviour);

            }
        }

        /// <summary>
        ///     Check and play door animations properly 
        /// </summary>
        /// <param name="loadedFromSave">Bool to check if loading from save</param>
        public void PlayDoorAnimation(AnimBehaviour Anim, bool loadedFromSave = false)
        {
            if (Anim == null)
            {
                return;
            }

            // set the speed of the animation
            Anim.Animator.speed = 1;

            // speed up the animation if it's been loaded from a save
            if (loadedFromSave)
                Anim.Animator.speed = 10;


            if (_openTriggers.Contains(Anim.TriggerParamString))
            {
                ToggleDoor(Anim, true);
            }
            else
            {
                ToggleDoor(Anim, false);
            }

        }

        /// <summary>
        /// Changes the door state in the animator controler
        /// </summary>
        /// <param name="animBehaviour">The door animator controller</param>
        /// <param name="newState">The new door state</param>
        private void ToggleDoor(AnimBehaviour animBehaviour, bool Open)
        {
            //Check door direction for double doors close properly
            AnimatorStateInfo animatorStateInfo = animBehaviour.Animator.GetCurrentAnimatorStateInfo(0);

            //For the door we set a bool parameter on the animator controller called DoorState, where open = true & close = false 
            bool previousState = animBehaviour.Animator.GetBool(animBehaviour.BoolParamHash);

            if (Open)
            {
                animBehaviour.Animator.SetBool(animBehaviour.BoolParamHash, Open);
                PartialCloseAnimBehaviour.Animator.SetBool(PartialCloseAnimBehaviour.BoolParamHash, !Open);
            }
            else
            {
                animBehaviour.Animator.SetBool(animBehaviour.BoolParamHash, !Open);
                PartialOpenAnimBehaviour.Animator.SetBool(PartialOpenAnimBehaviour.BoolParamHash, Open);
            }
        }
    }


}
