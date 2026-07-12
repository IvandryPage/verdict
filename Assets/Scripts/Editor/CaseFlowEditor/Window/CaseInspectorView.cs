
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;

namespace Verdict.Editor.CaseFlow
{
    public sealed class CaseInspectorView : VisualElement
    {
        private readonly EditorSession session;

        private readonly Label emptyLabel;

        private readonly ScrollView content;

        public CaseInspectorView(
            EditorSession session)
        {
            this.session = session;

            style.flexGrow = 1;
            style.minWidth = 320;
            style.paddingLeft = 8;
            style.paddingRight = 8;
            style.paddingTop = 8;

            emptyLabel = new Label("Nothing Selected");

            content = new ScrollView();

            Add(emptyLabel);
            Add(content);

            session.Selection.SelectionChanged += Refresh;
        }

        private void Refresh()
        {
            Debug.Log("Inspector: Refresh!");
            StatementData statement =
                session.Selection.Get<StatementData>();
            Debug.Log($"Statement ID: {statement?.Id}");

            if (statement == null)
            {
                ClearInspector();
                return;
            }

            if (!session.TryGetContext(
                    statement.Id,
                    out StatementContext context))
            {
                ClearInspector();
                return;
            }

            Show(context);
        }

        private void Show(
            StatementContext context)
        {
            emptyLabel.style.display =
                DisplayStyle.None;

            content.Clear();

            DrawHeader(context);

            DrawStatement(context);
        }

        private void DrawHeader(
            StatementContext context)
        {
            content.Add(
                new Label($"Witness : {context.Witness.Id}"));

            content.Add(
                new Label($"Testimony : {context.Testimony.Title}"));

            content.Add(
                new Label($"Statement : {context.Statement.Id}"));

            content.Add(new VisualElement
            {
                style =
                {
                    height = 1,
                    marginTop = 6,
                    marginBottom = 6
                }
            });
        }

        private void DrawStatement(
            StatementContext context)
        {
            SerializedObject serializedObject =
                new SerializedObject(context.Case);

            SerializedProperty property =
                serializedObject.FindProperty(
                    context.StatementPropertyPath);

            if (property == null)
            {
                content.Add(
                    new Label(
                        $"Unable to locate property:\n{context.StatementPropertyPath}"));

                return;
            }

            PropertyField field =
                new PropertyField(property);

            field.Bind(serializedObject);

            content.Add(field);
        }

        private void ClearInspector()
        {
            content.Clear();

            emptyLabel.style.display =
                DisplayStyle.Flex;
        }
    }
}
