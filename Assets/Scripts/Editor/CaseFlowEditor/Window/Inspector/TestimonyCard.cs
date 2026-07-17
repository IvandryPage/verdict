using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Editor.CaseFlow.Service;

namespace Verdict.Editor.CaseFlow.Inspector
{
    public sealed class TestimonyCard : VisualElement
    {
        private readonly EditorSession session;
        private readonly CaseEditService editService;
        private readonly TestimonyContext context;

        public TestimonyCard(
            EditorSession session,
            CaseEditService editService,
            TestimonyContext context)
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
            Add(new Label("Testimony")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 14
                }
            });

            DrawProperties();

            Add(CreateSeparator());

            DrawStatements();
        }

        private void DrawProperties()
        {
            SerializedObject so =
                new SerializedObject(context.Case);

            SerializedProperty testimony =
                so.FindProperty(
                    context.PropertyPath);

            if (testimony == null)
            {
                Add(new Label("Testimony property not found."));
                return;
            }

            PropertyField id =
                new(testimony.FindPropertyRelative("id"));

            PropertyField title =
                new(testimony.FindPropertyRelative("title"));

            PropertyField description =
                new(testimony.FindPropertyRelative("description"));

            id.Bind(so);
            title.Bind(so);
            description.Bind(so);

            Add(id);
            Add(title);
            Add(description);
        }

        private void DrawStatements()
        {
            Foldout foldout = new()
            {
                text =
                    $"Statements ({context.Testimony.Statements.Count})",
                value = true
            };

            foreach (StatementData statement in context.Testimony.Statements)
            {
                StatementContext statementContext =
                    session.CreateStatementContext(
                        context.Witness,
                        context.Testimony,
                        statement);

                Button button =
                    new(() =>
                    {
                        session.Selection.SelectStatement(
                            statementContext);
                    });

                button.text = statement.Text;

                button.style.unityTextAlign =
                    TextAnchor.MiddleLeft;

                foldout.Add(button);
            }

            Button add =
                new(() =>
                {
                    editService.CreateStatement(
                        context.Testimony);
                })
                {
                    text = "+ Add Statement"
                };

            foldout.Add(add);

            Add(foldout);
        }

        private static VisualElement CreateSeparator()
        {
            return new VisualElement
            {
                style =
                {
                    height = 1,
                    marginTop = 8,
                    marginBottom = 8,
                    backgroundColor =
                        new Color(.22f,.22f,.22f)
                }
            };
        }
    }
}
