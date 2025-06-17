using System.Globalization;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class will contain the evidence for the inspection, we might want to add more information to this in the future
    /// </summary>
    public class Evidence
    {
        //Properties
        public string ToolName;
        public string ToolLabel;
        public bool IsCompliant;
        public string Reading;

        public Evidence() { }

        /// <summary>
        ///     This is the constructor for the Evidence class, it will take in the tool used, if the object is compliant, and the inspectable object 
        /// </summary>
        /// <param name="tool">Tool used for inspection</param>
        /// <param name="isCompliant">Result of inspection</param>
        /// <param name="obj">Inspectable object inspected</param>
        public Evidence(Tools tool, bool isCompliant, InspectableObject obj)
        {
            ToolName = tool.ToString();

            IsCompliant = isCompliant;
            //switch statement to determine which tool was used and populate the NonComplianceLog struct accordingly
            switch (tool)
            {
                case Tools.Visual:
                    ToolLabel = ToolStringsSO.LogVisualString;
                    if (obj.HasPhoto)
                    {
                        Reading = ToolStringsSO.LogPhotoAvailableString;
                    }
                    else
                    {
                        Reading = ToolStringsSO.LogPhotoUnavailableString;
                    }
                    break;

                case Tools.IRThermometer:
                    ToolLabel = ToolStringsSO.LogInfraredString;
                    Reading = obj.CurrentObjState.IRTemp.ToString("F1", CultureInfo.InvariantCulture) + "°C";
                    break;

                case Tools.ProbeThermometer:
                    ToolLabel = ToolStringsSO.LogProbeString;
                    Reading = obj.CurrentObjState.ProbeTemp.ToString("F1", CultureInfo.InvariantCulture) + "°C";
                    break;

                default: break;
            };
        }

    }
}
