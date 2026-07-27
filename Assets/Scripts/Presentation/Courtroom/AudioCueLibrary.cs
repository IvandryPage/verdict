using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verdict.Presentation.Courtroom
{
    [Serializable]
    public sealed class AudioCueEntry
    {
        public string Id;
        public AudioClip Clip;
    }

    /// <summary>
    /// The data-driven bridge between a Narrative Event's free-text
    /// Parameter (e.g. "theme_tense") and an actual AudioClip asset.
    /// Authors add entries here; nothing in Systems/Runtime/Data needs
    /// to change to support new tracks or sounds.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioCueLibrary", menuName = "Verdict/Presentation/Audio Cue Library")]
    public sealed class AudioCueLibrary : ScriptableObject
    {
        [SerializeField] private List<AudioCueEntry> entries = new();

        public bool TryGetClip(string id, out AudioClip clip)
        {
            AudioCueEntry entry = entries.FirstOrDefault(e => e.Id == id);
            clip = entry?.Clip;
            return clip != null;
        }
    }
}
