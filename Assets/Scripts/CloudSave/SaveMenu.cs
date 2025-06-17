using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Analytics;

namespace VARLab.PublicHealth
{

    /// <summary>
    ///     This class controls the save menu and the loading of the save file.
    /// </summary>
    public class SaveMenu : MonoBehaviour
    {
        public enum SaveMode
        {
            Continue,
            StartFromBeginning,
            TryAgain
        }

        [HideInInspector]
        private bool _hasLoaded;

        [SerializeField] private UIDocument _startPauseMenu;
        [SerializeField] private SplashScreen _splashScreen;

        // Reference to the restart confirmation modal scriptable object
        [SerializeField, Tooltip("Restart modal scriptable object")] private ModalPopupSO _restartModal;

        /// <summary><see cref="ModalPopupBuilder.HandleDisplayModal(ModalPopupSO)"/>/// </summary>
        public UnityEvent<ModalPopupSO> CreateModal;

        /// <summary>Listener <see cref="PHISceneManager.RestartScene"/></summary>
        public UnityEvent RestartScene;

        private const string ContinueBtn = "Continue";
        private const string RestartBtn = "Restart";


        private void Start()
        {
            _startPauseMenu.rootVisualElement.style.display = DisplayStyle.None;
            Label windowTitle = _startPauseMenu.rootVisualElement.Q<Label>("WindowTitle");
            windowTitle.text = "Start";
            SetupButtons();
        }

        /// <summary>
        ///     <see cref="SaveDataSupport.OnInitialize"/> CALLS THIS METHOD
        /// </summary>
        public void OnInitialize()
        {
            StartCoroutine(OnStartup());
        }

        /// <summary>
        ///     This method is responsible for the waiting for the save system to load and then checking the validity of the save file. 
        /// </summary>
        /// <returns></returns>
        public IEnumerator OnStartup()
        {
            yield return WaitForSaveSystem();

            yield return _splashScreen.EndLoadingBar();

            yield return StartCoroutine(Fade.Singleton.FadeButton(() => CheckValidity(), false, 0.2f));

            SaveDataSupport.Singleton.SetupInitialData();
        }

        /// <summary>
        ///     This method sets up the buttons for the save menu.
        /// </summary>
        private void SetupButtons()
        {
            Button _continue = _startPauseMenu.rootVisualElement.Q<Button>(ContinueBtn);
            Button _restart = _startPauseMenu.rootVisualElement.Q<Button>(RestartBtn);

            //change these eventually
            _continue.clicked += () =>
            {
                _startPauseMenu.rootVisualElement.style.display = DisplayStyle.None;
                StartCoroutine(Fade.Singleton.FadeButton(() => HandleSaveModeResult(SaveMode.Continue), false));
            };

            _restart.clicked += () =>
            {
                _startPauseMenu.rootVisualElement.style.display = DisplayStyle.None;
                StartCoroutine(Fade.Singleton.FadeButton(RestartScene.Invoke, false, 0, true));
            };
        }

        /// <summary>
        ///     This method is responsible for waiting for the save system to load.
        ///     has a set timeout of 30 seconds.
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForSaveSystem()
        {
            const float TIMEOUT = 30f;
            _hasLoaded = false;

            SaveDataSupport.Singleton.CloudSave.Load();

            for (float t = 0f; t <= TIMEOUT; t += Time.deltaTime)
            {
                yield return new WaitUntil(() => SaveDataSupport.Singleton.CloudSave.HasLoaded);
                _hasLoaded = SaveDataSupport.Singleton.CloudSave.HasLoaded;
                break;
            }
        }

        /// <summary>
        ///     This method is responsible for checking the validity of the save file.
        /// </summary>
        private bool CheckValidity()
        {
            if (SaveDataSupport.Singleton.SaveFileVersion() != Application.version || SaveDataSupport.Singleton.SaveFileEndInspection() == true)
            {
                SaveDataSupport.Singleton.ClearSaveData();
                HandleSaveModeResult(SaveMode.StartFromBeginning);
                return false;
            }

            ShowMenu();

            return true;
        }

        /// <summary>
        ///     Change the display style of the save menu to flex.
        /// </summary>
        public void ShowMenu()
        {
            _startPauseMenu.rootVisualElement.style.display = DisplayStyle.Flex;
            return;
        }

        /// <summary>   
        ///     This handles what path the sim will take based on the save menu that the user selects.
        ///     It will also be called if the save file is invalid. Starting a fresh game.
        /// </summary>
        private void HandleSaveModeResult(SaveMode _saveMenu)
        {
            switch (_saveMenu)
            {
                case SaveMode.Continue:
                    SaveDataSupport.Singleton.OnLoad?.Invoke();
                    break;
                case SaveMode.StartFromBeginning:
                    SaveDataSupport.Singleton.FreshLoad?.Invoke();
                    CoreAnalytics.SendDLXStartedEvent();
                    break;
                case SaveMode.TryAgain:
                    // Show Menu Again
                    break;
            }
            _startPauseMenu.rootVisualElement.style.display = DisplayStyle.None;
        }


        /// <summary>
        ///     Delete the save file.
        /// </summary>
        public void DeleteSave()
        {
            if (SaveDataSupport.Singleton.CloudSave.LoadSuccess == false)
            {
                SaveDataSupport.Singleton.TriggerDelete();
            }
        }

        /// <summary>
        /// Sets up the confirmation modal for the restart button
        /// </summary>
        private void SetUpConfirmationModal()
        {
            _startPauseMenu.rootVisualElement.style.display = DisplayStyle.None;
            _restartModal.SetPrimaryAction(() => StartCoroutine(Fade.Singleton.FadeButton(RestartScene.Invoke, false, 0, true)));
            _restartModal.SetSecondaryAction(() => _startPauseMenu.rootVisualElement.style.display = DisplayStyle.Flex);
            CreateModal?.Invoke(_restartModal);
        }
    }
}
