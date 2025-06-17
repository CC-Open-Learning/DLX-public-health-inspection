using UnityEngine;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class is used to manage the start info window that appears when the game is started.
    /// </summary>
    public class StartInfo : MonoBehaviour
    {
        //Public
        public UIDocument InfoWindow;

        //Private
        private Button _beginBtn;
        //private Button _helpBtn; //TODO Implement tutorial here eventually
        private VisualElement _root;
        public CameraController CameraController;

        public void Start()
        {
            GetAllReferences();
            SetupListeners();
            _root.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// single method to retrieve all refs
        /// </summary>
        private void GetAllReferences()
        {
            _root = InfoWindow.rootVisualElement;
            _beginBtn = _root.Q<Button>("BeginBtn");
            //_helpBtn = _root.Q<Button>("Help");
        }

        /// <summary>
        /// Assign behavior to buttons in here
        /// </summary>
        private void SetupListeners()
        {
            _beginBtn.clicked += CloseWindow;
            //_helpBtn.clicked += () => Debug.Log("You clicked for help!");
        }

        /// <summary>
        /// This is called to open the start info window
        /// </summary>
        public void OpenWindow()
        {
            _root.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// This is called when the begin button is clicked. Sets the window to not be displayed and interacts to true.
        /// </summary>
        public void CloseWindow()
        {
            _root.style.display = DisplayStyle.None;
            Interactions.Instance.SetInteract(true);
            Interactions.Instance.TurnOnUI();
            CameraController.HandleCameraOnLoad();
        }
    }
}
