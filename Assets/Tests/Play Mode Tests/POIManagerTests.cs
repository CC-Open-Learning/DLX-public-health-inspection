using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VARLab.PublicHealth;

public class POIManagerTests
{
    GameObject saveObj;
    Mock<ICloudSaving> mockCloudSave;
    SaveDataSupport saveSupp;
    SaveData saveData;
    POIManager poiManager;
    InformationModalSO restartModal;

    private const string StartPOI = "Back Kitchen"; //POI1 in test scene is back kitchen

    /// <summary>
    /// Loading Scene for testing
    /// </summary>
    [OneTimeSetUp]
    [Category("BuildServer")]
    public void LoadScene()
    {
        SceneManager.LoadScene("POIManagerTestScene");
    }

    [SetUp]
    [Category("BuildServer")]
    public void RunBeforeEveryTest()
    {
        //save system this can't be in setup beccause the loadscene isn't finished and will return null to saveObj
        mockCloudSave = new Mock<ICloudSaving>();
        saveObj = new GameObject();
        saveSupp = saveObj.AddComponent<SaveDataSupport>();
        saveSupp.CloudSave = mockCloudSave.Object;
        saveData = saveObj.AddComponent<SaveData>();
        restartModal = ScriptableObject.CreateInstance("InformationModalSO") as InformationModalSO;

        var so = new SerializedObject(saveSupp);
        so.FindProperty("_saveData").objectReferenceValue = saveData;
        so.ApplyModifiedProperties();
    }

