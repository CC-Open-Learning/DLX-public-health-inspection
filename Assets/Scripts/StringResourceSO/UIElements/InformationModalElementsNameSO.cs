using UnityEngine;

namespace VARLab.PublicHealth
{
    [CreateAssetMenu(fileName = "InformationElement", menuName = "ScriptableObjects/InformationElement")]
    public class InformationModalElementsNameSO : ScriptableObject
    {
        // Structure
        public string Container = "InformationWindow";
        public string BodyTitle = "BodyTitle";
        public string Body = "Body";
        public string Icon = "Icon";

        // Content
        public string WindowTitle = "WindowTitle";
        public string MessageTitle = "MessageTitle";
        public string MessageBody = "MessageBody";

        // Button
        public string Button = "ModalButton";
    }
}
