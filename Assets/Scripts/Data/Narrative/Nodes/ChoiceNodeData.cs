using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verdict.Data.Narrative
{
    /// <summary>
    /// Presents choices to the player. Pauses the graph until a choice
    /// is selected, then advances to that choice's NextNodeId.
    /// </summary>
    [Serializable]
    public sealed class ChoiceNodeData : NarrativeNodeData
    {
        [SerializeField]
        private List<NarrativeChoiceOptionData> choices = new();

        public ChoiceNodeData()
        {
        }

        public ChoiceNodeData(string nodeId, Vector2 position)
            : base(nodeId, position)
        {
        }

        public IReadOnlyList<NarrativeChoiceOptionData> Choices => choices;

        public NarrativeChoiceOptionData AddChoice(string text = "New Choice", string nextNodeId = null)
        {
            var choice = new NarrativeChoiceOptionData(text, nextNodeId);
            choices.Add(choice);
            return choice;
        }

        public void RemoveChoiceAt(int index)
        {
            if (index < 0 || index >= choices.Count)
            {
                return;
            }

            choices.RemoveAt(index);
        }

        public bool RemoveChoice(NarrativeChoiceOptionData choice)
        {
            if (choice == null)
            {
                return false;
            }

            return choices.Remove(choice);
        }

        public override IEnumerable<string> GetOutgoingNodeIds()
        {
            return choices
                .Where(choice => !string.IsNullOrWhiteSpace(choice.NextNodeId))
                .Select(choice => choice.NextNodeId);
        }
    }
}
