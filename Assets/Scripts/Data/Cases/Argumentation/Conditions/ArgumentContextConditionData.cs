using System;
using UnityEngine;

namespace Verdict.Data.Cases
{
    /// <summary>
    /// Passes when a key in the player's argument AdditionalContext bag
    /// equals a required value. This is the generic escape hatch for
    /// authoring-driven conditions (dialogue choices made, minigame
    /// outcomes, etc) that don't warrant a dedicated condition type and
    /// new C# code.
    /// </summary>
    [Serializable]
    public sealed class ArgumentContextConditionData : ArgumentConditionData
    {
        [SerializeField]
        private string contextKey;

        [SerializeField]
        private string requiredValue;

        public ArgumentContextConditionData()
        {
        }

        public ArgumentContextConditionData(string contextKey, string requiredValue)
        {
            this.contextKey = contextKey;
            this.requiredValue = requiredValue;
        }

        public string ContextKey => contextKey;

        public string RequiredValue => requiredValue;

        public void SetContextKey(string key) => contextKey = key;

        public void SetRequiredValue(string value) => requiredValue = value;
    }
}
