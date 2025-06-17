using System.Collections.Generic;
using UnityEngine;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class will manage door animation behaviours
    /// For double doors we have 4 different animations: OpenFromInside, OpenFromOutside, CloseFromInside, CloseFromOutside
    /// </summary>
    public class TargetDoorAnimBehaviour : MonoBehaviour
    {
        //Set all animator controllers and parameters on the inspector
        [Header("Enter Anim Behaviour")]
        [Tooltip("Animator controllers for the door behaviour when entering")] public AnimBehaviour EnterAnimBehaviour;

        //Set all animator controllers and parameters on the inspector
        [Header("Exit Anim Behaviour")]
        [Tooltip("Animator controllers for the door behaviour when exiting")] public AnimBehaviour ExitAnimBehaviour;

        //set if this door is a double door
        [Tooltip("Is This a double door")] public bool isDoubleDoor;

        private bool _openedFromInside;

        //Set all animator controllers and parameters on the inspector
        [Header("Double Door Enter From Inside")]
        [Tooltip("Animator controllers for the door behaviour when entering from inside")] public AnimBehaviour DoubleDoorEnterAnimBehaviourInside;

        //Set all animator controllers and parameters on the inspector
        [Header("Double Door Exit From Inside")]
        [Tooltip("Animator controllers for the door behaviour when exiting from inside")] public AnimBehaviour DoubleDoorExitAnimBehaviourInside;


        //For double doors, OpenFromOutside euqals rotate on positive Y axis for the left door
        private List<string> _openTriggers = new List<string> { "Open", "OpenFromInside", "OpenFromOutside" };

        public static bool IsDoorOpen = false;


        public void OnTriggerEnter(Collider collision)
        {
            IsDoorOpen = true;
            if (collision.gameObject.tag == "Player")
            {
                if (isDoubleDoor == false)
                { PlayDoorAnimation(EnterAnimBehaviour); }
                else
                {
                    Vector3 relativePosition = gameObject.transform.InverseTransformPoint(collision.gameObject.transform.position);

                    if (relativePosition.x <= 0)
                    {
                        PlayDoorAnimation(EnterAnimBehaviour);
                        _openedFromInside = false;
                    }
                    else
                    {
                        PlayDoorAnimation(DoubleDoorEnterAnimBehaviourInside);
                        _openedFromInside = true;
                    }

                    CinemachineExtensionLayerMask.Instance.DefaultLayers.value = -1;
                }
            }
        }

        public void OnTriggerExit(Collider collision)
        {
            IsDoorOpen = false;

            if (collision.gameObject.tag == "Player")
            {
                if (isDoubleDoor == false)
                { PlayDoorAnimation(ExitAnimBehaviour); }
                else
                {
                    if (_openedFromInside == false)
                    {
                        PlayDoorAnimation(ExitAnimBehaviour);
                    }
                    else
                    {
                        PlayDoorAnimation(DoubleDoorExitAnimBehaviourInside);
                    }

                }
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
        private void ToggleDoor(AnimBehaviour animBehaviour, bool newState)
        {
            //Check door direction for double doors close properly
            AnimatorStateInfo animatorStateInfo = animBehaviour.Animator.GetCurrentAnimatorStateInfo(0);

            //If the door is open from outside and CloseFromInside is triggered, we don't want to close it
            if (animBehaviour.TriggerParamString == "CloseFromInside" && animatorStateInfo.IsName("OpenFromOutside"))
            {
                return;
            }

            //If the door is open from inside and CloseFromOutside is triggered, we don't want to close it
            if (animBehaviour.TriggerParamString == "CloseFromOutside" && animatorStateInfo.IsName("OpenFromInside"))
            {
                return;
            }

            //For the door we set a bool parameter on the animator controller called DoorState, where open = true & close = false 
            bool previousState = animBehaviour.Animator.GetBool(animBehaviour.BoolParamHash);

            if (newState != previousState)
            {
                animBehaviour.Animator.SetTrigger(animBehaviour.TriggerParamHash);
                animBehaviour.Animator.SetBool(animBehaviour.BoolParamHash, newState);
            }

        }
    }


}
