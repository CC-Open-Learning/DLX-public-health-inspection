using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Velcro;


namespace VARLab.PublicHealth
{
    public class SettingsMenuBuilder : MonoBehaviour
    {
        [SerializeField, Tooltip("Reference to the SettingsMenu UI Document")] private UIDocument _settings;
        [SerializeField, Tooltip("Reference to the MenuButtons UI Document")] private UIDocument _menu;
        [SerializeField, Tooltip("Reference to the Blur Background")] BlurBackground Blur;

        // Buttons
        private Button _settingsBtn; //From the menu
        private Button _closeBtn;

        // Toggle
        private SlideToggle _soundToggle;

        // Label
        private Label _soundLabel;

        // Sliders
        private Slider _volumeSlider;
        private Slider _soundEffectsSlider;
        private Slider _dialogueSlider;
        private Slider _mouseSensitivitySlider;

        // Constants UI
        private const string SettingBtnID = "SettingsBtn";
        private const string CloseBtnID = "CloseBtn";
        private const string SoundTagID = "SoundTag";
        private const string SoundToggleBtnID = "SoundToggleBtn";
        private const string VolumeSliderID = "VolumeSliderBtn";
        private const string SoundEffectsSliderID = "SoundEffectSliderBtn";
        private const string DialogueSliderID = "DialogueSliderBtn";
        private const string MouseSliderID = "MouseSliderBtn";

        // Constants Mouse Sensitivity
        private const float MinSensitivity = 0.1f;
        private const float MaxSensitivity = 2.0f;
        private const float DefaultSensitivity = 1.5f;

        // Constants Volume
        private const float MaxVolume = 1f;
        private const float MinVolume = 0.0001f;
        private const string MasterVolume = "MasterVolume";
        private const string VoiceVolume = "VoiceVolume";
        private const string SFXVolume = "SFXVolume";

        // Unity Events

        /// <summary>
        /// <see cref="AudioManager.ToggleMasterVolume(string, bool)"/>
        /// </summary>
        [SerializeField, Tooltip("Event triggered when the volume toggle changes")]
        private UnityEvent<string, bool> volumeToggleChanged;

        /// <summary>
        /// <see cref="AudioManager.SetVolume(string, float)"/>
        /// </summary>
        [SerializeField, Tooltip("Event triggered when the  voice volume slider changes")]
        private UnityEvent<string, float> volumeChanged;

        /// <summary>
        /// <see cref="PlayerController.UpdateCameraPanSensitivity"/>
        /// </summary>
        [SerializeField, Tooltip("Event triggered when the mouse sensitivity slider changes")]
        private UnityEvent<float> mouseSensitivityChanged;

        private void Start()
        {
            // Hide the settings menu on start
            _settings.rootVisualElement.style.display = DisplayStyle.None;

            GetReferences();

            SetUpListeners();

            SetUpMouseSlider();

            SetUpVolumeSliders();
        }

        private void SetUpVolumeSliders()
        {
            // Set the sound to on by default on start
            _soundToggle.value = true;

            // Master Volume
            _volumeSlider.lowValue = MinVolume;
            _volumeSlider.highValue = MaxVolume;
            _volumeSlider.value = MaxVolume;

            // Voice Over Volume
            _dialogueSlider.lowValue = MinVolume;
            _dialogueSlider.highValue = MaxVolume;
            _dialogueSlider.value = MaxVolume;

            // Sound Effects Volume
            _soundEffectsSlider.lowValue = MinVolume;
            _soundEffectsSlider.highValue = MaxVolume;
            _soundEffectsSlider.value = MaxVolume;
        }

        private void SetUpMouseSlider()
        {
            _mouseSensitivitySlider.lowValue = MinSensitivity;
            _mouseSensitivitySlider.highValue = MaxSensitivity;

            _mouseSensitivitySlider.value = DefaultSensitivity;

#if UNITY_EDITOR
            _mouseSensitivitySlider.value = 2f;
#endif
        }

        private void DisplayWindow()
        {
            _settings.rootVisualElement.style.display = DisplayStyle.Flex;
            _menu.rootVisualElement.style.display = DisplayStyle.None;
            Blur.Activate();
            StartCoroutine(InteractionsCoroutine(false));
            TimerManager.Instance.PauseTimers();
        }

        private void HideWindow()
        {
            _settings.rootVisualElement.style.display = DisplayStyle.None;
            _menu.rootVisualElement.style.display = DisplayStyle.Flex;
            Blur.Deactivate();
            StartCoroutine(InteractionsCoroutine(true));
            if (PlayerController.IntroCompleted && PlayerController.HasWashedHands)
            {
                TimerManager.Instance.StartTimers();
            }
        }

        /// <summary>
        /// Get all the visual elements references
        /// </summary>
        private void GetReferences()
        {
            // Menu settings button, used to open the settings menu
            _settingsBtn = _menu.rootVisualElement.Q<Button>(SettingBtnID);

            // Settings window
            _closeBtn = _settings.rootVisualElement.Q<Button>(CloseBtnID);

            _soundToggle = _settings.rootVisualElement.Q<TemplateContainer>(SoundToggleBtnID).Q<SlideToggle>();
            _soundLabel = _settings.rootVisualElement.Q<Label>(SoundTagID);

            _volumeSlider = _settings.rootVisualElement.Q<TemplateContainer>(VolumeSliderID).Q<Slider>();

            _soundEffectsSlider = _settings.rootVisualElement.Q<TemplateContainer>(SoundEffectsSliderID).Q<Slider>();

            _dialogueSlider = _settings.rootVisualElement.Q<TemplateContainer>(DialogueSliderID).Q<Slider>();

            _mouseSensitivitySlider = _settings.rootVisualElement.Q<TemplateContainer>(MouseSliderID).Q<Slider>();
        }

        /// <summary>
        /// Set up all the listeners for the window.
        /// </summary>
        private void SetUpListeners()
        {
            _settingsBtn.clicked += DisplayWindow;
            _closeBtn.clicked += HideWindow;
            _soundToggle.RegisterValueChangedCallback((_) => ToggleSound(_soundToggle.value));

            _mouseSensitivitySlider.RegisterValueChangedCallback((_) => mouseSensitivityChanged?.Invoke(_mouseSensitivitySlider.value));

            _volumeSlider.RegisterValueChangedCallback((_) =>
            {
                volumeChanged?.Invoke(MasterVolume, _volumeSlider.value);
            });
            _dialogueSlider.RegisterValueChangedCallback((_) => volumeChanged?.Invoke(VoiceVolume, _dialogueSlider.value));
            _soundEffectsSlider.RegisterValueChangedCallback((_) => volumeChanged?.Invoke(SFXVolume, _soundEffectsSlider.value));
        }

        /// <summary>
        /// Coroutine responsible for toggling interactions
        /// </summary>
        /// <param name="enabled">Determines if the interaction should be enabled or disabled.</param>
        private IEnumerator InteractionsCoroutine(bool enabled)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Interactions.Instance.SetInteract(enabled);
        }

        /// <summary>
        /// This function updates the toggle label, toggles the volume sliders,
        /// and toggles the audio mixer master volume.
        /// </summary>
        /// <param name="enabled"></param>
        private void ToggleSound(bool enabled)
        {
            // set the toggle switch label
            _soundLabel.text = enabled ? "ON" : "OFF";

            // enable/disable the volume sliders
            _volumeSlider.SetEnabled(enabled);
            _soundEffectsSlider.SetEnabled(enabled);
            _dialogueSlider.SetEnabled(enabled);

            volumeToggleChanged?.Invoke(MasterVolume, enabled);
        }
    }
}
