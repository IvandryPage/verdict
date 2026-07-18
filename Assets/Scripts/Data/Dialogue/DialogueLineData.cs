using UnityEngine;
using Verdict.Data.Characters;

namespace Verdict.Data.Dialogue
{
    [System.Serializable]
    public sealed class DialogueLineData
    {
        public DialogueSpeakerType SpeakerType =
            DialogueSpeakerType.Character;

        public CharacterData Speaker;

        [TextArea(2,5)]
        public string Text;

        public CharacterEmotion Emotion;

        public DialogueWaitMode WaitMode =
            DialogueWaitMode.PlayerInput;

        public float AutoAdvanceDelay = 1f;
    }
}
