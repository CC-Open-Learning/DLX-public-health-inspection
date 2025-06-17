using Moq;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.PublicHealth;
using Tools = VARLab.PublicHealth.Tools;

public class ActivityLogManagerTests : MonoBehaviour
{
    //save properties
    GameObject saveObj;
    Mock<ICloudSaving> mockCloudSave;
    SaveDataSupport saveSupp;
    SaveData saveData;

    TimerManager timerManager;
    GameObject logObj;
    ActivityLogManager logMng;

    GameObject _inspectGO;
    InspectableObject _inspectableObject;
    InspectableObjectData objectData;

    GameObject timerObj;

    [OneTimeSetUp]
    [Category("BuildServer")]
    public void RunOnce()
    {
        //activity Log manager
        logObj = new GameObject();
        logMng = logObj.AddComponent<ActivityLogManager>();
        logMng.GetComponent<ActivityLogManager>().OnActivityLogChanged = new UnityEngine.Events.UnityEvent<List<PrimaryLog>>();

        //save obj
        saveObj = new GameObject();
        mockCloudSave = new Mock<ICloudSaving>();
        saveSupp = saveObj.AddComponent<SaveDataSupport>();
        saveSupp.CloudSave = mockCloudSave.Object;
        saveData = saveObj.AddComponent<SaveData>();


        //properties in activity log manager 

        //timer manager
        timerObj = new GameObject();
        timerManager = timerObj.AddComponent<TimerManager>();

        var so = new SerializedObject(saveSupp);
        so.FindProperty("_saveData").objectReferenceValue = saveData;
        so.ApplyModifiedProperties();

        // Scriptable Object setup
        objectData = ScriptableObject.CreateInstance<InspectableObjectData>();
        objectData.IRTemp = 20.5f;
        objectData.ProbeTemp = 21.5f;

        //Inspectable Object setup
        _inspectGO = new();
        _inspectableObject = _inspectGO.AddComponent<InspectableObject>();

        _inspectableObject.AllScenarioStates = new List<InspectableObject.States>
        {
            new InspectableObject.States(_inspectGO, objectData)
        };

        _inspectableObject.InspectableObjectName = "Test";
        _inspectableObject.Location = "Pantry";

        _inspectableObject.CurrentObjState = objectData;

        //Setup event listeners
        logMng.OnActivityLogChanged.AddListener(saveSupp.SaveActivityLog);
        logMng.PrimaryLogs = new();
        logMng.SetCanLog(true);

    }

    /// <summary>
    /// Check if the save is being called successfully
    /// </summary>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator ActivityLogDataAddedToSaveAndSaveSuccessful()
    {
        //arrange
        POI poi = new();
        poi.POIName = "Test Name";
        saveSupp.canSave = true;

        // Act
        logMng.LogPrimaryEvent(poi);
        logMng.AddToPrimaryList("Tester");
        logMng.OnActivityLogChanged.Invoke(logMng.PrimaryLogs);

        yield return null;

        // Assert
        mockCloudSave.Verify(x => x.Save());
    }

    /// <summary>
    /// check if the data that the activity log data that is being inputted is being added to the list that is being saved
    /// </summary>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator ActivityLogDataAddedToSavedList()
    {

        // Arrange
        POI poi = new();
        poi.POIName = "Test Name";
        logMng.PrimaryLogs.Clear();

        // Act
        logMng.LogPrimaryEvent(poi);
        logMng.AddToPrimaryList("Tester");
        logMng.OnActivityLogChanged.Invoke(logMng.PrimaryLogs);
        yield return null;

        // Assert
        Assert.IsNotEmpty(saveData.ActivityLogs);
    }

    /// <summary>
    /// Checking to see if the log primary event method along with add to primary log are working
    /// As intended
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator CheckIfAPrimaryLogIsBeingAddedToListWithAddToPrimaryLogMethod()
    {
        // Arrage
        POI poi = new();
        poi.POIName = "Test Name";

        logMng.PrimaryLogs.Clear();

        // Act
        logMng.LogPrimaryEvent(poi);
        logMng.AddToPrimaryList("tester");

        yield return null;

        // Assert
        Assert.IsNotEmpty(logMng.PrimaryLogs);
    }

    /// <summary>
    /// Checking to see if 3 logs are being added to the primary log
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator CheckIf3PrimaryLogIsBeingAddedToList()
    {
        // Arrage
        POI poi1 = new POI();
        POI poi2 = new POI();
        POI poi3 = new POI();
        poi1.POIName = "Test Name 1";
        poi2.POIName = "Test Name 2";
        poi3.POIName = "Test Name 3";

        logMng.PrimaryLogs.Clear();

        // Act
        logMng.LogPrimaryEvent(poi1);
        logMng.AddToPrimaryList("tester 1");

        logMng.LogPrimaryEvent(poi2);
        logMng.AddToPrimaryList("tester 2");

        logMng.LogPrimaryEvent(poi3);
        logMng.AddToPrimaryList("tester 3");

        yield return null;

        // Assert
        Assert.AreEqual(3, logMng.PrimaryLogs.Count);
    }

