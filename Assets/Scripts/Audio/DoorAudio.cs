using UnityEngine;
using UnityEngine.Audio;

namespace VARLab.PublicHealth
{
    public class DoorAudio : MonoBehaviour
    {
        [SerializeField, Tooltip("Audio Source component for where audio is playing from")] AudioSource source;
        [SerializeField, Tooltip("Audio clip containing open sound")] AudioResource openSound;
        [SerializeField, Tooltip("Audio clip containing close sound")] AudioResource closeSound;

        public void playOpenSound()
        {
            source.resource = openSound;
            source.Play();
        }
        public void playCloseSound()
        {
            source.resource = closeSound;
            source.Play();
        }
    }
}
