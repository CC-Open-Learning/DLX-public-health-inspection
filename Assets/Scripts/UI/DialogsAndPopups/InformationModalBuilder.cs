using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class is used to build the Information Modal window
    /// </summary>
    public class InformationModalBuilder : MonoBehaviour
    {
        //Public/Serialized Properties
        public UIDocument InformationModalDocument;
        public BlurBackground Blur;

        //Private Properties
        private Label _windowTitle;
        private Label _messageTitle;
        private Label _bodyContent;

        private Button _closeButton;

        private VisualElement _rootElement;
        private VisualElement _bodyTitle;
        private VisualElement _body;
        private VisualElement _icon;

        private InformationModalSO _currentSetting;

        [SerializeField] private InformationModalElementsNameSO _elementNames;

        private void Awake()
        {
            _rootElement = InformationModalDocument.rootVisualElement;
            _rootElement.style.display = DisplayStyle.None;
        }
        private void Start()
        {
            _windowTitle = _rootElement.Q<Label>(_elementNames.WindowTitle);
            _messageTitle = _rootElement.Q<Label>(_elementNames.MessageTitle);
            _bodyContent = _rootElement.Q<Label>(_elementNames.MessageBody);
            _bodyTitle = _rootElement.Q<VisualElement>(_elementNames.BodyTitle);
            _body = _rootElement.Q<VisualElement>(_elementNames.Body);
            _icon = _rootElement.Q<VisualElement>(_elementNames.Icon);

            _closeButton = _rootElement.Q<Button>(_elementNames.Button);

            _closeButton.clicked += () =>
            {
                _currentSetting?.OnPrimaryClick?.Invoke();
                Hide();
            };
        }

        /// <summary>
        /// Sets the content of the window according to the SO
        /// </summary>
        /// <param name="modalInfo">Scriptable Object containing the modal information</param>
        public void HandleDisplayModal(InformationModalSO modalInfo)
        {
            _currentSetting = modalInfo;
            SetContent();
            StartCoroutine(Show());
        }

        /// <summary>
        /// Dispays the Information Modal
        /// </summary>
        private IEnumerator Show()
        {
            Interactions.Instance.TurnOffUI();
            yield return null;
            _rootElement.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Sets the content for the Information Window
        /// </summary>
        private void SetContent()
        {
            // Set the information window labels
            SetLabel(_windowTitle, _currentSetting.WindowTitle);
            SetLabel(_messageTitle, _currentSetting.BodyTitle);
            SetLabel(_bodyContent, _currentSetting.BodyMessage);

            // Set Button
            SetButton(_currentSetting.ButtonText);

            //Set Optional Elements
            SetOptionalElements();
        }

        /// <summary>
        ///     Checks current settings and sets the optional elements accordingly
        /// </summary>
        private void SetOptionalElements()
        {
            if (_currentSetting.IsIconEnabled)
            {
                _icon.style.display = DisplayStyle.Flex;
            }
            else
            {
                _icon.style.display = DisplayStyle.None;
            }


            if (_currentSetting.IsTitleEnabled)
            {
                _bodyTitle.style.display = DisplayStyle.Flex;
            }
            else
            {
                _bodyTitle.style.display = DisplayStyle.None;
                _body.style.alignSelf = Align.Center;
            }

        }

        /// <summary>
        /// Sets the button text and display style
        /// </summary>
        /// <param name="text"></param>
        private void SetButton(string text)
        {
            _closeButton.style.display = DisplayStyle.Flex;
            _closeButton.text = text;
        }

        /// <summary>
        /// Hides the window after the button is clicked.
        /// </summary>
        private void Hide()
        {
            _rootElement.style.display = DisplayStyle.None;
            Interactions.Instance.TurnOnUI();
            if (_currentSetting.KeepUIOff == true)
            {
                Interactions.Instance.SetUIEnabled(false);
            }
            if (_currentSetting.KeepBlurOn == true)
            {
                Blur.Activate();
            }

        }

        /// <summary>
        /// Sets the text for the label visual elements of the window
        /// </summary>
        /// <param name="label"></param>
        /// <param name="text"></param>
        private void SetLabel(Label label, string text)
        {
            if (string.IsNullOrEmpty(text.Trim()))
            {
                label.style.display = DisplayStyle.None;
            }
            else
            {
                label.text = text;
                label.style.display = DisplayStyle.Flex;
            }
        }
    }
}
