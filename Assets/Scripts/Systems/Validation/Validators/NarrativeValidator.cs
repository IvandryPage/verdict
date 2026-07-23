using System;
using System.Collections.Generic;
using System.Linq;
using Verdict.Data.Cases;
using Verdict.Data.Narrative;

namespace Verdict.Systems.Validation
{
    /// <summary>
    /// Checks the narrative graph for missing nodes, broken links,
    /// duplicate node ids and statement references that don't exist.
    /// </summary>
    public sealed class NarrativeValidator : ICaseValidator
    {
        public void Validate(
            CaseData caseData,
            ValidationResult result)
        {
            if (caseData == null)
            {
                return;
            }

            NarrativeGraphData graph = caseData.Narrative;

            ValidationUtility.Warning(
                result,
                graph != null,
                ValidationScope.Narrative,
                $"Case '{caseData.name}' has no narrative graph.");

            if (graph == null)
            {
                return;
            }

            IReadOnlyList<NarrativeNodeData> nodes = graph.Nodes;

            if (nodes == null || nodes.Count == 0)
            {
                ValidationUtility.Warning(
                    result,
                    false,
                    ValidationScope.Narrative,
                    $"Case '{caseData.name}' has an empty narrative graph.");

                return;
            }

            var nodesById = new Dictionary<string, List<NarrativeNodeData>>(
                StringComparer.Ordinal);

            foreach (NarrativeNodeData node in nodes)
            {
                if (node == null)
                {
                    result.AddError(
                        ValidationScope.Narrative,
                        $"Case '{caseData.name}' has a null narrative node.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(node.NodeId))
                {
                    result.AddError(
                        ValidationScope.Narrative,
                        $"Case '{caseData.name}' has a narrative node with no id.");

                    continue;
                }

                if (!nodesById.TryGetValue(node.NodeId, out List<NarrativeNodeData> group))
                {
                    group = new List<NarrativeNodeData>();
                    nodesById.Add(node.NodeId, group);
                }

                group.Add(node);
            }

            foreach (KeyValuePair<string, List<NarrativeNodeData>> entry in nodesById)
            {
                ValidationUtility.Ensure(
                    result,
                    entry.Value.Count == 1,
                    ValidationScope.Narrative,
                    $"Case '{caseData.name}' has {entry.Value.Count} narrative nodes sharing id '{entry.Key}'.",
                    entry.Key);
            }

            ValidationUtility.Ensure(
                result,
                !string.IsNullOrWhiteSpace(graph.StartNodeId),
                ValidationScope.Narrative,
                $"Case '{caseData.name}' narrative graph has no StartNodeId.");

            if (!string.IsNullOrWhiteSpace(graph.StartNodeId))
            {
                ValidationUtility.Ensure(
                    result,
                    nodesById.ContainsKey(graph.StartNodeId),
                    ValidationScope.Narrative,
                    $"Case '{caseData.name}' StartNodeId '{graph.StartNodeId}' does not match any node.",
                    graph.StartNodeId);
            }

            ValidateLinks(caseData, nodes, nodesById, result);
            ValidateStatementReferences(caseData, nodes, result);
            ValidateReachability(caseData, graph, nodesById, result);
        }

        /// <summary>
        /// "Tidak ada Narrative Branch yang mustahil dicapai" - BFS from
        /// StartNodeId over every node's outgoing connections. Anything
        /// never reached is an unreachable branch - almost always an
        /// authoring mistake (dangling content or a forgotten link).
        /// </summary>
        private static void ValidateReachability(
            CaseData caseData,
            NarrativeGraphData graph,
            IReadOnlyDictionary<string, List<NarrativeNodeData>> nodesById,
            ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(graph.StartNodeId) ||
                !nodesById.ContainsKey(graph.StartNodeId))
            {
                // Already reported as an error by the StartNodeId check above.
                return;
            }

            HashSet<string> visited = new(StringComparer.Ordinal);
            Queue<string> queue = new();

            queue.Enqueue(graph.StartNodeId);
            visited.Add(graph.StartNodeId);

            while (queue.Count > 0)
            {
                string currentId = queue.Dequeue();

                if (!nodesById.TryGetValue(currentId, out List<NarrativeNodeData> group))
                {
                    continue;
                }

                foreach (NarrativeNodeData node in group)
                {
                    foreach (string nextId in node.GetOutgoingNodeIds())
                    {
                        if (string.IsNullOrWhiteSpace(nextId) || visited.Contains(nextId))
                        {
                            continue;
                        }

                        visited.Add(nextId);
                        queue.Enqueue(nextId);
                    }
                }
            }

            foreach (KeyValuePair<string, List<NarrativeNodeData>> entry in nodesById)
            {
                ValidationUtility.Warning(
                    result,
                    visited.Contains(entry.Key),
                    ValidationScope.Narrative,
                    $"Case '{caseData.name}' node '{entry.Key}' is never reachable from StartNodeId.",
                    entry.Key);
            }
        }

        private static void ValidateLinks(
            CaseData caseData,
            IReadOnlyList<NarrativeNodeData> nodes,
            IReadOnlyDictionary<string, List<NarrativeNodeData>> nodesById,
            ValidationResult result)
        {
            foreach (NarrativeNodeData node in nodes)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.NodeId))
                {
                    continue;
                }

