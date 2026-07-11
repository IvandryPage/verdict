using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;

namespace Verdict.Editor.CaseFlow
{
    public sealed class CaseEditorController
    {
        private readonly VisualElement root;

        private readonly EditorSession session;

        private readonly CaseGraphView graphView;

        private readonly CaseInspectorView inspector;

        private readonly CaseGraphBuilder graphBuilder;

        private ObjectField caseField;
        private CaseData currentCase;

        public CaseEditorController(
            VisualElement root)
        {
            this.root = root;

            session = new EditorSession();

            graphView = new CaseGraphView();

            graphView.StatementSelected += statement =>
            {
                session.Selection.Select(statement);
            };

            graphBuilder = new CaseGraphBuilder(graphView);

            inspector = new CaseInspectorView(session);
        }

        public void Initialize()
        {
            root.style.flexDirection =
                FlexDirection.Column;

            Toolbar toolbar =
                BuildToolbar();

            root.Add(toolbar);

            TwoPaneSplitView split =
                new TwoPaneSplitView(
                0,
                900,
                TwoPaneSplitViewOrientation.Horizontal);

            split.Add(graphView);

            split.Add(inspector);

            root.Add(split);
        }

        private Toolbar BuildToolbar()
        {
            Toolbar toolbar = new();

            toolbar.Add(new Label("Case"));

            caseField = new ObjectField
            {
                objectType = typeof(CaseData),
                allowSceneObjects = false
            };

            caseField.style.minWidth = 300;

            caseField.RegisterValueChangedCallback(OnCaseChanged);

            toolbar.Add(caseField);

            toolbar.Add(new ToolbarSpacer());

            toolbar.Add(new ToolbarButton(ReloadCase)
            {
                text = "Reload"
            });

            toolbar.Add(new ToolbarButton(SaveCase)
            {
                text = "Save"
            });

            toolbar.Add(new ToolbarButton(ValidateCase)
            {
                text = "Validate"
            });

            toolbar.Add(new ToolbarButton(PlayCase)
            {
                text = "Play"
            });

            return toolbar;
        }

        private void OnCaseChanged(ChangeEvent<Object> evt)
        {
            if (evt.newValue is not CaseData caseData)
            {
                return;
            }

            LoadCase(caseData);
        }

        private void SaveCase()
        {
            if (currentCase == null)
                return;

            EditorUtility.SetDirty(currentCase);

            AssetDatabase.SaveAssets();
        }

        private void ValidateCase()
        {
            if (currentCase == null)
                return;

            Debug.Log("Validate...");
        }

        private void PlayCase()
        {
            if (currentCase == null)
                return;

            Debug.Log("Play...");
        }

        public void HandleEvent(Event e)
        {
            if (e.commandName != "ObjectSelectorClosed")
                return;

            CaseData selected =
                EditorGUIUtility.GetObjectPickerObject() as CaseData;

            if (selected == null)
                return;

            LoadCase(selected);
        }

        private void LoadCase(CaseData caseData)
        {
            currentCase = caseData;

            session.LoadCase(caseData);

            graphBuilder.Build(session.FlowGraph);
        }

        private void ReloadCase()
        {
            RefreshGraph();
        }

        private void RefreshGraph()
        {
            if (currentCase == null)
                return;

            graphBuilder.Build(session.FlowGraph);
        }
    }
}
