using System;
using UnityEngine;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     This class is used to create a information modal object. This object will contain the modal name, window title, body title, body message, button text, and an action for the button.
    /// </summary>
    [CreateAssetMenu(fileName = "InformationModal", menuName = "ScriptableObjects/InformationModal")]
    public class InformationModalSO : ScriptableObject
    {
        [Header("Content")]
        //Labels
        public string ModalName;
        public string WindowTitle;
        public string BodyTitle;
        public string BodyMessage;
        public string ButtonText;

        [Header("Options")]
        public Action OnPrimaryClick; // Action added to the button
        public bool IsTitleEnabled = true;
        public bool IsIconEnabled = false;
        public bool KeepBlurOn = true;
        public bool KeepUIOff = false;

        public void SetPrimaryAction(Action action) { OnPrimaryClick = action; }
    }
}
