using Cinemachine;
using EPOOutline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Navigation.PointClick;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     This class handles the interactions between the player and the environment. It controls the player's ability to interact with the environment
    /// </summary>
    public class Interactions : MonoBehaviour
    {
        //Public Properties

        /// <summary>This is a static reference to the instance of the Interactions class</summary>
        public static Interactions Instance;

        /// <summary> <see cref="BlurBackground.Deactivate"/> </summary>
        public UnityEvent OnTurnOnUI;

        /// <summary> <see cref="BlurBackground.Activate"/> </summary>
        public UnityEvent OnTurnOffUI;

        /// <summary><see cref="PointClickNavigation.EnableCameraPanAndZoom"/></summary>
        public UnityEvent<bool> SetPanAndZoomAbility;

        /// <summary> <see cref="ModalPopupBuilder.HandleDisplayModal(ModalPopupSO)"/> </summary>
        public UnityEvent<InformationModalSO> ShowModalPopup;

        /// <summary>This is a reference to the review window that is used to display the review window</summary>
        public UIDocument ReviewWindow;

        /// <summary>This is a reference to the review button that is used to display the review window</summary>
        public Button ReviewBtn;

        /// <summary>This is a reference to the settings button that is used to display the review window</summary>
        public Button SettingsBtn;

        /// <summary>This is a reference to the slider that is used to display the timer</summary>
        public VisualElement Slider;

        /// <summary> This is a reference to the timer UI that is used to display the timer</summary>
        public UIDocument TimerUI;

        [Tooltip("Takes the pause behaviour found in the menu button game object")] public PauseBehaviour Pause;

        [Tooltip("Takes reference to the poi manager")] public POIManager PoiManager;

        [Tooltip("Takes reference to the cinemachine camera found in the point click navigation game object")] public CinemachineVirtualCamera CMCamera;

        /// <summary>This is a bool that is used to determine if the player can interact with the environment</summary>
        [HideInInspector] public bool CanInteract;

        /// <summary>
        ///     This bool is to deal with the UI flickering when the player is walking to the office in the intro. 
        ///     When false, the if check in <see cref="SetUIEnabled"/> will prevent the UI from enabling during the intro walk."
        /// </summary>
        private bool _introWalking = false;



        //Private Properties
        [SerializeField, Tooltip("The SO for player failing to introduce")] private InformationModalSO _restartSO;

        [SerializeField, Tooltip("The SO for player failing to wash hands")] private InformationModalSO _handwashFailSO;

        [SerializeField, Tooltip("Reference to the main camera from the player")] private Camera MainCamera;

        /// <summary>This is the current POI that the player is in</summary>
        private string _currentPOI;

        // string that tracks the ID of the Inspectable Object with raycast
        private string _currentObjID = null;

        private List<ObjectTooltips> _tooltips;

        //constants
        private const string SliderContainer = "SliderContainer";

        private const string InspectableTag = "InspectableObject";

        private const string ManagerTag = "Manager";

        private const string HandwashingTag = "HandwashingStation";

        private const string MovementTargetTag = "MovePoint";

        /// <summary>This is the current outlined item that the player is looking at</summary>
        private Outlinable _currentOutlinedItem;

        private void Awake()
        {
            Instance = this;
            CanInteract = true;

            _tooltips = new List<ObjectTooltips>();

            if (ReviewWindow != null && TimerUI != null)
            {
                Slider = TimerUI.rootVisualElement.Q(SliderContainer);
                ReviewBtn = ReviewWindow.rootVisualElement.Q<Button>("ReviewBtn");
                SettingsBtn = ReviewWindow.rootVisualElement.Q<Button>("SettingsBtn");
            }

            _currentPOI = PoiManager.CurrentPOI.ToString();
        }

        private void Start()
        {
            SetInteract(false);
        }

        void Update()
        {
            if (CanInteract)
            {
                RaycastCheck();
            }
        }

        /// <summary>
        /// This is called in update to control highlighting of objects
        /// </summary>
        private void RaycastCheck()
        {
            string tag;

            // Attempt the first raycast with an infinite distance for MovementTargetTag(waypoints).
            RaycastHit hitPoint = GetHitPoint(out tag, Mathf.Infinity);

            GameObject hit = null;

            ObjectTooltips currentTooltip;

            // If the raycast hits a waypoint marker's box collider
            if (hitPoint.collider != null && tag == MovementTargetTag)
            {
                // If the hit object has the 'ObjectTooltips' component and if the hit distance is greater than 1.5f, display the tooltip.
                // This prevents the tooltip from showing when the player is standing on the waypoint.
                if (hitPoint.collider.gameObject.TryGetComponent<ObjectTooltips>(out currentTooltip) && hitPoint.distance > 1.5f)
                {
                    ShowTooltip(currentTooltip);
                    _tooltips.Add(currentTooltip);
                }
                return;
            }

            // Perform the second raycast limited to the default distance for other interactable objects.
            hitPoint = GetHitPoint(out tag);

            // If the raycast hits something
            if (hitPoint.collider != null && !string.IsNullOrEmpty(tag))
            {
                switch (tag)
                {
                    case InspectableTag:
                        hit = HandleInspectableObject(hitPoint);
                        break;
                    case HandwashingTag:
                        hit = hitPoint.collider.gameObject;
                        break;
                    default:
                        DisableToolTip();
                        break;
                }
            }
            else
            {
                DisableToolTip();
            }

            if (hit != null)
            {
                Outline(hit.GetComponentInParent<Outlinable>());
            }
            else
            {
                DisableOutline();
            }
        }

        /// <summary>
        ///    This method will set the interact value to the value passed in
        /// </summary>
        /// <param name="interactValue">True: can interact, False: cannot interact</param>
        public void SetInteract(bool interactValue)
        {
            CanInteract = interactValue;

            if (_introWalking == true) return;

            if (PoiManager != null)
            {
                //set the move points to the interact value
                foreach (POI p in PoiManager.POIs)
                {
                    foreach (GameObject mvpnt in p.TargetsInPOI)
                    {
                        mvpnt.GetComponentInChildren<BoxCollider>().enabled = interactValue;
                    }
                }
            }

            if (!interactValue && _tooltips != null)
            {
                DisableToolTip();
            }

            SetPanAndZoomAbility?.Invoke(interactValue);
        }

        /// <summary>
        /// This method is called when a game object containd a tooltip to display it
        /// </summary>
        /// <param name="tooltip">Tooltip Object</param>
        private void ShowTooltip(ObjectTooltips tooltip, string objId = null)
        {
            if (tooltip.ShowFrom.Contains(PoiManager.CurrentPOI))
            {
                tooltip.MouseEnter();
                if (objId != null)
                {
                    _currentObjID = objId;
                }
            }
        }

        /// <summary>
        /// This is called on walk start to disable the tool tip that is currently shown
        /// Invoked from <see cref="PointClickNavigation.WalkStarted"/>
        /// </summary>
        public void DisableToolTip()
        {
            if (_tooltips != null)
            {
                foreach (ObjectTooltips tooltip in _tooltips)
                {
                    tooltip.MouseExit();
                }
                _currentObjID = null;
                _tooltips.Clear();
            }
        }

        /// <summary>
        ///     This method will raycast to see if the user is clicking on an inspectable object and return the game object
        ///     This will on return if the game object is in the current POI of the player
        /// </summary>
        /// <returns>GameObject: inspectable object that exists in the current poi of the player</returns>
        public GameObject HandleInspectableObject(RaycastHit hitPoint)
        {
            string objId = hitPoint.collider.gameObject.GetComponentInParent<InspectableObject>().InspectableObjectID;

            ObjectTooltips currentTooltip;

            if (_tooltips != null && _currentObjID != objId)
            {
                DisableToolTip();
            }

            if (hitPoint.collider.gameObject.TryGetComponent<ObjectTooltips>(out currentTooltip) && _currentObjID == null)
            {
                ShowTooltip(currentTooltip, objId);
                _tooltips.Add(currentTooltip);
            }
            //if the inspectable object is in the current POI
            if (InspectableInCurrentPOI(hitPoint))
            {
                //return the game object
                return hitPoint.collider.gameObject;
            }
            return null;
        }

        /// <summary>
        /// Checked if the inspectable object is the current POI
        /// </summary>
        /// <param name="hitPoint">raycast hit</param>
        /// <returns>True if the inspectable is in the current POI.</returns>
        private bool InspectableInCurrentPOI(RaycastHit hitPoint)
        {
            return hitPoint.collider.gameObject.GetComponentInParent<InspectableObject>().Location == _currentPOI;
        }

        /// <summary>
        /// This is added as a listener to the short click event in <see cref="CameraController"/> to only inspect on short clicks
        /// </summary>
        public void Inspect()
        {
            if (!CanInteract)
            {
                return;
            }

            RaycastHit hitpoint = GetHitPoint(out string tag);

            if (hitpoint.collider != null && tag == InspectableTag && InspectableInCurrentPOI(hitpoint))
            {
                //TODO : 12/03/2024 - There are no inspectables in the Reception POI, is this
                // first if clause required?
                if (!PlayerController.IntroCompleted)
                {
                    _restartSO.SetPrimaryAction(Fade.Singleton.SetFadeAction(() => PHISceneManager.Instance.RestartScene(), true));
                    SetInteract(false);
                    ShowModalPopup?.Invoke(_restartSO);
                }
                else if (!PlayerController.HasWashedHands)
                {
                    _handwashFailSO.SetPrimaryAction(Fade.Singleton.SetFadeAction(() => PHISceneManager.Instance.RestartScene(), true));
                    SetInteract(false);
                    ShowModalPopup?.Invoke(_handwashFailSO);
                }
                else if (InspectableInCurrentPOI(hitpoint))
                {
                    InspectableObject obj = hitpoint.collider.gameObject.GetComponentInParent<InspectableObject>();
                    obj.ToggleObjects();
                    obj.InspectionMade?.Invoke(obj); //this will have the listener to open the inspection window

                    DisableOutline();
                }
            }
        }

        /// <summary>
        /// This is pretty janky, but prevent raycasts that are fired on mouse down also firing off events when the buttons are clicked.
        /// </summary>
        public void ReEnableActions()
        {
            StartCoroutine(ReEnableActionsRoutine());
        }

        /// <summary>
        /// The co routine to re enable actions since the method needs to be called from non mono behaviours.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReEnableActionsRoutine()
        {
            yield return new WaitForFixedUpdate();
            Instance.SetInteract(true);
        }

        /// <summary>
        ///     This method will outline the object that the player is looking at
        /// </summary>
        /// <param name="_outliner">The outlinable component to toggle</param>
        private void Outline(Outlinable _outliner)
        {
            if (_outliner.enabled == false)
            {
                _outliner.enabled = true;
            }

            //if we're on a new item, turn off the old one
            if (_currentOutlinedItem != _outliner && _currentOutlinedItem != null)
            {
                _currentOutlinedItem.enabled = false;
            }

            //set the current item to the new item
            _currentOutlinedItem = _outliner;
        }

        /// <summary>
        ///    This method will disable the outline of the object that the player is looking at
        /// </summary>
        private void DisableOutline()
        {
            if (_currentOutlinedItem != null)
            {
                _currentOutlinedItem.enabled = false;
                _currentOutlinedItem = null;
            }
        }

        public RaycastHit GetHitPoint(out string tag, float distance = 3f)
        {
            // setting up a ray cast for when the player clicks
            Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            // getting the hitpoint for objects within the specified distance (default is 3 units)
            // this allows flexibility for detecting objects either nearby or far away, depending on the situation
            Physics.Raycast(ray, out RaycastHit hitPoint, distance);

            if (hitPoint.collider != null)
            {
                tag = hitPoint.collider.tag;
            }
            else
            {
                tag = null;
            }

            return hitPoint;
        }

        /// <summary>
        ///     This method will hide the UI elements for the review window and the timer
        /// </summary>
        public void TurnOffUI()
        {
            if (ReviewWindow != null && TimerUI != null)
            {
                ToggleUI(false);
                OnTurnOffUI?.Invoke();
            }
        }

        /// <summary>
        /// special case for turning off interactions on start up, the brain must be disabled after a period of time after start to not start
        /// looking at the back door and snap when the splash screen fades.
        /// </summary>
        public void StartUpInteract()
        {
            CanInteract = false;
            if (PoiManager != null)
            {
                //set the move points to the interact value
                foreach (POI p in PoiManager.POIs)
                {
                    foreach (GameObject mvpnt in p.TargetsInPOI)
                    {
                        mvpnt.GetComponentInChildren<BoxCollider>().enabled = false;
                    }
                }
            }
        }

        /// <summary>
        ///    This method will display UI elements for the review window and the timer
        /// </summary>
        public void TurnOnUI()
        {
            if (ReviewWindow != null && TimerUI != null)
            {
                ToggleUI(true);
                OnTurnOnUI?.Invoke();
            }
        }

        /// <summary>
        ///     Event listener for when the user clicks on a POI to set the current POI value to the clicked POI
        /// </summary>
        /// <param name="poi">name of the current poi</param>
        public void SetCurrentPOI(string poi)
        {
            _currentPOI = poi;
        }

        /// <summary>
        ///     Method for toggling the UI elements for the review window and the timer
        /// </summary>
        /// <param name="value"></param>
        private void ToggleUI(bool value)
        {
            ReviewBtn.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            SettingsBtn.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            Slider.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            Pause.PauseBtn.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        ///     This method will set the state of the UI elements for the review window and the timer
        ///     Invoked from <see cref="PointClickNavigation.WalkCompleted"/>
        /// </summary>
        /// <param name="value"></param>
        public void SetUIEnabled(bool value)
        {
            //Event needs to placed on the UI Button presses and not the walks. 
            //!ReviewBtn.enabledSelf is to ensure that the UI is not enabled during the intro walk when we go from move point to move point.
            if (_introWalking == true && !ReviewBtn.enabledSelf) return;

            ReviewBtn.SetEnabled(value);
            Slider.SetEnabled(value);
            Pause.PauseBtn.SetEnabled(value);
            SettingsBtn.SetEnabled(value);
        }

        /// <summary>
        ///     Called from <see cref="PlayerController.IntroWalkCompleted"/>
        ///     This will set the intro walking bool to stop the UI from enabling during the intro walk.
        /// </summary>
        public void IntroWalkCompleted()
        {
            _introWalking = false;
        }


        /// <summary>
        ///    Called from <see cref="PlayerController.IntroWalkStarted"/>
        ///    This will set the intro walking bool to allowing the UI and interactions.
        /// </summary>
        public void IntroWalkStarted()
        {
            _introWalking = true;
        }
    }
}

