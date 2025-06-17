using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// The purpose of this class is to act as the "inbetween" the SaveData object, The cloudSave system, and the rest of the Unity project
    /// use this class to add data to the SaveData object, get data from the SaveData object, trigger saves and loads, etc.
    /// </summary>
    public class SaveDataSupport : MonoBehaviour
    {
        [SerializeField] private SaveData _saveData;

        public ICloudSaving CloudSave; //needs to be public to mock out

        [SerializeField] private InspectableManager _inspectableManager;

        public bool canSave;

        public UnityEvent OnLoad; //this is an event to trigger any methods when a successful load happens

        public UnityEvent OnInitialize;

        /// <summary> Called from <see cref="SaveMenu.HandleSaveModeResult(SaveMenu.SaveMode)"/>" </summary>
        public UnityEvent FreshLoad;


        /// <summary>
        /// <see cref="PlayerController.MoveOnLoad(string)"/>
        /// </summary>
        public UnityEvent<string> MovePlayer;

        /// <summary>
        /// <see cref="POIManager.OnLoadSetVisitedPOIs(List{string})"/>
        /// </summary>
        public UnityEvent<List<string>> onLoadVisitedPOIs;

        /// <summary>
        /// <see cref="ActivityLogManager.OnLoadSetLogs(List{PrimaryLog})"/>
        /// </summary>
        public UnityEvent<List<PrimaryLog>> onLoadLogs;

        /// <summary>
        /// <see cref="ScenarioManager.InitializeScenario(string)"/>
        /// </summary>
        public UnityEvent<string> onLoadScenario;

        // This is a static bool that tracks if the simulation has been restarted (Restart button or player has been forced to restart)
        public static bool Restarted = false;

        public static SaveDataSupport Singleton;

        private void Start()
        {
            Singleton = this;
            canSave = false;
            CloudSave ??= GetComponentInChildren<ICloudSaving>();
            // Initialize will be called via the scorm integrator in Web GL
#if UNITY_EDITOR
            Initialize();
#endif
        }

        /// <summary>
        /// This function is called when scorm integrator is initialized
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {
            OnInitialize?.Invoke();
        }

        //These methods are added here instead of directly calling cloud save so we can mock out the save system for testing.
        public void TriggerSave()
        {
            if (canSave)
            {
                SaveTimer();
                CloudSave.Save();
            }
        }

        /// <summary>
        /// This is called as the first event listener to <see cref="SaveDataSupport.FreshLoad"/> it will setup the application version in the fresh save data object
        /// </summary>
        public void SetupInitialData()
        {
            _saveData.Version = Application.version;
        }

        /// <summary>
        ///     This method will trigger a load from the cloud save system and then call the OnLoad event
        /// </summary>
        /// <returns></returns>
        public IEnumerator TriggerLoad()
        {
            CloudSave.Load();
            yield return new WaitUntil(() => CloudSave.LoadSuccess != null);
            if (CloudSave.LoadSuccess == true)
            {
                OnLoad?.Invoke();
            }
        }
        /// <summary>
        ///     Deletes the save file from the cloud save system
        /// </summary>
        public void TriggerDelete()
        {
            CloudSave.Delete();
        }

        /// <summary>
        ///     This method saves the current time from the TimerManager to the save data object
        /// </summary>
        public void SaveTimer()
        {
            TimeSpan ts = TimerManager.Instance.GetTimeSpan();
            _saveData.Time = ts;
        }

        /// <summary>
        ///    This method loads the time from the save data object into the TimerManager. Called when the sim is loading.
        /// </summary>
        public void LoadTimer()
        {
            TimeSpan ts = _saveData.Time;
            TimerManager.Instance.Offset = ts;
        }

        /// <summary>
        /// This method is called when a Photo is taken, it ensures that the photo list in the photo manager is added to the save data object
        /// and triggers a save.
        /// </summary>
        public void SavePhotos(Dictionary<String, InspectablePhoto> d)
        {
            Dictionary<string, string> tempList = new();
            foreach (KeyValuePair<string, InspectablePhoto> kvp in d)
            {
                tempList.Add(kvp.Key, kvp.Value.TimeStamp);
            }
            _saveData.PhotoIDsAndTime = tempList;
            TriggerSave();
        }


        /// <summary>
        /// This method will need to be called whenever the learner does an action that will be save to the activity log
        /// <see cref="ActivityLogManager.OnActivityLogChanged"/>
        /// </summary>
        public void SaveActivityLog(List<PrimaryLog> logs)
        {
            if (canSave == false)
            {
                return;
            }

            Dictionary<string, List<string>> tempDict = new();

            foreach (PrimaryLog primaryLog in logs)
            {
                List<string> tempList = new();
                tempDict.Add(primaryLog.ParentLog.LogContent, tempList);

                foreach (Log log in primaryLog.logs)
                {
                    tempDict[primaryLog.ParentLog.LogContent].Add(log.LogContent);
                }
            }

            _saveData.ActivityLogs = tempDict;
            TriggerSave();
        }

        /// <summary>
        /// this function is called when the end inspection button is confirmed. 
        /// this is for resetting the save file the next time it is loaded.
        /// <see cref="InspectionReview.EndInspection"/>
        /// </summary>
        public void SaveEndInspection()
        {
            _saveData.EndInspection = true;
            TriggerSave();
        }

        /// <summary>
        /// Gets the activity log info from the saved dictionary, then puts it back into the manager
        /// </summary>
        public void LoadActivityLog()
        {
            List<PrimaryLog> primaryLogList = new();

            foreach (KeyValuePair<string, List<string>> kvp in _saveData.ActivityLogs)
            {
                PrimaryLog primaryLog = new();
                primaryLog.ParentLog.LogContent = kvp.Key;
                primaryLogList.Add(primaryLog);
                primaryLog.logs = new();

                foreach (string log in kvp.Value)
                {
                    Log newLog = new()
                    {
                        LogContent = log
                    };
                    primaryLog.logs.Add(newLog);
                }
            }

            onLoadLogs?.Invoke(primaryLogList);
        }

        /// <summary>
        /// Called after all required introduction events have been completed, to
        /// allow the sim to start making cloud saves
        /// </summary>
        /// <param name="save"></param>
        public void SetCanSave(bool save)
        {
            canSave = save;
            if (save)
            {
                TriggerSave();
            }
        }

        /// <summary>
        /// Saves the list of inspections to the save data object and triggers a save
        /// </summary>
        public void SaveInspectionData()
        {
            Dictionary<string, InspectionDataSave> tempDict = new();

            foreach (KeyValuePair<string, Inspection> inspection in _inspectableManager.Inspections)
            {
                InspectionDataSave tempStruct = new InspectionDataSave();
                Dictionary<string, EvidenceDataSave> tempEvidenceDict = new Dictionary<string, EvidenceDataSave>();
                foreach (KeyValuePair<string, Evidence> evidence in inspection.Value.InspectionEvidences)
                {
                    EvidenceDataSave tempEvidence = new();
                    tempEvidence.ToolName = evidence.Value.ToolName;
                    tempEvidence.IsCompliant = evidence.Value.IsCompliant;
                    tempEvidence.Reading = evidence.Value.Reading;
                    tempEvidence.ToolLabel = evidence.Value.ToolLabel;
                    tempEvidenceDict.Add(evidence.Value.ToolName, tempEvidence);
                }
                tempStruct.InspectableObjectName = inspection.Value.InspectableObjectName;
                tempStruct.InspectionEvidences = tempEvidenceDict;
                tempDict.Add(inspection.Key, tempStruct);
            }

            _saveData.Inspections = tempDict;
            TriggerSave();
        }

        /// <summary>
        ///  Loads the list of inspections from the save data object into the inspectable manager
        /// </summary>
        public void LoadInspectionData()
        {
            Dictionary<string, Inspection> tempDict = new();

            foreach (KeyValuePair<string, InspectionDataSave> state in _saveData.Inspections)
            {
                Inspection tempStruct = new Inspection();
                Dictionary<string, Evidence> tempEvidenceDict = new Dictionary<string, Evidence>();
                foreach (KeyValuePair<string, EvidenceDataSave> evidence in state.Value.InspectionEvidences)
                {
                    Evidence tempEvidence = new();
                    tempEvidence.ToolName = evidence.Value.ToolName;
                    tempEvidence.IsCompliant = evidence.Value.IsCompliant;
                    tempEvidence.Reading = evidence.Value.Reading;
                    tempEvidence.ToolLabel = evidence.Value.ToolLabel;
                    tempEvidenceDict.Add(evidence.Value.ToolName, tempEvidence);
                }
                tempStruct.InspectableObjectName = state.Value.InspectableObjectName;
                tempStruct.InspectionEvidences = tempEvidenceDict;
                tempDict.Add(state.Key, tempStruct);
            }
            _inspectableManager.Inspections = tempDict;

        }

        /// <summary>
        /// Saves the current POI Name, generally called when they move to a new POI
        /// </summary>
        public void SavePlayerLocation(string name)
        {
            _saveData.LastPOI = name;
            TriggerSave();
        }

        /// <summary>
        /// This loads the POI target and passes it along to the player controller to move the player to the pois default spot.
        /// </summary>
        public void LoadPlayerLocation()
        {
            MovePlayer?.Invoke(_saveData.LastPOI);
        }


        /// <summary>
        ///     This method will save the list of visited POIs to the save data object and trigger a save
        /// </summary>
        public void SaveVisitedPOIs(List<string> pois)
        {
            _saveData.VisitedPOIs = pois;
            TriggerSave();
        }

        /// <summary>
        ///     This method will load the list of visited POIs from the save data object into the POI manager
        /// </summary>
        public void LoadVisitedPOIs()
        {
            onLoadVisitedPOIs?.Invoke(_saveData.VisitedPOIs);
        }

        /// <summary>
        /// Load images from the save data object back into the manager, and currently also writes to disk this is mostly for testing and may be removed at some point
        /// </summary>
        public void LoadPhotos()
        {
            foreach (KeyValuePair<string, string> kvp in _saveData.PhotoIDsAndTime)
            {
                //since each id is supposed to be unique find it in inspectable manager list
                InspectableObject obj = _inspectableManager.InspectableObjects.Find(x => x.InspectableObjectID == kvp.Key);
                obj.TakePhoto(kvp.Value);
            }
        }

        public string SaveFileVersion()
        {
            return _saveData.Version;
        }

        public bool SaveFileEndInspection()
        {
            return _saveData.EndInspection;
        }

        /// <summary>
        /// When a new scenario is loaded the scenario name gets saved.
        /// </summary>
        /// <param name="scenarioName"></param>
        public void SaveScenario(string scenarioName)
        {
            _saveData.ScenarioName = scenarioName;
        }

        /// <summary>
        /// Loades the saved scenario.
        /// </summary>
        public void LoadScenario()
        {
            onLoadScenario.Invoke(_saveData.ScenarioName);
        }

        public void ClearSaveData()
        {
            if (_saveData.PhotoIDsAndTime != null) _saveData.PhotoIDsAndTime.Clear();
            if (_saveData.ActivityLogs != null) _saveData.ActivityLogs.Clear();
            if (_saveData.Inspections != null) _saveData.Inspections.Clear();
            if (_saveData.LastPOI != null) _saveData.LastPOI = "Reception";
            _saveData.ScenarioName = null;
            _saveData.EndInspection = false;
            TriggerSave();
        }
    }
}
