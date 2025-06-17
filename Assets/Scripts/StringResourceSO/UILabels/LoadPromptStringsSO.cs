using UnityEngine;

namespace VARLab.PublicHealth
{
    public class LoadPromptStringsSO : ScriptableObject
    {
        public string windowTitleText = "Menu";

        // Buttons
        public string continueBtnText = "Continue";
        public string restartBtnText = "Restart";

        [TextArea(2, 50)] public string restartWarningText = "If you choose to restart, your current progress will be lost!";

        // Loading test

        public string loading = "Loading...";

        public string loadingComplete = "Loading Complete...";
    }
}
