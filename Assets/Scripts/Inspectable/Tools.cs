namespace VARLab.PublicHealth
{
    /// <summary>
    /// This is a place to hold the enum in the name space
    /// IMPORTANT NOTE: MUST MATCH ORDER OF TOOLBAR!!!
    /// </summary>
    public enum Tools
    {
        Visual,
        IRThermometer,
        ProbeThermometer,
        None
    }

    /// <summary>
    /// Extension method for printing the tool number to get the enum name just ToString() it.
    /// </summary>
    public static class ToolsExtension
    {
        // string for enum conversion into human readable text
        private const string _irName = "IR Thermometer";
        private const string _probeName = "Probe Thermometer";

        /// <summary>
        ///     Helper function to convert tool enum to string
        /// </summary>
        /// <param name="u">The enum value to convert</param>
        /// <returns>The string value of the enum</returns>
        public static string ToIntegerString(this Tools u)
        {
            return ((int)u).ToString();
        }

        /// <summary>
        ///  Helper function to convert log tool to tool enum
        /// </summary>
        public static string ConvertLogToolToEnum(string logToolUsed)
        {
            return logToolUsed switch
            {
                ToolStringsSO.LogVisualString => Tools.Visual.ToString(),
                ToolStringsSO.LogInfraredString => Tools.IRThermometer.ToString(),
                ToolStringsSO.LogProbeString => Tools.ProbeThermometer.ToString(),
                _ => Tools.None.ToString(),
            };
        }

        /// <summary>
        /// Helper function to convert Enum to human redable strings
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ConvertEnumToString(this Tools value)
        {
            switch (value)
            {
                case Tools.IRThermometer:
                    return _irName;
                case Tools.ProbeThermometer:
                    return _probeName;
                default:
                    return value.ToString();
            }

        }
    }


}
