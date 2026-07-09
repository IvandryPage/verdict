using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Verdict.Data.Cases;
using Verdict.Presentation;
using Verdict.Systems;

namespace Verdict.UI
{
    public sealed class CourtroomView : MonoBehaviour
    {
        [Header("Case")]

        [SerializeField]
        private CaseData caseData;

        [Header("Texts")]

        [SerializeField]
        private TMP_Text statementText;

        [SerializeField]
        private TMP_Text witnessText;

        [SerializeField]
        private TMP_Text testimonyText;

        [SerializeField]
        private TMP_Text judgeTrustText;

        [SerializeField]
        private TMP_Text penaltyText;

        [Header("Buttons")]

        [SerializeField]
        private Button pressButton;

        [SerializeField]
        private Button continueButton;

        [SerializeField]
        private Button retryButton;

        [SerializeField]
        private Button presentButton;

        [Header("UI")]
        [SerializeField]
        private EvidencePanelView evidencePanel;

        private CaseSessionManager caseSessionManager;

        private CourtroomController controller;

        private CourtroomPresenter presenter;

        private void Awake()
        {
            BuildGameplay();

            BindPresenter();

            BindButtons();

            presenter.Refresh();
        }

        private void OnDestroy()
        {
            if (presenter == null)
                return;

            presenter.StatementChanged -= OnStatementChanged;
            presenter.WitnessChanged -= OnWitnessChanged;
            presenter.TestimonyChanged -= OnTestimonyChanged;
            presenter.JudgeTrustChanged -= OnJudgeTrustChanged;
            presenter.PenaltyChanged -= OnPenaltyChanged;
        }

        private void BuildGameplay()
        {
            caseSessionManager = new();

            caseSessionManager.LoadCase(caseData);

            var flow =
                new CourtroomFlow(
                    caseSessionManager.CurrentCase);

            var evaluation =
                new EvaluationSystem(flow);

            var processor =
                new CourtStateEffectProcessor(caseSessionManager.CurrentCase.CourtState);

            controller =
                new CourtroomController(
                    caseSessionManager,
                    flow,
                    evaluation,
                    processor);

            controller.BeginCase();

            presenter =
                new CourtroomPresenter(
                    controller);

            evidencePanel.Initialize(presenter, caseSessionManager.CurrentCase);
        }

        private void BindPresenter()
        {
            presenter.StatementChanged += OnStatementChanged;

            presenter.WitnessChanged += OnWitnessChanged;

            presenter.TestimonyChanged += OnTestimonyChanged;

            presenter.JudgeTrustChanged += OnJudgeTrustChanged;

            presenter.PenaltyChanged += OnPenaltyChanged;
        }

        private void BindButtons()
        {
            pressButton.onClick.AddListener(
                presenter.Press);

            continueButton.onClick.AddListener(
                presenter.Continue);

            retryButton.onClick.AddListener(
                presenter.Retry);

            presentButton.onClick.AddListener(
                presenter.PresentSelectedEvidence);
        }

        private void OnStatementChanged(string text)
        {
            statementText.text = text;
        }

        private void OnWitnessChanged(string witness)
        {
            witnessText.text = witness;
        }

        private void OnTestimonyChanged(string testimony)
        {
            testimonyText.text = testimony;
        }

        private void OnJudgeTrustChanged(float trust)
        {
            judgeTrustText.text = $"Judge Trust : {trust:0}";
        }

        private void OnPenaltyChanged(int penalty)
        {
            penaltyText.text = $"Penalty : {penalty}";
        }
    }
}
