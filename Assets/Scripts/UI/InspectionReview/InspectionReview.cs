using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Analytics;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class Exists as a sort of manager for the UI related to the main inspection review window, it handles setting up all the elements callbacks/events, references, etc.
    /// </summary>
    public class InspectionReview : MonoBehaviour
    {
        [SerializeField] private InspectionReviewElementNamesSO _elementNames;
        //properties
        //analytics channel
        public AnalyticsEvent Channel;
        //tabs
        public Tab NonComplianceBtn;
        public Tab ActivityBtn;
        public Tab GalleryBtn;

        private List<Tab> _tabs;

        //Tab Content Area
        public VisualElement TabContent;

        //submit button
        private Button _endBtn;

        //close buttons
        private Button _topClose;
        private Button _inspectionReviewButton;

        [SerializeField, Tooltip("Gallery Builder Component")] private GalleryBuilder _galleryBuilder;

        [SerializeField, Tooltip("Activity Log Builder Component")] private ActivityLogBuilder _activityLogBuilder;

        [SerializeField, Tooltip("Inspection Log Builder Component")] private InspectionLogBuilder _inspectionLogBuilder;

        //UIBlocker
        UIBlocker _uiBlocker;

        // ProgressBarBuilder Component
        [SerializeField, Tooltip("Progress Bar Builder Component")] private ProgressBarBuilder _progressBarBuilder;

        [SerializeField, Tooltip("Inspection Review Window UIDocument Component")] private UIDocument _inspectionReviewDoc;

        [SerializeField, Tooltip("Menu Button UIDocument Component")] private UIDocument _toolbarDoc;

        /// <summary>
        /// DownloadEvents
        /// <see cref="InspectionSummaryBuilder.HandleDisplay"/>
        /// <see cref="DownloadAlert.DownloadReady"/>
        /// <see cref="SaveDataSupport.SaveEndInspection"/>
        /// </summary>
        public UnityEvent EndInspection;

        // End Inspection Events
        /// <summary><see cref="ModalPopupBuilder.HandleDisplayModal(ModalPopupSO)"/>/// </summary>
        public UnityEvent<ModalPopupSO> EndInspectionConfirmation;
        [SerializeField] private ModalPopupSO _endInspectionModal;// Confirmation dialog scriptable object
        public UnityEvent EndInspectionToggleEvent;

        // inspection review alert
        private StyleColor _yellowColor = new StyleColor(new Color(1, 0.8f, 0.2f));
        private StyleColor _blackColor = new StyleColor(new Color(0, 0, 0));
        private StyleColor _whiteColor = new StyleColor(new Color(1, 1, 1));
        private bool _alertActive = false;

        // Start is called before the first frame update
        void Start()
        {
            //get references
            TabContent = _inspectionReviewDoc.rootVisualElement.Q(_elementNames.TabContent);
            _inspectionReviewButton = _toolbarDoc.rootVisualElement.Q<Button>(_elementNames.ReviewBtn);
            NonComplianceBtn = new Tab(_inspectionReviewDoc.rootVisualElement.Q<Button>(_elementNames.NonComplianceTab), TabContent);
            ActivityBtn = new Tab(_inspectionReviewDoc.rootVisualElement.Q<Button>(_elementNames.ActivityTab), TabContent);
            GalleryBtn = new Tab(_inspectionReviewDoc.rootVisualElement.Q<Button>(_elementNames.GalleryTab), TabContent);
            _endBtn = _inspectionReviewDoc.rootVisualElement.Q<Button>(_elementNames.EndBtn);
            _topClose = _inspectionReviewDoc.rootVisualElement.Q<Button>(_elementNames.CloseBtn);

            _galleryBuilder = gameObject.GetComponent<GalleryBuilder>();
            _activityLogBuilder = gameObject.GetComponent<ActivityLogBuilder>();
            _inspectionLogBuilder = gameObject.GetComponent<InspectionLogBuilder>();
            _progressBarBuilder = gameObject.GetComponent<ProgressBarBuilder>();

            //setup master list of tabs
            _tabs = new List<Tab>
            {
                NonComplianceBtn,
                ActivityBtn,
                GalleryBtn
            };

            //setup the tab classes reference to master list
            foreach (Tab tab in _tabs)
            {
                tab.Tabs = _tabs;
            }

            //Register Blocking UI calls
            _uiBlocker = UIBlocker.Instance;//to avoid typing .instance over and over

            _uiBlocker.RegisterMouseEnterCallback(_inspectionReviewButton);
            _uiBlocker.RegisterMouseLeaveCallback(_inspectionReviewButton);

            //register click events
            _topClose.clicked += CloseWindow;
            _topClose.clicked += _galleryBuilder.DestroyTextures;
            _inspectionReviewButton.clicked += DisplayWindow;

            _endBtn.SetEnabled(false);
            _endBtn.clicked += () =>
            {
                _endInspectionModal.SetToggleAction((val) => EndInspectionToggleEvent.Invoke());
                _endInspectionModal.SetSecondaryToggleAction((val) => EndInspectionToggleEvent.Invoke());
                _endInspectionModal.SetPrimaryAction(Submit);// Set the primary action before showing the modal
                EndInspectionConfirmation.Invoke(_endInspectionModal);
            };

            //Gallery building on tab click event
            GalleryBtn.Btn.clicked += _galleryBuilder.ResetFlags;
            GalleryBtn.Btn.clicked += _galleryBuilder.BuildGallery;

            // Activity log building on tab click event
            ActivityBtn.Btn.clicked += _activityLogBuilder.BuildActivityLog;
            ActivityBtn.Btn.clicked += _galleryBuilder.DestroyTextures;

            // NonCompliance building on tab click event
            NonComplianceBtn.Btn.clicked += _inspectionLogBuilder.BuildInspectionLog;
            NonComplianceBtn.Btn.clicked += _galleryBuilder.DestroyTextures;


            //hide window until it is to be displayed
            //do not use CloseWindow() here as it will turn on interactions/ui when they should be off.
            TabContent.Clear();
            _inspectionReviewDoc.rootVisualElement.style.display = DisplayStyle.None;
            _progressBarBuilder.IsOpen = false;
        }

        /// <summary>
        /// This method is used to set the state of the window when it will be displayed and change the display style to actually display it.
        /// </summary>
        public void DisplayWindow()
        {
            _inspectionReviewDoc.rootVisualElement.style.display = DisplayStyle.Flex;
            TabContent.Clear();
            NonComplianceBtn.TabClicked();
            _inspectionLogBuilder.BuildInspectionLog();
            _progressBarBuilder.BuildProgressBar();
            _endBtn.SetEnabled(PlayerController.HasWashedHands);
            _alertActive = false;
            StartCoroutine(DisableInteractions());
        }

        private IEnumerator DisableInteractions()
        {
            Interactions.Instance.TurnOffUI();
            //This method needs 2 wait for updates to let the display buttons mouse leave event fixed update finish turning on interactions, then turn it off again on the next update
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Interactions.Instance.SetInteract(false);
        }

        /// <summary>
        /// sets the state of things for how things should be on close, and changes style to hide.
        /// </summary>
        public void CloseWindow()
        {
            TabContent.Clear();
            _inspectionReviewDoc.rootVisualElement.style.display = DisplayStyle.None;
            Interactions.Instance.ReEnableActions();
            _progressBarBuilder.IsOpen = false;
            Interactions.Instance.TurnOnUI();
        }

        /// <summary>
        /// Placeholder method for submission
        /// </summary>
        private void Submit()
        {
            // Stop the timer when the inspection is submitted
            TimerManager.Instance.PauseTimers();
            // Hide the inspection review window
            CloseWindow();
            CoreAnalytics.SendDLXCompletedEvent();
            CoreAnalytics.CustomEvent("Inspection_Complete", "", 0);
            EndInspection?.Invoke();

            //// If project is running in WebGL it will invoke the Download event to download files when submit is clicked.
            //DownloadEvent?.Invoke();
            //CoreAnalytics.CustomEvent("Downloaded_Report", "", 0);

        }

        /// <summary>
        /// This class exists within this page because it is specific to this window and the Tab/Button behaviour, The tabs need a refernce to all other tabs to shift the styling on them
        /// My solution to this was to create a "tab" class that holds the reference to the button and list of tabs with the onclick method built specifically for the tabs
        /// </summary>
        public class Tab
        {
            public Button Btn { get; private set; }

            public List<Tab> Tabs;

            const string TabOn = "TabBtnSelected";
            private readonly VisualElement TabContent;

            public Tab(Button btn, VisualElement tabContent)
            {
                Btn = btn;
                this.TabContent = tabContent;
                Btn.clicked += TabClicked;
            }
            /// <summary>
            /// This Method is the action that will happen when a tab is clicked, currently it only changes the styling of the tabs so they toggle appearance, in future
            /// the addition of a reference to the UIDocument which will be queried for "TabContent" 
            /// </summary>
            public void TabClicked()
            {
                TabContent.Clear();
                //turn off selected styling
                foreach (Tab Tab in Tabs)
                {
                    Tab.Btn.RemoveFromClassList(TabOn);
                    Tab.Btn.SetEnabled(true);
                }

                //turn on selected tab for this button
                foreach (Tab Tab in Tabs)
                {
                    if (Tab.Btn == Btn)
                    {
                        Tab.Btn.AddToClassList(TabOn);
                        Tab.Btn.SetEnabled(false);
                    }
                }
                /// Tab content is handled in individual files, <see cref="GalleryBuilder"/> for an example
            }

        }
        /// <summary>
        /// this is to start the coroutine BUT NOT THE NOTIFICATION. this is so we can decouple when the rountine is running from when the notification starts. <see cref="PlayerController.HandwashEvent"/>
        /// </summary>
        public void StartAlertCoroutine()
        {
            StartCoroutine(AlertFlash());
        }

        /// <summary>
        /// function to be used by external unity event to start notification. this is so it can be activated by the first inspection but not every inspection. <see cref="InspectableManager.OnInspectionChanged"/>
        /// </summary>
        public void PrimeAlert()
        {
            _alertActive = true;
        }

        /// <summary>
        /// coroutine to alternate colors on a 1 second delay. 1 second time is being used to avoid triggering epilepsy 
        /// </summary>
        /// <returns></returns>
        public IEnumerator AlertFlash()
        {
            yield return new WaitUntil(() => _alertActive == true);
            while (_alertActive == true)
            {
                SetReviewIconYellow();
                yield return new WaitForSeconds(1);
                SetReviewIconBlack();
                yield return new WaitForSeconds(1);
            }
        }

        /// <summary>
        /// quick function to change icon to white and black variant
        /// </summary>
        private void SetReviewIconBlack()
        {
            _inspectionReviewButton.style.backgroundColor = _blackColor;
            _inspectionReviewButton.style.unityBackgroundImageTintColor = _whiteColor;
            _inspectionReviewButton.style.borderTopColor = _whiteColor;
            _inspectionReviewButton.style.borderBottomColor = _whiteColor;
            _inspectionReviewButton.style.borderLeftColor = _whiteColor;
            _inspectionReviewButton.style.borderRightColor = _whiteColor;
        }

        /// <summary>
        /// quick function to change icon to black and yellow variant
        /// </summary>
        private void SetReviewIconYellow()
        {
            _inspectionReviewButton.style.backgroundColor = _yellowColor;
            _inspectionReviewButton.style.unityBackgroundImageTintColor = _blackColor;
            _inspectionReviewButton.style.borderTopColor = _blackColor;
            _inspectionReviewButton.style.borderBottomColor = _blackColor;
            _inspectionReviewButton.style.borderLeftColor = _blackColor;
            _inspectionReviewButton.style.borderRightColor = _blackColor;
        }
    }

}
