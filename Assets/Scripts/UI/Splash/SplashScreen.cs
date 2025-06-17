using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class is used to display the splash screen when the application is loading. 
    /// It also contains the loading bar that will be displayed during the loading process.
    /// The loading bar will only reach 100% when the loading is completed. which is set when <see cref="SaveMenu.OnStartup"/> is called.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class SplashScreen : MonoBehaviour
    {
        /// <summary> The splash screen UI document </summary>
        public UIDocument Splash;

        /// <summary> The radial progress bar </summary>
        private RadialProgress _radialProgress;

        /// <summary> The radial container </summary>
        private VisualElement _radialContainer;

        /// <summary> The load prompt strings </summary>
        [SerializeField] private LoadPromptStringsSO _loadPromptStrings;

        /// <summary> Reference to the Coroutine so we can end it when loading is over </summary>
        private Coroutine _loadingBarRoutine;

        /// <summary> A boolean to check if the loading bar is still loading </summary>
        private bool _loading;

        public void Start()
        {
            Interactions.Instance.StartUpInteract();
            Interactions.Instance.TurnOffUI();

            GetReferencesAndSetValues();
        }

        /// <summary>
        ///     Called from: 
        ///         <see cref="SaveDataSupport.OnLoad"/>
        ///         <see cref="SaveDataSupport.FreshLoad"/>
        /// </summary>
        public void TurnOffSplash()
        {
            Splash.rootVisualElement.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///     This method is called when during the loading process.
        ///     It updates the loading bar value until it reaches 99%.
        ///     - Only when the load is completed, the loading bar will reach 100%.
        /// </summary>
        /// <returns></returns>
        public IEnumerator LoadingBar()
        {
            _radialProgress.progress = 0.0f;
            yield return null;

            while (_radialProgress.progress < 99.0f)
            {
                _radialProgress.progress += 1f;
                yield return new WaitForSeconds((float)0.02);
            }
        }


        /// <summary>
        ///     This method is called when the loading is completed.
        /// </summary>
        public IEnumerator EndLoadingBar()
        {
            //if loading bar routine has already been stopped then return 
            if (_loading == false)
                yield break;


            StopCoroutine(_loadingBarRoutine);
            _loading = false;

            _radialProgress.progress = 100.0f;
            _radialContainer.Q<Label>("LoadingText").text = _loadPromptStrings.loadingComplete;

            yield return new WaitForSeconds(1);
            //Hide the loading bar.
            _radialContainer.style.display = DisplayStyle.None;

        }


        /// <summary>
        ///     This method is responsible for getting the references of the UI elements and setting the values on start.
        /// </summary>
        private void GetReferencesAndSetValues()
        {
            _radialProgress = Splash.rootVisualElement.Q<RadialProgress>("RadialLoadingBar");
            _radialContainer = Splash.rootVisualElement.Q<VisualElement>("Container");

            //set the text of the loading bar
            _radialContainer.Q<Label>("LoadingText").text = _loadPromptStrings.loading;

            _loadingBarRoutine = StartCoroutine(LoadingBar());
            _loading = true;
        }
    }
}
