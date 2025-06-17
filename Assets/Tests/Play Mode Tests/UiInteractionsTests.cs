using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VARLab.PublicHealth;

public class UiInteractionsTests
{
    [OneTimeSetUp]
    [Category("BuildServer")]
    public void RunOnce()
    {
        SceneManager.LoadScene("TestUIScene");
    }
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator TestUIBlockerTurnsOn()
    {
        yield return new WaitUntil(() => SceneManager.GetSceneByName("TestUIScene").isLoaded);

        var obj = GameObject.Find("UIObj");
        UIBlocker blocker = new();
        UIDocument doc = obj.GetComponent<UIDocument>();
        VisualElement elem = doc.rootVisualElement.Q("TestBox");

        blocker.RegisterMouseEnterCallback(elem);
        blocker.RegisterMouseLeaveCallback(elem);

        //try and synth event here?
        var evt = MouseEnterEvent.GetPooled();
        evt.target = elem;
        elem.SendEvent(evt);

        yield return new WaitForSeconds(0.1f); //needs a wait for the call back

        Assert.IsFalse(obj.GetComponent<Interactions>().CanInteract);
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator TestUIBlockerTurnsOff()
    {

        yield return new WaitUntil(() => SceneManager.GetSceneByName("TestUIScene").isLoaded);

        var obj = GameObject.Find("UIObj");
        UIBlocker blocker = new();
        UIDocument doc = obj.GetComponent<UIDocument>();
        VisualElement elem = doc.rootVisualElement.Q("TestBox");

        blocker.RegisterMouseEnterCallback(elem);
        blocker.RegisterMouseLeaveCallback(elem);

        //try and synth event here?
        var evt = MouseLeaveEvent.GetPooled();
        evt.target = elem;
        elem.SendEvent(evt);
        yield return new WaitForSeconds(0.1f); //needs a wait for the call back

        Assert.IsTrue(obj.GetComponent<Interactions>().CanInteract);
    }
}
