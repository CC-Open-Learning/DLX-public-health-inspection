using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     This class handles the creation and display of a modal popup window invoked by the ModalPopupSO
    ///     <see cref="PlayerController._restartSO"/> for an example of how this class is used. 
    /// </summary>
    public class ModalPopupBuilder : MonoBehaviour
    {
        //UIDocument
        public UIDocument ModalDocument;

        //Labels
        private Label _modalTitle;
        private Label _contentTitle;
        private Label _content;
        private Label _footerText;

        //Toggle
        private Toggle _confirmationToggle;
        private Toggle _secondaryConfirmationToggle;

        //Buttons
        private Button _primaryBtn;
        private Button _secondaryBtn;
        private Button _closeBtn;

        //Visual Elements
        private VisualElement _rootElement;
        private VisualElement _blurBackground;
        private VisualElement _container;
        private VisualElement _footerRow;
        private VisualElement _footerSprite;

        //Colors
        private Color _blurColor = new Color(0.46f, 0.46f, 0.46f, 0.57f);
        private Color _transparentColor = new Color(0, 0, 0, 0);

        //Settings
        private ModalPopupSO _currentSettings;

        // Toggle states tracker variables
        private bool _isPrimaryToggleChecked;
        private bool _isSecondaryToggleChecked;

        //const strings for styles
        private const string OverInspectionStyleClass = "WindowOverInspection";
        private const string OverReviewStyleClass = "WindowOverReview";

        //Element Names
        [SerializeField] private ModalPopupElementNamesSO _elementNames;

        /// <summary>
        ///     Gets references to all the modifiable elements of the modal
        /// </summary>
        void Start()
        {
            _rootElement = ModalDocument.rootVisualElement;

            _blurBackground = _rootElement.Q<VisualElement>(_elementNames.Background);
            _container = _rootElement.Q<VisualElement>(_elementNames.Container);
            _footerRow = _rootElement.Q<VisualElement>(_elementNames.Footer);

            _modalTitle = _rootElement.Q<Label>(_elementNames.MainTitle);
            _contentTitle = _rootElement.Q<Label>(_elementNames.ContentTitle);
            _content = _rootElement.Q<Label>(_elementNames.Content);
            _footerText = _rootElement.Q<Label>(_elementNames.FooterText);
            _footerSprite = _rootElement.Q<VisualElement>(_elementNames.FooterSprite);

            _confirmationToggle = _rootElement.Q<Toggle>(_elementNames.ConfirmationToggle);
            _secondaryConfirmationToggle = _rootElement.Q<Toggle>(_elementNames.SecondaryConfirmationToggle);
            _primaryBtn = _rootElement.Q<Button>(_elementNames.PrimaryBtn);
            _secondaryBtn = _rootElement.Q<Button>(_elementNames.SecondaryBtn);
            _closeBtn = _rootElement.Q<Button>(_elementNames.CloseBtn);

            _primaryBtn.clicked += () =>
            {
                _currentSettings?.OnPrimaryClick?.Invoke();
                Hide();
            };

            _secondaryBtn.clicked += () =>
            {
                _currentSettings?.OnSecondaryClick?.Invoke();
                Hide();
            };

            _closeBtn.clicked += () =>
            {
                _currentSettings?.OnSecondaryClick?.Invoke();
                Hide();
            };

            // Listen to toggle value changes
            _confirmationToggle.RegisterValueChangedCallback((evt) =>
            {
                _isPrimaryToggleChecked = evt.newValue;
                _currentSettings?.OnToggleClick?.Invoke(evt.newValue);
            });
            _secondaryConfirmationToggle.RegisterValueChangedCallback((evt) =>
            {
                _isSecondaryToggleChecked = evt.newValue;
                _currentSettings?.OnSecondaryToggleClick?.Invoke(evt.newValue);
            });

            Hide();
        }

        /// <summary>
        ///     Sets the content of the modal then displays it
        /// </summary>
        /// <param name="modalInfo">The settings to apply to the modal</param>
        public void HandleDisplayModal(ModalPopupSO modalInfo)
        {
            _currentSettings = modalInfo;
            SetContent();
            StartCoroutine(Show());
        }

        /// <summary>
        ///     Sets the content of the modal
        /// </summary>
        private void SetContent()
        {
            // Set Labels
            SetLabel(_modalTitle, _currentSettings.ModalTitleAlign, _currentSettings.ModalTitle);
            SetLabel(_contentTitle, _currentSettings.ContentTitleAlign, _currentSettings.ContentTitle);
            SetLabel(_content, _currentSettings.ContentAlign, _currentSettings.Content);
            // Set the content font size
            _content.style.fontSize = _currentSettings.ContentFontSize;

            // Set Footer
            SetLabel(_footerText, _currentSettings.FooterAlign, _currentSettings.FooterText);
            if (_currentSettings.FooterIcon != null)
            {
                _footerSprite.style.backgroundImage = new StyleBackground(_currentSettings.FooterIcon);
                _footerSprite.style.display = DisplayStyle.Flex;
            }
            else
            {
                _footerSprite.style.display = DisplayStyle.None;
            }

            if (_footerSprite.style.display == DisplayStyle.None && _footerText.style.display == DisplayStyle.None)
            {
                _footerRow.style.display = DisplayStyle.None;
            }
            else
            {
                _footerRow.style.display = DisplayStyle.Flex;
            }

            // Set Toggles
            SetToggle(_confirmationToggle, _currentSettings.IsConfirmationToggleEnabled,
                _currentSettings.ConfirmationToggleAlign, _currentSettings.ConfirmationToggleText);
            SetToggle(_secondaryConfirmationToggle, _currentSettings.IsSecondaryConfirmationToggleEnabled,
                _currentSettings.SecondaryConfirmationToggleAlign, _currentSettings.SecondaryConfirmationToggleText);

            // Set Buttons
            SetButton(_primaryBtn, _currentSettings.IsPrimaryBtnEnabled, _currentSettings.PrimaryBtnColor,
                _currentSettings.PrimaryBtnTxtColor, _currentSettings.PrimaryBtnText);
            SetButton(_secondaryBtn, _currentSettings.IsSecondaryBtnEnabled, _currentSettings.SecondaryBtnColor,
                _currentSettings.SecondaryBtnTxtColor, _currentSettings.SecondaryBtnText);
            SetButton(_closeBtn, _currentSettings.IsCloseBtnEnabled, _transparentColor, Color.white, "X");

            // Style container & Background
            _container.style.width = _currentSettings.ModalWidth;
            _container.style.height = _currentSettings.ModalHeight;
            if (_currentSettings.IsBackgroundDimmed)
            {
                _blurBackground.style.backgroundColor = _blurColor;
            }
            else
            {
                _blurBackground.style.backgroundColor = _transparentColor;
            }

            // Apply inspection window modifications
            if (_currentSettings.BaseWindow == ModalPopupSO.BaseWindowType.Inspection)
            {
                SetWindowOffset(OverInspectionStyleClass, OverReviewStyleClass);
            }
            else
            {
                SetWindowOffset(OverReviewStyleClass, OverInspectionStyleClass);
            }
            // Set the width of the primary button based on SO configuration
            _primaryBtn.style.width = _currentSettings.PrimaryBtnWidth;
            // Set the clickable state of the primary button (clickable by default)
            if (!_currentSettings.IsPrimaryBtnClickable)
            {
                _primaryBtn.SetEnabled(false);
            }
            else
            {
                _primaryBtn.SetEnabled(true);
            }

            // Resize the font size of the toggles based on the SO configuration
            _confirmationToggle.style.fontSize = _currentSettings.ConfirmationToggleFontSize;
            _secondaryConfirmationToggle.style.fontSize = _currentSettings.ConfirmationToggleFontSize;
        }

        /// <summary>
        ///     Wait one frame for all updates to apply, then display the window
        /// </summary>
        /// <returns></returns>
        private IEnumerator Show()
        {
            yield return null;
            _rootElement.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        ///     Closes the modal window
        /// </summary>
        private void Hide()
        {
            // Hide the window
            _rootElement.style.display = DisplayStyle.None;
            _currentSettings = null;
        }

        /// <summary>
        ///     Applies the given settings to a specific label. If text is empty, disables that label.
        /// </summary>
        /// <param name="label">The label to apply the settings to</param>
        /// <param name="align">The alignment to apply to that label</param>
        /// <param name="text">The text for the label. If empty, the label is disabled</param>
        private void SetLabel(Label label, Align align, string text)
        {
            if (string.IsNullOrEmpty(text.Trim()))
            {
                label.style.display = DisplayStyle.None;
            }
            else
            {
                label.text = text;
                label.style.alignSelf = align;
                label.style.display = DisplayStyle.Flex;
            }
        }

        /// <summary>
        /// Applies the given settings to a toggle button
        /// </summary>
        /// <param name="toggle">The toggle to stylize</param>
        /// <param name="isEnabled">Whether or not the toggle is enabled</param>
        /// <param name="align">The alignment of the toggle</param>
        /// <param name="text">The text to display for the toggle</param>
        private void SetToggle(Toggle toggle, bool isEnabled, Align align, string text)
        {
            toggle.value = false;
            if (isEnabled)
            {
                toggle.text = text;
                toggle.style.alignSelf = align;
                toggle.style.display = DisplayStyle.Flex;
            }
            else
            {
                toggle.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        ///    Updates the state of the primary button based on the state of the toggles
        ///    and the settings of the modal
        /// </summary>
        public void UpdatePrimaryBtnState()
        {
            if (_isPrimaryToggleChecked && !_currentSettings.IsSecondaryConfirmationToggleEnabled)
            {
                _primaryBtn.SetEnabled(true);
            }
            else if (_isPrimaryToggleChecked && _isSecondaryToggleChecked)
            {
                _primaryBtn.SetEnabled(true);
            }
            else
            {
                _primaryBtn.SetEnabled(false);
            }
        }

        /// <summary>
        ///     Applies the given styles to the given button
        /// </summary>
        /// <param name="button">The button to stylize</param>
        /// <param name="isEnabled">Whether or not the button should be visible</param>
        /// <param name="background">Background color of the button</param>
        /// <param name="textColor">Text color of the button</param>
        /// <param name="text">The text displayed on the button</param>
        private void SetButton(Button button, bool isEnabled, Color background, Color textColor, string text)
        {

            if (!isEnabled)
            {
                button.style.display = DisplayStyle.None;
                return;
            }

            button.style.display = DisplayStyle.Flex;
            button.style.fontSize = _currentSettings.BtnFontSize; // Font size will not be changed by custom USS classes
            button.text = text;

            if (_currentSettings.CustomBtnUSSClasses.Count > 0)
            {
                ApplyBtnCustomUssClasses();
                return;
            }

            button.style.backgroundColor = background;
            button.style.color = textColor;
        }

        /// <summary>
        ///     Modifies the window and blur so it is correctly positioned over the Inspection window
        /// </summary>
        private void SetWindowOffset(string toSet, string toRemove)
        {
            _blurBackground.AddToClassList(toSet);
            _blurBackground.RemoveFromClassList(toRemove);
        }

        /// <summary>
        ///    Applies custom button USS classes to the modal
        /// </summary>
        private void ApplyBtnCustomUssClasses()
        {
            if (_currentSettings.CustomBtnUSSClasses.Count > 0)
            {
                foreach (CustomUSSClass customClass in _currentSettings.CustomBtnUSSClasses)
                {
                    _rootElement.Q<Button>(customClass.elementName).ClearClassList();
                    _rootElement.Q<Button>(customClass.elementName).AddToClassList(_elementNames.DefaultBtnClass);
                    _rootElement.Q<VisualElement>(customClass.elementName).AddToClassList(customClass.className);
                }

            }
        }
    }
}
