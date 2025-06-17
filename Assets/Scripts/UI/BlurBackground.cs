using UnityEngine;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class is used to manage the blur background in the game. It will activate and deactivate the blur background. 
    /// </summary>
    public class BlurBackground : MonoBehaviour
    {
        //Awake is called when the script instance is being loaded
        void Awake()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Deactivates the blur background
        /// <see cref="Interactions.OnTurnOnUI"/> </summary>
        /// </summary>
        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Activates the blur background
        /// <see cref="Interactions.TurnOffUI"/> </summary>
        /// </summary>
        public void Activate()
        {
            gameObject.SetActive(true);
        }
    }
}
