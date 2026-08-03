using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verdict.Data.Narrative;

namespace Verdict.Presentation.Courtroom
{
    [Serializable]
    public sealed class FeatureUnlockEntry
    {
        public string Id;
        public GameObject Target;
    }

    /// <summary>
    /// Example IGameplayEventHandler for the UnlockFeature category -
    /// activates a registered GameObject (a new UI button, a tool icon,
    /// whatever) by matching GameplayEventId. StartMinigame/Checkpoint
    /// are logged as hooks here since their actual behaviour is entirely
    /// project-specific - copy this file's pattern for those instead of
    /// extending it.
    /// </summary>
    public sealed class FeatureUnlockHandler : MonoBehaviour, IGameplayEventHandler
    {
        [SerializeField] private List<FeatureUnlockEntry> features = new();

        public bool CanHandle(GameplayEventCategory category)
        {
            return category == GameplayEventCategory.UnlockFeature;
        }

        public void Handle(GameplayNodeData node)
        {
            FeatureUnlockEntry entry = features.FirstOrDefault(f => f.Id == node.GameplayEventId);

            if (entry?.Target == null)
            {
                Debug.LogWarning($"{nameof(FeatureUnlockHandler)}: No feature registered for id '{node.GameplayEventId}'.");
                return;
            }

            entry.Target.SetActive(true);
        }
    }
}
