using UnityEngine;

namespace VARLab.PublicHealth
{

    /// <summary>
    ///     This class is meant to rotate the fan in the walk-in cooler. 
    ///     It takes a GameObject and rotates it at a set speed when the player enters the trigger area
    /// </summary>
    public class FanRotate : MonoBehaviour
    {
        [SerializeField, Tooltip("This controls the speed in which the object will rotates")] private float _rotateSpeed = 1000f;

        [SerializeField, Tooltip("The fan game object to be rotated")] private GameObject _fan;

        /// <summary>The object will only rotate when this bool is true</summary>
        private bool _isActive = false;

        void Update()
        {
            if (_isActive)
            {
                _fan.transform.Rotate(new Vector3(0f, 0f, _rotateSpeed) * Time.deltaTime);
            }
        }

        /// <summary>
        ///     This method checks if the player has entered the trigger area by checking the tag of the collider and will set the bool to true
        /// </summary>
        /// <param name="other">This is the collider that the player has</param>
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                _isActive = true;
            }
        }


        /// <summary>
        ///     This method checks if the player has exited the trigger area by checking the tag of the collider and will set the bool to false
        /// </summary>
        /// <param name="other">This is the collider that the player has</param>
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                _isActive = false;
            }
        }
    }
}
