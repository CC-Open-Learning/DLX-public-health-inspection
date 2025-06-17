using System.Collections.Generic;
using UnityEngine;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class is used to manage the Points of Interest in the game. It will store the name of the POI and the targets in the POI.
    /// </summary>
    public class POI : MonoBehaviour
    {
        [HideInInspector]
        public string POIName;

        /// <summary>List of move point targets within the poi</summary>
        public List<GameObject> TargetsInPOI;

        [Tooltip("Look At target on POI Load")] public Transform LookAtTarget = null;

        //this is used to determine if the POI has been interacted with vs walked through.
        public bool Interacted;

        [SerializeField, Tooltip("Inspectable in POI")] public bool ContainsInspectable;

        [SerializeField, Tooltip("Select the POI name from the list")] private POIList.POIName _poiName;

        private void Start()
        {
            POIName = POIList.GetPOIName(_poiName.ToString());
            Interacted = false;
        }
    }
}
