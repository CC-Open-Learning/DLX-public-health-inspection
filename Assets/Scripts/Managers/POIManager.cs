using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// POI Manager manages all the inspectables in the POI making them interactable.
    /// OnExitPOI should be called before the OnEnterPOI to ensure we always disable the correct inspectables and if some inspectables should remain
    /// interactable the will be enabled again on enter POI.
    /// </summary>
    public class POIManager : MonoBehaviour
    {
        /// <summary><see cref="SaveDataSupport.SavePlayerLocation"/>
        /// <see cref="AudioManager.TogglePoiSFX(string)"/> </summary>
        public UnityEvent<string> POIChanged; //this event is used to trigger things that happen when POI changes(like saving)

        /// <summary><see cref="SaveDataSupport.SaveVisitedPOIs"/> </summary>
        public UnityEvent<List<string>> OnPOIInteracted; //this event is used to trigger things that happen when an inspectable is interacted with

        /// <summary> <see cref="ActivityLogManager.LogPrimaryEvent(POI)"/></summary>
        public UnityEvent<POI> LogNewPrimary;

        /// <summary> <see cref="ActivityLogManager.AddToPrimaryList(string)"/></summary>
        public UnityEvent<string> AddLogToPrimary;

        /// <summary><see cref="Interactions.SetCurrentPOI(string)"/></summary>
        public UnityEvent<string> SetCurrentPOI;

        /// <summary><see cref="LayerManager.SetLayers(POI)"/></summary>
        public UnityEvent<POI> TargetClicked;

        [SerializeField, Tooltip("List of POIs")] private List<POI> _pOIs;

        [Tooltip("Reference to the Start/Current POI")] public POI CurrentPOI;

        public List<POI> POIs { get => _pOIs; }

        //this list will need to be saved to the cloud
        public List<string> VisitedPOIs { get; set; }

        //count of inspectable POIs
        public int InspectablePOIsCount { get; set; } = 0;

        // Enable the inspectables in the Start POI.
        private void Awake()
        {
            VisitedPOIs = new List<string>();

            //Call the event to set the current POI
            SetCurrentPOI?.Invoke(CurrentPOI.POIName);

            //Count the number of inspectable POIs
            foreach (POI inspectablePOI in POIs)
            {
                if (inspectablePOI.ContainsInspectable)
                {
                    InspectablePOIsCount++;
                }
            }

        }

        /// <summary>
        /// This function will get the POI for the target clicked and if it is in a different POI then the 
        /// _currentPOI it will manage the inspectables in both POIs and log the activity in the Activity Log.
        /// </summary>
        /// <param name="targetClicked">Target clicked by the learner</param>
        public void OnTargetClicked(GameObject targetClicked)
        {
            POI clickedPOI = null;

            foreach (POI poi in _pOIs)
            {
                if (poi != null && poi.TargetsInPOI.Find(p => p.gameObject == targetClicked))
                {
                    clickedPOI = poi;
                    TargetClicked?.Invoke(clickedPOI);
                    break;
                }
            }

            //check if poi has changed
            if (clickedPOI == CurrentPOI || clickedPOI == null)
            {
                return;
            }

            //invoke additive log of us leaving the poi
            AddLogToPrimary?.Invoke("Exited " + CurrentPOI.POIName);

            //change the poi
            CurrentPOI = clickedPOI;

            POIChanged?.Invoke(CurrentPOI.POIName);

            SetCurrentPOI?.Invoke(CurrentPOI.POIName);

            //invoke new log of us arriving at new POI
            LogNewPrimary?.Invoke(CurrentPOI);
        }

        /// <summary>
        /// create a listener for when an inspectable is interacted with and log it <see cref="InspectionWindow.POIInteractedEvent"/>
        /// </summary>
        /// <param name="poi"></param>
        public void OnInspectableInteracted(POI poi)
        {

            //Add the poi to the visited list if it is not already there
            if (!VisitedPOIs.Contains(poi.POIName))
            {
                poi.Interacted = true;
                VisitedPOIs.Add(poi.POIName);

                //Save the visited POIs to the cloud
                OnPOIInteracted?.Invoke(VisitedPOIs);
            }
        }

        /// <summary>
        /// Invoked from <see cref="SaveDataSupport.onLoadVisitedPOIs"/>
        /// Sets the visited POIs to Interacted.
        /// </summary>
        /// <param name="list"></param>
        public void OnLoadSetVisitedPOIs(List<string> list)
        {
            VisitedPOIs = list;
            List<string> invalidPOI = new();

            foreach (string poi in VisitedPOIs)
            {
                POI visited = _pOIs.Find(p => p.POIName == poi);

                if (visited != null)
                {
                    visited.Interacted = true;
                }
                else
                {
                    Debug.Log($"POI {poi} is not a valid POI");
                    invalidPOI.Add(poi);
                }
            }

            VisitedPOIs.RemoveAll(poi => invalidPOI.Contains(poi));
        }

        /// <summary>
        /// Gets a list of POIs not visited for the Inspection Summary
        /// </summary>
        /// <returns></returns>
        public List<string> POIsNotVisited()
        {
            List<string> list = new List<string>();

            foreach (POI poi in _pOIs)
            {
                if (poi.ContainsInspectable && !poi.Interacted)
                {
                    list.Add(poi.POIName);
                }
            }

            return list;
        }

    }
}