using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     The purpose of this class is to manage the toast messages that appear on the screen
    ///     This allows other classes to call the DisplayToast method without having to worry about the instance of this class
    /// </summary>
    public class ToastManager : MonoBehaviour
    {
        /// <summary>Singleton instance of the ToastManager</summary>
        public static ToastManager Instance { get; private set; }

        //Blue color used for toast
        public static Color _defaultColor = new(6 / 255f, 113 / 255f, 224 / 255f, 255f);

        /// <summary>Toast UI doc that gets set in GetReferences</summary>
        private UIDocument _toastDialog;

        /// <summary>Toast template container for toast messages</summary>
        private TemplateContainer _toastTemplateContainer;

        /// <summary>The visual tree asset found in the UI doc</summary>
        private VisualTreeAsset _toastTemplate;

        /// <summary> List of icons that can be displayed on the toast</summary>
        public List<Texture2D> Icons;

        /// <summary>
        ///     Strings used to find the elements in toast template and styles
        /// </summary>
        private const string _inspectionFrameElement = "InspectionFrame";
        private const string _inspectionPhotoFrameElement = "ItemVisualArea";
        private const string _toastContainerElement = "ToastContainer";
        private const string _toastElement = "Toast";
        private const string _toastLabelElement = "ToastLabel";
        private const string _fadeIn = "FadeIn";
        private const string _fadeOut = "FadeOut";
        private const string _iconElement = "InfoIcon";

        //The toast icon is used to display the icon on the toast
        private VisualElement _toastIcon;

        //The toast container is used to display the toast messages on the screen
        private VisualElement _toastContainer;

        //The inspection frame is used to display the toast messages in the inspection window
        private VisualElement _inspectionFrame;

        private VisualElement _inspectionPhotoFrame;

        //List of all the toasts that are currently displayed on the screen 
        private List<VisualElement> _toasts;

        private void Start()
        {
            Instance = this;
            GetReferences();
        }


        /// <summary>
        ///    This method is used to display a toast message on the screen
        /// </summary>
        /// <param name="message">The message to be displayed</param>
        /// <param name="color">The color of the toast</param>
        public void DisplayToast(string message, Color color)
        {
            DisplayToast(message, 3.0f, color, ToastSize.Auto, ToastBorderStyle.BorderNone, ToastAlignment.Top);
        }

        /// <summary>
        /// Display a toast message with the default display time of 3 seconds
        /// </summary>
        public void DisplayToast(string message, Color color, bool displayInMiddle = false, ToastIcons icon = ToastIcons.Info, bool multiToast = false)
        {
            if (displayInMiddle)
                DisplayToast(message, 3.0f, color, ToastSize.Auto, ToastBorderStyle.BorderNone, ToastAlignment.Middle, icon, multiToast);
            else
                DisplayToast(message, 3.0f, color, ToastSize.Auto, ToastBorderStyle.BorderNone, ToastAlignment.Top, icon, multiToast);
        }

        ///<summary>
        /// Display a toast message with the default size and border size
        /// </summary>
        public void DisplayToast(string message, float displayTime, Color color, ToastAlignment alignment, ToastIcons icon = ToastIcons.Info, bool multiToast = false)
        {
            DisplayToast(message, displayTime, color, ToastSize.Auto, ToastBorderStyle.BorderNone, alignment, icon, multiToast);
        }

        /// <summary>
        /// Display a toast message with the default icon and display time
        /// </summary>
        public void DisplayInspectionToast(string message, Color color, bool tf)
        {
            DisplayToast(message, color, tf, ToastIcons.Info);
        }

        /// <summary>
        ///    This method is used to display a toast message on the screen
        /// </summary>
        /// <param name="message">The message to be displayed</param>
        /// <param name="color">The color of the toast</param>
        /// <param name="displayInMiddle">If the toast should be displayed in the middle of the screen</param>
        /// <param name="alignment">The alignment of the toast</param>
        /// <param name="size">The size of the toast</param>
        /// <param name="borderStyle">The border style of the toast</param>
        /// <param name="icon">The icon to be displayed on the toast</param>
        /// <param name="multiToast">If more than one toast will be displayed at the same time</param>
        public void DisplayToast(string message, float displayTime, Color color, ToastSize size, ToastBorderStyle borderStyle,
                                        ToastAlignment alignment, ToastIcons icon = ToastIcons.Info, bool multiToast = false)
        {
            if (!multiToast)
                ClearToasts();

            //Instantiate the toast template
            _toastTemplateContainer = _toastTemplate.Instantiate();

            //Get the toast element from the template
            VisualElement _toast = _toastTemplateContainer.Q(_toastElement);

            //Get the icon element from the template
            _toastIcon = _toast.Q(_iconElement);

            //Register the geometry changed event to add the fade in class
            _toast.RegisterCallback<GeometryChangedEvent>(ev => OnGeometryChanged(_toast));

            //Get the label element from the template and set the text
            Label _toastLabel = _toast.Q(_toastLabelElement) as Label;
            _toastLabel.text = message;

            //Apply the styles to the toast
            ApplyToastStyles(_toast, _toastLabel, color, size, borderStyle, alignment, icon);

            //Check if the toast should be displayed in the inspection window
            if (alignment == ToastAlignment.TopInspectionWindow || alignment == ToastAlignment.Middle)
            {
                //Add the toast to the inspection frame
                _inspectionPhotoFrame.Add(_toast);
            }
            else
            {
                _toastContainer.Add(_toast);
            }

            StartCoroutine(AutoHideToastRoutine(_toast, displayTime, alignment));

            _toasts.Add(_toast);

            _toastContainer.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Apply the styles to the toast
        /// </summary>
        /// <param name="toast">Toast element</param>
        /// <param name="toastLabel">Toast message</param>
        /// <param name="color">Toast color</param>
        /// <param name="size">Toast Size</param>
        /// <param name="borderStyle">Toast Border</param>
        /// <param name="alignment">Position of the toast</param>
        /// <param name="icon">Toast icon to use</param>
        private void ApplyToastStyles(VisualElement toast, Label toastLabel, Color color, ToastSize size,
                                            ToastBorderStyle borderStyle, ToastAlignment alignment, ToastIcons icon)
        {
            if (icon != ToastIcons.None)
            {
                _toastIcon.style.backgroundImage = Icons[(int)icon];
            }
            else
            {
                _toastIcon.style.display = DisplayStyle.None;
                toastLabel.style.unityTextAlign = StyleKeyword.Null;
                toastLabel.style.marginRight = StyleKeyword.Auto;
                toastLabel.style.marginLeft = StyleKeyword.Auto;
            }

            if (ToastSize.Small == size)
                toast.style.width = Length.Percent(40);
            else if (ToastSize.Medium == size)
                toast.style.width = Length.Percent(60);
            else if (ToastSize.Large == size)
                toast.style.width = Length.Percent(80);


            if (borderStyle != ToastBorderStyle.BorderNone)
            {
                toast.AddToClassList("BorderThin");
            }

            toast.style.backgroundColor = color;
            toast.style.top = StyleKeyword.Null;

            if (alignment == ToastAlignment.TopInspectionWindow || alignment == ToastAlignment.Middle)
            {
                _inspectionFrame.style.display = DisplayStyle.Flex;
                toast.style.alignSelf = Align.FlexStart;
                toast.style.left = 0;
            }

            if (alignment == ToastAlignment.TopInspectionWindow)
            {
                _inspectionPhotoFrame.style.justifyContent = Justify.FlexStart;
                toast.style.alignSelf = Align.Center;
            }

            if (alignment == ToastAlignment.Middle)
            {
                _inspectionPhotoFrame.style.justifyContent = Justify.Center;
                toast.style.alignSelf = Align.Center;
                toast.style.marginTop = 0;
            }

            if (alignment == ToastAlignment.Bottom)
                toast.style.top = Length.Percent(86.8f);


            toast.style.display = DisplayStyle.Flex;

        }

        /// <summary>
        /// Hide the toast immediately without waiting for the fade out animation
        /// </summary>
        public void HideToastImediattly(VisualElement _toast)
        {
            _toast.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///     Coroutine to auto hide the toast after 3 seconds
        /// </summary>
        /// <returns></returns>
        private IEnumerator AutoHideToastRoutine(VisualElement _toast, float displayTime, ToastAlignment alignment)
        {
            yield return new WaitForSeconds(displayTime);
            _toast.AddToClassList(_fadeOut);
            yield return new WaitForSeconds((float).5);//Wait for fade out to finish
            HideToast(_toast);

            if (alignment == ToastAlignment.TopInspectionWindow || alignment == ToastAlignment.Middle)
                _inspectionPhotoFrame.Clear();
            else
                _toastContainer.Remove(_toast);

            _toasts.Remove(_toast);
        }

        public void ClearToasts()
        {
            foreach (VisualElement toast in _toasts)
            {
                StopAllCoroutines();
                HideToast(toast);
            }
        }

        /// <summary>
        ///     hides the toast
        /// </summary>
        private void HideToast(VisualElement _toast)
        {
            _toastContainer.style.display = DisplayStyle.None;
            _toast.style.display = DisplayStyle.None;
            _inspectionFrame.style.display = DisplayStyle.None;
            _toast.RemoveFromClassList(_fadeOut);
            _toast.RemoveFromClassList(_fadeIn);
        }

        /// <summary>
        /// We need to for the layout to be updated before we can add the fade in class
        /// </summary>
        public void OnGeometryChanged(VisualElement _toast)
        {
            _toast.UnregisterCallback<GeometryChangedEvent>(ev => OnGeometryChanged(_toast));
            _toast.AddToClassList(_fadeIn);
        }

        /// <summary>
        /// Get the references for the toast
        /// </summary>
        public void GetReferences()
        {
            _toastTemplate = Resources.Load<VisualTreeAsset>("Toast");
            _toastDialog = GetComponent<UIDocument>();
            _toastContainer = _toastDialog.rootVisualElement.Q(_toastContainerElement);
            _inspectionFrame = _toastDialog.rootVisualElement.Q(_inspectionFrameElement);
            _inspectionPhotoFrame = _toastDialog.rootVisualElement.Q(_inspectionPhotoFrameElement);
            _inspectionFrame.style.display = DisplayStyle.None;
            _toasts = new List<VisualElement>();
        }

    }

    /// <summary>
    ///    Enum for the different icons that can be displayed on the toast
    /// </summary>
    public enum ToastIcons
    {
        Info = 0,
        Check = 1,
        None = 2
    }

    /// <summary>
    ///     Enum for the different alignments of the toast
    /// </summary>
    public enum ToastAlignment
    {
        Top,
        Middle,
        Bottom,
        TopInspectionWindow
    }

    /// <summary>
    ///     enum for the different sizes of the toast
    /// </summary>
    public enum ToastSize
    {
        Small,
        Medium,
        Large,
        Auto
    }

    /// <summary>
    ///    Enum for the different border styles of the toast
    /// </summary>
    public enum ToastBorderStyle
    {
        BorderThin,
        BorderThick,
        BorderNone
    }
}