    /// <summary>
    /// Checking if the starting POI is the Dining Room
    /// </summary>
    [UnityTest, Order(1)]
    [Category("BuildServer")]
    public IEnumerator Start_POI_Is_Back_Kitchen_POI()
    {
        // Arrange
        poiManager = GameObject.Find("POIManager").GetComponent<POIManager>();

        yield return null;

        Debug.Log("READ THIS ONE: StartPOI-Current:" + poiManager.CurrentPOI.POIName);

        // Assert
        Assert.AreEqual(StartPOI, poiManager.CurrentPOI.POIName);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator SavedPOIIsLastClicked()
    {
        poiManager.CurrentPOI = GameObject.Find("POI1").GetComponent<POI>();
        poiManager.CurrentPOI.POIName = "Back Kitchen";

        // Arrage
        var Target = GameObject.Find("POI2").GetComponent<POI>();
        var POI = "Pantry"; //POI 2 in test scene is pantry

        poiManager.POIChanged.AddListener(saveSupp.SavePlayerLocation);
        // Act
        yield return null;

        poiManager.OnTargetClicked(Target.TargetsInPOI[0]);

        yield return null;

        // Assert
        Assert.AreEqual(POI, saveData.LastPOI);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator OnLoadPlayerPositionMoves()
    {
        // Arrange
        // variable to track player position
        PlayerController pc = GameObject.Find("Player").GetComponent<PlayerController>();

        // set the restart modal
        var so = new SerializedObject(pc.GetComponent<PlayerController>());
        so.FindProperty("_restartSO").objectReferenceValue = restartModal;
        so.ApplyModifiedProperties();

        // save system mock cloud save load success response
        mockCloudSave.Setup(x => x.LoadSuccess).Returns(true);

        saveSupp = GameObject.Find("saveDataTest").GetComponent<SaveDataSupport>();
        saveData = GameObject.Find("saveDataTest").GetComponent<SaveData>();
        saveSupp.CloudSave = mockCloudSave.Object;

        // POI manager to keep track of the current POI
        poiManager = GameObject.Find("POIManager").GetComponent<POIManager>();
        pc.SetIntroCompleted();

        // set the last POI to pantry
        string poiToMoveto = "Pantry";
        saveData.LastPOI = poiToMoveto;

        // call trigger load to load the player position
        yield return saveSupp.TriggerLoad();

        // wait again to make sure the player has moved
        yield return null;

        Assert.IsTrue(poiManager.CurrentPOI.POIName == poiToMoveto); //we dont mess with y here(vertical position doesn't matter to check really)
        Assert.IsTrue(Math.Truncate(pc.transform.position.x * 1000) == Math.Truncate(poiManager.CurrentPOI.TargetsInPOI[0].transform.position.x * 1000));
        Assert.IsTrue(Math.Truncate(pc.transform.position.z * 1000) == Math.Truncate(poiManager.CurrentPOI.TargetsInPOI[0].transform.position.z * 1000));

    }



    /// <summary>
    /// Checking if the current POI changes if a target in the kitchen is clicked.
    /// </summary>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator TargetClicked_IsKitchen_POI()
    {
        // Arrage
        var kitchenTarget = GameObject.Find("POI1").GetComponent<POI>();
        var kitchenPOI = "Back Kitchen";

        // Act
        poiManager.OnTargetClicked(kitchenTarget.TargetsInPOI[0]);

        yield return null;

        // Assert
        Assert.AreEqual(kitchenPOI, poiManager.CurrentPOI.POIName);
    }


    /// <summary>
    ///     The purpose of this test is to ensure that a POI is added to the visited list when the user interacts with it
    ///     This interaction will be simulated by calling the OnInspectableInteracted method in the POIManager
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsurePOIIsAddedToVisitedCorrectly()
    {
        poiManager.OnPOIInteracted.AddListener(saveSupp.SaveVisitedPOIs);

        var poi = GameObject.Find("POI1").GetComponent<POI>();
        var poiName = "Back Kitchen";

        //Act
        poiManager.OnInspectableInteracted(poi);

        yield return null;

        //Assert
        Assert.IsTrue(poiManager.VisitedPOIs.Contains(poiName));
        Assert.IsTrue(saveData.VisitedPOIs.Contains(poiName));
    }


    /// <summary>
    ///     The purpose of this test is to ensure that a POI is not added to the visited list if it is already in the list
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsurePOIIsNotAddedToVisitedIfAlreadyVisited()
    {
        //Arrange
        var poi = GameObject.Find("POI1").GetComponent<POI>();
        var poiName = "Back Kitchen";

        //Act
        poiManager.OnInspectableInteracted(poi);

        yield return null;

        //Assert
        Assert.IsTrue(poiManager.VisitedPOIs.Contains(poiName));

        //Act
        poiManager.OnInspectableInteracted(poi);


        //Assert
        Assert.IsTrue(poiManager.VisitedPOIs.Contains(poiName));
        Assert.IsTrue(poiManager.VisitedPOIs.Count == 1);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator OnLoadVisitedListIsPopulated()
    {
        //Arrange
        mockCloudSave.Setup(x => x.LoadSuccess).Returns(true);

        //Act
        saveSupp.OnLoad.AddListener(saveSupp.LoadVisitedPOIs);
        saveSupp.onLoadVisitedPOIs.AddListener(poiManager.OnLoadSetVisitedPOIs);
        var visitedList = saveObj.GetComponent<SaveData>().VisitedPOIs = new();

        //Add a visited poi to the visited list.
        visitedList.Add(poiManager.POIs[0].POIName);

        yield return saveSupp.TriggerLoad();

        //Assert 
        Assert.IsNotEmpty(poiManager.VisitedPOIs);
        Assert.AreEqual(visitedList, poiManager.VisitedPOIs);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator OnLoadVisitedListIsPopulatedAndUnvalidPOIsAreRemovedFromTheList()
    {
        //Arrange
        mockCloudSave.Setup(x => x.LoadSuccess).Returns(true);
        string testPOI = "Test POI";

        //Act
        saveSupp.OnLoad.RemoveAllListeners();
        saveSupp.OnLoad.AddListener(saveSupp.LoadVisitedPOIs);
        saveSupp.onLoadVisitedPOIs.AddListener(poiManager.OnLoadSetVisitedPOIs);
        var visitedList = saveObj.GetComponent<SaveData>().VisitedPOIs = new();

        //Add a visited poi to the visited list.
        visitedList.Add(poiManager.POIs[0].POIName);
        visitedList.Add(testPOI);

        yield return saveSupp.TriggerLoad();

        //Assert 
        Assert.IsNotEmpty(poiManager.VisitedPOIs);
        Assert.IsTrue(visitedList.Contains(testPOI) == false);
    }
}
