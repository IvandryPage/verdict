
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;

namespace Verdict.Editor.CaseFlow
{
    public sealed class CaseGraphView : GraphView
    {
        private readonly Dictionary<string, StatementNodeView> statementNodes
            = new();

        public IReadOnlyDictionary<string, StatementNodeView>
            StatementNodes => statementNodes;

        public event Action<StatementData> StatementSelected;

        public CaseGraphView()
        {
            style.flexGrow = 1;

            SetupZoom(
                ContentZoomer.DefaultMinScale,
                ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground grid = new();
            Insert(0, grid);

            grid.style.flexGrow = 1;
        }

        public StatementNodeView CreateStatementNode(
            StatementData statement,
            WitnessData witness,
            TestimonyData testimony,
            Vector2 position)
        {
            StatementNodeView node =
                new StatementNodeView(
                    statement,
                    witness,
                    testimony);

            node.SetPosition(
                new Rect(
                    position,
                    new Vector2(300, 180)));

            node.Selected += HandleNodeSelected;

            AddElement(node);

            statementNodes.Add(
                statement.Id,
                node);

            return node;
        }

        public void ClearGraph()
        {
            DeleteElements(graphElements.ToList());

            statementNodes.Clear();
        }

        private void HandleNodeSelected(
            StatementNodeView node)
        {
            StatementSelected?.Invoke(node.Statement);
        }
    }
}
