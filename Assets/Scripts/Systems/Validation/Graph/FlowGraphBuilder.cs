using System.Collections.Generic;
using UnityEngine;
using Verdict.Data.Cases;

namespace Verdict.Systems.Validation.Graph
{
    public static class FlowGraphBuilder
    {
        public static FlowGraph Build(
            CaseData caseData)
        {
            FlowGraph graph = new();

            if (caseData == null)
            {
                return graph;
            }

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
                                out FlowGraphNode from))
                        {
                            continue;
                        }

                        string targetId =
                            current.NextStatementId;

                        // fallback ke statement berikutnya
                        if (string.IsNullOrWhiteSpace(targetId))
                        {
                            if (i >= statements.Count - 1)
                                continue;

                            targetId =
                                statements[i + 1].Id;
                        }

                        if (!graph.TryGetNode(
                                targetId,
                                out FlowGraphNode to))
                        {
                            continue;
                        }

                        from.AddEdge(
                            new FlowGraphEdge(
                                from,
                                to,
                                targetId,
                                FlowEdgeType.NextStatement));
                    }
                }
            }
        }


        private static void LinkGameplayEffects(
            FlowGraph graph)
        {
            foreach (FlowGraphNode node
                in graph.Nodes.Values)
            {
                IReadOnlyList<ClaimData> claims =
                    node.Statement.Claims;


                foreach (ClaimData claim in claims)
                {
                    foreach (EvaluationRuleData rule
                        in claim.EvaluationRules)
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
            FlowEdgeType? edgeType =
                ConvertEffectToEdgeType(
                    effect.Effect);


            if (!edgeType.HasValue)
            {
                return;
            }


            if (!graph.TryGetNode(
                    effect.TargetId,
                    out FlowGraphNode target))
            {
                return;
            }


            node.AddEdge(
                new FlowGraphEdge(
                    node,
                    target,
                    effect.TargetId,
                    edgeType.Value));
        }


        private static FlowEdgeType? ConvertEffectToEdgeType(
            CourtStateEffect effect)
        {
            return effect switch
            {
                CourtStateEffect.RevealStatement =>
                    FlowEdgeType.RevealStatement,

                CourtStateEffect.JumpStatement =>
                    FlowEdgeType.JumpStatement,


                CourtStateEffect.RevealWitness =>
                    FlowEdgeType.RevealWitness,

                CourtStateEffect.JumpWitness =>
                    FlowEdgeType.JumpWitness,


                CourtStateEffect.RevealTestimony =>
                    FlowEdgeType.RevealTestimony,

                CourtStateEffect.JumpTestimony =>
                    FlowEdgeType.JumpTestimony,


                _ => null
            };
        }
    }
}
