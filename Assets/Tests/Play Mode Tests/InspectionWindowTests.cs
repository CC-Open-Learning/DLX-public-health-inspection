using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VARLab.PublicHealth;

public class InspectionWindowTests
{
    #region Variables
    private UIDocument inspectionWindowElement;

    private InspectableObject inspectableObject;

    private InspectableManager inspectableManager;

    private InspectionWindow inspectionWindow;

    private VisualElement rootElement;

    private bool firstTimeSetUp = false;

    #endregion

    #region SetUp&TearDowns
    [OneTimeSetUp]
    [Category("BuildServer")]
    public void RunOnce()
    {
        SceneManager.LoadScene("InspectionReviewTestScene");
    }
    [UnitySetUp]
    [Category("BuildServer")]
    public IEnumerator SceneLoaded()
    {
        // unity one time setup work around
        if (!firstTimeSetUp)
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitUntil(() => SceneManager.GetSceneByName("InspectionReviewTestScene").isLoaded);
            inspectableManager = GameObject.FindAnyObjectByType<InspectableManager>();
            inspectionWindow = GameObject.FindAnyObjectByType<InspectionWindow>();
            inspectableObject = GameObject.FindAnyObjectByType<InspectableObject>();
            inspectionWindowElement = inspectionWindow.GetComponent<UIDocument>();
            rootElement = inspectionWindowElement.rootVisualElement;
            firstTimeSetUp = true;
        }
        // everytime set up goes below here
    }
    [TearDown]
    [Category("BuildServer")]
    public void Teardown()
    {
        inspectableManager.Inspections.Clear();
    }
    #endregion

    #region BasicTests
    /// <summary>
    /// This is a basic test for opening the inspection window
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator DisplayInspectionWindow()
    {
        // Arrange

        // Act
        inspectableManager.OnInspectionMade.Invoke(inspectableObject);
        yield return null;

        // Assert
        Assert.AreEqual((StyleEnum<DisplayStyle>)DisplayStyle.Flex, rootElement.style.display);
    }


    /// <summary>
    /// basic test for closing the inspection window
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator CloseInspectionWindow()
    {
        // Arrange
        inspectionWindow.OpenWindow(inspectableObject);
        yield return null;
        Button _closeBtn = rootElement.Q<Button>("CloseBtn");

        // Act
        inspectionWindow.CloseWindow();
        yield return null;

        // Assert
        Assert.AreEqual((StyleEnum<DisplayStyle>)DisplayStyle.None, rootElement.style.display);
    }

    [Test]
    [Category("BuildServer")]
    public void TakePhoto()
    {
        // Arrange
        openWindowSetup();

        // Act
        inspectionWindow.TakePhoto();


        // Assert
        Assert.AreEqual(VARLab.PublicHealth.Tools.Visual, inspectionWindow.SelectedTool);
    }
    #endregion

    #region DisplayTests
    [Test]
    [Category("BuildServer")]
    public void DisplayIRThermometer()
    {
        // Arrange
        openWindowSetup();

        // Act
        inspectionWindow.DisplayIRTemp();


        // Assert
        Assert.AreEqual(VARLab.PublicHealth.Tools.IRThermometer, inspectionWindow.SelectedTool);
    }

    [Test]
    [Category("BuildServer")]
    public void DisplayProbeThermometer()
    {
        // Arrange
        openWindowSetup();

        // Act
        inspectionWindow.DisplayProbeTemp();


        // Assert
        Assert.AreEqual(VARLab.PublicHealth.Tools.ProbeThermometer, inspectionWindow.SelectedTool);
    }


    [Test]
    [Category("BuildServer")]
    public void DisplayVisual()
    {
        // Arrange
        openWindowSetup();

        // Act
        inspectionWindow.DisplayIRTemp();
        inspectionWindow.DisplayVisual();


        // Assert
        Assert.AreEqual(VARLab.PublicHealth.Tools.Visual, inspectionWindow.SelectedTool);
    }
    #endregion

    #region SubmissionTests
    #region Compliant
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator CompliantVisual()
    {
        // Arrange
        openWindowSetup();
        InspectableObject temp = null;
        Evidence evidence = null;
        inspectableManager.LogCompliantEvent.AddListener((x, y) => { temp = x; evidence = y; });

        // Act
        inspectionWindow.TakePhoto();
        inspectionWindow.Compliant();
        yield return null;

        // Assert
        Assert.AreEqual(inspectableObject, temp);
        Assert.AreEqual(VARLab.PublicHealth.Tools.Visual.ToString(), evidence.ToolName);
        Assert.AreEqual(true, evidence.IsCompliant);
    }
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator CompliantIRThermometer()
    {
        // Arrange
        openWindowSetup();
        InspectableObject temp = null;
        Evidence evidence = null;
        inspectableManager.LogCompliantEvent.AddListener((x, y) => { temp = x; evidence = y; });

        // Act
        inspectionWindow.DisplayIRTemp();
        inspectionWindow.Compliant();
        yield return null;

        // Assert
        Assert.AreEqual(inspectableObject, temp);
        Assert.AreEqual(VARLab.PublicHealth.Tools.IRThermometer.ToString(), evidence.ToolName);
        Assert.AreEqual(true, evidence.IsCompliant);
    }
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator CompliantProbeThermometer()
    {
        // Arrange
        openWindowSetup();
        InspectableObject temp = null;
        Evidence evidence = null;
        inspectableManager.LogCompliantEvent.AddListener((x, y) => { temp = x; evidence = y; });

        // Act
        inspectionWindow.DisplayProbeTemp();
        inspectionWindow.Compliant();
        yield return null;

        // Assert
        Assert.AreEqual(inspectableObject, temp);
        Assert.AreEqual(VARLab.PublicHealth.Tools.ProbeThermometer.ToString(), evidence.ToolName);
        Assert.AreEqual(true, evidence.IsCompliant);
    }
    #endregion

    #region NonCompliant

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator NonCompliantVisual()
    {
        // Arrange
        openWindowSetup();
        InspectableObject temp = null;
        Evidence evidence = null;
        inspectableManager.LogCompliantEvent.AddListener((x, y) => { temp = x; evidence = y; });

        // Act
        inspectionWindow.TakePhoto();
        inspectionWindow.NonCompliant();
        yield return null;

        // Assert
        Assert.AreEqual(inspectableObject, temp);
        Assert.AreEqual(VARLab.PublicHealth.Tools.Visual.ToString(), evidence.ToolName);
        Assert.AreEqual(false, evidence.IsCompliant);
    }
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator NonCompliantIRThermometer()
    {
        // Arrange
        inspectionWindow.OpenWindow(inspectableObject);
        yield return null;
        InspectableObject temp = null;
        Evidence evidence = null;
        inspectableManager.LogCompliantEvent.AddListener((x, y) => { temp = x; evidence = y; });

        // Act
        inspectionWindow.DisplayIRTemp();
        inspectionWindow.NonCompliant();
        yield return null;

        // Assert
        Assert.AreEqual(inspectableObject, temp);
        Assert.AreEqual(VARLab.PublicHealth.Tools.IRThermometer.ToString(), evidence.ToolName);
        Assert.AreEqual(false, evidence.IsCompliant);
    }
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator NonCompliantProbeThermometer()
    {
        // Arrange
        inspectionWindow.OpenWindow(inspectableObject);
        yield return null;
        InspectableObject temp = null;
        Evidence evidence = null;
        inspectableManager.LogCompliantEvent.AddListener((x, y) => { temp = x; evidence = y; });

        // Act
        inspectionWindow.DisplayProbeTemp();
        yield return null;
        inspectionWindow.NonCompliant();
        yield return null;

        // Assert
        Assert.AreEqual(inspectableObject, temp);
        Assert.AreEqual(VARLab.PublicHealth.Tools.ProbeThermometer.ToString(), evidence.ToolName);
        Assert.AreEqual(false, evidence.IsCompliant);
    }
    #endregion

    #region ComplexSubmission
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator TakeTwoPhotosCompliant()
    {
        // Arrange
        openWindowSetup();
        bool InvokedAssert = false;
        inspectionWindow.CreateToast.AddListener((x, y, z) => InvokedAssert = z);

        // Act
        inspectionWindow.TakePhoto();
        inspectionWindow.Compliant();
        yield return null;
        inspectionWindow.OpenWindow(inspectableObject);
        yield return null;
        inspectionWindow.TakePhoto();
        inspectionWindow.Compliant();
        yield return null;


        // Assert
        Assert.IsTrue(InvokedAssert);
    }
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator TakeTwoPhotosNonCompliant()
    {
        // Arrange
        inspectionWindow.OpenWindow(inspectableObject);
        yield return null;
        bool InvokedAssert = false;
        inspectionWindow.CreateToast.AddListener((x, y, z) => InvokedAssert = z);

        // Act
        inspectionWindow.TakePhoto();
        inspectionWindow.NonCompliant();
        yield return null;
        inspectionWindow.OpenWindow(inspectableObject);
        yield return null;
        inspectionWindow.TakePhoto();
        inspectionWindow.NonCompliant();
        yield return null;


        // Assert
        Assert.IsTrue(InvokedAssert);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator TakePhotoAndSwitch()
    {
        // Arrange
        inspectionWindow.OpenWindow(inspectableObject);
        yield return null;
        ModalPopupSO temp = null;
        inspectionWindow.CreateModal.AddListener((x) => temp = x);

        // Act
        inspectionWindow.TakePhoto();
        inspectionWindow.DisplayWarning(inspectionWindow.DisplayIRTemp);
        yield return null;


        // Assert
        Assert.IsNotNull(temp);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator ReplaceCompliant()
    {
        // Arrange
        inspectionWindow.OpenWindow(inspectableObject);
        yield return null;
        ModalPopupSO temp = null;
        inspectionWindow.CreateModal.AddListener((x) => temp = x);

        // Act
        inspectionWindow.TakePhoto();
        inspectionWindow.Compliant();
        yield return null;
        inspectionWindow.OpenWindow(inspectableObject);
        yield return null;
        inspectionWindow.TakePhoto();
        inspectionWindow.NonCompliant();


        // Assert
        Assert.IsNotNull(temp);
        Assert.IsTrue(inspectionWindow.ReinspectDialogOpen);
        // this is clean up for the next test
        inspectionWindow.ReinspectDialogOpen = false;
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator ReplaceNonCompliant()
    {
        // Arrange
        inspectionWindow.OpenWindow(inspectableObject);
        yield return null;
        ModalPopupSO temp = null;
        inspectionWindow.CreateModal.AddListener((x) => temp = x);

        // Act
        inspectionWindow.TakePhoto();
        inspectionWindow.NonCompliant();
        yield return null;
        inspectionWindow.OpenWindow(inspectableObject);
        yield return null;
        inspectionWindow.TakePhoto();
        inspectionWindow.Compliant();


        // Assert
        Assert.IsNotNull(temp);
        Assert.IsTrue(inspectionWindow.ReinspectDialogOpen);
        // this is clean up for next test
        inspectionWindow.ReinspectDialogOpen = false;
    }


    #endregion
    #endregion

    #region HelperFunctions
    private IEnumerator openWindowSetup()
    {
        inspectionWindow.OpenWindow(inspectableObject);
        yield return null;
    }
    #endregion
}
