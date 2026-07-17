using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Verdict.Editor.CaseFlow.Hierarchy
{
    public sealed class CaseHierarchyView : VisualElement
    {
        private readonly TreeView tree;
        private readonly HierarchyBuilder builder;

        public event Action<WitnessContext> WitnessSelected;
        public event Action<TestimonyContext> TestimonySelected;
        public event Action<StatementContext> StatementSelected;

        public event Action CreateWitnessRequested;

        public event Action<WitnessContext> CreateTestimonyRequested;

        public event Action<TestimonyContext> CreateStatementRequested;

        public event Action<WitnessContext> DeleteWitnessRequested;

        public event Action<TestimonyContext> DeleteTestimonyRequested;

        public event Action<StatementContext> DeleteStatementRequested;

        public CaseHierarchyView()
        {
            style.flexGrow = 1;
            style.minWidth = 250;

            builder = new HierarchyBuilder();

            tree = new TreeView();

            tree.selectionType = SelectionType.Single;
            tree.fixedItemHeight = 22;

            tree.makeItem = () =>
            {
                Label label = new();

                return label;
            };

            tree.bindItem = (element, index) =>
            {
                Label label = (Label)element;

                HierarchyItem item =
                    tree.GetItemDataForIndex<HierarchyItem>(index);

                label.text = item.Name;

                label.AddManipulator(
                    new ContextualMenuManipulator(evt =>
                    {
                        BuildContextMenu(evt, item);
                    }));

                label.userData = item;
            };

            tree.selectedIndicesChanged += OnSelectionChanged;

            Add(tree);
        }

        public void Rebuild(EditorSession session)
        {
            List<TreeViewItemData<HierarchyItem>> root = builder.Build(session);

            tree.SetRootItems(root);

            tree.Rebuild();

            ExpandRecursive(root);
        }

        private void OnSelectionChanged(IEnumerable<int> indices)
        {
            foreach (int index in indices)
            {
                HierarchyItem item =
                    tree.GetItemDataForIndex<HierarchyItem>(index);

                switch (item.Type)
                {
                    case HierarchyType.Witness:
                        WitnessSelected?.Invoke(item.Witness);
                        break;

                    case HierarchyType.Testimony:
                        TestimonySelected?.Invoke(item.Testimony);
                        break;

                    case HierarchyType.Statement:
                        StatementSelected?.Invoke(item.Statement);
                        break;
                }
            }
        }

        private void ExpandRecursive(IEnumerable<TreeViewItemData<HierarchyItem>> items)
        {
            foreach (var item in items)
            {
                tree.ExpandItem(item.id);

                ExpandRecursive(item.children);
            }
        }

        private void BuildContextMenu(
            ContextualMenuPopulateEvent evt,
            HierarchyItem item)
        {
            switch (item.Type)
            {
                case HierarchyType.Witness:

                    evt.menu.AppendAction(
                        "Create Testimony",
                        _ => CreateTestimonyRequested?.Invoke(item.Witness));

                    evt.menu.AppendSeparator();

                    evt.menu.AppendAction(
                        "Delete Witness",
                        _ => DeleteWitnessRequested?.Invoke(item.Witness));

                    break;

                case HierarchyType.Testimony:

                    evt.menu.AppendAction(
                        "Create Statement",
                        _ => CreateStatementRequested?.Invoke(item.Testimony));

                    evt.menu.AppendSeparator();

                    evt.menu.AppendAction(
                        "Delete Testimony",
                        _ => DeleteTestimonyRequested?.Invoke(item.Testimony));

                    break;

                case HierarchyType.Statement:

                    evt.menu.AppendAction(
                        "Delete Statement",
                        _ => DeleteStatementRequested?.Invoke(item.Statement));

                    break;
            }

            evt.menu.AppendSeparator();

            evt.menu.AppendAction(
                "Create Witness",
                _ => CreateWitnessRequested?.Invoke());
        }
    }
}