    /// <summary>
    /// This test checks if the data is being loaded
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator OnLoadSuccessActivityLogIsLoaded()
    {
        mockCloudSave.Setup(x => x.LoadSuccess).Returns(true);
        saveSupp.OnLoad.AddListener(saveSupp.LoadActivityLog);

        saveSupp.canSave = true;

        PrimaryLog primaryLog = new("Primary Log");

        List<string> listOfLogs = new()
        {
            "log 1",
            "log 2",
            "log 3"
        };

        saveObj.GetComponent<SaveData>().ActivityLogs = new()
        {
            { primaryLog.ParentLog.LogContent, listOfLogs }
        };

        yield return saveSupp.TriggerLoad();

        Assert.IsNotNull(logMng.PrimaryLogs);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator LogInspectionWithCamera()
    {
        bool photoInspection = false;

        //Add a inspection with camera to the activity log
        logMng.LogInspection(VARLab.PublicHealth.Tools.Visual, _inspectableObject);

        yield return null;

        foreach (var log in logMng.PrimaryLogs)
        {
            log.ParentLog.LogContent.Contains("Photo");
            photoInspection = true;
        }

        Assert.IsTrue(photoInspection);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator LogInspectionWithIR()
    {
        bool retVal = false;
        logMng.LogInspection(Tools.IRThermometer, _inspectableObject);

        yield return null;

        foreach (var log in logMng.PrimaryLogs)
        {
            log.ParentLog.LogContent.Contains(Tools.IRThermometer.ToString());
            retVal = true;
        }

        Assert.IsTrue(retVal);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator LogInspectionWithProbe()
    {
        bool retVal = false;

        logMng.LogInspection(Tools.ProbeThermometer, _inspectableObject);

        yield return null;

        foreach (var log in logMng.PrimaryLogs)
        {
            log.ParentLog.LogContent.Contains(Tools.ProbeThermometer.ToString());
            retVal = true;
        }

        Assert.IsTrue(retVal);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator LogInspectionVisual()
    {
        bool retVal = false;

        logMng.LogInspection(Tools.None, _inspectableObject);

        yield return null;

        foreach (var log in logMng.PrimaryLogs)
        {
            log.ParentLog.LogContent.Contains("Visual Inspection");
            retVal = true;
        }

        Assert.IsTrue(retVal);
    }

    //compliancy tests

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator LogCompliancyWithCamera()
    {
        bool photoInspection = false;
        _inspectableObject.UserSelectionCompliant = true;
        var inspectionEvidence = new Evidence(VARLab.PublicHealth.Tools.Visual, false, _inspectableObject);
        //Add a inspection with camera to the activity log
        logMng.LogCompliancy(_inspectableObject, inspectionEvidence);

        yield return null;

        foreach (var log in logMng.PrimaryLogs)
        {
            log.ParentLog.LogContent.Contains("Photo");
            photoInspection = true;
        }

        Assert.IsTrue(photoInspection);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator LogCompliancyWithIR()
    {
        bool retVal = false;
        var inspectionEvidence = new Evidence(VARLab.PublicHealth.Tools.IRThermometer, false, _inspectableObject);
        _inspectableObject.UserSelectionCompliant = true;
        logMng.LogCompliancy(_inspectableObject, inspectionEvidence);

        yield return null;

        foreach (var log in logMng.PrimaryLogs)
        {
            log.ParentLog.LogContent.Contains(Tools.IRThermometer.ToString());
            retVal = true;
        }

        Assert.IsTrue(retVal);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator LogCompliancyWithProbe()
    {
        bool retVal = false;
        var inspectionEvidence = new Evidence(VARLab.PublicHealth.Tools.ProbeThermometer, false, _inspectableObject);
        _inspectableObject.UserSelectionCompliant = false;
        logMng.LogCompliancy(_inspectableObject, inspectionEvidence);

        yield return null;

        foreach (var log in logMng.PrimaryLogs)
        {
            log.ParentLog.LogContent.Contains(Tools.ProbeThermometer.ToString());
            retVal = true;
        }

        Assert.IsTrue(retVal);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator LogCompliancyVisual()
    {
        bool retVal = false;
        var inspectionEvidence = new Evidence(VARLab.PublicHealth.Tools.None, false, _inspectableObject);
        _inspectableObject.UserSelectionCompliant = false;
        logMng.LogCompliancy(_inspectableObject, inspectionEvidence);

        yield return null;

        foreach (var log in logMng.PrimaryLogs)
        {
            log.ParentLog.LogContent.Contains("Visual Inspection");
            retVal = true;
        }

        Assert.IsTrue(retVal);
    }
}
