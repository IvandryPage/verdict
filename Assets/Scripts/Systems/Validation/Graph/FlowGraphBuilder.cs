using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Systems.Validation.Graph
{
    public static class FlowGraphBuilder
    {
        public static FlowGraph Build(
            CaseData caseData)
        {
            FlowGraph graph = new();

            CreateNodes(
                graph,
                caseData);

            LinkSequentialFlow(
                graph,
                caseData);

            LinkGameplayEffects(
                graph);

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

        /// <summary>
        /// Builds the default testimony flow.
        /// If NextStatementId is empty, the next statement in the testimony is used.
        /// </summary>
        private static void LinkSequentialFlow(
            FlowGraph graph,
            CaseData caseData)
        {
            foreach (WitnessData witness in caseData.Witnesses)
            {
                foreach (TestimonyData testimony in witness.Testimonies)
                {
                    IReadOnlyList<StatementData> statements =
                        testimony.Statements;

                    for (int i = 0; i < statements.Count; i++)
                    {
                        StatementData current =
                            statements[i];

                        if (!graph.TryGetNode(
                            current.Id,
                            out FlowGraphNode currentNode))
                        {
                            continue;
                        }

                        string targetId = current.NextStatementId;

                        // fallback ke statement berikutnya
                        if (string.IsNullOrWhiteSpace(targetId))
                        {
                            if (i >= statements.Count - 1)
                            {
                                continue;
                            }

                            targetId =
                                statements[i + 1].Id;
                        }

                        graph.TryGetNode(
                            targetId,
                            out FlowGraphNode targetNode);

                        currentNode.AddEdge(
                            new FlowGraphEdge(
                                currentNode,
                                targetNode,
                                targetId,
                                FlowEdgeType.NextStatement));
                    }
                }
            }
        }

        private static void LinkGameplayEffects(
            FlowGraph graph)
        {
            foreach (FlowGraphNode node in graph.Nodes.Values)
            {
                foreach (ClaimData claim in node.Statement.Claims)
                {
                    foreach (EvaluationRuleData rule in claim.EvaluationRules)
                    {
                        LinkEffects(
                            node,
                            graph,
                            rule.SuccessEffects);

                        LinkEffects(
                            node,
                            graph,
                            rule.FailureEffects);
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

            if (!edgeType.HasValue)
            {
                return;
            }

            graph.TryGetNode(
                effect.TargetId,
                out FlowGraphNode targetNode);

            node.AddEdge(
                new FlowGraphEdge(
                    node,
                    targetNode,
                    effect.TargetId,
                    edgeType.Value));
        }
    }
}
