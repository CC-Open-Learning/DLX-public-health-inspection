using UnityEngine;

namespace VARLab.PublicHealth
{
    public class ModalPopupElementNamesSO : ScriptableObject
    {
        // Structure
        public string Background = "BlurBackground";
        public string Container = "Container";
        public string Footer = "FooterRow";

        // Content
        public string MainTitle = "ModalTitle";
        public string ContentTitle = "ContentTitle";
        public string Content = "Content";
        public string FooterText = "FooterText";
        public string FooterSprite = "FooterSprite";

        // Buttons
        public string ConfirmationToggle = "ConfirmationToggle";
        public string SecondaryConfirmationToggle = "SecondaryConfirmationToggle";
        public string PrimaryBtn = "PrimaryBtn";
        public string SecondaryBtn = "SecondaryBtn";
        public string CloseBtn = "CloseBtn";

        // Default USS classes
        public string DefaultBtnClass = ".BtnDefault";
    }
}
