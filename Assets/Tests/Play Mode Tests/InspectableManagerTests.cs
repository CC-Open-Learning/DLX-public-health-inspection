using Moq;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.PublicHealth;

/// <summary>
///     This file contains the testing for the inspectable manager. 
///     All the functionality of the inspectable manager is tested here.
/// </summary>
public class InspectableManagerTests
{
    //GameObjects 
    GameObject managerObject;
    GameObject imageManagerObj;
    GameObject InspectionWindowObj;
    GameObject baseInspectObjectOne;
    GameObject baseInspectObjectTwo;

    //Inspectable objects
    InspectableObject inspectableObjectOne;
    InspectableObject inspectableObjectTwo;

    //Scriptable objects
    InspectableObjectData scriptObj;
    InspectableObjectData scriptObj2;

    //save properties
    GameObject saveObj;
    Mock<ICloudSaving> mockCloudSave;
    SaveDataSupport saveSupp;

    [OneTimeSetUp]
    [Category("BuildServer")]
    public void OneTimeSetup()
    {
        //Remove any inspectable objects from previous tests
        var inspectableObjects = GameObject.FindObjectsOfType<InspectableObject>();
        foreach (var inspectableObject in inspectableObjects)
        {
            GameObject.DestroyImmediate(inspectableObject.gameObject);
        }
    }

    [SetUp]
    [Category("BuildServer")]
    public void RunBeforeEveryTest()
    {
        //manager object
        managerObject = new GameObject();
        var inspectManager = managerObject.AddComponent<InspectableManager>();
        managerObject.GetComponent<InspectableManager>().OnInspectionChanged = new UnityEngine.Events.UnityEvent();

        //img manager
        imageManagerObj = new GameObject();
        imageManagerObj.AddComponent<ImageManager>();
        imageManagerObj.GetComponent<ImageManager>().TempImageTaken = new UnityEngine.Events.UnityEvent<InspectablePhoto>();

        //inspection window
        InspectionWindowObj = new GameObject();
        InspectionWindowObj.AddComponent<InspectionWindow>();

        //Save system setup
        saveObj = new GameObject();

        //mock the cloud save
        mockCloudSave = new Mock<ICloudSaving>();

        //add the save data support
        saveSupp = saveObj.AddComponent<SaveDataSupport>();
        saveSupp.CloudSave = mockCloudSave.Object;

        //Set the save data support properties
        var so = new SerializedObject(saveObj.GetComponent<SaveDataSupport>());
        so.FindProperty("_saveData").objectReferenceValue = saveObj.AddComponent<SaveData>();
        so.FindProperty("_inspectableManager").objectReferenceValue = managerObject.GetComponent<InspectableManager>();
        so.ApplyModifiedProperties();


        //inspectable object to interact with 
        baseInspectObjectOne = new GameObject();
        inspectableObjectOne = baseInspectObjectOne.AddComponent<InspectableObject>();
        inspectableObjectOne.InspectableObjectName = "Test";
        inspectableObjectOne.UserSelectionCompliant = true;
        inspectableObjectOne.InspectionMade = new(); //for triggering save
        inspectableObjectOne.Location = "TestLocation";

        //scriptable setup
        scriptObj = ScriptableObject.CreateInstance<InspectableObjectData>();
        scriptObj.name = "Test";

        //create the list of the first inspectable
        inspectableObjectOne.AllScenarioStates = new List<InspectableObject.States>
        {
            new InspectableObject.States(baseInspectObjectOne, scriptObj)
        };

        //inspectable object to interact with 
        baseInspectObjectTwo = new GameObject();
        inspectableObjectTwo = baseInspectObjectTwo.AddComponent<InspectableObject>();
        inspectableObjectTwo.InspectableObjectName = "TestObject2";
        inspectableObjectTwo.InspectionMade = new();
        inspectableObjectTwo.Location = "TestLocation2";

        //scriptable setup
        scriptObj2 = ScriptableObject.CreateInstance<InspectableObjectData>();
        scriptObj2.name = "Test2";

        //create the list for the second inspectable
        inspectableObjectTwo.AllScenarioStates = new List<InspectableObject.States>
        {
            new InspectableObject.States(baseInspectObjectTwo, scriptObj2)
        };

        //Create the list
        inspectManager.InspectableObjects = new List<InspectableObject>
        {
            //Add to the list 
            inspectableObjectOne,
            inspectableObjectTwo
        };

        // Creating the scenario data scriptable object.
        ScenarioData scenarioData = ScriptableObject.CreateInstance<ScenarioData>();
        scenarioData.name = "Test";
        scenarioData.ScenarioName = "Test";
        scenarioData.InspectablesAndStates = new List<ScenarioData.Inspectable>
        {
            new(inspectableObjectOne.name, scriptObj.name),
            new(inspectableObjectTwo.name, scriptObj2.name)
        };

        //Generate the IDs for the inspectable objects in the list. 
        inspectManager.GenerateIDs();

        //initialize the current states of the objects
        inspectManager.InitializeInspectableObjects(scenarioData);

        managerObject.GetComponent<InspectableManager>().OnInspectionChanged.AddListener(saveSupp.SaveInspectionData);

        inspectManager.Inspections = new Dictionary<string, Inspection>();

        //Add an inspection
        inspectManager.OnInspectionCompleted(inspectableObjectOne, VARLab.PublicHealth.Tools.Visual);
    }


