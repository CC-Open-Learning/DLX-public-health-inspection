using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    public class InspectionSummaryBuilder : MonoBehaviour
    {
        [SerializeField, Tooltip("Reference to the UI Document")] private UIDocument _inspectionSummaryDoc;
        [SerializeField, Tooltip("Reference to the UI Elements SO")] private InspectionSummaryElementNamesSO _elements;
        [Tooltip("Root Visual Element")] private VisualElement _root;

        [SerializeField, Tooltip("Reference to the Inspectable Manager")] private InspectableManager _inspectableManager;

        [Tooltip("Play Again Button Events")] public UnityEvent PlayAgain;
        [Tooltip("Download Button Events")] public UnityEvent Download;

        // Overview Section
        private Label _inspectionDate;
        private Label _totalTime;
        private Label _nonCompliancesReported;
        private Label _compliancesReported;
        private Label _locationsInspected;

        // Locations not inspected
        private Label _locationsNotInspected;
        private Label _locationsCount;
        private string _locationString;
        private string _locationCountString;
        private bool _informationCollected = false;

        // Buttons
        private Button _primaryButton;
        private Button _secondaryButton;

        private void Start()
        {
            _root = _inspectionSummaryDoc.rootVisualElement;
            _root.style.display = DisplayStyle.None;

            GetReferences();
            SetListeners();
        }

        /// <summary>
        /// Gets the references for the visual elements in the UI
        /// </summary>
        private void GetReferences()
        {
            // Overview
            _inspectionDate = _root.Q<Label>(_elements.InspectionDateTxt);
            _totalTime = _root.Q<Label>(_elements.TimeTxt);
            _nonCompliancesReported = _root.Q<Label>(_elements.NonComplainceTxt);
            _compliancesReported = _root.Q<Label>(_elements.ComplianceTxt);
            _locationsInspected = _root.Q<Label>(_elements.LocationsTxt);

            // Locations not Inspected
            _locationsNotInspected = _root.Q<Label>(_elements.LocationsNotInspectectedTxt);
            _locationsCount = _root.Q<Label>(_elements.LocationCount);

            // Buttons
            _primaryButton = _root.Q<Button>(_elements.PrimaryButton);
            _secondaryButton = _root.Q<Button>(_elements.SecondaryButton);
        }

        /// <summary>
        /// Sets up the listeners for the buttons in the UI
        /// </summary>
        private void SetListeners()
        {
            _primaryButton.clicked += PrimaryClicked;
            _secondaryButton.clicked += SecondaryClicked;
        }

        /// <summary>
        /// <see cref=""/>
        /// Invokes the Download functions
        /// </summary>
        private void PrimaryClicked()
        {
            // TODO: Add trigger to open download confirmation window.
            Download?.Invoke();
        }

        /// <summary>
        /// <see cref="PHISceneManager.RestartScene"/>
        /// Starts a fresh load
        /// </summary>
        private void SecondaryClicked()
        {
            PlayAgain?.Invoke();
        }

        /// <summary>
        /// Coroutine that populates the information for the inspection summary screen.
        /// </summary>
        /// <returns></returns>
        private IEnumerator PopulateWindow()
        {
            GetLocationsNotVisitedInfo();

            yield return new WaitUntil(() => _informationCollected);

            // Overview
            _inspectionDate.text = DateAndTime.GetDateString();
            _totalTime.text = TimerManager.Instance.ConvertTimeSpanToString();
            _nonCompliancesReported.text = _inspectableManager.GetNonComplianceCount().ToString();
            _compliancesReported.text = _inspectableManager.GetComplianceCount().ToString();
            _locationsInspected.text = $"{PHISceneManager.Instance.PoiManager.VisitedPOIs.Count} / {PHISceneManager.Instance.PoiManager.InspectablePOIsCount}";

            // Locations Not Inspected
            _locationsNotInspected.text = _locationString;
            _locationsCount.text = $"Count: {_locationCountString}";
        }

        /// <summary>
        /// Displayes the Inspection Summary and disables interactions
        /// </summary>
        public void HandleDisplay()
        {
            StartCoroutine(PopulateWindow());
            _root.style.display = DisplayStyle.Flex;
            StartCoroutine(DisableInteractions());
        }

        /// <summary>
        /// Gets the locations not visited count and formats the list of locations not visited.
        /// </summary>
        private void GetLocationsNotVisitedInfo()
        {

            List<string> locations = PHISceneManager.Instance.PoiManager.POIsNotVisited();

            locations.Sort();

            foreach (string location in locations)
            {
                _locationString += $"{location}\n";
            }

            _locationCountString = locations.Count.ToString();

            _informationCollected = true;
        }

        /// <summary>
        /// Coroutine that is used to disable interactions
        /// </summary>
        /// <returns></returns>
        private IEnumerator DisableInteractions()
        {
            Interactions.Instance.TurnOffUI();
            //This method needs 2 wait for updates to let the display buttons mouse leave event fixed update finish turning on interactions, then turn it off again on the next update
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Interactions.Instance.SetInteract(false);
        }
    }
}
