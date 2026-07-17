using System.Collections.Generic;
using UnityEngine.UIElements;
using Verdict.Data.Cases;

namespace Verdict.Editor.CaseFlow.Hierarchy
{
    public sealed class HierarchyBuilder
    {
        private int id;

        public List<TreeViewItemData<HierarchyItem>> Build(EditorSession session)
        {
            id = 0;

            List<TreeViewItemData<HierarchyItem>> roots = new();

            List<TreeViewItemData<HierarchyItem>> witnesses = new();

            foreach (WitnessData witness in session.CurrentCase.Witnesses)
            {
                witnesses.Add(BuildWitness(session, witness));
            }

            roots.Add(
                new TreeViewItemData<HierarchyItem>(
                    NextId(),
                    new HierarchyItem
                    {
                        Name = session.CurrentCase.name,
                        Type = HierarchyType.Case
                    },
                    witnesses));

            return roots;
        }

        private TreeViewItemData<HierarchyItem> BuildWitness(
            EditorSession session,
            WitnessData witness)
        {
            session.TryGetWitnessContext(
                witness.Id,
                out WitnessContext witnessContext);

            List<TreeViewItemData<HierarchyItem>> testimonies = new();

            foreach (TestimonyData testimony in witness.Testimonies)
            {
                testimonies.Add(
                    BuildTestimony(
                        session,
                        witness,
                        testimony));
            }

            return new TreeViewItemData<HierarchyItem>(
                NextId(),
                new HierarchyItem
                {
                    Name = HierarchyDisplayUtility.GetWitnessName(witness),
                    Type = HierarchyType.Witness,
                    Witness = witnessContext
                },
                testimonies);
        }

        private TreeViewItemData<HierarchyItem> BuildTestimony(
            EditorSession session,
            WitnessData witness,
            TestimonyData testimony)
        {
            session.TryGetTestimonyContext(
                testimony.Id,
                out TestimonyContext testimonyContext);

            List<TreeViewItemData<HierarchyItem>> statements = new();

            foreach (StatementData statement in testimony.Statements)
            {
                statements.Add(
                    BuildStatement(
                        session,
                        witness,
                        testimony,
                        statement));
            }

            return new TreeViewItemData<HierarchyItem>(
                NextId(),
                new HierarchyItem
                {
                    Name = HierarchyDisplayUtility.GetTestimonyName(testimony),
                    Type = HierarchyType.Testimony,
                    Testimony = testimonyContext
                },
                statements);
        }

        private TreeViewItemData<HierarchyItem> BuildStatement(
            EditorSession session,
            WitnessData witness,
            TestimonyData testimony,
            StatementData statement)
        {
            StatementContext context =
                session.CreateStatementContext(
                    witness,
                    testimony,
                    statement);

            return new TreeViewItemData<HierarchyItem>(
                NextId(),
                new HierarchyItem
                {
                    Name = HierarchyDisplayUtility.GetStatementName(statement),
                    Type = HierarchyType.Statement,
                    Statement = context
                });
        }


        private int NextId()
        {
            return ++id;
        }
    }
}
