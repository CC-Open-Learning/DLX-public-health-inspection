using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VARLab.PublicHealth;

/// <summary>
///     The purpose of this test is to ensure that the update inspectable list manual function is working as intended. 
/// </summary>
public class UpdateInspectableListTests
{
    /// <summary>
    ///     Inspectable manager parent object
    /// </summary>
    GameObject _managerObj;

    /// <summary>
    ///     Inspectable object parent 
    /// </summary>
    GameObject _inspectableObj;

    /// <summary>
    ///     Scriptable object that will be added to the inspectable object
    /// </summary>
    InspectableObjectData _scriptObj;

    /// <summary>
    ///    The ID of the inspectable object
    /// </summary>
    const string InspectableID = "Location_TestObject";


    /// <summary>
    ///     The setup is ran before every test
    /// </summary>
    [SetUp]
    public void RunBeforeEveryTest()
    {
        //create objects
        _managerObj = new GameObject();
        _inspectableObj = new GameObject();

        //assign components to the object
        var managerData = _managerObj.AddComponent<InspectableManager>();
        var inspectData = _inspectableObj.AddComponent<InspectableObject>();

        //set the name of the object
        _managerObj.name = "Inspectable Manager";

        //setup manager
        var so = new SerializedObject(_managerObj.GetComponent<InspectableManager>());
        so.ApplyModifiedProperties();

        //creating an instance of a scriptable obj
        _scriptObj = ScriptableObject.CreateInstance<InspectableObjectData>();
        _scriptObj.name = "Test";

        //create a list 
        List<InspectableObject.States> list = new();

        //assign the list
        inspectData.AllScenarioStates = list;

        //create a list 
        List<InspectableObject> managerList = new();

        //assign the list
        managerData.InspectableObjects = managerList;

        //add the scriptable object 
        inspectData.AllScenarioStates.Add(new InspectableObject.States(_inspectableObj, _scriptObj));
        inspectData.InspectableObjectName = "TestObject";
        inspectData.CurrentObjState = inspectData.AllScenarioStates[0].InspectableObjectState;
        inspectData.InspectableObjectID = InspectableID;

        //add the inspectable object to the manager list
        managerList.Add(inspectData);
    }

    /// <summary>
    ///     The purpose of the test is to ensure that object list in the inspectable manager is being updated when the function is called and not empty. 
    /// </summary>
    [Test]
    public void EnsureInspectableManagerListIsNotEmpty()
    {
        var im = _managerObj.GetComponent<InspectableManager>();
        im.UpdateInspectableObjectsList();

        // Use the Assert class to test conditions
        Assert.IsNotEmpty(_managerObj.GetComponent<InspectableManager>().InspectableObjects);
    }

    /// <summary>
    ///     The purpose of this test is to ensure that no duplicate IDs are added
    ///     This also indirectly tests the GenerateIDs function
    /// </summary>
    [Test]
    public void EnsureUniqueIDsAreAdded()
    {
        //inspectable object scripts 
        InspectableObjectData test1Script;
        InspectableObjectData test2Script;

        //setup duplicate object to be rejected 
        GameObject test1 = new();

        var test1Data = test1.AddComponent<InspectableObject>();
        test1Data.Location = "TestLocation1";
        test1Data.InspectableObjectID = "TestLocation1_TestObject";

        test1Script = ScriptableObject.CreateInstance<InspectableObjectData>();
        test1Script.name = "Test";

        //create a list 
        List<InspectableObject.States> test1list = new();
        test1Data.AllScenarioStates = test1list;

        //add the scriptable object 
        test1Data.AllScenarioStates.Add(new InspectableObject.States(test1, test1Script));
        test1Data.InspectableObjectName = "Unique1";


        //setup unique object to be accepted
        GameObject test2 = new GameObject();

        var test2Data = test2.AddComponent<InspectableObject>();
        //Location is needed to generate the ID
        test2Data.Location = "TestLocation2";
        test1Data.InspectableObjectID = "TestLocation2_Unique";
        test2Script = ScriptableObject.CreateInstance<InspectableObjectData>();
        test2Script.name = "Unique2";

        //create a list 
        List<InspectableObject.States> test2list = new();
        test2Data.AllScenarioStates = test2list;

        //add the scriptable object 
        test2Data.AllScenarioStates.Add(new InspectableObject.States(test2, test2Script));
        //Location is needed to generate the ID
        test2Data.InspectableObjectName = "Unique";

        //update the list with the new values
        var im = _managerObj.GetComponent<InspectableManager>();
        im.GenerateIDs();
        im.UpdateInspectableObjectsList();

        //check if the list contains the unique ID
        Assert.IsTrue(im.InspectableObjects.Contains(test2Data));
        Assert.IsTrue(im.InspectableObjects.Contains(test1Data));

    }

    /// <summary>
    ///     Ensures that a valid state is updated in the inspectable manager 
    /// </summary>
    [Test]
    public void EnsureInspectableObjectStateIsUpdated()
    {
        const string newValue = "Temp";
        InspectableManager im = _managerObj.GetComponent<InspectableManager>();
        //Create a second state for the inspectable object
        InspectableObjectData scriptObj = ScriptableObject.CreateInstance<InspectableObjectData>();
        scriptObj.name = newValue;
        //add the scriptable object
        im.InspectableObjects[0].AllScenarioStates.Add(new InspectableObject.States(im.InspectableObjects[0].gameObject, scriptObj));

        im.UpdateInspectableObjectState(InspectableID, newValue);

        Assert.IsTrue(im.InspectableObjects[0].CurrentObjState.name == newValue);
    }

    /// <summary>
    ///     Ensures that an invalid state is not updated and no error is thrown
    /// </summary>
    [Test]
    public void EnsureInspectableObjectStateIsNotUpdated()
    {
        //create a fake state
        const string Fake = "fake";

        //get the original value
        var im = _managerObj.GetComponent<InspectableManager>();
        string originalValue = im.InspectableObjects[0].InspectableObjectName;

        //update the state with the fake value
        im.UpdateInspectableObjectState(InspectableID, Fake);

        //check if the value wasn't updated and is the same as the original value
        Assert.IsTrue(im.InspectableObjects[0].InspectableObjectName == originalValue);
    }
}
