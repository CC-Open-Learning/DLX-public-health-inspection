using NUnit.Framework;
using UnityEngine;


/// <summary>
///     The purpose of this class in to run tests on the prefab for inspectable objects to ensure that the object is being created
///     with the correct children as well.
/// </summary>
public class InspectObjectIntegrationTest
{

    /// <summary>instance of the object</summary>
    private GameObject _prefabInstance;

    /// <summary>
    ///     This setup is going to run before every test and will give each test a new game object
    /// </summary>
    [SetUp]
    [Category("BuildServer")]
    public void RunBeforeEveryTest()
    {
        _prefabInstance = Resources.Load("InspectableObjectTest") as GameObject;
    }

    /// <summary>
    ///     The purpose of this test is to unsure that the object created from the prefab also has a child camera component
    /// </summary>
    [Test]
    [Category("BuildServer")]
    public void CheckCamera()
    {
        //get the camera component 
        Camera cam = _prefabInstance.GetComponentInChildren<Camera>();
        //check
        Assert.IsNotNull(cam);
    }

    /// <summary>
    ///     This tests will check if the object has been instantiated, then confirm that the object is interactable by setting that activity to false 
    /// </summary>
    [Test]
    [Category("BuildServer")]
    public void ObjectExists()
    {
        //check if the object was created 
        Assert.IsNotNull(_prefabInstance);

        //deactivate the game object
        _prefabInstance.SetActive(false);

        //check ensure that the game object was properly deactivated 
        Assert.AreEqual(_prefabInstance.activeSelf, false);
    }
}
