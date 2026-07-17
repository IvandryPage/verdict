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

            StatementData statement = context.Statement;

            title = statement.Id;

            extensionContainer.Add(
                new Label(statement.Text));


            inputPort = InstantiatePort(
                Orientation.Horizontal,
                Direction.Input,
                Port.Capacity.Multi,
                typeof(bool));

            inputPort.portName = "Input";


            outputPort = InstantiatePort(
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


        public override void OnSelected()
        {
            base.OnSelected();

            if (SuppressSelectedEvent)
                return;
            Debug.Log("1. Node Selected");
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
    }
}
