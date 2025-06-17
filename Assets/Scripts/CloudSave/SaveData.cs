using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using VARLab.CloudSave;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class is the SaveData object which will store the information that we wish to save, SaveDataSupport is the class that should be 
    /// interacting with this class to move data between the PHI scene and into this object. Upon save triggers it will be serialized 
    /// then sent to the API.
    /// </summary>
    [CloudSaved]
    [JsonObject(MemberSerialization.OptIn)]
    public class SaveData : MonoBehaviour
    {
        //List of photos to save for now
        [JsonProperty]
        public Dictionary<string, string> PhotoIDsAndTime = new();

        [JsonProperty]
        public Dictionary<string, List<string>> ActivityLogs = new();

        [JsonProperty]
        public string LastPOI;

        [JsonProperty]
        public List<string> VisitedPOIs = new();

        [JsonProperty]
        public Dictionary<string, InspectionDataSave> Inspections = new();

        [JsonProperty]
        public string Version;

        [JsonProperty]
        public bool EndInspection = false;

        [JsonProperty]
        public TimeSpan Time;

        [JsonProperty]
        public string ScenarioName;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct InspectionDataSave
    {
        [JsonProperty] public string InspectableObjectName;

        [JsonProperty] public Dictionary<string, EvidenceDataSave> InspectionEvidences;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct EvidenceDataSave
    {
        [JsonProperty] public string ToolName;

        [JsonProperty] public string ToolLabel;

        [JsonProperty] public bool IsCompliant;

        [JsonProperty] public string Reading;
    }
}
