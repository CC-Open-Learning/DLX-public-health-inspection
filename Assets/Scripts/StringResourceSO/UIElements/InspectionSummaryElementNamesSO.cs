using UnityEngine;

namespace VARLab.PublicHealth
{
    [CreateAssetMenu(menuName = "PHI/Scriptable Objects/InspectionSummaryElements")]
    public class InspectionSummaryElementNamesSO : ScriptableObject
    {
        [Header("Overview")]
        public string InspectionDateTxt = "InspectionDateTxt";
        public string TimeTxt = "TimeTxt";
        public string NonComplainceTxt = "NonComplianceTxt";
        public string ComplianceTxt = "ComplianceTxt";
        public string LocationsTxt = "LocationsTxt";

        [Header("Locations Not Inspected")]
        public string LocationsNotInspectectedTxt = "LocationsListLbl";
        public string LocationCount = "LocationCount";

        [Header("Buttons")]
        public string PrimaryButton = "PrimaryButton";
        public string SecondaryButton = "SecondayButton";
    }
}
