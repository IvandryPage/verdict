using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Narrative
{
    /// <summary>
    /// Broad category for a Gameplay node, so it's self-documenting in
    /// the editor instead of being an opaque free-text id. Pick Custom
    /// when none of the built-in categories fit your project.
    /// </summary>
    public enum GameplayEventCategory
    {
        /// <summary>Anything project-specific - GameplayEventId's meaning is up to your own code.</summary>
        Custom,

        /// <summary>Unlocks a gameplay mechanic or UI feature (e.g. a new investigation tool) that isn't a CourtStateEffect.</summary>
        UnlockFeature,

        /// <summary>Launches a specific minigame or special interactive sequence.</summary>
        StartMinigame,

        /// <summary>Marks a save/checkpoint/milestone point your game code can react to.</summary>
        Checkpoint
    }

    /// <summary>
    /// Hook into your OWN gameplay systems - not presentation. Use
    /// Dialogue Node Events for camera/music/sound; use this for things
    /// like unlocking a mechanic, launching a minigame, or marking a
    /// checkpoint. Fires GameplayEventId (plus Category, for context) and
    /// immediately continues to NextNodeId - it does not pause on its
    /// own. Note: a Statement node already announces itself when the
    /// graph reaches it (via StatementReached), and a Choice node
    /// already announces itself (via ChoiceRequested) - you don't need a
    /// Gameplay node just to "signal a choice is coming"; use one when
    /// you need to trigger something those nodes don't already cover.
    /// </summary>
    [Serializable]
    public sealed class GameplayNodeData : NarrativeNodeData
    {
        [SerializeField]
        private GameplayEventCategory category = GameplayEventCategory.Custom;

        [SerializeField]
        private string gameplayEventId;

        [SerializeField]
        private string nextNodeId;

        public GameplayNodeData()
        {
        }

        public GameplayNodeData(string nodeId, Vector2 position)
            : base(nodeId, position)
        {
        }

        public GameplayEventCategory Category => category;

        public string GameplayEventId => gameplayEventId;

        public string NextNodeId => nextNodeId;

        public void SetCategory(GameplayEventCategory newCategory)
        {
            category = newCategory;
        }

        public void SetGameplayEventId(string id)
        {
            gameplayEventId = id;
        }

        public void SetNextNodeId(string id)
        {
            nextNodeId = id;
        }

        public override IEnumerable<string> GetOutgoingNodeIds()
        {
            if (!string.IsNullOrWhiteSpace(nextNodeId))
            {
                yield return nextNodeId;
            }
        }
    }
}
