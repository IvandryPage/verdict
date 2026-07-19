using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Narrative
{
    /// <summary>
    /// Plays a short sequence of dialogue entries, then advances to
    /// NextNodeId. Contains dialogue content. Nothing else - no gameplay,
    /// no reference to Statement.
    /// </summary>
    [Serializable]
    public sealed class DialogueNodeData : NarrativeNodeData
    {
        [SerializeField]
        private List<NarrativeDialogueEntryData> entries = new();

        [SerializeField]
        private string nextNodeId;

        public DialogueNodeData()
        {
        }

        public DialogueNodeData(string nodeId, Vector2 position)
            : base(nodeId, position)
        {
        }

        public IReadOnlyList<NarrativeDialogueEntryData> Entries => entries;

        public string NextNodeId => nextNodeId;

        public void SetNextNodeId(string id)
        {
            nextNodeId = id;
        }

        public void AddEntry(NarrativeDialogueEntryData entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            entries.Add(entry);
        }

        public void InsertEntry(int index, NarrativeDialogueEntryData entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            index = Mathf.Clamp(index, 0, entries.Count);
            entries.Insert(index, entry);
        }

        public void RemoveEntryAt(int index)
        {
            if (index < 0 || index >= entries.Count)
            {
                return;
            }

            entries.RemoveAt(index);
        }

        public void MoveEntry(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= entries.Count)
            {
                return;
            }

            toIndex = Mathf.Clamp(toIndex, 0, entries.Count - 1);

            NarrativeDialogueEntryData entry = entries[fromIndex];
            entries.RemoveAt(fromIndex);
            entries.Insert(toIndex, entry);
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
