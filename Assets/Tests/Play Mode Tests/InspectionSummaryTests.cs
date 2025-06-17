using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VARLab.PublicHealth;

public class InspectionSummaryTests
{
    private InspectionSummaryBuilder _inspectionSummary;
    private InspectableManager _inspectableManager;
    private UIDocument _uiDoc;
    private InspectionSummaryElementNamesSO _elementNames;

    private const string SceneName = "InspectionSummaryTestScene";

    [OneTimeSetUp]
    [Category("BuildServer")]
    public void RunOnce()
    {
        SceneManager.LoadScene(SceneName);
        _elementNames = ScriptableObject.CreateInstance<InspectionSummaryElementNamesSO>();
    }

    [UnityTest, Order(0)]
    [Category("BuildServer")]
    public IEnumerator SceneLoaded()
    {
        yield return new WaitUntil(() => SceneManager.GetSceneByName(SceneName).isLoaded);

        _inspectionSummary = GameObject.FindAnyObjectByType<InspectionSummaryBuilder>();
        _uiDoc = _inspectionSummary.GetComponent<UIDocument>();

        Assert.IsTrue(SceneManager.GetSceneByName(SceneName).isLoaded);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator DisplaySummary()
    {
        _inspectionSummary.HandleDisplay();

        yield return null;

        Assert.IsTrue(_uiDoc.rootVisualElement.style.display.ToString() == DisplayStyle.Flex.ToString());
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator IntercationsDisabledWhenWindowClosed()
    {
        _inspectionSummary.HandleDisplay();

        yield return null;

        Assert.IsFalse(Interactions.Instance.CanInteract);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator PrimaryButtonEventCalled()
    {
        bool wasCalled = false;
        Button primaryButton;

        _inspectionSummary.HandleDisplay();
        primaryButton = _uiDoc.rootVisualElement.Q<Button>(_elementNames.PrimaryButton);
        _inspectionSummary.Download.AddListener(() => wasCalled = true);

        // Act
        var e = new NavigationSubmitEvent() { target = primaryButton };
        primaryButton.SendEvent(e);

        yield return null;

        Assert.IsTrue(wasCalled);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator SecondaryButtonEventCalled()
    {
        bool wasCalled = false;
        Button secondaryButton;

        _inspectionSummary.HandleDisplay();
        secondaryButton = _uiDoc.rootVisualElement.Q<Button>(_elementNames.SecondaryButton);
        _inspectionSummary.PlayAgain.AddListener(() => wasCalled = true);

        // Act
        var e = new NavigationSubmitEvent() { target = secondaryButton };
        secondaryButton.SendEvent(e);

        yield return null;

        Assert.IsTrue(wasCalled);
    }
}
