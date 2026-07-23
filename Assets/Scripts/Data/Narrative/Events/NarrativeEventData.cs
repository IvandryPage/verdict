using System;

namespace Verdict.Data.Narrative
{
    /// <summary>
    /// One presentation cue. Parameter is a free-form id whose meaning
    /// depends on Type (a music track name, an SFX clip id, a camera
    /// preset name, a background id...). Value is an optional number for
    /// cues that need one (fade duration, shake intensity, move duration).
    /// The actual camera/audio system that reads these doesn't exist yet
    /// in this project - authoring this just prepares the data for it.
    /// </summary>
    [Serializable]
    public sealed class NarrativeEventData
    {
        public NarrativeEventType Type;

        public string Parameter;

        public float Value;
    }
}
