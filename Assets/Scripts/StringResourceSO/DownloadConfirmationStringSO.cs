using UnityEngine;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     This class is used to create a data container for the Download Confirmation UI.
    ///     This object will contain the window title, message title, message body, and button text,
    ///     allowing for easy management and modification of UI text elements from a single location.
    /// </summary>
    [CreateAssetMenu(menuName = "PHI/Scriptable Objects/DownloadConfirmationStringSO")]
    public class DownloadConfirmationStringSO : ScriptableObject
    {
        // Labels for the Download Confirmation UI
        public string WindowTitle = "Download";
        public string MessageTitle = "Wait, Inspector!";
        public string MessageBody = "Before you close the browser window, check your downloads!";
        public string Button = "OK";
    }
}
