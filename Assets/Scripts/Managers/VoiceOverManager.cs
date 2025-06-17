using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// Original singleton manager from LSM7.07 Motive Power Circuits: 
    /// https://bitbucket.org/VARLab/motive-power-circuits/src/development/Assets/Scripts/Game%20Managers/VoiceOverManager.cs
    /// Minor changes to the class to fit the project
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    [DisallowMultipleComponent]
    public class VoiceOverManager : MonoBehaviour
    {
        public static VoiceOverManager Instance { get; private set; }
        public AudioSource audioSource;
        private Coroutine playAudioClips = null;

        public bool IsPlaying { get { return playAudioClips != null || audioSource.isPlaying; } }

        public bool IsPaused;

        private void Awake()
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Call this method to play audio clips in sequence
        /// </summary>
        /// <param name="audioSegment">Audio clip data SO</param>
        /// <param name="previousAudioSegment">Previous audio segment</param>
        /// <param name="startDelay">Start delay time in seconds</param>
        /// <param name="timeBetweenClips">Delay between clips in seconds</param>
        public void PlayAudioClips(AudioSegment audioSegment, AudioSegment previousAudioSegment = null, float startDelay = 0f, float timeBetweenClips = 0f)
        {
            // If there's no audio segment, return
            if (audioSegment == null)
                return;

            // Stop current audio source
            audioSource.Stop();
            // Stop the previous audio sequence if it's playing
            if (playAudioClips != null)
                StopCoroutine(playAudioClips);

            // Play the audio clips in sequence
            playAudioClips = StartCoroutine(_PlayAudioClips(audioSegment, previousAudioSegment, startDelay, timeBetweenClips));
        }

        public void PlayAudioClipsFromList(List<AudioClip> audioClips, float startDelay = 0f, float timeBetweenClips = 0f)
        {
            // if the list is empty return
            if (audioClips == null)
            {
                return;
            }

            //if something is playing in the audio source stop.
            audioSource.Stop();

            //if the audioclip coroutine is not null stop it.
            if (playAudioClips != null)
            {
                StopCoroutine(playAudioClips);
            }

            //play all clips
            playAudioClips = StartCoroutine(PlayAudioClipsCoroutine(audioClips, startDelay, timeBetweenClips));
        }

        /// <summary>
        /// Coroutine that plays the audio clips in sequence
        /// </summary>
        /// <param name="audioSegment">Audio clip data SO</param>
        /// <param name="previousAudioSegment">Previous audio segment</param>
        /// <param name="startDelay">Start delay time in seconds</param>
        /// <param name="timeBetweenInstructions">Delay between clips in seconds</param>
        private IEnumerator _PlayAudioClips(AudioSegment audioSegment, AudioSegment previousAudioSegment, float startDelay = 0f, float timeBetweenInstructions = 0f)
        {
            yield return new WaitForSeconds(startDelay);
            yield return new WaitUntil(() => !IsPaused);

            // Play the previous audio segment if it exists
            if (previousAudioSegment != null)
            {
                foreach (var audioClip in previousAudioSegment.audioClips)
                {
                    audioSource.Stop();
                    audioSource.PlayOneShot(audioClip);
                    yield return new WaitForSeconds(audioClip.length + timeBetweenInstructions);
                    yield return new WaitUntil(() => !IsPaused);
                    yield return new WaitUntil(() => !audioSource.isPlaying);
                }
            }

            // Play the current audio segment
            foreach (var audioClip in audioSegment.audioClips)
            {
                audioSource.Stop();
                audioSource.PlayOneShot(audioClip);
                yield return new WaitForSeconds(audioClip.length + timeBetweenInstructions);
                yield return new WaitUntil(() => !IsPaused);
                yield return new WaitUntil(() => !audioSource.isPlaying);
            }

            playAudioClips = null;
        }

        private IEnumerator PlayAudioClipsCoroutine(List<AudioClip> audioclips, float startDelay = 0f, float timeBetweenInstructions = 0f)
        {
            yield return new WaitForSeconds(startDelay);
            yield return new WaitUntil(() => !IsPaused);

            // Play the current audio segment
            foreach (var audioClip in audioclips)
            {
                audioSource.Stop();
                audioSource.PlayOneShot(audioClip);
                yield return new WaitForSeconds(audioClip.length + timeBetweenInstructions);
                yield return new WaitUntil(() => !IsPaused);
                yield return new WaitUntil(() => !audioSource.isPlaying);
            }

            playAudioClips = null;
        }

        /// <summary>
        /// Stop the audio clip
        /// </summary>
        public void StopAudioClip()
        {
            audioSource.Stop();
        }

        /// <summary>
        /// Stop all audio clips
        /// </summary>
        public void StopAllClips()
        {
            if (playAudioClips != null)
                StopCoroutine(playAudioClips);

            audioSource.Stop();
        }
    }
}