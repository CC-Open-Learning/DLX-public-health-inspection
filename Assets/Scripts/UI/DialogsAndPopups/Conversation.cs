using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    public class Conversation : MonoBehaviour
    {
        [SerializeField] private VisualTreeAsset _conversationBodyUXML;
        public BlurBackground Blur;

        public UnityEvent OnConversationEnded;

        /// Invokes <see cref="VoiceOverManager.PlayAudioClips(AudioSegment, AudioSegment, float, float)"/>
        public UnityEvent<AudioClip[]> OnConversationStarted;

        private VisualElement _root;
        private Label _windowName;
        private Label _scenario;
        private Label _scenarioText;
        private Button _primaryBtn;
        private ConversationSO _currentConversation;
        private List<AudioClip> _audioClips;

        private int _currentIndex = 0;
        private const int DefaultBorderRadius = 10;

        private void Start()
        {
            GetReferences();

            SetUpListeners();

            _root.style.display = DisplayStyle.None;

            if (OnConversationEnded == null) { OnConversationEnded = new UnityEvent(); }

            if (OnConversationStarted == null) { OnConversationStarted = new UnityEvent<AudioClip[]>(); }
        }

        private void SetUpListeners()
        {
            _primaryBtn.clicked += () =>
            {
                OnConversationEnded?.Invoke();
                HandleResetConversation();
            };
        }

        /// <summary>
        /// Invoked from <see cref="IntroductionHandler.OnIntroStart"/>
        /// Invoked from <see cref="OfficeHandler.OnOfficeArrive"/>
        /// This method is supposed to be a single access public method to populate the conversation with the
        /// dialogue entries in ConversationSO and display it
        /// </summary>
        public void HandleDisplayConversation(ConversationSO conversation)
        {
            _currentIndex = 0;
            _currentConversation = conversation;

            HandleSetContent();
            HandleShowConversation();
            Interactions.Instance.TurnOffUI();
            VoiceOverManager.Instance.PlayAudioClipsFromList(_audioClips, 0.5f, 0.5f);
            StartCoroutine(InteractionsRoutine(false));
        }

        /// <summary>
        /// This method iterates over all dialogue entries in ConversationSO scriptable object which populates
        /// the conversation UI. Every dialogue entry specifies the speaker, text, image, text colour by using
        /// rich text tags, and the image border colour. Every other dialogue entry is set to row reverse to 
        /// mimic a mobile conversation
        /// </summary>
        public void HandleSetContent()
        {
            _windowName.text = _currentConversation.Name;
            _scenario.text = _currentConversation.Scenario;
            _scenarioText.text = _currentConversation.ScenarioText;
            _primaryBtn.text = _currentConversation.ButtonText;
            _audioClips = new List<AudioClip>();

            foreach (ConversationSO.Dialogue dialogue in _currentConversation.dialogue)
            {
                _audioClips.Add(dialogue.AudioClip);

                VisualElement bodyElement = _conversationBodyUXML.CloneTree();
                VisualElement border = bodyElement.Q<VisualElement>("Border");
                VisualElement body = bodyElement.Q<VisualElement>("Body");
                VisualElement image = bodyElement.Q<VisualElement>("Image");
                Label description = bodyElement.Q<Label>("Description");
                Label caption = bodyElement.Q<Label>("Caption");

                border.style.borderBottomColor = dialogue.ImageBorderColour;
                border.style.borderTopColor = dialogue.ImageBorderColour;
                border.style.borderLeftColor = dialogue.ImageBorderColour;
                border.style.borderRightColor = dialogue.ImageBorderColour;
                image.style.backgroundImage = dialogue.Avatar.texture;
                caption.text = dialogue.Speaker;
                description.text = dialogue.Text;
                description.style.backgroundColor = dialogue.TextBackgroundColour;
                description.style.borderTopColor = dialogue.TextBorderTop;
                description.style.borderBottomColor = dialogue.TextBorderBottom;
                description.style.borderLeftColor = dialogue.TextBorderLeft;
                description.style.borderRightColor = dialogue.TextBorderRight;

                // Align every other dialogue to the right with row-reverse and change the border radius of the background
                // The alignment for the first speaker can be defined in the ConversationSO.
                if (_currentIndex % 2 != 0 && _currentConversation.StartDialogueOnLeft)
                {
                    body.Q<VisualElement>("Body").style.flexDirection = FlexDirection.RowReverse;
                    description.style.borderTopLeftRadius = DefaultBorderRadius;
                    description.style.borderTopRightRadius = 0;
                    caption.style.alignSelf = Align.FlexEnd;
                }
                else if (_currentIndex % 2 == 0 && !_currentConversation.StartDialogueOnLeft)
                {
                    body.Q<VisualElement>("Body").style.flexDirection = FlexDirection.RowReverse;
                    description.style.borderTopLeftRadius = DefaultBorderRadius;
                    description.style.borderTopRightRadius = 0;
                    caption.style.alignSelf = Align.FlexEnd;
                }

                _currentIndex++;
                body.AddToClassList("body");
                _root.Q<VisualElement>("Content").Add(bodyElement);
            }
        }


        public void HandleResetConversation()
        {
            _root.style.display = DisplayStyle.None;
            Blur.Activate();
            Interactions.Instance.TurnOnUI();
            StartCoroutine(InteractionsRoutine(true));
            VoiceOverManager.Instance.StopAllClips();
            _currentIndex = 0;
            _currentConversation = null;
            _root.Q<VisualElement>("Content").Clear();
        }

        public void HandleShowConversation()
        {
            _root.style.display = DisplayStyle.Flex;
            Blur.Activate();
        }

        private void GetReferences()
        {
            _root = gameObject.GetComponent<UIDocument>().rootVisualElement;

            _windowName = _root.Q<Label>("Name");
            _scenario = _root.Q<Label>("Scenario");
            _scenarioText = _root.Q<Label>("ScenarioText");
            _primaryBtn = _root.Q<Button>("BtnPrimary");
        }

        /// <summary>
        /// This Coroutine is to set interactions on/off with an update delay to prevent race conditions
        /// </summary>
        /// <param name="tf"> which way to turn the interactions
        /// True = on
        /// False = off
        /// </param>
        private IEnumerator InteractionsRoutine(bool tf)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Interactions.Instance.SetInteract(tf);
            Interactions.Instance.SetUIEnabled(tf);
        }
    }
}