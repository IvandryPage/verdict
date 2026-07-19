using UnityEngine;
using Verdict.Data.Characters;

namespace Verdict.Data.Narrative
{
    [System.Serializable]
    public sealed class NarrativeLineData
    {
        public NarrativeSpeakerType SpeakerType =
            NarrativeSpeakerType.Character;

        public CharacterData Speaker;

        [TextArea(2, 5)]
        public string Text;

        public CharacterEmotion Emotion;

        public NarrativeWaitMode WaitMode =
            NarrativeWaitMode.PlayerInput;

        public float AutoAdvanceDelay = 1f;
    }
}
