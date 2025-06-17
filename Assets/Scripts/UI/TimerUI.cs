using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     This class is used to control the timer UI and start the timer on start 
    /// </summary>
    public class TimerUI : MonoBehaviour
    {
        [SerializeField, Tooltip("Timer UI Document")] private UIDocument _timerUI;

        /// <summary>The main visual element for the timer to be on</summary>
        private VisualElement _slider;

        /// <summary>The button to open and close the slider </summary>
        private Button _slideBtn;

        /// <summary>The label of text the timer will be displayed on</summary>
        private Label _time;

        /// <summary>The blocker to prevent the slider from closing when the mouse is over it</summary>
        private UIBlocker _blocker;

        /// <summary>Whether the slider is hidden or not</summary>
        private bool _isSliderHidden;

        /// <summary>Constants for the timer to use, strings for arrow direction and ints for slider placement</summary>
        private const int SlideOpenValue = 0;
        private const int SlideCloseValue = -178; //pixels to move from absolute to be visible again
        private const string OpenMarker = ">";
        private const string CloseMarker = "<";

        // Element names
        private const string _slideButton = "SlideBtn";
        private const string _timeLabel = "Time";
        private const string _sliderContainer = "SliderContainer";

        //Tooltips
        private Label _timerTip;
        private int _tipClosePos = 35;
        private int _tipOpenPos = 213;

        // Start is called before the first frame update
        void Start()
        {
            // Set the slider to be open and the button to be the close marker
            _isSliderHidden = true;
            GetReferences();
            SetupListeners();
        }

        /// <summary>
        /// single place to hold all the gathering of references
        /// </summary>
        private void GetReferences()
        {
            // Get the visual elements from the UIDocument
            _slideBtn = _timerUI.rootVisualElement.Q<Button>(_slideButton);
            _time = _timerUI.rootVisualElement.Q<Label>(_timeLabel);
            _slider = _timerUI.rootVisualElement.Q<VisualElement>(_sliderContainer);
            _timerTip = _timerUI.rootVisualElement.Q<Label>("TimerTooltip");
        }

        /// <summary>
        /// Sets the tooltip behaviours
        /// </summary>
        private void SetupTooltip()
        {
            _slideBtn.RegisterCallback<MouseOverEvent>(evt => _timerTip.style.display = DisplayStyle.Flex);
            _slideBtn.RegisterCallback<MouseOutEvent>(evt => _timerTip.style.display = DisplayStyle.None);
        }

        /// <summary>
        /// Single place to hold all the setup of listeners
        /// </summary>
        private void SetupListeners()
        {
            // Set the button to toggle the slider
            _slideBtn.clicked += ToggleSlide;

            // Set the blocker to prevent the slider from closing when the mouse is over it
            _blocker = UIBlocker.Instance;
            _blocker.RegisterMouseEnterCallback(_slider);
            _blocker.RegisterMouseLeaveCallback(_slider);
            SetupTooltip();
        }

        private void Update()
        {
            // Update the time on the timer if the slider is not hidden
            if (!_isSliderHidden)
            {
                _time.text = TimerManager.Instance.GetElapsedTime();
            }
        }

        /// <summary>
        ///     This method is used to toggle the slider open and closed
        /// </summary>
        private void ToggleSlide()
        {
            _slider.style.right = _isSliderHidden ? OpenPanel() : ClosePanel();
            StartCoroutine(SlideTipRoutine());
        }

        /// <summary>
        /// This co routine is to allow the slider to open/close before shifting to position of the tooltip to prevent it from visually jumping
        /// </summary>
        /// <returns></returns>
        private IEnumerator SlideTipRoutine()
        {
            yield return new WaitForSeconds(0.5f);
            _timerTip.style.right = _isSliderHidden ? _tipClosePos : _tipOpenPos;
        }


        /// <summary>
        ///     Open the slider, change the button text, set the slider to not hidden
        /// </summary>
        /// <returns>The value to move the slider to open</returns>
        private int OpenPanel()
        {
            _slideBtn.text = OpenMarker;

            _isSliderHidden = false;

            return SlideOpenValue;
        }

        /// <summary>
        ///     Close the slider, change the button text, set the slider to hidden
        /// </summary>
        /// <returns>The value of move the slider to close</returns>
        private int ClosePanel()
        {
            _slideBtn.text = CloseMarker;

            _isSliderHidden = true;

            return SlideCloseValue;
        }
    }
}
