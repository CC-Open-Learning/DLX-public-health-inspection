using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.PublicHealth
{
    public class WashingHands : MonoBehaviour
    {
        [SerializeField] private AudioSource _runningWater;
        [SerializeField] private ToastManager _toastManager;
        [SerializeField] private InformationModalSO _handwashModal;
        [SerializeField, Tooltip("Reference to the handwashing station")] GameObject _handwashingWater;
        public GameObject HandwashingStation;

        private const string _sinkInspectionText = "The hand sink is in good condition, with hot and cold running water, liquid soap, and paper towels.";
        private const string _handWashText = "Now that you have washed your hands, you are ready to begin the inspection";
        private const string _audioCaptionText = "*sounds of washing hands*";
        private Color _green = new Color(9f / 255, 117f / 255, 56f / 255);
        private Color _blue = new Color(6f / 255, 113f / 255, 224f / 255);
        private Color _black = new Color(0, 0, 0);

        /// <summary> <see cref="ModalPopupBuilder.HandleDisplayModal(ModalPopupSO)"/> </summary>
        public UnityEvent<InformationModalSO> ShowModalPopup;

        public void HandleHandWash()
        {
            _handwashModal.SetPrimaryAction(CompleteHandwash);
            StartCoroutine(InteractionsRoutine(false));
            ShowModalPopup?.Invoke(_handwashModal);
        }

        public void CompleteHandwash()
        {
            StartCoroutine(HandwashCoroutine(_runningWater));
        }

        /// <summary>
        /// Wait for the water audio to finish playing, then display the toast
        /// </summary>
        public IEnumerator HandwashCoroutine(AudioSource clip)
        {
            // toast message for water clip playing
            _toastManager.DisplayToast(_audioCaptionText, _runningWater.clip.length, _black, ToastSize.Large, ToastBorderStyle.BorderThin, ToastAlignment.Bottom, ToastIcons.None, true);

            // play water clip
            clip.Play();

            // turn on water
            _handwashingWater.SetActive(true);

            // wait for water clip to stop
            yield return new WaitForSeconds(clip.clip.length);

            // turn off water
            _handwashingWater.SetActive(false);

            // wait for toast message to display properly
            yield return new WaitForSeconds(1f);
            _toastManager.DisplayToast(_handWashText, 3.0f, _green, ToastAlignment.Top, ToastIcons.Check, true);

            yield return new WaitForSeconds(1f);
            Interactions.Instance.SetInteract(true);
            Interactions.Instance.SetUIEnabled(true);
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
    }
}
