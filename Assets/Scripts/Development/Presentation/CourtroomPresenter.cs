using System;
using Verdict.Data.Evidence;
using Verdict.Runtime;
using Verdict.Systems;

namespace Verdict.Presentation
{
    public sealed class CourtroomPresenter
    {
        private readonly CourtroomController controller;
        private EvidenceData selectedEvidence;

        public EvidenceData SelectedEvidence => selectedEvidence;

        public event Action<string> StatementChanged;
        public event Action<string> WitnessChanged;
        public event Action<string> TestimonyChanged;

        public event Action<int> PenaltyChanged;
        public event Action<float> JudgeTrustChanged;

        public event Action<EvidenceData> SelectedEvidenceChanged;

        public event Action<EvaluationResult> EvaluationCompleted;

        public CourtroomPresenter(
            CourtroomController controller)
        {
            this.controller =
                controller ?? throw new ArgumentNullException(nameof(controller));

            controller.CurrentStatementChanged +=
                HandleStatementChanged;

            controller.EvaluationCompleted +=
                HandleEvaluationCompleted;
        }

        public string CurrentStatement =>
            controller.CurrentStatement?.Data.Text ?? string.Empty;

        public string CurrentWitness =>
            controller.CurrentWitness?.Data.Character.DisplayName ?? string.Empty;

        public string CurrentTestimony =>
            controller.CurrentTestimony?.Data.Title ?? string.Empty;

        public int Penalty =>
            controller.CourtState?.Penalty ?? 0;

        public float JudgeTrust =>
            controller.CourtState?.JudgeTrust ?? 0f;

        public void Press()
        {
            controller.Press();
        }

        public void PresentEvidence(
            EvidenceData evidence)
        {
            controller.PresentEvidence(evidence);
        }

        public void Continue()
        {
            controller.MoveNextStatement();
        }

        public void Retry()
        {
            controller.RestartCase();
        }

        private void HandleStatementChanged(
            StatementRuntime statement)
        {
            StatementChanged?.Invoke(
                statement.Data.Text);

            WitnessChanged?.Invoke(
                CurrentWitness);

            TestimonyChanged?.Invoke(
                CurrentTestimony);
        }

        private void HandleEvaluationCompleted(
            EvaluationResult result)
        {
            PenaltyChanged?.Invoke(
                Penalty);

            JudgeTrustChanged?.Invoke(
                JudgeTrust);

            EvaluationCompleted?.Invoke(result);
        }

        public void Refresh()
        {
            StatementChanged?.Invoke(
                CurrentStatement);

            WitnessChanged?.Invoke(
                CurrentWitness);

            TestimonyChanged?.Invoke(
                CurrentTestimony);

            PenaltyChanged?.Invoke(
                Penalty);

            JudgeTrustChanged?.Invoke(
                JudgeTrust);
        }

        public void SelectEvidence(EvidenceData evidence)
        {
            selectedEvidence = evidence;

            SelectedEvidenceChanged?.Invoke(evidence);
        }

        public void PresentSelectedEvidence()
        {
            if (selectedEvidence == null)
                return;

            controller.PresentEvidence(selectedEvidence);
        }
    }
}
