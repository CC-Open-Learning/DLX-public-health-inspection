using UnityEngine;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     This class is responsible for building the progress bar UI and updating the progress bar UI with the correct values
    ///     It uses the progress bar class to retrieve the correct values to be displayed on the UI
    /// </summary>

    public class ProgressBarBuilder : MonoBehaviour
    {
        /// <summary>
        ///     This class contains all the logic for retrieving the correct values to be displayed on the progress bar UI
        /// </summary>
        private ProgressBar _progressBar;

        [SerializeField, Tooltip("Reference to the inspectable manager")] private InspectableManager _inspectableManager;
        [SerializeField, Tooltip("Reference to the poi manager")] private POIManager _poiManager;
        [SerializeField, Tooltip("Reference to the timer manager")] private TimerManager _timerManager;
        [SerializeField, Tooltip("Reference to the progress bar UI document")] private UIDocument _progressBarUI;

        // Element strings
        private const string _timeTakenNum = "TimeTakenNum";
        private const string _locationNum = "LocationsNum";
        private const string _nonCompNum = "NonCompNum";

        /// <summary>
        ///     This bool is used to determine if the progress bar is open or not and will be used to update the time
        /// </summary>
        public bool IsOpen = false;

        // Start is called before the first frame update
        private void Awake()
        {
            _progressBar = new();
        }

        // Update is called once per frame
        private void Update()
        {
            //Only update the time if the progress bar is open
            if (IsOpen)
            {
                _progressBarUI.rootVisualElement.Q<Label>(_timeTakenNum).text = _progressBar.ConvertTimeSpanToString(TimerManager.Instance.GetTimeSpan());
            }

        }

        /// <summary>
        ///     This method is used to build the progress bar UI 
        ///     It Queries the UI document for the correct elements and updates the values of the elements with the correct values
        /// </summary>
        public void BuildProgressBar()
        {
            int x = _inspectableManager.GetNonComplianceCount();

            _progressBar.UpdateProgress(_poiManager.InspectablePOIsCount, _poiManager.VisitedPOIs.Count, x, TimerManager.Instance.GetTimeSpan());

            _progressBarUI.rootVisualElement.Q<Label>(_locationNum).text = _progressBar.LocationProgress;
            _progressBarUI.rootVisualElement.Q<Label>(_nonCompNum).text = _progressBar.NonComplianceProgress;
            _progressBarUI.rootVisualElement.Q<Label>(_timeTakenNum).text = _progressBar.TimeProgressString;

            IsOpen = true;
        }

        /// <summary>
        ///     This method is used to update the progress bar UI with the new non-compliance count when the user deletes a non-compliance
        /// </summary>
        public void UpdateNonComplianceText()
        {
            BuildProgressBar();
        }
    }
}
