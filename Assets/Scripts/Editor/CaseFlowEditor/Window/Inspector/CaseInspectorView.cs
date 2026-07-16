
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Editor.CaseFlow.Inspector;
using Verdict.Editor.CaseFlow.Service;

namespace Verdict.Editor.CaseFlow
{
    public sealed class CaseInspectorView : VisualElement
    {
        private readonly EditorSession session;

        private readonly CaseEditService editService;

        private readonly Label emptyLabel;

        private readonly ScrollView content;

        public CaseInspectorView(
            EditorSession session,
            CaseEditService editService)
        {
            this.session = session;
            this.editService = editService;

            style.flexGrow = 1;
            style.minWidth = 340;

            style.flexDirection = FlexDirection.Column;

            style.paddingLeft = 10;
            style.paddingRight = 10;
            style.paddingTop = 10;
            style.paddingBottom = 10;

            // Header
            Label title = new("Inspector");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 15;
            title.style.marginBottom = 8;

            Add(title);

            // Separator
            Add(new VisualElement
            {
                style =
                {
                    height = 1,
                    marginBottom = 8,
                    backgroundColor = new Color(.22f, .22f, .22f)
                }
            });

            emptyLabel = new Label("Select a Statement to edit.")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    color = new Color(.65f,.65f,.65f),
                    marginTop = 30,
                    marginBottom = 30
                }
            };

            content = new ScrollView
            {
                style =
                {
                    flexGrow = 1
                }
            };

            Add(emptyLabel);
            Add(content);

            session.Selection.SelectionChanged += Refresh;
        }

        private void Refresh()
        {
            StatementData statement =
                session.Selection.Statement;

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

            content.style.display =
                DisplayStyle.Flex;

            content.Clear();

            DrawStatementSection(context);

            DrawClaimsSection(context);
        }

        private void DrawStatementSection(
            StatementContext context)
        {
            content.Add(
                CreateSectionHeader("Statement"));

            content.Add(
                new StatementCard(
                    session,
                    editService,
                    context));

            content.Add(
                CreateSeparator());
        }

        private void DrawClaimsSection(
            StatementContext context)
        {
            content.Add(
                CreateSectionHeader(
                    $"Claims ({context.Statement.Claims.Count})"));

            foreach (ClaimData claim in context.Statement.Claims)
            {
                content.Add(
                    new ClaimCard(
                        session,
                        editService,
                        context,
                        claim));
            }

            Button addClaim =
                new(() =>
                {
                    editService.CreateClaim(context);
                })
                {
                    text = "+ Add Claim"
                };

            addClaim.style.marginTop = 8;

            content.Add(addClaim);
        }

        private Label CreateSectionHeader(
            string title)
        {
            Label label =
                new(title);

            label.style.unityFontStyleAndWeight =
                FontStyle.Bold;

            label.style.fontSize = 13;

            label.style.marginTop = 12;
            label.style.marginBottom = 6;

            return label;
        }

        private VisualElement CreateSeparator()
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

        private void ClearInspector()
        {
            content.Clear();

            content.style.display = DisplayStyle.None;
            emptyLabel.style.display = DisplayStyle.Flex;
        }
    }
}
