using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Verdict
{
    /// <summary>
    /// Minimal audio bridge for narrative dialogue events.
    /// Music and SFX are looked up by string id and can be assigned in the
    /// inspector without requiring a complex sound manager.
    /// </summary>
    public sealed class NarrativeAudioController : MonoBehaviour
    {
        [System.Serializable]
        public sealed class AudioCue
        {
            public string Id;
            public AudioClip Clip;
            [Range(0f, 1f)] public float Volume = 1f;
            [Range(-3f, 3f)] public float Pitch = 1f;
        }

        [Header("Music")]
        [SerializeField]
        private AudioSource musicSource;

        [SerializeField]
        private List<AudioCue> musicClips = new();

        [Header("SFX")]
        [SerializeField]
        private AudioSource sfxSource;

        [SerializeField]
        private List<AudioCue> sfxClips = new();

        [Header("UI Click")]
        [SerializeField]
        private string clickSoundId = "ui_click";

        [SerializeField]
        private float clickCooldownSeconds = 0.08f;

        private float lastClickTime;

        private void Update()
        {
            if (Mouse.current == null)
            {
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame &&
                Time.unscaledTime - lastClickTime >= clickCooldownSeconds)
            {
                lastClickTime = Time.unscaledTime;
                PlaySound(clickSoundId, 1f);
            }
        }

        public void PlayMusic(string musicId, float volume = 1f)
        {
            if (string.IsNullOrWhiteSpace(musicId))
            {
                return;
            }

            if (musicSource == null)
            {
                Debug.LogWarning($"[NarrativeAudioController] Missing music source for '{musicId}'.");
                return;
            }

            AudioCue cue = FindCue(musicClips, musicId);
            if (cue == null || cue.Clip == null)
            {
                Debug.LogWarning($"[NarrativeAudioController] Music clip '{musicId}' not found.");
                return;
            }

            musicSource.clip = cue.Clip;
            musicSource.volume = Mathf.Clamp01(volume * cue.Volume);
            musicSource.pitch = cue.Pitch;
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }

        public void StopMusic(string musicId = null)
        {
            if (musicSource == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(musicId))
            {
                AudioCue cue = FindCue(musicClips, musicId);
                if (cue == null || cue.Clip == null)
                {
                    return;
                }

                if (musicSource.clip == cue.Clip)
                {
                    musicSource.Stop();
                }
                return;
            }

            musicSource.Stop();
        }

        public void PlaySound(string soundId, float volume = 1f)
        {
            if (string.IsNullOrWhiteSpace(soundId))
            {
                return;
            }

            if (sfxSource == null)
            {
                Debug.LogWarning($"[NarrativeAudioController] Missing SFX source for '{soundId}'.");
                return;
            }

            AudioCue cue = FindCue(sfxClips, soundId);
            if (cue == null || cue.Clip == null)
            {
                Debug.LogWarning($"[NarrativeAudioController] SFX clip '{soundId}' not found.");
                return;
            }

            sfxSource.PlayOneShot(cue.Clip, Mathf.Clamp01(volume * cue.Volume));
        }

        private static AudioCue FindCue(List<AudioCue> cues, string id)
        {
            if (cues == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            foreach (AudioCue cue in cues)
            {
                if (cue != null && cue.Id == id)
                {
                    return cue;
                }
            }

            return null;
        }
    }
}
