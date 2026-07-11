using Verdict.Data.Cases;

namespace Verdict.Systems.Validation.Graph
{
#nullable enable
    public sealed class FlowGraphEdge
    {
        public FlowEdgeType Type { get; }

        public string? TargetId { get; }

        public FlowGraphNode From { get; }

        public FlowGraphNode? To { get; }

        public bool IsResolved => To != null;

        public FlowGraphEdge(
            FlowGraphNode from,
            FlowGraphNode? to,
            string targetId,
            FlowEdgeType type)
        {
            From = from;
            To = to;
            Type = type;
            TargetId = targetId;
        }
    }
}
