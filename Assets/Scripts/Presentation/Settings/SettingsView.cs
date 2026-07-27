using UnityEngine;
using UnityEngine.UI;

namespace Verdict.Presentation.Settings
{
    /// <summary>
    /// Binds a handful of sliders to GameSettings. Not tied to
    /// CourtroomBootstrap on purpose - this needs to work from the main
    /// menu too, before any case is loaded.
    /// </summary>
    public sealed class SettingsView : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("Sliders")]
        [SerializeField] private Slider dialogueSpeedSlider;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Actions")]
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            SetVisible(false);
        }

        private void OnEnable()
        {
            closeButton?.onClick.AddListener(Close);

            dialogueSpeedSlider?.onValueChanged.AddListener(HandleDialogueSpeedChanged);
            masterVolumeSlider?.onValueChanged.AddListener(HandleMasterVolumeChanged);
            musicVolumeSlider?.onValueChanged.AddListener(HandleMusicVolumeChanged);
            sfxVolumeSlider?.onValueChanged.AddListener(HandleSfxVolumeChanged);
        }

        private void OnDisable()
        {
            closeButton?.onClick.RemoveListener(Close);

            dialogueSpeedSlider?.onValueChanged.RemoveListener(HandleDialogueSpeedChanged);
            masterVolumeSlider?.onValueChanged.RemoveListener(HandleMasterVolumeChanged);
            musicVolumeSlider?.onValueChanged.RemoveListener(HandleMusicVolumeChanged);
            sfxVolumeSlider?.onValueChanged.RemoveListener(HandleSfxVolumeChanged);
        }

        public void Open()
        {
            RefreshFromSettings();
            SetVisible(true);
        }

        public void Close()
        {
            SetVisible(false);
        }

        private void RefreshFromSettings()
        {
            if (dialogueSpeedSlider != null) dialogueSpeedSlider.SetValueWithoutNotify(GameSettings.DialogueSpeed);
            if (masterVolumeSlider != null) masterVolumeSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
            if (musicVolumeSlider != null) musicVolumeSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
            if (sfxVolumeSlider != null) sfxVolumeSlider.SetValueWithoutNotify(GameSettings.SfxVolume);
        }

        private void HandleDialogueSpeedChanged(float value) => GameSettings.DialogueSpeed = value;

        private void HandleMasterVolumeChanged(float value) => GameSettings.MasterVolume = value;

        private void HandleMusicVolumeChanged(float value) => GameSettings.MusicVolume = value;

        private void HandleSfxVolumeChanged(float value) => GameSettings.SfxVolume = value;

        private void SetVisible(bool visible)
        {
            root?.SetActive(visible);
        }
    }
}
