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
        private readonly Port inputPort;
        private readonly Port outputPort;

        public StatementData Statement { get; }

        public Port InputPort => inputPort;

        public Port OutputPort => outputPort;

        public event Action<StatementNodeView> Selected;

        public StatementNodeView(
            StatementData statement)
        {
            Statement = statement;

            title = statement.Id;

            extensionContainer.Add(
                new Label(statement.Text));

            inputPort = InstantiatePort(
                Orientation.Horizontal,
                Direction.Input,
                Port.Capacity.Multi,
                typeof(bool));

            inputPort.portName = "";

            outputPort = InstantiatePort(
                Orientation.Horizontal,
                Direction.Output,
                Port.Capacity.Multi,
                typeof(bool));

            outputPort.portName = "";

            inputContainer.Add(inputPort);
            outputContainer.Add(outputPort);

            RefreshPorts();
            RefreshExpandedState();
        }

        public override void OnSelected()
        {
            base.OnSelected();
            Selected?.Invoke(this);
        }

        public void ApplyStyle(
            NodeStyle style)
        {
            mainContainer.style.backgroundColor =
                new StyleColor(style.Background);

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

        public void FocusToStatement()
        {
            BringToFront();

            AddToClassList("selected");
        }
    }
}
