using UnityEngine;

namespace VARLab.PublicHealth
{
    [CreateAssetMenu(fileName = "ModalPopup", menuName = "ScriptableObjects/InspectionWindow")]
    public class InspectionWindowElementNamesSO : ScriptableObject
    {
        // Main Window
        public string MainWindow = "MainWindow";

        // Buttons
        public string VisualBtn = "VisualBtn";
        public string IRTempBtn = "IRTempBtn";
        public string ProbeTempBtn = "ProbeBtn";
        public string CompliantBtn = "CompliantBtn";
        public string NonCompliantBtn = "NonCompliantBtn";
        public string CloseBtn = "CloseBtn";
        public string TakePhotoBtn = "TakePhotoBtn";

        // Labels
        public string InfoLabel = "InfoLbl";
        public string LocationLabel = "LocationLbl";
        public string ItemLabel = "ItemLbl";
        public string ProbeTemp = "ProbeTemp";
        public string InfraTemp = "InfraTemp";

        // Visual Elements
        public string DisplayArea = "ItemVisual";
        public string ProbePopup = "ProbePopUp";
        public string InfraPopup = "InfraPopUp";
        public string PhotoBorder = "PhotoBorder";
        public string FlashEffect = "Flash";
    }
}
