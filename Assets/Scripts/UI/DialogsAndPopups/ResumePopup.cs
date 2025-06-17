using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    public class ResumePopup : MonoBehaviour
    {
        #region Properties


        [SerializeField] private ResumePopupElementNamesSO _elementNames;

        // close button
        private Button _closePopupButton;

        // Location Label
        private Label _labelElement;


        public BlurBackground Blur;

        public UIDocument ResumePopupDoc;




        #endregion

        // Start is called before the first frame update
        void Start()
        {
            _closePopupButton = ResumePopupDoc.rootVisualElement.Q<Button>(_elementNames.CloseBtn);
            _labelElement = ResumePopupDoc.rootVisualElement.Q<Label>(_elementNames.LocationLabel);

            // resgister click events
            _closePopupButton.clicked += CloseWindow;

            ResumePopupDoc.rootVisualElement.style.display = DisplayStyle.None;

        }

        public void DisplayWindow(string location)
        {
            ResumePopupDoc.rootVisualElement.style.display = DisplayStyle.Flex;
            Blur.Activate();
            _labelElement.text = location;
            StartCoroutine(InteractionsRoutine(false));
        }

        public void CloseWindow()
        {
            ResumePopupDoc.rootVisualElement.style.display = DisplayStyle.None;
            Blur.Deactivate();
            StartCoroutine(InteractionsRoutine(true));
            Interactions.Instance.TurnOnUI();
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
            Interactions.Instance.SetUIEnabled(tf);
        }
    }
}
