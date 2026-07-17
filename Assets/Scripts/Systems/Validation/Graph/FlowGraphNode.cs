using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Systems.Validation.Graph
{
    public sealed class FlowGraphNode
    {
        public StatementData Statement { get; }

        public string Id => Statement.Id;

        public bool IsEntry => Statement.InitiallyVisible;

        public List<FlowGraphEdge> Outgoing { get; } = new();

        public FlowGraphNode(
            StatementData statement)
        {
            Statement = statement;
        }

        public void AddEdge(
            FlowGraphEdge edge)
        {
            Outgoing.Add(edge);
        }
    }
}
