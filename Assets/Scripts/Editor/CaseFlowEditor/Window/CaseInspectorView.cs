using UnityEngine.UIElements;
using Verdict.Data.Cases;

namespace Verdict.Editor.CaseFlow
{
    public sealed class CaseInspectorView : VisualElement
    {
        private readonly Label emptyLabel;

        private readonly ScrollView content;

        private readonly EditorSession session;

        public CaseInspectorView()
        {
            style.flexGrow = 1;

            style.minWidth = 320;

            style.paddingLeft = 8;

            style.paddingRight = 8;

            style.paddingTop = 8;

            emptyLabel = new Label("Nothing Selected");

            content = new ScrollView();

            Add(emptyLabel);

            Add(content);
        }

        public CaseInspectorView(
            EditorSession session)
            : this()
        {
            this.session = session;

            session.Selection.SelectionChanged += Refresh;
        }

        public void Show(
            StatementContext context)
        {
            emptyLabel.style.display = DisplayStyle.None;

            content.Clear();

            content.Add(
                new Label($"Statement : {context.Statement.Id}"));

            content.Add(
                new Label($"Witness : {context.Witness.Id}"));

            content.Add(
                new Label($"Testimony : {context.Testimony.Title}"));

            content.Add(
                new Label(context.Statement.Text));
        }

        public void ClearInspector()
        {
            content.Clear();

            emptyLabel.style.display =
                DisplayStyle.Flex;
        }

        private void Refresh()
        {
            StatementData statement =
                session.Selection.Get<StatementData>();

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
    }
}
