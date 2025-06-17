using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class is used to manage the Load Prompt in the game. It sets up the buttons for the prompt and listens for the button clicks.
    /// </summary>
    public class LoadPrompt : MonoBehaviour
    {
        /// <summary><see cref="SaveDataSupport.ClearSaveData"/> </summary>
        /// <summary> <see cref="BlurBackground.Deactivate"/> </summary>
        public UnityEvent OnRestart;

        /// <summary> <see cref="BlurBackground.Deactivate"/> </summary>
        public UnityEvent OnContinue;

        /// <summary>
        ///     Buttons for continue and restart
        /// </summary>
        private Button _continue;
        private Button _restart;

        //Const strings
        private const string _continueBtn = "ContinueBtn";
        private const string _restartBtn = "RestartBtn";


        [SerializeField, Tooltip("The UI doc that is contained within the LoadPrompt game object")] private UIDocument _prompt;

        void Start()
        {
            _continue = _prompt.rootVisualElement.Q(_continueBtn) as Button;
            _restart = _prompt.rootVisualElement.Q(_restartBtn) as Button;

            //Add listeners to the buttons
            _continue.clicked += () =>
            {
                OnContinue?.Invoke();
            };

            _restart.clicked += () =>
            {
                OnRestart?.Invoke();
            };
        }
    }
}
