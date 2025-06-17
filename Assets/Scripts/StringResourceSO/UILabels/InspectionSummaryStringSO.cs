using UnityEngine;

namespace VARLab.PublicHealth
{
    [CreateAssetMenu(menuName = "PHI/Scriptable Objects/InspectionSummaryStrings")]
    public class InspectionSummaryStringSO : ScriptableObject
    {
        [Header("Window Title")]
        public string InspectionSummary;

        [Header("Overview")]
        public string InspectionDate;
        public string Facility;
        public string Restaurant;
        public string TotalTime;
        public string NonCompliances;
        public string Compliances;
        public string Locations;

        [Header("Downloads")]
        public string DownloadInfo;
        public string PDFContains;
        public string InspectionLog;
        public string ActivityLog;
        public string ZIPContains;
        public string PhotoGallery;

        [Header("Locations")]
        public string LocationsNotInspected;

        [Header("Buttons")]
        public string PrimaryButton;
        public string SecondaryButton;
    }
}
