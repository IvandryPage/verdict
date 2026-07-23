using System;
using System.Collections.Generic;
using UnityEngine;
using Verdict.Data.Evidence;

namespace Verdict.Data.Cases
{
    /// <summary>
    /// Defines which player argument (action) can resolve a Claim, gated
    /// by a set of reusable ArgumentConditions (all must pass - AND
    /// logic), and the effects to apply on success or failure.
    /// </summary>
    [Serializable]
    public sealed class ArgumentRuleData
    {
        [SerializeField]
        private PlayerAction action;

        // SerializeReference is required here: ArgumentConditionData is
        // an abstract base with several concrete condition types, and
        // this is the only place gameplay composes them together.
        [SerializeField, SerializeReference]
        private List<ArgumentConditionData> conditions = new();

        [SerializeField]
        private List<CourtStateEffectData> successEffects = new();

        [SerializeField]
        private List<CourtStateEffectData> failureEffects = new();

        public PlayerAction Action => action;

        public IReadOnlyList<ArgumentConditionData> Conditions => conditions;

        public IReadOnlyList<CourtStateEffectData> SuccessEffects => successEffects;

        public IReadOnlyList<CourtStateEffectData> FailureEffects => failureEffects;

        public void SetAction(PlayerAction newAction)
        {
            action = newAction;
        }

        public void AddCondition(ArgumentConditionData condition)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            conditions.Add(condition);
        }

        public bool RemoveCondition(ArgumentConditionData condition)
        {
            if (condition == null)
                return false;

            return conditions.Remove(condition);
        }

        public void ClearConditions()
        {
            conditions.Clear();
        }

        public void AddSuccessEffect(
            CourtStateEffectData effect)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            successEffects.Add(effect);
        }

        public void InsertSuccessEffect(
            int index,
            CourtStateEffectData effect)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            if (index < 0 || index > successEffects.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            successEffects.Insert(index, effect);
        }

        public bool RemoveSuccessEffect(
            CourtStateEffectData effect)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            return successEffects.Remove(effect);
        }

        public void ClearSuccessEffects()
        {
            successEffects.Clear();
        }

        public void MoveSuccessEffect(
            int oldIndex,
            int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= successEffects.Count)
                throw new ArgumentOutOfRangeException(nameof(oldIndex));

            if (newIndex < 0 || newIndex >= successEffects.Count)
                throw new ArgumentOutOfRangeException(nameof(newIndex));

            CourtStateEffectData effect =
                successEffects[oldIndex];

            successEffects.RemoveAt(oldIndex);

            successEffects.Insert(newIndex, effect);
        }

        public void AddFailureEffect(
            CourtStateEffectData effect)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            failureEffects.Add(effect);
        }

        public void InsertFailureEffect(
            int index,
            CourtStateEffectData effect)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            if (index < 0 || index > failureEffects.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            failureEffects.Insert(index, effect);
        }

        public bool RemoveFailureEffect(
            CourtStateEffectData effect)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            return failureEffects.Remove(effect);
        }

        public void ClearFailureEffects()
        {
            failureEffects.Clear();
        }

        public void MoveFailureEffect(
            int oldIndex,
            int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= failureEffects.Count)
                throw new ArgumentOutOfRangeException(nameof(oldIndex));

            if (newIndex < 0 || newIndex >= failureEffects.Count)
                throw new ArgumentOutOfRangeException(nameof(newIndex));

            CourtStateEffectData effect =
                failureEffects[oldIndex];

            failureEffects.RemoveAt(oldIndex);

            failureEffects.Insert(newIndex, effect);
        }
    }
}
