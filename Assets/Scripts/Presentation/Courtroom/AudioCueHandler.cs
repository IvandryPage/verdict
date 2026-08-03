using UnityEngine;
using Verdict.Data.Narrative;
using Verdict.Presentation.Settings;

namespace Verdict.Presentation.Courtroom
{
    /// <summary>
    /// Example IPresentationEventHandler for audio cues. Register this on
    /// PresentationEventRouter's handler list. Value on the event is an
    /// authored 0-1 "how loud is this specific cue" factor (defaults to
    /// 1) - the actual volume applied is that times the player's Master
    /// and Music/SFX settings, so a settings change is heard immediately
    /// even on music that's already playing.
    /// </summary>
    public sealed class AudioCueHandler : MonoBehaviour, IPresentationEventHandler
    {
        [SerializeField] private AudioCueLibrary library;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        private float authoredMusicVolume = 1f;

        private void OnEnable()
        {
            GameSettings.Changed += HandleSettingsChanged;
        }

        private void OnDisable()
        {
            GameSettings.Changed -= HandleSettingsChanged;
        }

        public bool CanHandle(NarrativeEventType type)
        {
            return type is NarrativeEventType.PlayMusic
                or NarrativeEventType.StopMusic
                or NarrativeEventType.PlaySound;
        }

        public void Handle(NarrativeEventData eventData)
        {
            switch (eventData.Type)
            {
                case NarrativeEventType.PlayMusic:
                    PlayMusic(eventData);
                    break;

                case NarrativeEventType.StopMusic:
                    StopMusic();
                    break;

                case NarrativeEventType.PlaySound:
                    PlaySound(eventData);
                    break;
            }
        }

        private void PlayMusic(NarrativeEventData eventData)
        {
            if (musicSource == null || library == null)
            {
                return;
            }

            if (!library.TryGetClip(eventData.Parameter, out AudioClip clip))
            {
                Debug.LogWarning($"{nameof(AudioCueHandler)}: No music clip registered for id '{eventData.Parameter}'.");
                return;
            }

            authoredMusicVolume = eventData.Value > 0f ? eventData.Value : 1f;

            musicSource.clip = clip;
            musicSource.volume = authoredMusicVolume * GameSettings.GetEffectiveMusicVolume();
            musicSource.loop = true;
            musicSource.Play();
        }

        private void StopMusic()
        {
            musicSource?.Stop();
        }

        private void PlaySound(NarrativeEventData eventData)
        {
            if (sfxSource == null || library == null)
            {
                return;
            }

            if (!library.TryGetClip(eventData.Parameter, out AudioClip clip))
            {
                Debug.LogWarning($"{nameof(AudioCueHandler)}: No SFX clip registered for id '{eventData.Parameter}'.");
                return;
            }

            float authoredVolume = eventData.Value > 0f ? eventData.Value : 1f;

            sfxSource.PlayOneShot(clip, authoredVolume * GameSettings.GetEffectiveSfxVolume());
        }

        private void HandleSettingsChanged()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.volume = authoredMusicVolume * GameSettings.GetEffectiveMusicVolume();
            }
        }
    }
}
