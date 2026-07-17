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


        private ToolbarButton addButton;
        private ToolbarButton deleteButton;


        public CaseEditorController(
            VisualElement root)
        {
            this.root = root;


            session =
                new EditorSession();


            graphView =
                new CaseGraphView();


            graphView.StatementSelected +=
                HandleStatementSelected;


            graphView.EdgeCreated +=
                HandleEdgeCreated;


            graphView.EdgeRemoved +=
                HandleEdgeRemoved;


            session.Selection.SelectionChanged += () =>
            {
                UpdateToolbarButtons();
                HandleSelectionChanged();
            };



            graphBuilder =
                new CaseGraphBuilder(
                    graphView);



            validationPanel =
                new ValidationPanel();


            validationPanel.IssueSelected +=
                HandleValidationIssueSelected;



            editService =
                new CaseEditService(
                    session);


            editService.CaseModified +=
                HandleCaseModified;



            inspector =
                new CaseInspectorView(
                    session,
                    editService);
        }



        public void Initialize()
        {
            root.style.flexDirection =
                FlexDirection.Column;



            root.Add(
                BuildToolbar());



            TwoPaneSplitView mainSplit =
                new(
                    0,
                    900,
                    TwoPaneSplitViewOrientation.Horizontal);



            TwoPaneSplitView rightSplit =
                new(
                    0,
                    700,
                    TwoPaneSplitViewOrientation.Vertical);



            rightSplit.Add(inspector);

            rightSplit.Add(validationPanel);



            mainSplit.Add(graphView);

            mainSplit.Add(rightSplit);



            root.Add(mainSplit);
        }





        private Toolbar BuildToolbar()
        {
            Toolbar toolbar =
                new();



            toolbar.Add(
                new Label("Case"));



            caseField =
                new ObjectField
                {
                    objectType =
                        typeof(CaseData),

                    allowSceneObjects =
                        false
                };



            caseField.style.minWidth =
                300;



            caseField.RegisterValueChangedCallback(
                OnCaseChanged);



            toolbar.Add(caseField);



            toolbar.Add(
                new ToolbarSpacer());



            toolbar.Add(
                new ToolbarButton(SaveCase)
                {
                    text = "Save"
                });



            toolbar.Add(
                new ToolbarButton(ValidateCase)
                {
                    text = "Validate"
                });



            toolbar.Add(
                new ToolbarSpacer());



            addButton = new ToolbarButton(HandleCreate);
            deleteButton = new ToolbarButton(HandleDelete);

            toolbar.Add(new ToolbarSpacer());

            toolbar.Add(addButton);
            toolbar.Add(deleteButton);

            UpdateToolbarButtons();

            return toolbar;
        }


        private void UpdateToolbarButtons()
        {
            if (!HasCase)
            {
                addButton.SetEnabled(false);
                deleteButton.SetEnabled(false);
                return;
            }

            addButton.SetEnabled(true);

            if (session.Selection.HasStatement)
            {
                addButton.text = "+ Statement";
                deleteButton.text = "Delete Statement";
                deleteButton.SetEnabled(true);
            }
            else if (session.Selection.HasTestimony)
            {
                addButton.text = "+ Testimony";
                deleteButton.text = "Delete Testimony";
                deleteButton.SetEnabled(true);
            }
            else if (session.Selection.HasWitness)
            {
                addButton.text = "+ Witness";
                deleteButton.text = "Delete Witness";
                deleteButton.SetEnabled(true);
            }
            else
            {
                addButton.text = "+ Witness";
                deleteButton.SetEnabled(false);
            }
        }


        private void OnCaseChanged(
            ChangeEvent<Object> evt)
        {
            if (evt.newValue is not CaseData caseData)
                return;


            LoadCase(caseData);
        }





        private void LoadCase(
            CaseData caseData)
        {
            session.LoadCase(
                caseData);


            RefreshEditor();
        }





        private void SaveCase()
        {
            if (!HasCase)
                return;


            EditorUtility.SetDirty(
                session.CurrentCase);


            AssetDatabase.SaveAssets();
        }





        private void ValidateCase()
        {
            RefreshEditor();
        }



        private void HandleCreate()
        {
            if (!HasCase)
                return;

            if (session.Selection.HasStatement)
            {
                CreateStatement();
                return;
            }

            if (session.Selection.HasTestimony)
            {
                CreateTestimony();
                return;
            }

            CreateWitness();
        }

        private void CreateStatement()
        {
            StatementData statement =
                editService.CreateStatement(
                    session.Selection.TestimonyContext.Testimony);

            RefreshEditor();

            SelectStatement(statement);
        }



        private void CreateWitness()
        {
            WitnessData witness =
                editService.CreateWitness();

            RefreshEditor();

            if (session.TryGetWitnessContext(
                    witness.Id,
                    out WitnessContext context))
            {
                session.Selection.SelectWitness(context);
            }
        }

        private void CreateTestimony()
        {
            if (!session.Selection.HasWitness)
                return;

            TestimonyData testimony =
                editService.CreateTestimony(
                    session.Selection.WitnessContext.Witness);

            RefreshEditor();

            if (session.TryGetTestimonyContext(
                    testimony.Id,
                    out TestimonyContext context))
            {
                session.Selection.SelectTestimony(context);
            }
        }


        public void HandleEvent(
            Event e)
        {
            if (e.commandName !=
                "ObjectSelectorClosed")
            {
                return;
            }



            CaseData selected =
                EditorGUIUtility
                .GetObjectPickerObject()
                as CaseData;



            if (selected == null)
                return;



            LoadCase(selected);
        }


        private void HandleDelete()
        {
            if (session.Selection.HasStatement)
            {
                editService.DeleteStatement(
                    session.Selection.StatementContext);

                return;
            }

            if (session.Selection.HasTestimony)
            {
                editService.DeleteTestimony(
                    session.Selection.TestimonyContext.Witness,
                    session.Selection.TestimonyContext.Testimony);

                return;
            }

            if (session.Selection.HasWitness)
            {
                editService.DeleteWitness(
                    session.Selection.WitnessContext.Witness);

                return;
            }
        }


        private void RefreshEditor()
        {
            if (!HasCase)
                return;



            string selectedId =
                session.Selection.Statement?.Id;



            session.LoadCase(
                session.CurrentCase);



            ValidationResult result =
                Validate();



            RefreshGraph(
                result);



            RestoreSelection(
                selectedId);



            RefreshValidation(
                result);
        }





        private ValidationResult Validate()
        {
            return RuntimeValidator.Validate(
                session.CurrentCase);
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
            validationPanel.Show(
                result);
        }





        private void HandleCaseModified()
        {
            RefreshEditor();
        }





        private void HandleSelectionChanged()
        {
            if (!session.Selection.HasStatement)
                return;

            graphView.Frame(
                session.Selection.Statement.Id);
        }





        private void HandleStatementSelected(
            StatementContext context)
        {
            if (context == null)
                return;


            session.Selection.SelectStatement(
                context);
        }





        private void HandleValidationIssueSelected(
            ValidationIssue issue)
        {
            if (string.IsNullOrWhiteSpace(
                issue.ContextId))
            {
                return;
            }



            if (!session.TryGetContext(
                issue.ContextId,
                out StatementContext context))
            {
                return;
            }



            session.Selection.SelectStatement(
                context);



            graphView.Frame(
                context.Statement.Id);
        }





        private void SelectStatement(
            StatementData statement)
        {
            if (statement == null)
                return;



            if (!session.TryGetContext(
                statement.Id,
                out StatementContext context))
            {
                return;
            }



            session.Selection.SelectStatement(
                context);
        }





        private void RestoreSelection(
            string statementId)
        {
            if (string.IsNullOrWhiteSpace(
                statementId))
            {
                return;
            }



            if (!session.TryGetContext(
                statementId,
                out StatementContext context))
            {
                return;
            }



            session.Selection.SelectStatement(
                context);



            graphView.Frame(
                statementId);
        }





        private void HandleEdgeCreated(
            Edge edge)
        {
            if (edge.output.node
                is not StatementNodeView from)
            {
                return;
            }



            if (edge.input.node
                is not StatementNodeView to)
            {
                return;
            }



            editService.ConnectStatements(
                from.Context,
                to.Context);
        }





        private void HandleEdgeRemoved(
            Edge edge)
        {
            if (edge.output.node
                is not StatementNodeView from)
            {
                return;
            }



            if (edge.input.node
                is not StatementNodeView to)
            {
                return;
            }



            editService.DisconnectStatements(
                from.Context,
                to.Context);
        }
    }
}
