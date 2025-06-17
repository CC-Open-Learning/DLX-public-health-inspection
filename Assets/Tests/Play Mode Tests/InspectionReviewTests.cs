using NUnit.Framework;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VARLab.PublicHealth;

public class InspectionReviewTests
{
    private InspectionReview _review;
    private UIDocument _uiDoc;

    [OneTimeSetUp]
    [Category("BuildServer")]
    public void RunOnce()
    {
        SceneManager.LoadScene("InspectionReviewTestScene");
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest, Order(0)]
    [Category("BuildServer")]
    public IEnumerator SceneLoaded()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return new WaitUntil(() => SceneManager.GetSceneByName("InspectionReviewTestScene").isLoaded); ;

        Assert.IsTrue(true);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator DisplayReview()
    {
        InspectionReview obj = GameObject.FindAnyObjectByType<InspectionReview>();
        InspectableManager mng = GameObject.FindAnyObjectByType<InspectableManager>();
        InspectableObject inspObj = GameObject.FindAnyObjectByType<InspectableObject>();
        UIDocument uIDocument = obj.GetComponent<UIDocument>();

        mng.InspectableObjects.Add(inspObj);


        inspObj.CurrentObjState = new() { ProbeTemp = 20 };
        inspObj.Location = "TestLocation";
        inspObj.name = "TestName";
        inspObj.UserSelectionCompliant = true;

        yield return null;

        mng.OnInspectionCompleted(inspObj, VARLab.PublicHealth.Tools.ProbeThermometer);

        yield return null;

        obj.DisplayWindow();

        Assert.IsTrue(uIDocument.rootVisualElement.style.display.ToString() == DisplayStyle.Flex.ToString());
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator TabClickTest()
    {
        InspectionReview obj = GameObject.FindAnyObjectByType<InspectionReview>();
        obj.ActivityBtn.TabClicked();
        yield return null;

        Assert.IsTrue(obj.ActivityBtn.Btn.ClassListContains("TabBtnSelected"));
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator DisplayActivity()
    {
        InspectionReview obj = GameObject.FindAnyObjectByType<InspectionReview>();
        ActivityLogManager mng = GameObject.FindAnyObjectByType<ActivityLogManager>();
        ActivityLogBuilder activityLog = obj.GetComponent<ActivityLogBuilder>();

        mng.LogPrimaryEvent(GameObject.FindAnyObjectByType<POI>());
        mng.AddToPrimaryList("Test");
        activityLog.BuildActivityLog();
        yield return null;

        Assert.IsTrue(activityLog._activityLogContainer != null);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator DisplayGallery()
    {
        InspectionReview obj = GameObject.FindAnyObjectByType<InspectionReview>();
        GalleryBuilder gallery = obj.GetComponent<GalleryBuilder>();

        InspectablePhoto photo = new(File.ReadAllBytes("Assets/Resources/Sprites/Non-CompliantIcon.png"), "testLocation_test", "testLocation", "testTime");
        GameObject.FindAnyObjectByType<ImageManager>().AddPhotoToGallery(photo);
        gallery.BuildGallery();

        yield return null;

        Assert.IsTrue(gallery._galleryContainer != null);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator NotificationAlertActive()
    {
        InspectionReview obj = GameObject.FindAnyObjectByType<InspectionReview>();
        UIDocument MenuUI = GameObject.Find("MenuButton").GetComponent<UIDocument>();
        Button reviewButton = MenuUI.rootVisualElement.Q<Button>("ReviewBtn");
        obj.StartAlertCoroutine();
        obj.PrimeAlert();
        StyleColor expectedActiveColor = new StyleColor(new Color(1, 0.8f, 0.2f));
        StyleColor expectedDeActivatedColor = new StyleColor(new Color(0, 0, 0));

        yield return null;
        Time.timeScale = 100;
        Assert.IsTrue(reviewButton.style.backgroundColor == expectedActiveColor, "Alert Active Color Correct.");
        obj.DisplayWindow();
        yield return new WaitForSeconds(2);
        Time.timeScale = 1;
        Assert.IsTrue(reviewButton.style.backgroundColor == expectedDeActivatedColor, "Alert DeActive Color Correct.");
    }
}
