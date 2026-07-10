using System.Collections.Generic;
using System.Linq;
using Verdict.Data.Cases;
using Verdict.Systems.Validation.Graph;

namespace Verdict.Systems.Validation
{
    public sealed class FlowValidator : ICaseValidator
    {
        public void Validate(
            CaseData caseData,
            ValidationResult result)
        {
            FlowGraph graph =
                FlowGraphBuilder.Build(caseData);

            ValidateEntryNodes(graph, result);
            ValidateBrokenEdges(graph, result);
            ValidateDeadEnds(graph, result);
            ValidateCycles(graph, result);
            ValidateReachability(graph, result);
        }

        private static void ValidateEntryNodes(
            FlowGraph graph,
            ValidationResult result)
        {
            if (graph.Nodes.Values.Any(n => n.Statement.InitiallyVisible))
            {
                return;
            }

            result.AddError(
                ValidationScope.Flow,
                "Case contains no entry statement.");
        }

        private static void ValidateBrokenEdges(
            FlowGraph graph,
            ValidationResult result)
        {
            foreach(FlowGraphNode node in graph.Nodes.Values)
            {
                foreach (FlowGraphEdge edge in node.Outgoing)
                {
                    if (edge.IsResolved)
                    {
                        continue;
                    }

                    result.AddError(
                        ValidationScope.Flow,
                        $"Broken {edge.Type} edge from '{edge.From.Statement.Id}' to '{edge.TargetId}'.");
                }
            }
        }

        private static void ValidateDeadEnds(
            FlowGraph graph,
            ValidationResult result)
        {
            foreach (FlowGraphNode node in graph.Nodes.Values)
            {
                bool terminal =
                    node.Statement.NextStatementId == null &&
                    node.Outgoing.Count == 0;

                if (!terminal)
                {
                    continue;
                }

                result.AddWarning(
                    ValidationScope.Flow,
                    $"Statement '{node.Statement.Id}' is a dead end.");
            }
        }

        private static void ValidateCycles(
            FlowGraph graph,
            ValidationResult result)
        {
            var visited = new HashSet<FlowGraphNode>();
            var stack = new HashSet<FlowGraphNode>();

            foreach (FlowGraphNode node in graph.Nodes.Values)
            {
                Visit(node);
            }

            void Visit(FlowGraphNode node)
            {
                if (visited.Contains(node))
                    return;

                if (stack.Contains(node))
                {
                    result.AddError(
                        ValidationScope.Flow,
                        $"Cycle detected involving statement '{node.Statement.Id}'.");

                    return;
                }

                stack.Add(node);

                foreach (FlowGraphEdge edge in node.Outgoing)
                {
                    if (edge.To != null)
                        Visit(edge.To);
                }

                stack.Remove(node);
                visited.Add(node);
            }
        }

        private static void ValidateReachability(
            FlowGraph graph,
            ValidationResult result)
        {
            Queue<FlowGraphNode> queue = new();

            HashSet<FlowGraphNode> visited = new();

            foreach (FlowGraphNode node in graph.Nodes.Values)
            {
                if (!node.Statement.InitiallyVisible)
                    continue;

                queue.Enqueue(node);
                visited.Add(node);
            }

            while (queue.Count > 0)
            {
                FlowGraphNode current =
                    queue.Dequeue();

                foreach (FlowGraphEdge edge in current.Outgoing)
                {
                    if (edge.To == null)
                        continue;

                    if (visited.Add(edge.To))
                    {
                        queue.Enqueue(edge.To);
                    }
                }
            }

            foreach (FlowGraphNode node in graph.Nodes.Values)
            {
                if (visited.Contains(node))
                    continue;

                result.AddWarning(
                    ValidationScope.Flow,
                    $"Statement '{node.Statement.Id}' is unreachable.");
            }
        }
    }
}
