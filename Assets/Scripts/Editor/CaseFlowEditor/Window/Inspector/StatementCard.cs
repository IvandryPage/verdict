using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Editor.CaseFlow.Service;

namespace Verdict.Editor.CaseFlow.Inspector
{
    public sealed class StatementCard : VisualElement
    {
        private readonly EditorSession session;
        private readonly CaseEditService editService;
        private readonly StatementContext context;

        public StatementCard(
            EditorSession session,
            CaseEditService editService,
            StatementContext context)
        {
            this.session = session;
            this.editService = editService;
            this.context = context;

            style.marginBottom = 12;
            style.paddingLeft = 8;
            style.paddingRight = 8;
            style.paddingTop = 8;
            style.paddingBottom = 8;

            Build();
        }

        private void Build()
        {
            Add(new Label("Statement")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 14
                }
            });

            DrawHeader();
            DrawProperties();
            DrawButtons();
        }

        private void DrawHeader()
        {
            Add(new Label($"Witness : {context.Witness.Id}"));
            Add(new Label($"Testimony : {context.Testimony.Title}"));
            Add(new Label($"Statement : {context.Statement.Id}"));

            Add(new VisualElement
            {
                style =
                {
                    height = 1,
                    marginTop = 6,
                    marginBottom = 6
                }
            });
        }

        private void DrawProperties()
        {
            SerializedObject so =
                new SerializedObject(context.Case);

            SerializedProperty statementProperty =
                so.FindProperty(
                    context.StatementPropertyPath);

            if (statementProperty == null)
            {
                Add(new Label("Statement property not found."));
                return;
            }

            SerializedProperty textProperty =
                statementProperty.FindPropertyRelative("text");

            SerializedProperty visibleProperty =
                statementProperty.FindPropertyRelative("initiallyVisible");

            SerializedProperty nextProperty =
                statementProperty.FindPropertyRelative("nextStatementId");

            PropertyField textField =
                new(textProperty);

            PropertyField visibleField =
                new(visibleProperty);

            PropertyField nextField =
                new(nextProperty);

            textField.Bind(so);
            visibleField.Bind(so);
            nextField.Bind(so);

            Add(textField);
            Add(visibleField);
            Add(nextField);
        }

        private void DrawButtons()
        {
            Button add =
                new Button(() =>
                {
                    editService.CreateStatementAfter(context);
                })
                {
                    text = "Add Statement After"
                };

            Button delete =
                new Button(() =>
                {
                    editService.DeleteStatement(context);
                })
                {
                    text = "Delete Statement"
                };

            Add(add);
            Add(delete);
        }
    }
}
