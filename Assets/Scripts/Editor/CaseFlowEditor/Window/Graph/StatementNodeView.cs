using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Editor.CaseFlow.Theme;

namespace Verdict.Editor.CaseFlow
{
    public sealed class StatementNodeView : Node
    {
        private NodeStyle currentStyle;
        internal bool SuppressSelectedEvent { get; set; }
        private readonly Port inputPort;
        private readonly Port outputPort;

        public StatementContext Context { get; }

        public StatementData Statement =>
            Context.Statement;

        public Port InputPort => inputPort;

        public Port OutputPort => outputPort;

        public event Action<StatementNodeView> Selected;

        public StatementNodeView(
            StatementContext context)
        {
            Context = context;

            capabilities |=
                Capabilities.Selectable |
                Capabilities.Movable |
                Capabilities.Deletable |
                Capabilities.Ascendable;

            StatementData statement =
                context.Statement;

            title =
                HierarchyDisplayUtility.GetStatementName(
                    statement);

            style.width = 320;
            style.maxWidth = 320;
            style.minWidth = 260;
            style.flexShrink = 0;


            Label preview =
                new Label(GetPreview(statement))
                {
                    tooltip = statement.Text
                };

            preview.style.whiteSpace =
                WhiteSpace.Normal;

            preview.style.flexShrink = 1;
            preview.style.flexWrap = Wrap.Wrap;

            preview.style.overflow =
                Overflow.Hidden;
            preview.style.unityOverflowClipBox =
                OverflowClipBox.ContentBox;
            preview.style.textOverflow =
                TextOverflow.Ellipsis;
            preview.style.maxWidth = 300;

            preview.style.unityTextAlign =
                TextAnchor.UpperLeft;

            preview.style.marginTop = 4;

            preview.style.marginBottom = 4;

            preview.style.fontSize = 11;

            preview.style.color =
                new Color(.82f, .82f, .82f);

            extensionContainer.Add(preview);


            inputPort =
                InstantiatePort(
                    Orientation.Horizontal,
                    Direction.Input,
                    Port.Capacity.Multi,
                    typeof(bool));

            inputPort.portName = "Input";


            outputPort =
                InstantiatePort(
                    Orientation.Horizontal,
                    Direction.Output,
                    Port.Capacity.Single,
                    typeof(bool));

            outputPort.portName = "Next";


            inputContainer.Add(inputPort);
            outputContainer.Add(outputPort);

            RefreshPorts();
            RefreshExpandedState();
        }

        public void ApplyStyle(
            NodeStyle style)
        {
            currentStyle = style;

            mainContainer.style.backgroundColor =
                new StyleColor(style.Background);

            mainContainer.style.borderLeftWidth = 1;
            mainContainer.style.borderRightWidth = 1;
            mainContainer.style.borderTopWidth = 1;
            mainContainer.style.borderBottomWidth = 1;

            mainContainer.style.borderLeftColor =
                new StyleColor(style.Border);

            mainContainer.style.borderRightColor =
                new StyleColor(style.Border);

            mainContainer.style.borderTopColor =
                new StyleColor(style.Border);

            mainContainer.style.borderBottomColor =
                new StyleColor(style.Border);

            titleContainer.style.backgroundColor =
                new StyleColor(style.Border);

            titleContainer.style.color =
                new StyleColor(style.Title);
        }

        public override void OnSelected()
        {
            base.OnSelected();

            ApplySelectionStyle();

            if (!SuppressSelectedEvent)
            {
                Selected?.Invoke(this);
            }
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            RestoreSelectionStyle();
        }

        private void ApplySelectionStyle()
        {
            if (currentStyle == null)
                return;

            Color selectionColor = new Color(0.26f, 0.63f, 0.96f);

            mainContainer.style.borderLeftWidth = 3;
            mainContainer.style.borderRightWidth = 3;
            mainContainer.style.borderTopWidth = 3;
            mainContainer.style.borderBottomWidth = 3;
            mainContainer.style.borderLeftColor = new StyleColor(selectionColor);
            mainContainer.style.borderRightColor = new StyleColor(selectionColor);
            mainContainer.style.borderTopColor = new StyleColor(selectionColor);
            mainContainer.style.borderBottomColor = new StyleColor(selectionColor);
        }

        private void RestoreSelectionStyle()
        {
            if (currentStyle == null)
                return;

            mainContainer.style.borderLeftWidth = 1;
            mainContainer.style.borderRightWidth = 1;
            mainContainer.style.borderTopWidth = 1;
            mainContainer.style.borderBottomWidth = 1;
            mainContainer.style.borderLeftColor = new StyleColor(currentStyle.Border);
            mainContainer.style.borderRightColor = new StyleColor(currentStyle.Border);
            mainContainer.style.borderTopColor = new StyleColor(currentStyle.Border);
            mainContainer.style.borderBottomColor = new StyleColor(currentStyle.Border);
        }


        private static string GetPreview(
            StatementData statement)
        {
            if (string.IsNullOrWhiteSpace(statement.Text))
                return "<Empty Statement>";

            string text =
                statement.Text.Replace('\n', ' ');

            const int maxLength = 80;

            if (text.Length <= maxLength)
                return text;

            return text[..maxLength] + "...";
        }
    }
}
