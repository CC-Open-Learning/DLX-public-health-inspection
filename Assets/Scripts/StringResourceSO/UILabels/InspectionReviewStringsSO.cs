using UnityEngine;

namespace VARLab.PublicHealth
{
    public class InspectionReviewStringsSO : ScriptableObject
    {
        // Window title
        public string windowTitleText = "Inspection Review";

        // Tab headers
        public string nonComplianceLogTabText = "Non-Compliance Log";
        public string activityLogTabText = "Activity Log";
        public string galleryTabText = "Gallery";

        // Progress frame
        public string progressTitleText = "My Progress";
        public string locationTrackerTitleText = "Locations Visited";
        public string reportCounterTitleText = "Non-Compliances Reported";
        public string timeTitleText = "Total Time Taken";

        // Submission
        public string EndBtnText = "End Inspection";
    }
}
