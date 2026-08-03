using System;
using UnityEngine;

namespace Verdict.Presentation.Settings
{
    /// <summary>
    /// The smallest useful settings surface: how fast dialogue types out,
    /// and three volume sliders. Values are normalized 0-1 everywhere -
    /// anything that needs a different range (like TypewriterText's
    /// characters-per-second) converts internally, so the UI and save
    /// data never have to think about those units.
    /// </summary>
    public static class GameSettings
    {
        private const string DialogueSpeedKey = "verdict.settings.dialogue_speed";
        private const string MasterVolumeKey = "verdict.settings.master_volume";
        private const string MusicVolumeKey = "verdict.settings.music_volume";
        private const string SfxVolumeKey = "verdict.settings.sfx_volume";

        private const float DefaultDialogueSpeed = 0.5f;
        private const float DefaultVolume = 1f;

        public static event Action Changed;

        public static float DialogueSpeed
        {
            get => PlayerPrefs.GetFloat(DialogueSpeedKey, DefaultDialogueSpeed);
            set => SetValue(DialogueSpeedKey, Mathf.Clamp01(value));
        }

        public static float MasterVolume
        {
            get => PlayerPrefs.GetFloat(MasterVolumeKey, DefaultVolume);
            set => SetValue(MasterVolumeKey, Mathf.Clamp01(value));
        }

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicVolumeKey, DefaultVolume);
            set => SetValue(MusicVolumeKey, Mathf.Clamp01(value));
        }

        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat(SfxVolumeKey, DefaultVolume);
            set => SetValue(SfxVolumeKey, Mathf.Clamp01(value));
        }

        /// <summary>
        /// Converts the normalized 0-1 DialogueSpeed into the characters-
        /// per-second unit TypewriterText actually uses. 0 = deliberately
        /// slow, 1 = near-instant.
        /// </summary>
        public static float GetDialogueCharactersPerSecond()
        {
            const float minCharsPerSecond = 12f;
            const float maxCharsPerSecond = 120f;

            return Mathf.Lerp(minCharsPerSecond, maxCharsPerSecond, DialogueSpeed);
        }

        /// <summary>
        /// The effective volume for a channel after Master is applied -
        /// what an AudioSource's .volume should actually be set to.
        /// </summary>
        public static float GetEffectiveMusicVolume() => MasterVolume * MusicVolume;

        public static float GetEffectiveSfxVolume() => MasterVolume * SfxVolume;

        private static void SetValue(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();

            Changed?.Invoke();
        }
    }
}
