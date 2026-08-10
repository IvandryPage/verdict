using System;
using UnityEngine;
using Verdict.Data.Characters;

namespace Verdict.Systems.Narrative
{
    /// <summary>
    /// Resolves a character portrait from a CharacterData asset
    /// using the requested emotion.
    ///
    /// This class does not decide what emotion a character should have.
    /// It only resolves the visual asset for an already-selected emotion.
    /// </summary>
    public static class CharacterPortraitResolver
    {
        public static Sprite Resolve(
            CharacterData character,
            CharacterEmotion emotion)
        {
            if (character == null)
            {
                return null;
            }

            foreach (PortraitEntry portrait in character.Portraits)
            {
                if (portrait.Emotion == emotion)
                {
                    return portrait.Portrait;
                }
            }

            return null;
        }
    }
}
