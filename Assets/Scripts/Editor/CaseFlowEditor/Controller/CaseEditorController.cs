using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Editor.CaseFlow.Validation;
using Verdict.Systems.Validation;

namespace Verdict.Editor.CaseFlow
{
    public sealed class CaseEditorController
    {
        private readonly VisualElement root;

        private readonly EditorSession session;

        private readonly CaseGraphView graphView;

        private readonly CaseInspectorView inspector;

        private readonly CaseGraphBuilder graphBuilder;

        private readonly ValidationPanel validationPanel;

        private ObjectField caseField;

        private bool HasCase =>
            session.CurrentCase != null;

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

            session.Selection.SelectionChanged += HandleSelectionChanged;

            graphBuilder = new CaseGraphBuilder(graphView);

            inspector = new CaseInspectorView(session);

            validationPanel =
                new ValidationPanel();

            validationPanel.IssueSelected +=
                HandleValidationIssueSelected;
        }

        public void Initialize()
        {
            root.style.flexDirection =
                FlexDirection.Column;

            Toolbar toolbar =
                BuildToolbar();

            root.Add(toolbar);

            // Graph | Right Panel
            TwoPaneSplitView horizontalSplit =
                new TwoPaneSplitView(
                    0,
                    900,
                    TwoPaneSplitViewOrientation.Horizontal);

            // Inspector
            // ----------
            // Validation
            TwoPaneSplitView verticalSplit =
                new TwoPaneSplitView(
                    0,
                    350,
                    TwoPaneSplitViewOrientation.Vertical);

            verticalSplit.Add(inspector);

            verticalSplit.Add(validationPanel);

            horizontalSplit.Add(graphView);

            horizontalSplit.Add(verticalSplit);

            root.Add(horizontalSplit);
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
            if (!HasCase)
                return;

            EditorUtility.SetDirty(session.CurrentCase);

            AssetDatabase.SaveAssets();
        }

        private void ValidateCase()
        {
            RefreshEditor();
        }

        private void PlayCase()
        {
            if (!HasCase)
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
            session.LoadCase(caseData);

            RefreshEditor();
        }

        private void ReloadCase()
        {
            RefreshEditor();
        }

        private void RefreshEditor()
        {
            if (!HasCase)
                return;

            RebuildSession();

            ValidationResult result =
                Validate();

            RefreshGraph(result);

            RefreshValidation(result);
        }

        private void RebuildSession()
        {
            session.LoadCase(session.CurrentCase);
        }

        private ValidationResult Validate()
        {
            return RuntimeValidator.Validate(session.CurrentCase);
        }

        private void RefreshGraph(
            ValidationResult result)
        {
            graphBuilder.Build(
                session.FlowGraph,
                result);
        }

        private void RefreshValidation(
            ValidationResult result)
        {
            validationPanel.Show(result);
        }

        private void HandleValidationIssueSelected(
            ValidationIssue issue)
        {
            if (string.IsNullOrWhiteSpace(issue.ContextId))
            {
                return;
            }

            if (!session.TryGetContext(
                    issue.ContextId,
                    out StatementContext context))
            {
                Debug.Log("Context not FOUND");
                return;
            }

            Debug.Log("Context FOUND");

            session.Selection.Select(
                context.Statement);

            graphView.Frame(
                context.Statement.Id);
        }

        private void HandleSelectionChanged()
        {
            StatementData statement =
                session.Selection.Get<StatementData>();

            if (statement == null)
                return;

            graphView.Frame(statement.Id);
        }
    }
}
