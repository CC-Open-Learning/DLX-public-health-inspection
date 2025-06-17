using UnityEngine;

namespace VARLab.PublicHealth
{
    //This needs to be renamed inspection log strings so 
    public class NonComplianceLogStringsSO : ScriptableObject
    {
        // Column headers
        public string locationColText = "LOCATION";
        public string itemColText = "ITEM";
        public string toolUsedColText = "TOOL USED";
        public string compliancyColText = "COMPLIANCY";
        public string infoColText = "INFO";

        //Sort buttons for inspection log
        public string allBtnText = "All";
        public string compliantBtnText = "Compliant";
        public string nonCompliantBtnText = "Non-Compliant";
        public string emptyMessage = "You do not have any non-compliance issues reported at this moment!";
    }
}
