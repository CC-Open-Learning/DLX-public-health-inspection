using UnityEngine;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class is used to build and manage the Download Confirmation window.
    /// It controls the visibility of the UI elements and handles user interactions.
    /// </summary>
    public class DownloadConfirmationBuilder : MonoBehaviour
    {
        // Reference to the UI Document that contains the Download Confirmation elements
        [SerializeField, Tooltip("Reference to the download confirmation UI document")] private UIDocument _doc;

        // Reference to the "OK" button within the Download Confirmation window
        private Button _button;

        private void Start()
        {
            // Retrieve the "OK" button from the UI Document using its name
            _button = _doc.rootVisualElement.Q<Button>("ModalButton");

            // Assign the 'HideWindow()' method to the "OK" button's click event to hide the window when clicked
            _button.clicked += () => HideWindow();

            // Initially hide the window 
            _doc.rootVisualElement.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Displays the window and disables user interactions with other background elements
        /// </summary>
        public void HandleDisplayWindow()
        {
            _doc.rootVisualElement.style.display = DisplayStyle.Flex;
            Interactions.Instance.SetInteract(false);
        }

        /// <summary>
        /// Hides the window
        /// </summary>
        private void HideWindow()
        {
            _doc.rootVisualElement.style.display = DisplayStyle.None;
        }
    }
}
