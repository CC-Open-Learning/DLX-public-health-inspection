using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// The Activity Log Manager manages all the data from the player interacting with objects and areas around them
    /// This will store all this data into a list, that will be then saved to the cloud
    /// Each interaction will have a list of data variables that will be passed in and formatted to be displayed in the UI
    /// </summary>
    public class ActivityLogManager : MonoBehaviour, IActivityLogManager
    {
        /// <summary><see cref="SaveDataSupport.SaveActivityLog"/> </summary>
        public UnityEvent<List<PrimaryLog>> OnActivityLogChanged;

        /// <summary> List of all the primary logs</summary>
        public List<PrimaryLog> PrimaryLogs;

        /// <summary>This is the currently active "Drop down"</summary>
        private PrimaryLog _currentPrimaryLog;

        /// <summary>Flag to indicate when a log can be recorded</summary>
        private bool _canLog = false;

        private void Start()
        {
            PrimaryLogs = new();
        }

        /// <summary>
        /// This method sets up the system to log a primary event for that the user has done
        /// </summary>
        /// <param name="poi">The poi in which the log was made</param>
        public void LogPrimaryEvent(POI poi)
        {
            if (_canLog)
            {
                PrimaryLog primaryLog = new("Entered " + poi.POIName);
                PrimaryLogs.Add(primaryLog);
                _currentPrimaryLog = primaryLog;

                // Must be last, update the save
                OnActivityLogChanged?.Invoke(PrimaryLogs);
            }
        }

        /// <summary>
        ///     This method is used to add a new log to the primary list
        /// </summary>
        /// <param name="newLog">This is the string that is built out from<see cref="LogInspection"/>"</param>
        public void AddToPrimaryList(string newLog)
        {
            if (_canLog)
            {
                //Add new log to current primary log
                _currentPrimaryLog.logs.Add(new(newLog));

                // Must be last, saves the new log
                OnActivityLogChanged?.Invoke(PrimaryLogs);
            }
        }

        /// <summary>
        ///     Builds out the activity log for the inspection that was done. 
        ///     Invoked from <see cref="InspectionWindow.LogInspectionEvent"/>
        /// </summary>
        /// <param name="tools">Enum value of the tool used in the inspection</param>
        /// <param name="inspectableObject">The inspectable object that was inspected so the log can be built from the current obj state information</param>
        public void LogInspection(Tools tools, InspectableObject inspectableObject)
        {
            string str = tools.ConvertEnumToString() + " - " + inspectableObject.InspectableObjectName;

            switch (tools)
            {
                case Tools.Visual:
                    str = inspectableObject.InspectableObjectName + " - Visual Inspection ";
                    break;
                case Tools.IRThermometer:
                    str = str + " - Temperature " + inspectableObject.CurrentObjState.IRTemp.ToString("F1", CultureInfo.InvariantCulture) + "°C";
                    break;
                case Tools.ProbeThermometer:
                    str = str + " - Temperature " + inspectableObject.CurrentObjState.ProbeTemp.ToString("F1", CultureInfo.InvariantCulture) + "°C";
                    break;
                case Tools.None:
                    str = inspectableObject.InspectableObjectName + " - Photo Taken";
                    break;

                default: break;
            };

            //Add new log to current primary log
            AddToPrimaryList(str);
        }

        /// <summary>
        /// This is used to mark the logs of compliant and non compliant events
        /// Event listener. This is called when an inspection marked as compliant/non-compliant. <see cref="InspectableManager.LogCompliantEvent"/>
        /// </summary>
        /// <param name="tools">Enum value of the tool used in the inspection</param>
        /// <param name="inspectableObject">The inspectable object that was inspected so the log can be built from the current obj state information</param>
        public void LogCompliancy(InspectableObject inspectableObject, Evidence evidence)
        {
            string str = inspectableObject.InspectableObjectName;
            string compliant = evidence.IsCompliant ? "Compliant" : "Non-compliant";

            Tools tools = (Tools)Enum.Parse(typeof(Tools), evidence.ToolName);

            switch (tools)
            {
                case Tools.ProbeThermometer:
                    str = tools.ConvertEnumToString() + " - " + str + " - Temperature " + evidence.Reading + " - " + compliant;
                    break;
                case Tools.IRThermometer:
                    str = tools.ConvertEnumToString() + " - " + str + " - Temperature " + evidence.Reading + " - " + compliant;
                    break;
                case Tools.Visual:
                    if (inspectableObject.HasPhoto)
                    {
                        str = inspectableObject.InspectableObjectName + " - Visual Inspection with photo - " + compliant;
                    }
                    else
                    {
                        str = inspectableObject.InspectableObjectName + " - Visual Inspection - " + compliant;
                    }
                    break;
                default: break;
            };

            AddToPrimaryList(str);
        }

        /// <summary>
        ///     Invoked from <see cref="ImageManager.DeletePhoto(string)"/>
        ///     This method will log the deletion of a photo. 
        /// </summary>
        /// <param name="objName"> The name of the object that had its photo deleted </param>
        public void LogPhotoDeleted(string objName)
        {
            string log = $"Photo of {objName} was deleted";
            AddToPrimaryList(log);
        }

        /// <summary>
        ///     <see cref="InspectableManager.LogInspectionDeleted"/> is the event that has this as a listener
        /// </summary>
        /// <param name="objName"> The objects name </param>
        /// <param name="toolUsed"> The tool used in the inspection to delete </param>
        public void LogInspectionDeleted(string objName, string toolUsed)
        {
            string log = $"{toolUsed} inspection of {objName} was deleted";
            AddToPrimaryList(log);
        }

        /// <summary>
        ///     Invoked from <see cref="SaveDataSupport.onLoadLogs"/> 
        ///     This takes the logs from the save obj, re adds them to the manager, and calls 
        ///     a routine to append the "load simulation" logs after a wait time.
        /// </summary>
        /// <param name="logs"> The list of logs that was loaded </param>
        public void OnLoadSetLogs(List<PrimaryLog> logs)
        {
            // Retrieve saved logs
            PrimaryLogs = logs;

            StartCoroutine(LoadLogsRoutine());
        }

        /// <summary>
        ///     This method loads the logs from the save file and appends the "Simulation Loaded" log. 
        ///     This is done after a 1 second delay to allow the initial primary log to be set before the new one is added on load.
        /// </summary>
        /// <returns>IEnumerator</returns>
        private IEnumerator LoadLogsRoutine()
        {
            yield return new WaitForSeconds(1);
            if (PrimaryLogs.Count > 1)
            {
                // Extract previous last log
                string previousLog = PrimaryLogs.Last().ParentLog.LogContent;
                previousLog = previousLog.Substring(previousLog.IndexOf(" ") + 1);

                // Create simulation loaded log
                PrimaryLog loadLog = new("Simulation Loaded: " + previousLog);
                PrimaryLogs.Add(loadLog);
                _currentPrimaryLog = loadLog;
            }
            else
            {
                _currentPrimaryLog = PrimaryLogs[0];
            }
        }

        /// <summary>
        ///     Changes the flag that allows logs to be saved. 
        ///     If the count of primary logs is 0, a new primary log is created indicating the start of the inspection.
        /// </summary>
        /// <param name="canLog">What to set canLog to</param>
        public void SetCanLog(bool canLog)
        {
            _canLog = canLog;
            if (PrimaryLogs.Count == 0)
            {
                PrimaryLog primaryLog = new("Inspection Started");
                PrimaryLogs.Add(primaryLog);
                _currentPrimaryLog = primaryLog;
            }
        }
    }

    /// <summary>
    ///     Parent class of PrimaryLog, used to format a log with the global delta time variable
    /// </summary>
    public class Log
    {
        public string LogContent;

        public Log()
        {
        }

        public Log(string log)
        {
            this.LogContent = TimerManager.Instance.GetElapsedTime() + " " + log;
        }
    }

    /// <summary>
    ///     Helper class that is used to track the users primary events in the simulation
    /// </summary>
    public class PrimaryLog : Log
    {
        public Log ParentLog = new();
        public List<Log> logs = new();

        public PrimaryLog()
        {
        }

        public PrimaryLog(string log)
        {
            ParentLog = new(log);
        }
    }

}
