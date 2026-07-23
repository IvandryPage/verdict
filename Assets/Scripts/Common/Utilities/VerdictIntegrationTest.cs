using System;
using UnityEngine;
using Verdict.Data.Cases;
using Verdict.Runtime;
using Verdict.Systems;
using Verdict.Systems.Evaluation;

public sealed class VerdictIntegrationTest : MonoBehaviour
{
    [Header("Test Case")]
    [SerializeField]
    private CaseData caseData;

    private CourtroomFlow courtroomFlow;

    private ResolverEngine resolver;

    private CourtStateEffectProcessor processor;

    private void Start()
    {
        if (caseData == null)
        {
            Debug.LogError("No CaseData assigned.");
            return;
        }

        Debug.Log("========== CREATE RUNTIME ==========");

        CaseRuntime runtime =
            RuntimeFactory.Create(caseData);

        courtroomFlow =
            new CourtroomFlow(runtime);

        resolver =
            new ResolverEngine(courtroomFlow);

        processor =
            new CourtStateEffectProcessor(runtime);

        Debug.Log("Runtime Created");

        RunIntegration(runtime);
    }

    private void RunIntegration(CaseRuntime runtime)
    {
        Debug.Log("========== BEGIN TEST ==========");

        StatementRuntime statement =
            courtroomFlow.CurrentStatement;

        if (statement == null)
        {
            Debug.LogError("No active statement.");
            return;
        }

        Debug.Log($"Current Statement : {statement.Data.Text}");

        //----------------------------------------------------
        // Example:
        // Present first unlocked evidence
        //----------------------------------------------------

        if (runtime.Evidence.Count == 0)
        {
            Debug.LogWarning("No evidence.");
            return;
        }

        var evidence =
            runtime.Evidence[1].Data;

        Debug.Log(evidence.DisplayName);

        PlayerArgumentData argument =
            PlayerArgumentData.PresentEvidence(
                evidence,
                statement.Data);

        ResolverResult result =
            resolver.Resolve(argument);

        Debug.Log($"Success : {result.IsSuccess}");

        foreach (var log in result.Diagnostics)
            Debug.Log(log);

        CourtStateEffectProcessingResult process =
            processor.Apply(result);


        Debug.Log($"Generated Effects : {result.GeneratedEffects.Count}");

        CourtStateEffectProcessingResult processing =
            processor.Apply(result);

        Debug.Log($"Judge Trust : {runtime.CourtState.GetCourtStat(CourtStat.JudgeTrust)}");

        Debug.Log("========== END ==========");
    }
}
