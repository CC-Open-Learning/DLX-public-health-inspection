using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VARLab.PublicHealth;

public class AudioManagerTests
{
    private AudioManager _audioManager;
    private POI BackKitchen;
    private POI Bar;
    private List<AudioSource> _audioSources;

    private const string MasterVolume = "MasterVolume";


    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        SceneManager.LoadScene("AudioManagerTestScene");
        _audioSources = new List<AudioSource>();
    }

    [UnityTest, Order(0)]
    public IEnumerator SceneLoader()
    {
        yield return new WaitUntil(() => SceneManager.GetSceneByName("AudioManagerTestScene").isLoaded);
        _audioManager = GameObject.FindAnyObjectByType<AudioManager>();
        BackKitchen = GameObject.Find("POI One").GetComponent<POI>();
        Bar = GameObject.Find("POI Two").GetComponent<POI>();
        Assert.IsTrue(true);
    }

    // A Test behaves as an ordinary method
    [Test]
    public void CheckMasterVolumeIsSetToZero()
    {
        var expectedVolume = 0f;

        Assert.AreEqual(expectedVolume, _audioManager.GetVolume(MasterVolume));
    }

    [Test]
    public void SetVolumeToMinusTen()
    {
        var expectedVolume = -6.02060032f;

        _audioManager.SetVolume(MasterVolume, 0.5f);

        Assert.AreEqual(expectedVolume, _audioManager.GetVolume(MasterVolume));
    }

    [UnityTest]
    public IEnumerator MuteMasterVolume()
    {
        var expectedVolume = -80f;

        _audioManager.ToggleMasterVolume(MasterVolume, false);

        yield return new WaitForEndOfFrame();

        Assert.AreEqual(expectedVolume, _audioManager.GetVolume(MasterVolume));
    }
    [UnityTest]
    public IEnumerator UnmuteMasterVolume()
    {
        var expectedVolume = -6.02060032f;

        _audioManager.SetVolume(MasterVolume, 0.5f);

        _audioManager.ToggleMasterVolume(MasterVolume, false);

        yield return new WaitForEndOfFrame();

        _audioManager.ToggleMasterVolume(MasterVolume, true);

        yield return new WaitForEndOfFrame();

        Assert.AreEqual(expectedVolume, _audioManager.GetVolume(MasterVolume));
    }

    [Test]
    public void EnableAudioSourcesInBackKitchen()
    {
        _audioSources.Clear();

        _audioManager.TogglePoiSFX("Back Kitchen");

        _audioSources = BackKitchen.GetComponentsInChildren<AudioSource>().ToList();

        Assert.That(_audioSources.Count, Is.EqualTo(3));
        Assert.That(_audioSources[0].enabled, Is.True);
        Assert.That(_audioSources[1].enabled, Is.True);
        Assert.That(_audioSources[2].enabled, Is.True);

    }

    [Test]
    public void DisableAudioSourcesInBackKitchen()
    {
        List<AudioSource> _barAudioSources = new();
        _audioSources.Clear();

        _audioManager.TogglePoiSFX("Bar");

        _audioSources = BackKitchen.GetComponentsInChildren<AudioSource>().ToList();
        _barAudioSources = Bar.GetComponentsInChildren<AudioSource>().ToList();

        Assert.That(_audioSources.Count, Is.EqualTo(3));
        Assert.That(_audioSources[0].enabled, Is.False);
        Assert.That(_audioSources[1].enabled, Is.False);
        Assert.That(_audioSources[2].enabled, Is.False);

        Assert.That(_barAudioSources.Count, Is.EqualTo(4));
        Assert.That(_barAudioSources[0].enabled, Is.True);
        Assert.That(_barAudioSources[1].enabled, Is.True);
        Assert.That(_barAudioSources[2].enabled, Is.True);
        Assert.That(_barAudioSources[3].enabled, Is.True);
    }
}
