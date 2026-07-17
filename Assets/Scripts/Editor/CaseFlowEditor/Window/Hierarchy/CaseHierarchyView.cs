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

        public CaseHierarchyView()
        {
            style.flexGrow = 1;
            style.minWidth = 250;

            builder = new HierarchyBuilder();

            tree = new TreeView();

            tree.selectionType = SelectionType.Single;
            tree.fixedItemHeight = 22;

            tree.makeItem = () => new Label();

            tree.bindItem = (element, index) =>
            {
                Label label = (Label)element;

                HierarchyItem item = tree.GetItemDataForIndex<HierarchyItem>(index);

                label.text = item.Name;
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
    }
}
