using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Editor.CaseFlow.Service;
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

        private readonly CaseEditService editService;

        private ObjectField caseField;

        private bool HasCase =>
            session.CurrentCase != null;

        public CaseEditorController(
            VisualElement root)
        {
            this.root = root;

            session = new EditorSession();

            graphView = new CaseGraphView();

            graphView.StatementSelected += session.Selection.SelectStatement;

            session.Selection.SelectionChanged += HandleSelectionChanged;

            graphView.EdgeCreated +=
                HandleEdgeCreated;


            graphView.EdgeRemoved +=
                HandleEdgeRemoved;

            graphBuilder = new CaseGraphBuilder(graphView);

            validationPanel =
                new ValidationPanel();

            validationPanel.IssueSelected +=
                HandleValidationIssueSelected;

            editService = new CaseEditService(session);

            editService.CaseModified +=
                HandleCaseModified;

            inspector = new CaseInspectorView(session, editService);
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

            TwoPaneSplitView vertical =
                new(
                    0,
                    350,
                    TwoPaneSplitViewOrientation.Vertical);

            vertical.Add(inspector);
            vertical.Add(validationPanel);// Inspector

            horizontalSplit.Add(graphView);

            horizontalSplit.Add(vertical);

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

            toolbar.Add(new ToolbarButton(SaveCase)
            {
                text = "Save"
            });

            toolbar.Add(new ToolbarButton(ValidateCase)
            {
                text = "Validate"
            });

            toolbar.Add(new ToolbarSpacer());

            toolbar.Add(new ToolbarButton(PlayCase)
            {
                text = "Play"
            });

            toolbar.Add(new ToolbarSpacer());

            toolbar.Add(new ToolbarButton(CreateStatement)
            {
                text = "+ Statement"
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

        private void CreateStatement()
        {
            if (!HasCase) return;

            if (!session.Selection.HasStatement)
            {
                Debug.LogWarning("Select a statement first.");
                return;
            }

            StatementContext context = session.Selection.StatementContext;
            StatementData newStatement = editService.CreateStatementAfter(context);
            RefreshEditor();
            SelectStatement(newStatement);
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

        private void RefreshEditor()
        {
            if (!HasCase)
                return;

            string selectedId = session.Selection.Statement?.Id;

            RebuildSession();

            ValidationResult result =
                Validate();

            RefreshGraph(result);

            RestoreSelection(selectedId);

            RefreshValidation(result);

            RefreshInspector();
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
                session,
                result);
        }

        private void RefreshValidation(
            ValidationResult result)
        {
            validationPanel.Show(result);
        }

        private void RefreshInspector()
        {
            // Future
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
                return;
            }

            session.Selection.SelectStatement(context);

            graphView.Frame(
                context.Statement.Id);
        }

        private void HandleSelectionChanged()
        {
            if (!session.Selection.HasStatement)
                return;

            graphView.Frame(
                session.Selection.Statement.Id);
        }

        private void HandleStatementSelected(
            StatementData statement)
        {
            if (!session.TryGetContext(
                statement.Id,
                out StatementContext context))
            {
                return;
            }

            session.Selection.SelectStatement(context);
        }

        private void HandleCaseModified()
        {
            RefreshEditor();
        }

        private void SelectStatement(
            StatementData statement)
        {
            if (!session.TryGetContext(
                    statement.Id,
                    out StatementContext context))
            {
                return;
            }

            session.Selection.SelectStatement(context);
        }

        private void RestoreSelection(
            string statementId)
        {
            if (string.IsNullOrWhiteSpace(statementId))
                return;

            if (!session.TryGetContext(
                statementId,
                out StatementContext context))
            {
                return;
            }

            session.Selection.SelectStatement(context);

            graphView.Frame(statementId);
        }


        private void HandleEdgeCreated(
            Edge edge)
        {
            if (edge.output.node is not StatementNodeView from)
                return;


            if (edge.input.node is not StatementNodeView to)
                return;


            editService.ConnectStatements(
                from.Context,
                to.Context);
        }

        private void HandleEdgeRemoved(
            Edge edge)
        {
            if (edge.output.node is not StatementNodeView from)
                return;

            if (edge.input.node is not StatementNodeView to)
                return;

            editService.DisconnectStatements(
                from.Context,
                to.Context);
        }
    }
}
