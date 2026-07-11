using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Verdict.Data.Cases;

namespace Verdict.Editor.CaseFlow
{
    public sealed class StatementNodeView : Node
    {
        public StatementData Statement { get; }

        public WitnessData Witness { get; }

        public TestimonyData Testimony { get; }

        public event Action<StatementNodeView> Selected;

        public StatementNodeView(
            StatementData statement,
            WitnessData witness,
            TestimonyData testimony)
        {
            Statement = statement;
            Witness = witness;
            Testimony = testimony;

            title = witness.Character.DisplayName;

            extensionContainer.Add(
                new Label(testimony.Title));

            extensionContainer.Add(
                new Label(statement.Text));

            RefreshExpandedState();
            RefreshPorts();
        }

        public override void OnSelected()
        {
            base.OnSelected();
            Selected?.Invoke(this);
        }
    }
}
