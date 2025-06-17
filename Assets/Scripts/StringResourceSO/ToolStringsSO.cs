using UnityEngine;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class is used to store all of the strings used in the tools
    /// </summary>
    public class ToolStringsSO : ScriptableObject
    {
        public const string LogInfraredString = "Infrared";
        public const string LogProbeString = "Probe";
        public const string LogPhotoString = "Photo";
        public const string LogVisualString = "Visual";

        public const string LogPhotoAvailableString = "View Photo";
        public const string LogPhotoUnavailableString = "------";
    }
}
