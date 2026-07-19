using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Verdict.Editor.CaseEditor.Hierarchy
{
    public sealed class CaseHierarchyView : VisualElement
    {
        private readonly TreeView tree;
        private readonly TextField searchField;
        private readonly HierarchyBuilder builder;

        private bool suppressSelectionChanged;

        private EditorSession currentSession;

        private int selectedItemId = -1;

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

            searchField = new TextField("Search")
            {
                isDelayed = true
            };

            searchField.RegisterValueChangedCallback(OnSearchChanged);

            Add(searchField);

            tree = new TreeView
            {
                selectionType = SelectionType.Single,
                fixedItemHeight = 22,
                reorderable = true
            };

            tree.makeItem = MakeItem;
            tree.bindItem = BindItem;
            tree.selectedIndicesChanged += OnSelectionChanged;

            Add(tree);
        }

        public void Rebuild(EditorSession session)
        {
            currentSession = session;

            List<TreeViewItemData<HierarchyItem>> roots =
                builder.Build(session);

            tree.SetRootItems(roots);

            tree.Rebuild();

            if (roots.Count > 0)
            {
                tree.ExpandItem(roots[0].id);
            }

            RestoreSelection();
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            builder.Query = evt.newValue?.Trim();

            if (currentSession != null)
            {
                Rebuild(currentSession);
            }
        }

        private VisualElement MakeItem()
        {
            return new Label();
        }

        private void BindItem(
            VisualElement element,
            int index)
        {
            Label label = (Label)element;

            HierarchyItem item =
                tree.GetItemDataForIndex<HierarchyItem>(index);

            label.text = item.Name;

            label.userData = item;

            label.ClearManipulators();

            label.AddManipulator(
                new ContextualMenuManipulator(evt =>
                {
                    BuildContextMenu(evt, item);
                }));
        }

        private void OnSelectionChanged(IEnumerable<int> indices)
        {
            if (suppressSelectionChanged)
                return;

            foreach (int index in indices)
            {
                HierarchyItem item =
                    tree.GetItemDataForIndex<HierarchyItem>(index);

                selectedItemId = item.Id;

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

        public void SelectCurrent(EditorSelection selection)
        {
            if (selection == null)
                return;

            if (selection.HasStatement &&
                TrySelectStatement(selection.Statement.Id))
            {
                return;
            }

            if (selection.HasTestimony &&
                TrySelectTestimony(selection.Testimony.Id))
            {
                return;
            }

            if (selection.HasWitness &&
                TrySelectWitness(selection.Witness.Id))
            {
                return;
            }
        }

        private void RestoreSelection()
        {
            if (currentSession != null && currentSession.Selection != null)
            {
                if (currentSession.Selection.HasStatement ||
                    currentSession.Selection.HasTestimony ||
                    currentSession.Selection.HasWitness)
                {
                    SelectCurrent(currentSession.Selection);
                    return;
                }
            }

            if (selectedItemId < 0)
                return;

            tree.SetSelectionById(selectedItemId);
        }

        private bool TrySelectStatement(string statementId)
        {
            if (!builder.TryGetItemId($"statement:{statementId}", out int itemId))
                return false;

            SelectById(itemId);
            return true;
        }

        private bool TrySelectTestimony(string testimonyId)
        {
            if (!builder.TryGetItemId($"testimony:{testimonyId}", out int itemId))
                return false;

            SelectById(itemId);
            return true;
        }

        private bool TrySelectWitness(string witnessId)
        {
            if (!builder.TryGetItemId($"witness:{witnessId}", out int itemId))
                return false;

            SelectById(itemId);
            return true;
        }

        private void SelectById(int itemId)
        {
            suppressSelectionChanged = true;
            selectedItemId = itemId;
            tree.SetSelectionById(itemId);
            suppressSelectionChanged = false;
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

    internal static class VisualElementExtensions
    {
        public static void ClearManipulators(this VisualElement element)
        {
            // Intentionally left blank.
            // Unity UI Toolkit currently doesn't expose a public API
            // to remove manipulators individually.
            // This method exists to document the intent and can be
            // expanded in the future if needed.
        }
    }
}
