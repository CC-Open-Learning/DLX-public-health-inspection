using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.PublicHealth;

public class VoiceOverManagerTests
{
    GameObject masterOhmLawTrainerBoard;

    GameObject voiceOverManagerGO;
    VoiceOverManager voiceOverManager;

    GameObject startPanel;
    AudioSegment startingAudioSegment;

    AudioSegment audioSegment;
    AudioClip audioClip;
    AudioSource audioSource;

    [UnityTest]
    public IEnumerator PlayAudioClips_Successfully()
    {
        Assert.AreEqual(false, voiceOverManager.IsPlaying);

        voiceOverManager.PlayAudioClips(audioSegment, null, 0, 0);
        yield return null;

        Assert.AreEqual(true, voiceOverManager.IsPlaying);
    }

    [UnityTest]
    public IEnumerator PlayStartingAudioClip_Correct()
    {
        yield return null;

        Assert.AreEqual(startingAudioSegment.audioClips.First().name, "WelcomeMessage");
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SetUpAudio();
        SetUpVoiceOverManager(true);

        yield return null;

    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {

        //using [UnityTearDown], IEnumerator, & yield return null to ensure the Singleton static INSTANCE is destroyed before the next test
        GameObject.Destroy(GameObject.FindFirstObjectByType<VoiceOverManager>());

        yield return null;
    }
    private void SetUpAudio()
    {
        audioSegment = ScriptableObject.CreateInstance<AudioSegment>();
        audioSegment.audioClips = new();
        audioClip = AudioClip.Create("sampleForTesting", 100, 1, 1000, false);
        audioSegment.audioClips.Add(audioClip);

        startingAudioSegment = ScriptableObject.CreateInstance<AudioSegment>();
        startingAudioSegment.audioClips = new();
        audioClip = AudioClip.Create("WelcomeMessage", 100, 1, 1000, false);
        startingAudioSegment.audioClips.Add(audioClip);
    }

    private void SetUpVoiceOverManager(bool isActive)
    {
        voiceOverManagerGO = new();
        voiceOverManagerGO.SetActive(false);

        voiceOverManager = voiceOverManagerGO.AddComponent<VoiceOverManager>();

        voiceOverManagerGO.SetActive(isActive);
    }




}