                foreach (string targetId in node.GetOutgoingNodeIds())
                {
                    if (string.IsNullOrWhiteSpace(targetId))
                    {
                        continue;
                    }

                    ValidationUtility.Ensure(
                        result,
                        nodesById.ContainsKey(targetId),
                        ValidationScope.Narrative,
                        $"Case '{caseData.name}' node '{node.NodeId}' links to missing node '{targetId}'.",
                        node.NodeId);
                }
            }
        }

        private static void ValidateStatementReferences(
            CaseData caseData,
            IReadOnlyList<NarrativeNodeData> nodes,
            ValidationResult result)
        {
            HashSet<string> statementIds = CollectStatementIds(caseData);

            var seenStatementIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (NarrativeNodeData node in nodes)
            {
                if (node is not StatementNodeData statementNode)
                {
                    continue;
                }

                ValidationUtility.Ensure(
                    result,
                    !string.IsNullOrWhiteSpace(statementNode.StatementId),
                    ValidationScope.Narrative,
                    $"Case '{caseData.name}' StatementNode '{statementNode.NodeId}' has no StatementId.",
                    statementNode.NodeId);

                if (string.IsNullOrWhiteSpace(statementNode.StatementId))
                {
                    continue;
                }

                ValidationUtility.Ensure(
                    result,
                    statementIds.Contains(statementNode.StatementId),
                    ValidationScope.Narrative,
                    $"Case '{caseData.name}' StatementNode '{statementNode.NodeId}' references unknown statement '{statementNode.StatementId}'.",
                    statementNode.NodeId);

                if (!seenStatementIds.Add(statementNode.StatementId))
                {
                    result.AddWarning(
                        ValidationScope.Narrative,
                        $"Case '{caseData.name}' has more than one StatementNode bound to statement '{statementNode.StatementId}'.",
                        statementNode.NodeId);
                }
            }
        }

        private static HashSet<string> CollectStatementIds(CaseData caseData)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);

            if (caseData.Witnesses == null)
            {
                return ids;
            }

            foreach (WitnessData witness in caseData.Witnesses)
            {
                if (witness?.Testimonies == null)
                {
                    continue;
                }

                foreach (TestimonyData testimony in witness.Testimonies)
                {
                    if (testimony?.Statements == null)
                    {
                        continue;
                    }

                    foreach (StatementData statement in testimony.Statements)
                    {
                        if (statement != null && !string.IsNullOrWhiteSpace(statement.Id))
                        {
                            ids.Add(statement.Id);
                        }
                    }
                }
            }

            return ids;
        }
    }
}
