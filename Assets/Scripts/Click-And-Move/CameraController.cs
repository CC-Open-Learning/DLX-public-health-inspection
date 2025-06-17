using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VARLab.CORECinema;
using VARLab.Navigation.PointClick;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class will serve as the main script for the player view movement and camera control. 
    /// It controls where the player is looking during transitions between move points. 
    /// It also handles the click events for the camera with short and long clicks.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        //Properties
        [Tooltip("Transform for the camera to look at")] public Transform StartLookAt;

        [Tooltip("Transform for the starting move point")] public Transform StartMvPnt;

        [Tooltip("Called on a regular/short mouse click")] public UnityEvent ShortClick;

        [Tooltip("Called on a long mouse click")] public UnityEvent LongClick;

        [Tooltip("Bool to track if lookAt is complete")] public static bool LookAtComplete = true;

        /// <summary>The rotation of the camera on the X axis</summary>
        [HideInInspector] public float RotationX = 0f;

        /// <summary>The rotation of the camera on the Y axis</summary>
        [HideInInspector] public float RotationY = 0f;

        [SerializeField, Tooltip("Reference to the player + camera")] private PlayerController _player;

        [SerializeField, Tooltip("Duration (seconds) to consider a click as a long click")] private float _longClickDuration = 0.17f;

        [SerializeField, Tooltip("Reference to the Main Camera")] private CinemachineVirtualCamera _virtualCamera;

        /// <summary>Reference to the PointClickNavigation script. This is private as the reference is found in start</summary>
        private PointClickNavigation Nav;

        // Reference the the Cinemachine Brain
        private CinemachineBrain _brain;

        /// <summary>Enum to keep track of the mouse button states</summary>
        private enum MouseBtnState
        {
            Neutral,
            Down,
            Up
        }
        private MouseBtnState _mouseBtnState;

        /// <summary> This tracks how long the mouse has been pressed for </summary>
        private float _mouseBtnHoldTime = 0f;

        public void Start()
        {
            Nav = transform.parent.gameObject.GetComponent<PointClickNavigation>();
            if (Nav != null) Nav.LookAtTarget = StartLookAt;
        }

        /// <summary>This method will reset the look at point to null</summary>
        public void ResetLookAt()
        {
            Nav.LookAtTarget = null;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _mouseBtnState = MouseBtnState.Down;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _mouseBtnState = MouseBtnState.Up;
            }

            HandleMouseInput();
        }

        /// <summary>
        ///     This method will look at a point after a short delay
        /// </summary>
        /// <param name="point">The transform of the point we want to look at</param>
        /// <returns>IEnumerator</returns>
        private IEnumerator LookAtPoint(Transform point)
        {
            LookAtComplete = false;
            // wait for player to start walking
            yield return new WaitForSeconds(0.25f);

            // wait until player stops walking
            yield return new WaitUntil(() => Nav.IsPlayerWalking == false);

            // disable player panning
            Nav.EnableCameraPanAndZoom(false);
            // disable all navigation
            Nav.EnableNavigation(false);
            // move the camera to look at something
            Nav.LookAt(point);

            // wait for camera to recenter
            yield return new WaitUntil(() => Nav.PovCam.GetCinemachineComponent<CinemachinePOV>().m_HorizontalRecentering.m_enabled == false);
            LookAtComplete = true;
            // wait for next interaction
            yield return new WaitForSeconds(0.50f);
            // enable player panning
            Nav.EnableCameraPanAndZoom(true);
            // enable all navigation
            Nav.EnableNavigation(true);
        }

        public void LookAt(Transform transform)
        {
            StartCoroutine(LookAtPoint(transform));
        }


        /// <summary>
        ///     This method will handle the mouse input timing for short and long clicks.
        /// </summary>  
        private void HandleMouseInput()
        {
            if (_mouseBtnState == MouseBtnState.Down)
            {
                _mouseBtnHoldTime += Time.deltaTime;
            }

            else if (_mouseBtnState == MouseBtnState.Up)
            {
                if (_mouseBtnHoldTime > _longClickDuration)
                {
                    LongClick?.Invoke();
                }
                else
                {
                    ShortClick?.Invoke();
                }

                _mouseBtnHoldTime = 0f;
                _mouseBtnState = MouseBtnState.Neutral;
            }
        }


        /// <summary>
        ///     This method will reset the camera controller to its default position
        /// </summary>
        public void ResetCameraController()
        {
            RotationX = 0f;
            RotationY = 0f;
            transform.localEulerAngles = new Vector3(RotationX, RotationY, 0f);
        }

        public void HandleCameraOnLoad()
        {
            if (SaveDataSupport.Restarted)
            {
                StartCoroutine(HandleCameraOnLoadCoroutine());
                SaveDataSupport.Restarted = false;
            }
        }

        private IEnumerator HandleCameraOnLoadCoroutine()
        {
            _virtualCamera.gameObject.SetActive(false);
            yield return new WaitForEndOfFrame();
            _virtualCamera.gameObject.SetActive(true);
            Camera.main.GetComponent<CinemachineZoom>().enabled = true;
            Camera.main.GetComponent<CinemachineZoom>().OnApplicationFocus(true);
        }
    }
}
