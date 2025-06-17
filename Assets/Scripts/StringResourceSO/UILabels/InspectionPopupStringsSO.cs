using UnityEngine;

namespace VARLab.PublicHealth
{
    public class InspectionPopupStringsSO : ScriptableObject
    {
        // Log buttons
        public string CompliantBtnText = "Compliant";
        public string NonCompliantBtnText = "Non-Compliant";

        // Tool buttons
        [TextArea(2, 50)] public string visualToolBtn = "Visual\nInspection";
        public string ProbeToolBtn = "Probe\nReading";
        public string InfraredToolBtn = "Infrared\nReading";

        // Inspection messages
        public string VisualInfo = "Is this visual inspection compliant or non-compliant?";
        public string ProbeInfo = "Does the probe reading meet standards?";
        public string IRInfo = "Does the infrared reading meet standards?";
        public string CameraInfo = "To add the photo to the galley, report as compliant or non-compliant.";

        // Previous inspection messages
        public string CompliantEnding = "compliant.";
        public string NonCompliantEnding = "non-compliant.";
        public string VisualInspection = "Visual inspection reported as ";
        public string VisualPhotoInspection = "Visual inspection with photo reported as ";
        public string ProbeInspection = "Probe inspection reported as ";
        public string IRInspection = "Infrared inspection reported as ";

        //Toast messages
        public string ToastCompliant = "Item reported as compliant.";
        public string ToastNonCompliant = "Item reported as non-compliant.";
        public string ToastPreviousCompliant = "You have already reported this as compliant.";
        public string ToastPreviousNonCompliant = "You have already reported this as non-compliant.";
        public string ToastPhotoAddedToInspection = "Photo added to visual inspection.";
        public string ToastPhotoTaken = "Photo taken";
        public string ToastPhotoAddedToVisual = "Photo added to visual inspection";

        // Current inspection message
        [HideInInspector]
        public string CurrentInspectionMesssage;

        /// <summary>
        /// Sets the current inspection message based on the tool and if it is compliant or not
        /// </summary>
        /// <param name="tool">Inspection tool</param>
        /// <param name="isCompliant">Inspection result</param>
        public void SetCurrentInspectionMessage(Tools tool, bool? isCompliant, bool hasPhoto = false)
        {
            // If isCompliant is null, then the inspection is not complete, so we should display the info message
            if (isCompliant == null)
            {
                switch (tool)
                {
                    case Tools.Visual:
                        CurrentInspectionMesssage = VisualInfo;
                        break;
                    case Tools.IRThermometer:
                        CurrentInspectionMesssage = IRInfo;
                        break;
                    case Tools.ProbeThermometer:
                        CurrentInspectionMesssage = ProbeInfo;
                        break;
                }
                return;
            }

            bool isCompliantBool = (bool)isCompliant;

            if (hasPhoto && tool == Tools.Visual)
            {
                CurrentInspectionMesssage = VisualPhotoInspection + (isCompliantBool ? CompliantEnding : NonCompliantEnding);
                return;
            }

            // If we have previous inspection data, we should display the previous inspection message
            switch (tool)
            {
                case Tools.Visual:
                    CurrentInspectionMesssage = VisualInspection + (isCompliantBool ? CompliantEnding : NonCompliantEnding);
                    break;
                case Tools.IRThermometer:
                    CurrentInspectionMesssage = IRInspection + (isCompliantBool ? CompliantEnding : NonCompliantEnding);
                    break;
                case Tools.ProbeThermometer:
                    CurrentInspectionMesssage = ProbeInspection + (isCompliantBool ? CompliantEnding : NonCompliantEnding);
                    break;
            }
        }

    }
}
