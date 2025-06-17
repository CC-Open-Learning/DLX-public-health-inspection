using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     This class is responsible for creating a modal popup window. It will contain the content, title, buttons, and actions for the window.
    ///     You can create a modal popup by right clicking in the project window and selecting Create -> ScriptableObjects -> ModalPopup
    ///     Then you can use it by passing it to the modal popup builder by using an event.
    ///     <see cref="PlayerController._restartSO"/> for example.
    /// </summary>
    [CreateAssetMenu(fileName = "ModalPopup", menuName = "ScriptableObjects/ModalPopup")]
    public class ModalPopupSO : ScriptableObject
    {
        [Header("Content")]
        // Labels
        public string ModalName;                    // Name of window
        public string ModalTitle;                   // Title displayed in modal
        public string ContentTitle;                 // Title inside content box
        [TextArea(3, 50)] public string Content;    // Content of the modal
        public int ContentFontSize = 27;            // Font size of the content
        public string FooterText;                   // Optional footer, default light blue font
        public Texture2D FooterIcon;                // Icon to go alongside footer

        // Toggle
        public string ConfirmationToggleText;       // Display string for the optional toggle button
        public string SecondaryConfirmationToggleText; // Display string for the secondary optional toggle button
        public int ConfirmationToggleFontSize = 24; // Font size of the optional toggle button

        // Buttons
        public string PrimaryBtnText;               // Text of the primary button
        public string SecondaryBtnText = "Cancel";  // Text of the secondary button
        public Color PrimaryBtnColor;               // Color of the primary button
        public Color SecondaryBtnColor = new Color(1f, 1f, 1f);                             // Color of the secondary button
        public Color PrimaryBtnTxtColor;            // Color of the primary button's text
        public Color SecondaryBtnTxtColor = new Color(26f / 255, 26f / 255, 26f / 255);     // Color of the secondary button's text
        public int BtnFontSize = 20;         // Font size of the primary button
        public int PrimaryBtnWidth = 144;    // Width of the primary button

        [Header("Alignment")]
        public Align ModalTitleAlign;
        public Align ContentTitleAlign;
        public Align ContentAlign;
        public Align FooterAlign;
        public Align ConfirmationToggleAlign;
        public Align SecondaryConfirmationToggleAlign;

        [Header("Options")]
        public bool IsPrimaryBtnEnabled = true;             // Toggles visibility of the primary button
        public bool IsPrimaryBtnClickable = true;          // Toggles whether the primary button is clickable
        public bool IsSecondaryBtnEnabled = true;           // Toggles visibility of the secondary button
        public bool IsCloseBtnEnabled = true;               // Toggles visisbility of the close button
        public BaseWindowType BaseWindow;                   // Enum specifying the window modal is covering
        public bool IsBackgroundDimmed = false;             // Toggle whether the background dim is active or not
        public bool IsConfirmationToggleEnabled = false;    // Toggle visibility of the toggle button
        public bool IsSecondaryConfirmationToggleEnabled = false;    // Toggle visibility of the secondary toggle button
        public Action OnPrimaryClick;                       // Action added to the click of the primary button
        public Action OnSecondaryClick;                     // Action added to the click of the secondary button
        public Action<bool> OnToggleClick;                  // Action added to the click of the toggle button
        public Action<bool> OnSecondaryToggleClick;         // Action added to the click of the secondary toggle button

        [Header("Size")]
        public int ModalWidth = 708;
        public int ModalHeight = 250;

        /// <summary>
        ///     Set the primary action for the button
        /// </summary>
        /// <param name="action"></param>
        public void SetPrimaryAction(Action action) { OnPrimaryClick = action; }
        /// <summary>
        ///     Set the secondary action for the button
        /// </summary>
        /// <param name="action"></param>
        public void SetSecondaryAction(Action action) { OnSecondaryClick = action; }
        public void SetToggleAction(Action<bool> action) { OnToggleClick = action; }
        public void SetSecondaryToggleAction(Action<bool> action) { OnSecondaryToggleClick = action; }

        public enum BaseWindowType
        {
            Inspection,     // Inspection window
            Review          // Inspection review window
        };

        [Header("Buttons Custom USS Classes")]
        public List<CustomUSSClass> CustomBtnUSSClasses = new List<CustomUSSClass>();
    }

    [Serializable]
    public struct CustomUSSClass
    {
        public string elementName;
        public string className;
    }

}
