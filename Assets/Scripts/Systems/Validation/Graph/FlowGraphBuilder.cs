
using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Systems.Validation.Graph
{
    public static class FlowGraphBuilder
    {
        public static FlowGraph Build(CaseData caseData)
        {
            FlowGraph graph = new();

            CreateNodes(graph, caseData);

            LinkNextStatements(graph);

            LinkEffects(graph);

            return graph;
        }

        private static void CreateNodes(
            FlowGraph graph,
            CaseData caseData)
        {
            foreach (WitnessData witness in caseData.Witnesses)
            {
                foreach (TestimonyData testimony in witness.Testimonies)
                {
                    foreach (StatementData statement in testimony.Statements)
                    {
                        graph.AddNode(
                            new FlowGraphNode(statement));
                    }
                }
            }
        }

        private static void LinkNextStatements(
            FlowGraph graph)
        {
            foreach (FlowGraphNode node in graph.Nodes.Values)
            {
                StatementData statement = node.Statement;

                if (string.IsNullOrWhiteSpace(statement.NextStatementId))
                {
                    continue;
                }

                graph.Nodes.TryGetValue(
                    statement.NextStatementId,
                    out FlowGraphNode target);

                node.Outgoing.Add(
                    new FlowGraphEdge(
                        node,
                        target,
                        statement.NextStatementId,
                        FlowEdgeType.NextStatement));
            }
        }

        private static void LinkEffects(
            FlowGraph graph)
        {
            foreach (FlowGraphNode node in graph.Nodes.Values)
            {
                foreach (ClaimData claim in node.Statement.Claims)
                {
                    foreach (EvaluationRuleData rule in claim.EvaluationRules)
                    {
                        LinkEffects(node, graph, rule.SuccessEffects);

                        LinkEffects(node, graph, rule.FailureEffects);
                    }
                }
            }
        }

        private static void LinkEffects(
            FlowGraphNode node,
            FlowGraph graph,
            IReadOnlyList<CourtStateEffectData> effects)
        {
            foreach (CourtStateEffectData effect in effects)
            {
                LinkEffect(
                    node,
                    graph,
                    effect);
            }
        }

        private static void LinkEffect(
            FlowGraphNode node,
            FlowGraph graph,
            CourtStateEffectData effect)
        {
            FlowEdgeType? edgeType = effect.Effect switch
            {
                CourtStateEffect.RevealStatement => FlowEdgeType.RevealStatement,
                CourtStateEffect.JumpStatement => FlowEdgeType.JumpStatement,
                CourtStateEffect.RevealWitness => FlowEdgeType.RevealWitness,
                CourtStateEffect.JumpWitness => FlowEdgeType.JumpWitness,
                CourtStateEffect.RevealTestimony => FlowEdgeType.RevealTestimony,
                CourtStateEffect.JumpTestimony => FlowEdgeType.JumpTestimony,

                _ => null
            };

            if (edgeType == null)
            {
                return;
            }

            graph.Nodes.TryGetValue(
                effect.TargetId,
                out FlowGraphNode target);

            node.Outgoing.Add(
                new FlowGraphEdge(
                    node,
                    target,
                    effect.TargetId,
                    edgeType.Value));
        }
    }
}
