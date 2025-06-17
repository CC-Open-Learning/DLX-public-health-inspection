using System.Collections.Generic;
using UnityEngine;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// Audio segment data ScriptableObject. This will hold a list of audio clips
    /// </summary>
    [CreateAssetMenu(fileName = "AudioSegment", menuName = "ScriptableObjects/AudioSegment")]
    public class AudioSegment : ScriptableObject
    {
        public List<AudioClip> audioClips;

        /// <summary>
        ///     Get the length of the audio segment
        /// </summary>
        /// <returns>The length in seconds from the whole sequence</returns>
        public float Length()
        {
            float sum = 0f;
            foreach (var a in audioClips)
            {
                sum += a.length;
            }
            return sum;
        }
    }
}