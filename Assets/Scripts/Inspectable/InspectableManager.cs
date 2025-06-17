using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     The purpose of this class is to handle all the inspectable object states and give a state to each object on load. 
    ///     This is done by using a seed value to determine which scenario is going to be used. Then the objects states that 
    ///     correlate with that scenario will be assigned. 
    /// </summary>
    public class InspectableManager : MonoBehaviour
    {
        //Events 
        /// <summary><see cref="ActivityLogManager.LogCompliancy(Tools, InspectableObject)"/> </summary>
        public UnityEvent<InspectableObject, Evidence> LogCompliantEvent;

        /// <summary>
        ///     <see cref="SaveDataSupport.SaveInspectionData"/>
        ///     <see cref="ProgressBarBuilder.UpdateNonComplianceText"/>
        ///     <see cref="InspectionReview.PrimeAlert"/>
        /// </summary>
        public UnityEvent OnInspectionChanged;

        /// <summary>
        ///     <see cref="InspectionWindow.OpenWindow(InspectableObject)"/>
        ///     <see cref="ImageManager.TakeTempPhoto(InspectableObject)"/>
        /// </summary>
        public UnityEvent<InspectableObject> OnInspectionMade;

        /// <summary> <see cref="ImageManager.DeletePhoto(string)"/></summary>
        public UnityEvent<string> OnInspectionDeleted;

        /// <summary><see cref="ActivityLogManager.LogInspectionDeleted(string, string)"/></summary>
        public UnityEvent<string, string> LogInspectionDeleted;

        /// <summary>This is a list of the game objects that have an inspectable component</summary> 
        public List<InspectableObject> InspectableObjects;

        /// <summary> This is a dictionary of the objects that have been inspected </summary>
        public Dictionary<string, Inspection> Inspections;


        /// <summary> This is a list of the game objects that need to have their inspectable object tag set at a different point in the sim </summary>
        public List<GameObject> ObjectsToAddTag;

        /// <summary> Image manager reference to dynamically add listener to objects</summary>
        public ImageManager ImageManagerObj;

        /// <summary> Awake is called before the first frame </summary>
        public void Start()
        {
            Inspections = new Dictionary<string, Inspection>();
        }


        /// <summary>
        ///     This function will set the state of the inspectable objects. This will be done by using a seed value to determine which state is used 
        /// </summary>
        public void InitializeInspectableObjects(ScenarioData data)
        {
            foreach (InspectableObject obj in InspectableObjects)
            {
                // Get the scenario state string for the object
                string name = data.InspectablesAndStates.Find(x => x.InspectableID == obj.InspectableObjectID).InspectableState;

                int scenarioIndex;

                // Get the scenario state
                if (obj.AllScenarioStates.Count <= 1)
                {
                    scenarioIndex = 0;
                }
                else
                {
                    scenarioIndex = obj.AllScenarioStates.FindIndex(x => x.InspectableObjectState.name == name);
                }

                // Assign the a state to the current object state
                obj.CurrentObjState = obj.AllScenarioStates[scenarioIndex].InspectableObjectState;

                //Set all the children game objects to inactive
                foreach (var go in obj.AllScenarioStates)
                {
                    go.InspectableGameObject.SetActive(false);
                }

                // Find the game object that matches the current state and make it active.
                foreach (var go in obj.AllScenarioStates)
                {
                    if (go.InspectableObjectState == obj.CurrentObjState)
                    {
                        go.InspectableGameObject.SetActive(true);
                    }
                }

                //Add the listener to the inspectable object
                obj.InspectionMade.AddListener((obj) => OnInspectionMade?.Invoke(obj));
                if (ImageManagerObj != null) //this way we can leave null if this is in a test for example that won't need to be coupled
                {
                    obj.TakePhotoOfObj.RemoveAllListeners();
                    obj.TakePhotoOfObj.AddListener(ImageManagerObj.TakePhotoForLoad);
                }
            }
        }

        /// <summary>
        ///     Added to the inspector as a button. 
        ///     This function finds all the inspectable objects in the scene and adds them to a list. The ID property of the inspectable object must be unique and 
        ///     duplicates will not be added and a message to the console notifying the user will be sent. 
        /// </summary>
        public void UpdateInspectableObjectsList()
        {

            //clear the list before updating
            InspectableObjects.Clear();

            //Find all the objects with the inspectable object component
            InspectableObject[] inspectableObjects = FindObjectsByType<InspectableObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (InspectableObject io in inspectableObjects)
            {
                //check if io has no name 
                if (String.IsNullOrWhiteSpace(io.InspectableObjectID))
                {
                    var poiName = io.Location;
                    Debug.Log("Inspectable object with no ID found in " + poiName + " " + io.name);
                    continue;
                }

                //If null assign a new list
                InspectableObjects ??= new List<InspectableObject>();

                //if the name exist already, log the error and continue 
                if (InspectableObjects.Exists(insObj => insObj.InspectableObjectID == io.InspectableObjectID))
                {
                    Debug.Log("Object: " + io.InspectableObjectID + " could not be added. Another object shares the same ID.");
                    continue;
                }

                //Add the object
                InspectableObjects.Add(io);
#if UNITY_EDITOR
                EditorUtility.SetDirty(io);
#endif
            }
        }

        /// <summary>
        ///     Update an inspectable object state by providing a key and value.
        /// </summary>
        /// <param name="inspectableObjectID"> The unique ID found in the inspectable object. </param>
        /// <param name="value"> The name of the new state to swap to </param>
        public void UpdateInspectableObjectState(string inspectableObjectID, string value)
        {
            //find the inspectable object with the key
            var io = InspectableObjects.Find(insObj => insObj.InspectableObjectID == inspectableObjectID);
            // Find the state with the value
            var state = io.AllScenarioStates.Find(state => state.InspectableObjectState.name == value);
            if (io != null && state.InspectableObjectState != null)
            {
                var selectedState = io.AllScenarioStates.Find(state => state.InspectableObjectState.name == value);
                io.CurrentObjState = selectedState.InspectableObjectState;
            }
            else
            {
                Debug.Log(inspectableObjectID + " is not a valid state for " + value);
            }
        }

        /// <summary>
        /// listener for <see cref="GalleryBuilder.DeletePhoto"/> to renable photo taking of an object.
        /// ReEnables the photo, then updates the Inspection Evidence to have no photo
        /// </summary>
        /// /// <param name="inspectableID"> name of object being updated </param>
        public void OnPhotoDeleted(string inspectableID)
        {
            InspectableObject changedObj = ReEnablePhoto(inspectableID);
            OnPhotoDeleteChangeInspection(changedObj, Tools.Visual);
        }

        /// <summary>
        /// ReEnables a photo for an inspectable object
        /// </summary>
        /// <param name="inspectableID"> ID of object to reenable photo on </param>
        public InspectableObject ReEnablePhoto(string inspectableID)
        {
            InspectableObject changedObj = null;
            foreach (InspectableObject obj in InspectableObjects)
            {
                if (obj.InspectableObjectID == inspectableID)
                {
                    obj.HasPhoto = false;
                    changedObj = obj;
                    break;
                }
            }

            return changedObj;
        }

        /// <summary>
        /// Triggered by inspection window when an item is marked as compliant or non-compliant
        /// <see cref="InspectionWindow.InspectionCompleted"/> to renable photo taking of an object
        /// </summary>
        public void OnInspectionCompleted(InspectableObject obj, Tools toolUsed)
        {
            string toolUsedName = toolUsed.ToString();
            Evidence inspectionEvidence;
            bool? previousInspectionCompliancy = PreviousInspectionCompliancy(obj.InspectableObjectID, toolUsedName);

            if (previousInspectionCompliancy.HasValue)
            {
                //Replace current inspection with new inspection
                if (!obj.UserSelectionCompliant.HasValue)
                {
                    obj.UserSelectionCompliant = previousInspectionCompliancy;
                }
                inspectionEvidence = new Evidence(toolUsed, (bool)obj.UserSelectionCompliant, obj);
                Inspections[obj.InspectableObjectID].InspectionEvidences[toolUsedName] = inspectionEvidence;
                LogCompliantEvent?.Invoke(obj, inspectionEvidence);
            }
            else
            {
                if (Inspections.ContainsKey(obj.InspectableObjectID))
                {
                    //Add evidence to the inspection
                    Inspections[obj.InspectableObjectID].AddInspectionEvidence(toolUsed, (bool)obj.UserSelectionCompliant, obj);
                }
                else
                {
                    //Add the inspection to the dictionary
                    Inspections.Add(obj.InspectableObjectID, new Inspection(toolUsed, (bool)obj.UserSelectionCompliant, obj));
                }
                inspectionEvidence = Inspections[obj.InspectableObjectID].InspectionEvidences[toolUsedName];
                LogCompliantEvent?.Invoke(obj, inspectionEvidence);

            }
            OnInspectionChanged?.Invoke();
        }

        /// <summary>
        /// Basically the OnInspectionCompleted method, but without logging compliancy
        /// </summary>
        public void OnPhotoDeleteChangeInspection(InspectableObject obj, Tools toolUsed)
        {
            string toolUsedName = toolUsed.ToString();
            Inspections[obj.InspectableObjectID].InspectionEvidences[toolUsedName].Reading = ToolStringsSO.LogPhotoUnavailableString;

            OnInspectionChanged?.Invoke();
        }

        /// <summary>
        /// Triggered by NonComplianceManager when a non-compliant inspection is deleted
        /// <see cref="GalleryBuilder.DeleteInspection"/> <see cref="InspectionLogBuilder.DeleteInspection"/>
        /// <param name="itemID"> The name of the item inspected </param>
        /// <param name="toolUsed"> The tool used for the inspection </param>
        /// </summary>
        public void RemoveInspection(string itemID, string toolUsed)
        {
            if (!Inspections.ContainsKey(itemID))
            {
                throw new ArgumentException("Inspection does not exist");
            }

            if (!Inspections[itemID].ContainsInspectionEvidence(toolUsed))
            {
                throw new ArgumentException("Inspection does not contain evidence for this tool");
            }

            Inspection inspection = Inspections[itemID];

            if (toolUsed == ToolStringsSO.LogVisualString &&
                inspection.InspectionEvidences[toolUsed].Reading == ToolStringsSO.LogPhotoAvailableString)
            {
                ReEnablePhoto(itemID);
                OnInspectionDeleted?.Invoke(itemID);
            }

            if (inspection.InspectionEvidences.Count == 1)
            {
                //Remove the inspection
                Inspections.Remove(itemID);
            }
            else
            {
                //Remove the evidence from the inspection
                Inspections[itemID].RemoveInspectionEvidence(toolUsed);
            }

            //trim the name from the ID. 
            string itemName = itemID.Substring(itemID.IndexOf('_') + 1);

            LogInspectionDeleted?.Invoke(itemName, toolUsed);

            OnInspectionChanged?.Invoke();
        }

        /// <summary>
        /// Check if a previous inspection exists with the same tool
        /// </summary>
        public bool? PreviousInspectionCompliancy(string inspectableId, string toolUsed)
        {
            if (!Inspections.ContainsKey(inspectableId))
            {
                return null;
            }

            if (!Inspections[inspectableId].ContainsInspectionEvidence(toolUsed))
            {
                return null;
            }

            return Inspections[inspectableId].InspectionEvidences[toolUsed].IsCompliant;
        }

        /// <summary>
        ///    This function will return the location of the inspectable object. 
        /// </summary>
        /// <param name="inspectableObjectID"></param>
        /// <returns></returns>
        public string GetInspectableObjectLocation(string inspectableObjectID)
        {
            if (!Inspections.ContainsKey(inspectableObjectID))
            {
                throw new ArgumentException("Inspection does not exist");
            }
            return InspectableObjects.Find(obj => obj.InspectableObjectID == inspectableObjectID).Location;
        }


        /// <summary>
        ///     This function will get the count of non-compliant inspections
        /// </summary>
        /// <returns> Returns the amount of non-compliances found</returns>
        public int GetNonComplianceCount()
        {
            int count = 0;
            foreach (var kvp in Inspections)
            {
                foreach (var evidence in kvp.Value.InspectionEvidences)
                {
                    if (evidence.Value.IsCompliant == false)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        ///     This function will get the count of compliant inspections
        /// </summary>
        /// <returns> Returns the amount of compliances found</returns>
        public int GetComplianceCount()
        {
            int count = 0;
            foreach (var kvp in Inspections)
            {
                foreach (var evidence in kvp.Value.InspectionEvidences)
                {
                    if (evidence.Value.IsCompliant == true)
                    {
                        count++;
                    }
                }
            }

            return count;
        }


        /// <summary>
        ///     This method will iterate through all the inspectable objects and call the GenerateID method on each object
        ///     This method is used to generate the ID for each inspectable object
        ///     -- The generate ID is the combination of the location and the name of the object with an underscore in between
        /// </summary>  
        public void GenerateIDs()
        {
            //find all the objects with the inspectable object components in the scene 
            InspectableObjects = new List<InspectableObject>(FindObjectsByType<InspectableObject>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            foreach (var item in InspectableObjects)
            {
                item.GenerateID();
#if UNITY_EDITOR
                EditorUtility.SetDirty(item);
#endif
            }
        }

        /// <summary>
        /// This method is used to add the tag of "InspectableObject" to enable certain objects after an event has happened.
        /// <see cref="PlayerController.HandwashEvent"/> and <see cref="SaveDataSupport.OnLoad"/> have this as listeners.
        /// </summary>
        public void AddTags()
        {
            foreach (GameObject obj in ObjectsToAddTag)
            {
                obj.tag = "InspectableObject";
            }
        }
    }

}
