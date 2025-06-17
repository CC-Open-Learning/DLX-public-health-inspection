using UnityEngine;

namespace VARLab.PublicHealth
{
    [CreateAssetMenu(fileName = "ResumePopupElement", menuName = "ScriptableObjects/ResumePopupElement")]
    public class ResumePopupElementNamesSO : ScriptableObject
    {
        // Text Element
        public string LocationLabel = "LocationText";

        // Buttons
        public string CloseBtn = "CloseBtn";
    }
}
