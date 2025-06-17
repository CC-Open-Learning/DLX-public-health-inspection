using UnityEngine;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     This class is used on the manager NPC to make the NPC look at the camera.
    /// </summary>
    public class LookAtCamera : MonoBehaviour
    {
        public Transform Rotator; //where the object will rotate from. Can be assigned to an empty parent to get more desirable results

        [SerializeField, Tooltip("Manager Office Position")] private Transform _managerOfficePosition;

        public float LookAtDistance = 7; //the distance at which the rotator will attempt to face the target. When the target is outside of this radial check, the rotator returns to its default orientation
        public float LookAtSpeed = 3; //the speed at which the rotator will turn

        public Transform LookAtTarget; //where the object should look towards. Generally, this will be the player object, or the player's camera

        private Quaternion _startRotation; //saves the beginning orientation of the object to be returned to if the player leaves the range
        private Quaternion _currentRotation; //store the direction the rotator is currently facing

        void Start()
        {
            _startRotation = Rotator.rotation; //store the initial orientation at start
        }

        void LateUpdate() //late update is used so that this script can override animation information
        {
            if (LookAtDistance > Vector3.Distance(Rotator.position, LookAtTarget.position))
            {
                _currentRotation = Quaternion.Slerp(_currentRotation, Quaternion.LookRotation(LookAtTarget.position - Rotator.position), Time.deltaTime * LookAtSpeed);
                Rotator.rotation = _currentRotation;
            }
            else
            {
                _currentRotation = Quaternion.Slerp(_currentRotation, _startRotation, Time.deltaTime * LookAtSpeed);
                Rotator.rotation = _currentRotation;
            }
        }

        /// <summary>
        /// Move the manager from the reception to the office and disable the manager collider.
        /// <see cref="Conversation.OnConversationEnded"/>
        /// </summary>
        public void MoveManagerToOffice()
        {
            if (PlayerController.IntroCompleted && !PlayerController.VisitedOffice)
            {
                Rotator.gameObject.transform.position = _managerOfficePosition.position;

                Rotator.GetComponent<CapsuleCollider>().enabled = false;
            }
        }
    }
}
