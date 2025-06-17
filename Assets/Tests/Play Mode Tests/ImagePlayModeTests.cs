using Moq;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.PublicHealth;

public class ImagePlayModeTests
{
    GameObject inspectableObj;
    GameObject cameraObj;
    GameObject imageManagerObj;
    GameObject saveObj;
    Mock<ICloudSaving> mockCloudSave;
    SaveDataSupport saveSupp;
    GameObject inspectableManagerObject;
    InspectableObjectData scriptObj;

    GameObject InspectionWindowObj;

    [SetUp]
    public void RunBeforeEveryTest()
    {
        //manager object
        inspectableManagerObject = new GameObject();
        var inspectManager = inspectableManagerObject.AddComponent<InspectableManager>();

        //inspection window
        InspectionWindowObj = new GameObject();
        InspectionWindowObj.AddComponent<InspectionWindow>();

        //img manager
        imageManagerObj = new GameObject();
        imageManagerObj.AddComponent<ImageManager>();
        imageManagerObj.GetComponent<ImageManager>().TempImageTaken = new UnityEngine.Events.UnityEvent<InspectablePhoto>();
        imageManagerObj.GetComponent<ImageManager>().SavePhotos = new UnityEngine.Events.UnityEvent<Dictionary<string, InspectablePhoto>>();

        //inspectable setup
        inspectableObj = new GameObject();
        inspectableObj.AddComponent<InspectableObject>();
        inspectableObj.GetComponent<InspectableObject>().InspectableObjectName = "Test";
        inspectableObj.GetComponent<InspectableObject>().Location = "Test Location";
        //Create the list
        inspectManager.InspectableObjects = new List<InspectableObject>
        {
            //Add to the list 
            inspectableObj.GetComponent<InspectableObject>()
        };

        //camera obj
        cameraObj = new GameObject();
        Camera cam = cameraObj.AddComponent<Camera>();
        cameraObj.transform.SetParent(inspectableObj.transform);

        //save obj
        saveObj = new GameObject();
        mockCloudSave = new Mock<ICloudSaving>();
        saveSupp = saveObj.AddComponent<SaveDataSupport>();
        saveSupp.CloudSave = mockCloudSave.Object;

        var so = new SerializedObject(saveObj.GetComponent<SaveDataSupport>());
        so.FindProperty("_saveData").objectReferenceValue = saveObj.AddComponent<SaveData>();
        so.FindProperty("_inspectableManager").objectReferenceValue = inspectManager;
        so.ApplyModifiedProperties();

        //reference setup
        so = new SerializedObject(inspectableObj.GetComponent<InspectableObject>());
        so.FindProperty("_cameraForPhoto").objectReferenceValue = cameraObj.GetComponent<Camera>();
        so.ApplyModifiedProperties();

        //creating an instance of a scriptable obj
        scriptObj = ScriptableObject.CreateInstance<InspectableObjectData>();
        scriptObj.name = "Test";

        inspectableObj.GetComponent<InspectableObject>().CurrentObjState = scriptObj;
        inspectableObj.GetComponent<InspectableObject>().AllScenarioStates = new List<InspectableObject.States>
        {
            new InspectableObject.States(inspectableObj, scriptObj)
        };

        inspectableObj.GetComponent<InspectableObject>().TakePhotoOfObj.AddListener(imageManagerObj.GetComponent<ImageManager>().TakePhotoForLoad);
        inspectableObj.GetComponent<InspectableObject>().InspectableObjectID = "Test Location_Test";
        inspectManager.GenerateIDs();
    }

    [UnityTest]
    public IEnumerator PhotoAddedToImageList()
    {
        //setup serialized fields
        InspectableObject inspectable = inspectableObj.GetComponent<InspectableObject>();
        inspectable.TakePhotoOfObj?.Invoke(inspectable, "time");

        yield return null;

        Assert.IsNotEmpty(imageManagerObj.GetComponent<ImageManager>().Photos);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator InspectableCameraIsDisabled()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;

        Assert.IsFalse(cameraObj.GetComponent<Camera>().enabled);
    }

    [UnityTest]
    public IEnumerator SaveIsCalledAfterPhotoTaken()
    {
        mockCloudSave.Setup(x => x.Save()).Verifiable();
        imageManagerObj.GetComponent<ImageManager>().SavePhotos.AddListener(saveSupp.SavePhotos);
        saveSupp.canSave = true;
        //act (take photo)
        InspectableObject inspectable = inspectableObj.GetComponent<InspectableObject>();
        inspectable.InspectionMade.AddListener(imageManagerObj.GetComponent<ImageManager>().TakePhotoForInspection);
        inspectable.InspectionMade?.Invoke(inspectable);

        yield return null;

        //Asess
        mockCloudSave.Verify(x => x.Save());
    }

    [UnityTest]
    public IEnumerator PhotoIsAddedToSaveDataObj()
    {
        imageManagerObj.GetComponent<ImageManager>().SavePhotos.AddListener(saveSupp.SavePhotos);
        imageManagerObj.AddComponent<TimerManager>(); //needed to add incase no other test files ran before this one so a timer manager exists, if other tests ran before this one the timer manager will destroy itself.
        //act (take photo)
        InspectableObject inspectable = inspectableObj.GetComponent<InspectableObject>();
        inspectable.InspectionMade.AddListener(imageManagerObj.GetComponent<ImageManager>().TakePhotoForInspection);
        inspectable.InspectionMade?.Invoke(inspectable);

        yield return null;

        Assert.IsNotEmpty(saveObj.GetComponent<SaveData>().PhotoIDsAndTime);
    }

    [UnityTest, Order(0)]
    public IEnumerator OnLoadSuccessImagesRetaken()
    {
        mockCloudSave.Setup(x => x.LoadSuccess).Returns(true);

        saveSupp.OnLoad.AddListener(saveSupp.LoadPhotos);
        saveObj.GetComponent<SaveData>().PhotoIDsAndTime = new()
        {
            { "None_Test", "time" }
        };

        yield return saveSupp.TriggerLoad();

        yield return new WaitForEndOfFrame();


        Assert.IsNotEmpty(imageManagerObj.GetComponent<ImageManager>().Photos);
    }

    [UnityTest]
    public IEnumerator OnLoadFailDoNothing()
    {
        mockCloudSave.Setup(x => x.LoadSuccess).Returns(false);

        saveSupp.OnLoad.AddListener(saveSupp.LoadPhotos);
        saveObj.GetComponent<SaveData>().PhotoIDsAndTime = new()
        {
            { "Test", "time" }
        };

        yield return saveSupp.TriggerLoad();

        Assert.IsEmpty(imageManagerObj.GetComponent<ImageManager>().Photos);
    }
}