    /// <summary>
    /// This test will ensure that an inspection is replaced correctly
    /// </summary>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsureInspectionIsReplacedCorrectly()
    {
        //Arrange 
        var inspectableManager = managerObject.GetComponent<InspectableManager>();

        bool originalInspectionResult;
        bool modifiedInspectionResult;

        VARLab.PublicHealth.Tools tool = VARLab.PublicHealth.Tools.Visual;
        string toolUsed = VARLab.PublicHealth.Tools.Visual.ToString();

        //Act
        inspectableManager.OnInspectionCompleted(inspectableObjectOne, tool);
        yield return null;
        originalInspectionResult = inspectableManager.Inspections[inspectableObjectOne.InspectableObjectID].InspectionEvidences[toolUsed].IsCompliant;
        inspectableObjectOne.UserSelectionCompliant = false;
        //Replace the original inspectable object with the new one
        inspectableManager.OnInspectionCompleted(inspectableObjectOne, VARLab.PublicHealth.Tools.Visual);
        yield return null;
        modifiedInspectionResult = inspectableManager.Inspections[inspectableObjectOne.InspectableObjectID].InspectionEvidences[toolUsed].IsCompliant;


        //Assert
        Assert.IsTrue(originalInspectionResult != modifiedInspectionResult);
    }


    /// <summary>
    /// This test will ensure that an inspection is added to Inspections dictionary correctly
    /// </summary>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsureInspectionIsAddedToInspectionsCorrectly()
    {
        //Arrange 
        var inspectableManager = managerObject.GetComponent<InspectableManager>();
        VARLab.PublicHealth.Tools toolUsed = VARLab.PublicHealth.Tools.Visual;
        //Act
        inspectableManager.OnInspectionCompleted(inspectableObjectOne, toolUsed);

        yield return null;
        //Assert
        Assert.IsNotEmpty(inspectableManager.Inspections);
        Assert.IsTrue(inspectableManager.Inspections.ContainsKey(inspectableObjectOne.InspectableObjectID));
    }


    /// <summary>
    /// This test will ensure that an inspection is removed correctly
    /// </summary>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsureInspectionRemovedCorrectly()
    {
        //Arrange 
        var inspectableManager = managerObject.GetComponent<InspectableManager>();
        VARLab.PublicHealth.Tools tool = VARLab.PublicHealth.Tools.Visual;
        string toolUsed = VARLab.PublicHealth.Tools.Visual.ToString();

        inspectableManager.OnInspectionCompleted(inspectableObjectOne, tool);

        yield return null;

        //Act
        inspectableManager.RemoveInspection(inspectableObjectOne.InspectableObjectID, toolUsed);

        yield return null;
        //Assert
        Assert.IsEmpty(inspectableManager.Inspections);

    }


    /// <summary>
    /// This test will ensure that an evidence is added to the inspection correctly
    /// </summary>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsureEvidenceIsAddedToInspectionCorrectly()
    {
        //Arrange 
        var inspectableManager = managerObject.GetComponent<InspectableManager>();
        VARLab.PublicHealth.Tools toolUsed = VARLab.PublicHealth.Tools.Visual;
        VARLab.PublicHealth.Tools secondToolUsed = VARLab.PublicHealth.Tools.IRThermometer;
        //Act
        inspectableManager.OnInspectionCompleted(inspectableObjectOne, toolUsed);

        yield return null;

        inspectableManager.Inspections[inspectableObjectOne.InspectableObjectID].AddInspectionEvidence(secondToolUsed, true, inspectableObjectOne);

        yield return null;

        //Assert
        Assert.IsNotEmpty(inspectableManager.Inspections);
        Assert.IsTrue(inspectableManager.Inspections[inspectableObjectOne.InspectableObjectID].InspectionEvidences.Count > 1);
    }


