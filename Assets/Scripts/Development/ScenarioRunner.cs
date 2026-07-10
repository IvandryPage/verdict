using System;
using UnityEngine;
using Verdict.Data.Cases;
using Verdict.Data.Evidence;
using Verdict.Runtime;
using Verdict.Systems;

namespace Verdict.Development
{
    public sealed class ScenarioRunner : MonoBehaviour
    {
        [Header("Scenario")]
        [SerializeField] private CaseData caseData;

        [SerializeField] private EvidenceData testEvidence;

        private CaseSessionManager caseSessionManager;
        private CourtroomFlow courtroomFlow;
        private EvaluationSystem evaluationSystem;
        private CourtStateEffectProcessor effectProcessor;
        private CourtroomController courtroomController;

        private void Start()
        {
            if (caseData == null)
            {
                Debug.LogError("Case Data is missing.");
                return;
            }

            Initialize();

            RunScenario();
        }

        private void Initialize()
        {
            caseSessionManager = new CaseSessionManager();

            caseSessionManager.LoadCase(caseData);

            CaseRuntime runtime = caseSessionManager.CurrentCase;

            courtroomFlow = new CourtroomFlow(runtime);

            evaluationSystem = new EvaluationSystem(courtroomFlow);

            effectProcessor = new CourtStateEffectProcessor(runtime.CourtState);

            courtroomController = new CourtroomController(
                caseSessionManager,
                courtroomFlow,
                evaluationSystem,
                effectProcessor);

            courtroomController.EvaluationCompleted += OnEvaluationCompleted;
            courtroomController.CurrentStatementChanged += OnStatementChanged;
        }

        private void RunScenario()
        {
            LogHeader("Scenario Started");

            PrintCurrentStatement();

            PresentEvidence();

            Press();

            NextStatement();

            Question();

            RemainSilent();

            LogHeader("Scenario Finished");
        }

        private void PresentEvidence()
        {
            if (testEvidence == null)
            {
                return;
            }

            Debug.Log($"Present Evidence : {testEvidence.name}");

            courtroomController.PresentEvidence(testEvidence);
        }

        private void Press()
        {
            Debug.Log("Press");

            courtroomController.Press();
        }

        private void Question()
        {
            Debug.Log("Question");

            courtroomController.Question();
        }

        private void RemainSilent()
        {
            Debug.Log("Remain Silent");

            courtroomController.RemainSilent();
        }

        private void NextStatement()
        {
            if (!courtroomController.Continue())
            {
                Debug.Log("No more statements.");

                return;
            }

            PrintCurrentStatement();
        }

        private void PrintCurrentStatement()
        {
            StatementRuntime statement = courtroomFlow.CurrentStatement;

            Debug.Log($"Current Statement : {statement.Data.Text}");
        }

        private void OnEvaluationCompleted(EvaluationResult result)
        {
            Debug.Log("———————— Evaluation ————————");

            Debug.Log($"Success : {result.IsSuccess}");

            if (result.MatchedClaim != null)
            {
                Debug.Log($"Claim : {result.MatchedClaim.Id}");
            }

            if (result.MatchedRule != null)
            {
                Debug.Log($"Rule : {result.MatchedRule.EvaluationType}");
            }

            CourtStateRuntime state =
                caseSessionManager.CurrentCase.CourtState;

            Debug.Log($"Judge Trust : {state.JudgeTrust}");

            Debug.Log($"Penalty : {state.Penalty}");

            Debug.Log("——————————————————————————");
        }

        private void OnStatementChanged(StatementRuntime statement)
        {
            Debug.Log($"Statement Changed -> {statement.Data.Id}");
        }

        private static void LogHeader(string title)
        {
            Debug.Log($"——————————— {title} ———————————");
        }

        private void OnDestroy()
        {
            if (courtroomController == null)
            {
                return;
            }

            courtroomController.EvaluationCompleted -= OnEvaluationCompleted;
            courtroomController.CurrentStatementChanged -= OnStatementChanged;
        }
    }
}
