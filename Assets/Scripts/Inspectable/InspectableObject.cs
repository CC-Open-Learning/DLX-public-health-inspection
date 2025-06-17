using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     This is the class that is used to make objects inspectable. It contains all the information needed to inspect an object. 
    /// </summary>
    public class InspectableObject : MonoBehaviour
    {
        [HideInInspector] public UnityEvent<InspectableObject, string> TakePhotoOfObj; //might need to keep this for loading, to take pictures without inspections being made

        /// This is called when something is clicked with a tool active. These are set by the inspectable manager in <see cref="InspectableManager.InitializeInspectableObjects(ScenarioData)"/>
        [HideInInspector] public UnityEvent<InspectableObject> InspectionMade; //for now this is used to trigger save of inspectable states, later could be used to pass inspection information

        [Tooltip("This is a list of game objects to turn on/off when inspected")] public List<GameObject> Toggleables = new();
        [Tooltip("This is the bool to set if things should be toggled on/off")] public bool ShouldToggle = false;

        [Serializable]
        public struct States
        {
            public GameObject InspectableGameObject;
            public InspectableObjectData InspectableObjectState;

            public States(GameObject gameObject, InspectableObjectData state)
            {
                InspectableGameObject = gameObject;
                InspectableObjectState = state;
            }
        }

        [Tooltip("This list contains all the possible states an object can have")] public List<States> AllScenarioStates;

        [Tooltip("Current state of the object")] public InspectableObjectData CurrentObjState;

        [Tooltip("The name of the inspectable object")] public string InspectableObjectName;

        [HideInInspector, Tooltip("The ID of the inspectable object, This is set when using the inspectable manager generate ID button.")] public string InspectableObjectID;

        [HideInInspector, Tooltip("The location of the inspectable object")] public string Location;

        [SerializeField, Tooltip("Select a location from the list")] private POIList.POIName _location;

        [SerializeField, Tooltip("Virtual camera on the object")] private Camera _cameraForPhoto;

        [Tooltip("Reference to the handwashing station target")] public GameObject HandwashingStationTarget;

        public Camera CameraForPhoto { get { return _cameraForPhoto; } }

        public bool HasPhoto = false;

        //this is used to hold the user selection for the inspection, true: complaint false: noncompliant null: not inspected
        public bool? UserSelectionCompliant;

        /// <summary>
        ///     Ran at the first frame. Currently does some setup for the camera
        /// </summary>
        private void Start()
        {
            SetLocation();

            //this is to guard against forgetting to reference the inspectable associated camera in the editor, warning if a camera is not set as a child 
            _cameraForPhoto = GetComponentInChildren<Camera>(true);
            if (_cameraForPhoto == null)
            {
#if UNITY_EDITOR
                Debug.Log("No Camera set for an inspectable ");// + CurrentObjState.name);
#endif
                return;
            }
            //and a guard to prevent forgetting to turn off the cameras in the editor
            else if (_cameraForPhoto.enabled == true)
            {
                _cameraForPhoto.enabled = false;
            }
        }

        /// <summary>
        /// This method toggles the objects in the toggleables list setting their active state to the opposite value
        /// </summary>
        public void ToggleObjects()
        {
            if (ShouldToggle)
            {
                foreach (GameObject toggleable in Toggleables) toggleable.SetActive(!toggleable.activeSelf);
            }
        }

        /// <summary>
        /// This method sets the location of the inspectable object
        /// </summary>
        public void SetLocation()
        {
            Location = POIList.GetPOIName(_location.ToString());
        }

        /// <summary>
        /// place holder method to trigger photo event
        /// </summary>
        public void TakePhoto(string time)
        {
            TakePhotoOfObj?.Invoke(this, time);
        }

        /// <summary>
        ///    This method is used to get the names of all the scenarios that the object can be in
        /// </summary>
        /// <returns>List of strings containing the scenario names</returns>
        public List<string> GetAllScenarioNames()
        {
            List<string> names = new();
            if (AllScenarioStates == null) Debug.Log("states is null for: " + InspectableObjectID);

            foreach (var scenario in AllScenarioStates)
            {
                try
                {
                    names.Add(scenario.InspectableObjectState.name);
                }
                catch { Debug.Log("names.add is the fail point for: " + InspectableObjectID); }
            }

            return names;
        }

        /// <summary>
        ///   This method is used to generate the IDs of all the inspectable object
        /// </summary>
        public void GenerateID()
        {
            SetLocation();
            //if the location empty, do not generate ID and send debug to the user. 
            if ((Location == "") || (this.InspectableObjectName == ""))
            {
                Debug.Log("Invalid inspectable on " + this.InspectableObjectName + ", please set the location of the inspectable object");
                return;
            }

            //Using the location and the name of the object to create a unique ID
            InspectableObjectID = Location + "_" + InspectableObjectName;
        }
    }
}
