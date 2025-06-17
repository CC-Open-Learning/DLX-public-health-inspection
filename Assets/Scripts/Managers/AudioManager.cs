using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace VARLab.PublicHealth
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField, Tooltip("Reference to the Audio Mixer")] private AudioMixer _audioMixer;

        private POI _currentPOI;
        private List<AudioSource> _audioSourcesCurrentPOI;

        private const float MinVolume = -80f;
        private const float MaxVolume = 0f;
        private float _currentVolume = MaxVolume;

        private void Awake()
        {
            _audioSourcesCurrentPOI = new();
        }
        /// <summary>
        /// <see cref="POIManager.POIChanged"/>
        /// a simple function to go through all audio sources under a POI and turn them on or off a bit too powerful might need a more fine tuned version later
        /// </summary>
        /// <param name="currentPOI"> POI to look through for audio sources</param>
        /// <param name="enable">turn on or off the sources</param>
        public void TogglePoiSFX(string poiName)
        {
            POI poiEnetered = PHISceneManager.Instance.PoiManager.POIs.Find(p => p.POIName == poiName);

            if (_currentPOI != poiEnetered)
            {
                foreach (var audioSource in _audioSourcesCurrentPOI)
                {
                    ToggleAudioClips(audioSource, false);
                }

                _audioSourcesCurrentPOI.Clear();

                _audioSourcesCurrentPOI = poiEnetered.gameObject.GetComponentsInChildren<AudioSource>().ToList();

                foreach (var audioSource in _audioSourcesCurrentPOI)
                {
                    ToggleAudioClips(audioSource, true);
                }

                _currentPOI = poiEnetered;
            }
        }

        private void ToggleAudioClips(AudioSource audioSource, bool enable)
        {
            audioSource.enabled = enable;
        }

        public void SetVolume(string group, float value)
        {
            _audioMixer.SetFloat(group, Mathf.Log10(value) * 20);
        }

        /// <summary>
        /// Mutes/Unmutes the master volume
        /// </summary>
        /// <param name="group">name of the audio group that will be muted/unmuted. 
        /// Currently this function is only available for the master volume</param>
        /// <param name="enabled">
        /// True = sets the Master volume to the previously set volume
        /// False = saves the current volume and sets the Master Volume to -80</param>
        public void ToggleMasterVolume(string group, bool enabled)
        {
            if (!enabled)
            {
                _audioMixer.GetFloat(group, out _currentVolume);
                _audioMixer.SetFloat(group, MinVolume);
            }
            else
            {
                _audioMixer.SetFloat(group, _currentVolume);
            }
        }

        public float GetVolume(string group)
        {
            _audioMixer.GetFloat(group, out float volume);
            return volume;
        }
    }
}
