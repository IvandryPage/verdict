using System;
using UnityEngine;

namespace Verdict.Data.Characters
{
    [Serializable]
    public class PortraitEntry
    {
        [field: SerializeField]
        public CharacterEmotion Emotion { get; private set; }

        [field: SerializeField]
        public Sprite Portrait { get; private set; }
    }
}
