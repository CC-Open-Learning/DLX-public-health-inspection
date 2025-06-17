using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VARLab.Analytics;
using VARLab.Navigation.PointClick;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This will act as the player movement manager for the player object, and will use movement points and raycast to move the player
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// Listeners:
        /// <see cref="SaveDataSupport.SetCanSave(bool)"/>
        /// <see cref="TimerManager.StartTimers"/>
        /// <see cref="ActivityLogManager.SetCanLog(bool)"/>
        /// <see cref="InspectableManager.AddTags"/>
        /// <see cref="InspectionReview.StartAlertCoroutine"/>
        /// </summary>
        public UnityEvent HandwashEvent;

        /// <summary>
        ///     Listener: <see cref="Interactions.IntroWalkCompleted"/>
        /// </summary>
        public UnityEvent IntroWalkCompleted;

        /// <summary>
        ///     Listener: <see cref="Interactions.IntroWalkStarted"/>
        /// </summary>
        public UnityEvent IntroWalkStarted;

        /// <summary>
        ///     Listener: <see cref="ResumePopup.DisplayWindow(string)"/>
        /// </summary>
        public UnityEvent<string> Continue;

        public AnalyticsEvent Channel;

        /// <summary>This is a static bool that will be used to check if the user has washed their hands, indicating that the inspection has begun</summary>
        public static bool HasWashedHands = false;

        [Tooltip("This is a reference the NavMeshAgent within the object")] public NavMeshAgent Player { get { return _player; } }

        [Tooltip("A bool representing if the user has introduced themselves")] public static bool IntroCompleted = false;

        [Tooltip("A bool that keeps track if the player has completed the walk.")] public static bool IsPlayerWalking = false;

        [SerializeField, Tooltip("Reference to the Introduction Handler")] private IntroductionHandler _introductionHandler;

        [Tooltip("Keeps track if player has visited the office")] public static bool VisitedOffice = false;

        [SerializeField, Tooltip("Reference to the player camera")] private Camera _cam;

        [SerializeField, Tooltip("Reference to the player")] private NavMeshAgent _player;

        [SerializeField, Tooltip("Reference to the POI Manager")] private POIManager _poiManager;

        [SerializeField, Tooltip("The scriptable object for the restart window")] private InformationModalSO _restartSO;

        [SerializeField, Tooltip("List of target to reach the office")] private List<GameObject> _targetsToOffice;

        [SerializeField, Tooltip("Reference to the point and click navigation")] private PointClickNavigation _pointAndClick;

        [SerializeField, Tooltip("Reference to the start POI")] private POI _startPOI;

        private CameraController _controller;

        //constants
        private const string MainScene = "Main Scene";

        private const string ManagerTag = "Manager";

        private const string HandwashingTag = "HandwashingStation";

        private const string Introduction = "Introduction";

        private void Awake()
        {
            _controller = GetComponent<CameraController>();
            Player.Warp(_startPOI.TargetsInPOI[1].transform.position);

#if UNITY_EDITOR
            _pointAndClick.CameraPanSensitivity = 2f;
#endif
        }

        private void Update()
        {
            if (_pointAndClick != null)
            {
                IsPlayerWalking = _pointAndClick.IsPlayerWalking;
            }
        }

        public void MoveToPOIDefault(POI poi, int index = 0, bool walk = false)
        {
            if (walk)
            {
                _pointAndClick.Walk(poi.TargetsInPOI[index].GetComponentInChildren<Waypoint>());
            }
            else
            {
                _player.Warp(poi.TargetsInPOI[index].transform.position);
                OnTargetReached(poi.TargetsInPOI[index].transform.gameObject, !walk);
            }

            if (poi.LookAtTarget != null)
            {
                CameraController.LookAtComplete = false;
                _controller.LookAt(poi.LookAtTarget);
            }
        }

        /// <summary>
        /// Update the camera pan sensitivity in the point and click navigation
        /// </summary>
        /// <param name="value">new value for camera pnan sensitivity</param>
        /// <see cref="SettingsMenuBuilder.mouseSensitivityChanged"/>
        public void UpdateCameraPanSensitivity(float value)
        {
            _pointAndClick.CameraPanSensitivity = value;
        }

        /// <summary>
        /// This will check of the user clicks on a moveable point, and will move their character and camera to that destination. 
        /// This entire functionality must be moved onto the interactions script
        /// Invoked from <see cref="PointClickNavigation.WalkCompleted"/>. Broken cref link. can be found in editor. MainScene > Master Components > PointClickNavigation > WalkCompleted.
        /// </summary>
        public void PlayerMoved(Waypoint newSpot)
        {
            OnTargetReached(newSpot.transform.parent.gameObject);
        }

        /// <summary>
        /// Trigger target actions when the player reaches the target
        /// </summary>
        /// <param name="movementTile">The movement tile game object</param>
        /// <param name="loadedFromSave">If it's been called from load</param>
        public void OnTargetReached(GameObject movementTile, bool loadedFromSave = false)
        {
            _poiManager.OnTargetClicked(movementTile);

            //If Introduction has not been completed, start the intro coroutine
            if (!IntroCompleted) StartCoroutine(_introductionHandler.HandleIntroduction(movementTile));

            if (!VisitedOffice && movementTile.TryGetComponent(out OfficeHandler officeHandler))
            {
                StartCoroutine(officeHandler.EnterOffice());
            }

            if (movementTile.TryGetComponent(out WashingHands washingHands) && !HasWashedHands && IntroCompleted)
            {
                HasWashedHands = true;
                StartCoroutine(HandWashEvent(washingHands));
            }
        }

        /// <summary>
        /// Runs when the player reaches a valid HandWashing target, allows 
        /// the player to start interacting with objects
        /// </summary>
        /// <param name="washingHands">Object with the WashingHands attribute to be tiggered</param>
        public IEnumerator HandWashEvent(WashingHands washingHands)
        {
            // wait until the player stops walking
            yield return new WaitUntil(() => _pointAndClick.IsPlayerWalking == false);
            Interactions.Instance.SetUIEnabled(false);
            _controller.LookAt(washingHands.HandwashingStation.transform);

            // wait for camera to look at the handwashing station
            yield return new WaitUntil(() => CameraController.LookAtComplete == true);
            yield return new WaitForSeconds(0.5f);

            washingHands.HandleHandWash();

            HandwashEvent?.Invoke();

            // Analitics
            CoreAnalytics.CustomEvent("Introduction_Completed", "", 0);
        }

        /// <summary>
        ///     Invoked from <see cref="Interactions.ResetSim"/>
        ///     and <see cref="Conversation.OnConversationEnded"/>
        /// </summary>
        /// <param name="boolValue">The value to set intro completed to</param>
        public void SetIntroBool(bool boolValue)
        {
            IntroCompleted = boolValue;
        }

        public void SetOfficeBool(bool boolValue)
        {
            VisitedOffice = boolValue;
        }

        /// <summary>
        ///     Invoked from <see cref="SaveDataSupport.OnLoad"/>
        /// </summary>
        public void SetIntroCompleted()
        {
            IntroCompleted = true;
            HasWashedHands = true;
            VisitedOffice = true;
        }

        /// <summary>
        ///     This method is called when the player is moved the the default location of a POI.
        ///     If the POI is not found, the method will return immediately.
        ///     
        ///     Invoked from <see cref="SaveDataSupport.MovePlayer"/>
        /// </summary>
        /// <param name="p">The name of the poi to move to</param>
        public void MoveOnLoad(string p)
        {
            StartCoroutine(MoveOnLoadCoroutine(p));
        }

        private IEnumerator MoveOnLoadCoroutine(string p)
        {
            POI target = _poiManager.POIs.Find(poi => poi.POIName == p);
            if (target == null)
            {
                yield break;
            }
            //move to the default location of the POI
            MoveToPOIDefault(target);

            if (target != null && target.LookAtTarget != null)
            {
                yield return new WaitUntil(() => CameraController.LookAtComplete == false);

                yield return new WaitUntil(() => CameraController.LookAtComplete == true);
            }

            // The Camera Controller LookAt waits 0.5 second to reenable the pan and zoom, so we need to wait a bit loger to display the window and 
            // disable the pan and zoom while the window is opened.
            yield return new WaitForSeconds(0.7f);

            Continue.Invoke(p);
        }

        public IEnumerator WaitForDeleteAndLoadScene()
        {
            yield return SaveDataSupport.Singleton.CloudSave._Delete();
            SceneManager.LoadScene(MainScene);
        }

        public void MoveToOffice()
        {
            if (!VisitedOffice)
            {
                StartCoroutine(MoveToOfficeCoroutine());
            }

        }
        private IEnumerator MoveToOfficeCoroutine()
        {
            //This is to ensure that the UI and interactions are disabled during the intro walk to the office
            IntroWalkStarted?.Invoke();

            foreach (GameObject target in _targetsToOffice)
            {
                int index = -1;
                POI pOI = null;

                // Identify POI and target index
                foreach (POI poi in _poiManager.POIs)
                {
                    index = poi.TargetsInPOI.FindIndex(p => p.transform.Equals(target.transform));
                    if (index > -1)
                    {
                        pOI = poi;
                        break;
                    }
                }

                MoveToPOIDefault(pOI, index, true);
                _poiManager.OnTargetClicked(target);
                yield return new WaitForFixedUpdate();
                yield return new WaitUntil(() => _pointAndClick.IsPlayerWalking == false);
            }

            //This is the end of the intro walk to the office and will re-enable the UI and interactions
            IntroWalkCompleted?.Invoke();
        }

        /// <summary>
        /// Player walks to the handwashing station by clicking on it in the introduction steps.
        /// <see cref="CameraController.ShortClick"/>
        /// </summary>
        public void WalkToHandwashingStation()
        {
            if (!Interactions.Instance.CanInteract)
            {
                return;
            }

            // Get the hitpoint of object clicked
            RaycastHit hitpoint = Interactions.Instance.GetHitPoint(out string tag);

            // Checks if the object hit with the raycast has a handwashing station tag and the player is not walking
            if (hitpoint.collider != null && tag == HandwashingTag && _pointAndClick.IsPlayerWalking == false)
            {
                int index = 0;

                // Get the inspectable object component of the handwashing sink clicked.
                InspectableObject obj = hitpoint.collider.gameObject.GetComponent<InspectableObject>();

                // Find the POI where the handwashing station is located
                POI poi = _poiManager.POIs.Find(p => p.POIName == obj.Location);

                // Get the index of the target in front of the handwashing station clicked
                if (obj.HandwashingStationTarget != null)
                {
                    index = poi.TargetsInPOI.FindIndex(t => t.transform == obj.HandwashingStationTarget.transform);
                }
                else Debug.Log($"Movement target for {obj.InspectableObjectName} not found, moving to POI Default");

                // walk to the handwashing station target
                MoveToPOIDefault(poi, index, true);
            }

        }
    }
}
