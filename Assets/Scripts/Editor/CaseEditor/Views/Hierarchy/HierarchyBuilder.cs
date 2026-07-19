using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Verdict.Data.Cases;

namespace Verdict.Editor.CaseEditor.Hierarchy
{
    public sealed class HierarchyBuilder
    {
        public string Query { get; set; }

        private int nextId;

        private readonly Dictionary<string, int> itemIds =
            new(StringComparer.Ordinal);

        public List<TreeViewItemData<HierarchyItem>> Build(
            EditorSession session)
        {
            List<TreeViewItemData<HierarchyItem>> witnesses =
                new();

            foreach (WitnessData witness in session.CurrentCase.Witnesses)
            {
                if (ShouldIncludeWitness(session, witness, out var witnessItem))
                {
                    witnesses.Add(witnessItem);
                }
            }

            if (!string.IsNullOrWhiteSpace(Query) &&
                !MatchesQuery(session.CurrentCase.name) &&
                witnesses.Count == 0)
            {
                return new();
            }

            return new()
            {
                new TreeViewItemData<HierarchyItem>(
                    GetItemId($"case:{session.CurrentCase.name}"),
                    new HierarchyItem
                    {
                        Id = GetItemId($"case:{session.CurrentCase.name}"),
                        Key = $"case:{session.CurrentCase.name}",
                        Name = session.CurrentCase.name,
                        Type = HierarchyType.Case
                    },
                    witnesses)
            };
        }

        private bool ShouldIncludeWitness(
            EditorSession session,
            WitnessData witness,
            out TreeViewItemData<HierarchyItem> item)
        {
            session.TryGetWitnessContext(
                witness.Id,
                out WitnessContext witnessContext);

            List<TreeViewItemData<HierarchyItem>> testimonies =
                new();

            foreach (TestimonyData testimony in witness.Testimonies)
            {
                if (ShouldIncludeTestimony(
                        session,
                        witness,
                        testimony,
                        out var testimonyItem))
                {
                    testimonies.Add(testimonyItem);
                }
            }

            bool selfMatch =
                MatchesQuery(
                    HierarchyDisplayUtility.GetWitnessName(witness));

            if (!selfMatch && testimonies.Count == 0)
            {
                item = default;
                return false;
            }

            int id =
                GetItemId($"witness:{witness.Id}");

            item =
                new TreeViewItemData<HierarchyItem>(
                    id,
                    new HierarchyItem
                    {
                        Id = id,
                        Key = $"witness:{witness.Id}",
                        Name = HierarchyDisplayUtility.GetWitnessName(witness),
                        Type = HierarchyType.Witness,
                        Witness = witnessContext
                    },
                    testimonies);

            return true;
        }

        private bool ShouldIncludeTestimony(
            EditorSession session,
            WitnessData witness,
            TestimonyData testimony,
            out TreeViewItemData<HierarchyItem> item)
        {
            session.TryGetTestimonyContext(
                testimony.Id,
                out TestimonyContext testimonyContext);

            List<TreeViewItemData<HierarchyItem>> statements =
                new();

            foreach (StatementData statement in testimony.Statements)
            {
                if (ShouldIncludeStatement(
                        session,
                        witness,
                        testimony,
                        statement,
                        out var statementItem))
                {
                    statements.Add(statementItem);
                }
            }

            bool selfMatch =
                MatchesQuery(
                    HierarchyDisplayUtility.GetTestimonyName(testimony));

            if (!selfMatch && statements.Count == 0)
            {
                item = default;
                return false;
            }

            int id =
                GetItemId($"testimony:{testimony.Id}");

            item =
                new TreeViewItemData<HierarchyItem>(
                    id,
                    new HierarchyItem
                    {
                        Id = id,
                        Key = $"testimony:{testimony.Id}",
                        Name = HierarchyDisplayUtility.GetTestimonyName(testimony),
                        Type = HierarchyType.Testimony,
                        Testimony = testimonyContext
                    },
                    statements);

            return true;
        }

        private bool ShouldIncludeStatement(
            EditorSession session,
            WitnessData witness,
            TestimonyData testimony,
            StatementData statement,
            out TreeViewItemData<HierarchyItem> item)
        {
            bool match =
                MatchesQuery(
                    HierarchyDisplayUtility.GetStatementName(statement));

            if (!match)
            {
                item = default;
                return false;
            }

            StatementContext context =
                session.CreateStatementContext(
                    witness,
                    testimony,
                    statement);

            int id =
                GetItemId($"statement:{statement.Id}");

            item =
                new TreeViewItemData<HierarchyItem>(
                    id,
                    new HierarchyItem
                    {
                        Id = id,
                        Key = $"statement:{statement.Id}",
                        Name = HierarchyDisplayUtility.GetStatementName(statement),
                        Type = HierarchyType.Statement,
                        Statement = context
                    });

            return true;
        }

        private int GetItemId(
            string key)
        {
            if (itemIds.TryGetValue(key, out int id))
                return id;

            id = ++nextId;
            itemIds[key] = id;

            return id;
        }

        public bool TryGetItemId(string key, out int itemId)
        {
            return itemIds.TryGetValue(key, out itemId);
        }

        private bool MatchesQuery(
            string value)
        {
            return string.IsNullOrWhiteSpace(Query) ||
                   value.IndexOf(
                       Query,
                       StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