    /// <summary>
    /// This test will ensure that an evidence is removed from the inspection correctly
    /// </summary>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsureEvidenceIsRemovedFromInspectionCorrectly()
    {
        //Arrange 
        var inspectableManager = managerObject.GetComponent<InspectableManager>();
        VARLab.PublicHealth.Tools toolUsed = VARLab.PublicHealth.Tools.Visual;
        //Act
        inspectableManager.OnInspectionCompleted(inspectableObjectOne, toolUsed);

        yield return null;

        inspectableManager.Inspections[inspectableObjectOne.InspectableObjectID].RemoveInspectionEvidence(toolUsed.ToString());

        yield return null;

        //Assert
        Assert.IsNotEmpty(inspectableManager.Inspections);
        Assert.IsTrue(inspectableManager.Inspections[inspectableObjectOne.InspectableObjectID].InspectionEvidences.Count == 0);
    }

    /// <summary>
    ///     This function will ensure that all the inspectable objects have a state set by the inspectable manager. 
    /// </summary>
    /// <returns></returns>
    [Test]
    [Category("BuildServer")]
    public void InspectableObjectHasCorrectProperties()
    {
        Assert.IsTrue(CheckAllInspectableObjectsForState());
    }

    /// <summary>
    ///     This test ensures the correct location is returned for an inspectable object
    /// </summary>
    [Test]
    [Category("BuildServer")]
    public void EnsureGetInspectionLocationReturnsCorrectLocation()
    {
        var inspectableManager = managerObject.GetComponent<InspectableManager>();

        string location = inspectableObjectOne.Location;
        string locationTwo = inspectableManager.GetInspectableObjectLocation(inspectableObjectOne.InspectableObjectID);

        Assert.AreEqual(location, locationTwo);
    }

    /// <summary>
    ///     This test ensures that two objects with the same name and different ID are added to the list
    /// </summary>
    [Test]
    [Category("BuildServer")]
    public void EnsureTwoObjectsWithTheSameNameAndDifferentIDAreAddedToTheList()
    {
        var inspectableManager = managerObject.GetComponent<InspectableManager>();

        //Make the names the same 
        inspectableObjectTwo.InspectableObjectName = inspectableObjectOne.InspectableObjectName;

        //generate the IDs
        inspectableManager.GenerateIDs();

        //update the list
        // This would ignore objects with the same name and same ID not allowing them to be added to the list and we would not be able to find them
        inspectableManager.UpdateInspectableObjectsList();

        //Find both objects in the list
        bool foundOne = false;
        bool foundTwo = false;

        for (int i = 0; i < inspectableManager.InspectableObjects.Count; i++)
        {
            if (inspectableManager.InspectableObjects[i].InspectableObjectName == inspectableObjectOne.InspectableObjectName)
            {
                foundOne = true;
            }

            if (inspectableManager.InspectableObjects[i].InspectableObjectName == inspectableObjectTwo.InspectableObjectName)
            {
                foundTwo = true;
            }
        }

        Assert.IsTrue(foundOne && foundTwo);
    }

    /// <summary>
    ///    This test ensures that two objects with the same name and same ID are not added to the list
    /// </summary>
    public void EnsureTwoObjectsWithTheSameNameAndSameIDAreNotAddedToTheList()
    {
        var inspectableManager = managerObject.GetComponent<InspectableManager>();

        //make the name of the second object the same as the first
        inspectableObjectTwo.InspectableObjectName = inspectableObjectOne.InspectableObjectID;
        inspectableObjectTwo.InspectableObjectID = inspectableObjectOne.InspectableObjectID;
        inspectableManager.UpdateInspectableObjectsList();

        //check if the object was added to the list
        for (int i = 0; i < inspectableManager.InspectableObjects.Count; i++)
        {
            int idCount = 0;
            if (inspectableManager.InspectableObjects[i].InspectableObjectID == inspectableObjectOne.InspectableObjectID)
            {
                idCount++;
            }

            if (idCount > 1)
            {
                Assert.Fail();
            }
        }
    }

    /// <summary>
    ///     This function will ensure that all the objects have a current state assigned
    /// </summary>
    /// <returns></returns>
    private bool CheckAllInspectableObjectsForState()
    {
        //result value
        bool res = true;
        //reference to the inspectable manager
        var inspectManager = managerObject.GetComponent<InspectableManager>();

        //Iterate for each element to be created. 
        foreach (InspectableObject obj in inspectManager.InspectableObjects)
        {
            //get the inspectable object component from the game object
            //var inspectableObject = obj.GetComponent<InspectableObject>();

            //check if the object state is null 
            if (obj.InspectableObjectID == null)
            {
                res = false;
                break;
            }
        }

        return res;
    }

}
