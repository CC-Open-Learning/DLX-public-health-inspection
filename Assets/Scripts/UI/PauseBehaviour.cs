using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This Class is the Behaviour that will attached to the GameObject that holds the UI Document that contains the "main" UI, and is responsible for setting the pause buttons behaviour.
    /// <see cref="InspectionReview"/> is responsible for setting the behavior of the "ReviewBtn"
    /// </summary>
    public class PauseBehaviour : MonoBehaviour
    {
        //Public/Serialized Properties
        public UIDocument ButtonDoc;
        public BlurBackground Blur;
        public Button PauseBtn;
        public static bool IsPaused = false;

        //Private Properties
        private Button _reviewBtn;
        private Button _settingsBtn;
        private Button _continueBtn;
        private Button _restartBtn;
        private VisualElement _prompt;
        private VisualElement _root;
        private UIBlocker _blocker;
        private Label _pauseTip;
        private Label _reviewTip;
        private Label _windowTitle;
        private Label _settingsTip;

        // Reference to the restart confirmation modal scriptable object
        [SerializeField, Tooltip("Restart modal scriptable object")] private ModalPopupSO _restartModal;

        /// <summary><see cref="ModalPopupBuilder.HandleDisplayModal(ModalPopupSO)"/>/// </summary>
        public UnityEvent<ModalPopupSO> CreateModal;

        /// <summary>
        /// Listener <see cref="PHISceneManager.RestartScene"/>
        /// </summary>
        public UnityEvent RestartScene;

        void Start()
        {
            GetReferences();
            SetupListeners();
        }

        /// <summary>
        /// Sets the tooltips behaviours
        /// </summary>
        private void SetupTooltips()
        {
            _reviewBtn.RegisterCallback<MouseOverEvent>(evt => _reviewTip.style.display = DisplayStyle.Flex);
            _reviewBtn.RegisterCallback<MouseOutEvent>(evt => _reviewTip.style.display = DisplayStyle.None);
            _settingsBtn.RegisterCallback<MouseOverEvent>(evt => _settingsTip.style.display = DisplayStyle.Flex);
            _settingsBtn.RegisterCallback<MouseOutEvent>(evt => _settingsTip.style.display = DisplayStyle.None);
            PauseBtn.RegisterCallback<MouseOverEvent>(evt => _pauseTip.style.display = DisplayStyle.Flex);
            PauseBtn.RegisterCallback<MouseOutEvent>(evt => _pauseTip.style.display = DisplayStyle.None);
        }

        /// <summary>
        /// This is the method called when the button is pressed to pause the game
        /// </summary>
        public void PauseGame()
        {
            TimerManager.Instance.PauseTimers();
            Blur.Activate();
            OpenPrompt();
            StartCoroutine(InteractionsRoutine(false));
            IsPaused = true;
        }

        /// <summary>
        /// This behaviour is to resume the game upon continue handling the actions that need to happen
        /// </summary>
        public void ResumeGame()
        {
            ClosePrompt();
            Blur.Deactivate();
            StartCoroutine(InteractionsRoutine(true));
            IsPaused = false;
            if (PlayerController.IntroCompleted && PlayerController.HasWashedHands)
            {
                TimerManager.Instance.StartTimers();
            }
        }

        /// <summary>
        /// This Coroutine is to set interactions on/off with an update delay to prevent race conditions
        /// </summary>
        /// <param name="tf"> which way to turn the interactions
        /// True = on
        /// False = off
        /// </param>
        private IEnumerator InteractionsRoutine(bool tf)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Interactions.Instance.SetInteract(tf);
        }

        /// <summary>
        /// This method is responsible for opening the prompt for the user
        /// </summary>
        private void OpenPrompt()
        {
            _windowTitle.text = "Pause";
            _prompt.style.display = DisplayStyle.Flex;
            PauseBtn.style.display = DisplayStyle.None;
            _reviewBtn.style.display = DisplayStyle.None;
            _settingsBtn.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// This method is responsible for the actions when the prompt closes such as on continue resetting the visual elements to default and starting the timer again
        /// </summary>
        private void ClosePrompt()
        {
            _prompt.style.display = DisplayStyle.None;
            PauseBtn.style.display = DisplayStyle.Flex;
            _reviewBtn.style.display = DisplayStyle.Flex;
            _settingsBtn.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// All listeners to be setup in Start() are put in here
        /// </summary>
        private void SetupListeners()
        {
            PauseBtn.clicked += PauseGame;
            _continueBtn.clicked += ResumeGame;
            _restartBtn.clicked += () => StartCoroutine(Fade.Singleton.FadeButton(RestartScene.Invoke, false, 0, true));
            _blocker.RegisterMouseLeaveCallback(PauseBtn);
            _blocker.RegisterMouseEnterCallback(PauseBtn);

            _blocker.RegisterMouseLeaveCallback(_settingsBtn);
            _blocker.RegisterMouseEnterCallback(_settingsBtn);

            SetupTooltips();
        }

        /// <summary>
        /// This method is responsible for gathering all the various references within the UI document this behavior needs
        /// </summary>
        private void GetReferences()
        {
            if (ButtonDoc != null)
            {
                _root = ButtonDoc.rootVisualElement;
                PauseBtn = _root.Q<Button>("PauseBtn");
                _prompt = _root.Q<VisualElement>("StartPauseMenu");
                _reviewBtn = _root.Q<Button>("ReviewBtn");
                _settingsBtn = _root.Q<Button>("SettingsBtn");
                _continueBtn = _root.Q<Button>("Continue");
                _restartBtn = _root.Q<Button>("Restart");
                _blocker = UIBlocker.Instance;
            }

            _windowTitle = _root.Q<Label>("WindowTitle");

            _pauseTip = _root.Q<Label>("PauseTooltip");
            _reviewTip = _root.Q<Label>("ReviewTooltip");
            _settingsTip = _root.Q<Label>("SettingsTooltip");
        }

        /// <summary>
        /// Sets up the confirmation modal for the restart button
        /// </summary>
        private void SetUpConfirmationModal()
        {
            _prompt.style.display = DisplayStyle.None;
            _restartModal.SetPrimaryAction(() => StartCoroutine(Fade.Singleton.FadeButton(RestartScene.Invoke, false, 0, true)));
            _restartModal.SetSecondaryAction(() => _prompt.style.display = DisplayStyle.Flex);
            CreateModal?.Invoke(_restartModal);
        }
    }
}
