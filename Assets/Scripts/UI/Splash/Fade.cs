using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class is responsible for fading in and out the splash screen.
    ///     It is a singleton class and will be accessed by other classes to fade in and out the splash screen.
    ///     This fades in a black screen, allowing any action to be done behind the fade, and then fades out.
    /// </summary>
    public class Fade : MonoBehaviour
    {
        /// <summary>
        ///    The singleton instance of the Fade class.
        /// </summary>
        public static Fade Singleton;

        /// <summary>
        ///    The UIDocument that contains the fade screen.
        /// </summary>
        public UIDocument FadeDoc;

        /// <summary>
        ///     The visual element that will be faded in and out that is set in start.
        /// </summary>
        private VisualElement _fade;

        /// <summary>
        ///    constants for the fade in and fade out uss classes and the fade time.
        /// </summary>
        private const string FadeOutUSS = "fade-out";
        private const string FadeInUSS = "fade-in";
        private const float FadeTime = 1.0f;

        /// <summary>
        ///     <see cref="SaveDataSupport.OnLoad"/>
        ///     <see cref="SaveDataSupport.FreshLoad"/>
        /// </summary>
        public UnityEvent TurnOffSplash;

        // Start is called before the first frame update
        void Start()
        {
            Singleton = this;
            _fade = FadeDoc.rootVisualElement.Q<VisualElement>("Fade");
            FadeDoc.rootVisualElement.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///     This method allows for an action to be done behind the fade.
        /// </summary>
        /// <param name="act">The action to be executed</param>
        /// <param name="enableUIAfterFade">To enable interactions and ui when fade out is completed.</param>
        /// <returns>IEnumerator</returns>
        public IEnumerator FadeButton(Action act, bool enableUIAfterFade = false, float time = 0f, bool restartingScene = false)
        {
            //Fade in the splash screen
            yield return FadeIn();

            yield return new WaitForSeconds(time);
            if (restartingScene)
            {
                PHISceneManager.LoadCompleted = false;
            }

            act();
            if (!restartingScene)
            {
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                yield return new WaitUntil(() => PHISceneManager.LoadCompleted == true);
            }
            //Fade out the splash screen
            yield return FadeOut();

            //Remove the styles from the fade
            RemoveStylesFromFade();

            if (enableUIAfterFade)
                ReEnableUI();
        }

        /// <summary>
        ///     This method re-enables the interactions and UI after the fade out is completed and hides the fade screen.
        /// </summary>
        private void ReEnableUI()
        {
            Interactions.Instance.ReEnableActions();
            Interactions.Instance.TurnOnUI();
            FadeDoc.rootVisualElement.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///     This method will fade in the splash screen.
        /// </summary>
        /// <returns>IEnumerator</returns>
        public IEnumerator FadeIn()
        {
            yield return new WaitForFixedUpdate();

            //Fade in the splash screen
            FadeDoc.rootVisualElement.style.display = DisplayStyle.Flex;
            _fade.AddToClassList(FadeInUSS);

            yield return new WaitForSeconds(FadeTime);
        }


        /// <summary>
        ///    This method will fade out the splash screen.
        /// </summary>
        /// <returns>IEnumerator</returns>
        public IEnumerator FadeOut()
        {
            TurnOffSplash?.Invoke();
            yield return new WaitForFixedUpdate();

            _fade.AddToClassList(FadeOutUSS);
            yield return new WaitForSeconds(FadeTime);

            FadeDoc.rootVisualElement.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///   This method will remove the transition styles from the fade.
        /// </summary>
        private void RemoveStylesFromFade()
        {
            _fade.RemoveFromClassList(FadeOutUSS);
            _fade.RemoveFromClassList(FadeInUSS);
        }

        /// <summary>
        ///     This method will set the fade to an action.
        /// </summary>
        /// <param name="a">The action to have the fade effect.</param>
        /// <returns>The action wrapped with a fade.</returns>
        public Action SetFadeAction(Action a, bool restart = false)
        {
            return () => StartCoroutine(FadeButton(a, false, 0, restart));
        }
    }
}
