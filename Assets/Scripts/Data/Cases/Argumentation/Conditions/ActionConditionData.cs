using System;
using UnityEngine;

namespace Verdict.Data.Cases
{
    /// <summary>
    /// Passes when the player's current argument used a specific action.
    /// Usually redundant with ArgumentRuleData.Action (which already
    /// filters by action before conditions run), but useful when a rule
    /// wants to compose this alongside other conditions explicitly, or
    /// for future composite/pattern-based rules.
    /// </summary>
    [Serializable]
    public sealed class ActionConditionData : ArgumentConditionData
    {
        [SerializeField]
        private PlayerAction requiredAction;

        public ActionConditionData()
        {
        }

        public ActionConditionData(PlayerAction requiredAction)
        {
            this.requiredAction = requiredAction;
        }

        public PlayerAction RequiredAction => requiredAction;

        public void SetRequiredAction(PlayerAction action)
        {
            requiredAction = action;
        }
    }
}
