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
            const float TIMEOUT = 8f;
            float timer = 0f;
            _hasLoaded = false;

            // i don't think we need the cast but its a good idea to check if the type is correct
            var cloudSaving = SaveDataSupport.Singleton.CloudSave as CloudSaving;
            if (cloudSaving == null)
            {
                Debug.LogError("[SaveMenu] CloudSave is not of type CloudSaving.");
                yield break;
            }

            // Wait for CloudSave to finish initializing
            // This is a blocking call that will wait until the CloudSave system is initialized
            // this was done with waitUntil function but changed because the sim wasn't loading earlier
            // this change might be useless and is a better idea to rever to using waitUntil since 
            // its native to unity. However, I don't personally care to make it effecient as of now
            while (!cloudSaving.IsInitialized && timer < TIMEOUT)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (!cloudSaving.IsInitialized)
            {
                Debug.LogWarning("[SaveMenu] CloudSave initialization timed out.");
                yield break;
            }

            cloudSaving.Load();

            timer = 0f;
            while (!cloudSaving.HasLoaded && timer < TIMEOUT)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (!cloudSaving.HasLoaded)
            {
                Debug.LogWarning("[SaveMenu] WaitForSaveSystem timed out after 8 seconds.");
            }
            else
            {
                _hasLoaded = true;
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
