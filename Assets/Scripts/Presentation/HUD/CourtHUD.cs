using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Verdict.Presentation.Courtroom;
using Verdict.Presentation.Evidence;
using Verdict.Runtime;
using Verdict.Systems;

namespace Verdict.Presentation.HUD
{
    /// <summary>
    /// Live view of CourtState - drop one StatGaugeView per stat you want
    /// visible (Judge Trust, Penalty, etc) and this keeps them in sync.
    /// Refreshes whenever an argument resolves or the case (re)starts -
    /// those are the only moments CourtState can actually change. Also
    /// hosts a persistent "check evidence" button - browsing the court
    /// record shouldn't require pausing first.
    /// </summary>
    public sealed class CourtHUD : MonoBehaviour
    {
        [Header("Bootstrap")]
        [SerializeField] private CourtroomBootstrap bootstrap;

        [Header("Gauges")]
        [SerializeField] private List<StatGaugeView> gauges = new();

        [Header("Banner (optional)")]
        [SerializeField] private TMP_Text witnessNameText;
        [SerializeField] private TMP_Text testimonyTitleText;

        [Header("Evidence Quick Access")]
        [SerializeField] private Button checkEvidenceButton;
        [SerializeField] private EvidencePanel evidencePanel;

        private CourtroomController controller;

        /// <summary>
        /// Raised the moment any gauge crosses into its low-value warning
        /// state - hook a screen vignette, heartbeat sound, etc to this.
        /// </summary>
        public event Action<StatGaugeView> GaugeEnteredWarning;

        private void Awake()
        {
            if (bootstrap == null)
            {
                bootstrap = FindFirstObjectByType<CourtroomBootstrap>();
            }
        }

        private void Start()
        {
            if (bootstrap == null || bootstrap.Controller == null)
            {
                Debug.LogError($"{nameof(CourtHUD)}: Missing CourtroomBootstrap or Controller.");
                enabled = false;
                return;
            }

            controller = bootstrap.Controller;

            controller.CaseStarted += Refresh;
            controller.CaseRestarted += Refresh;
            controller.ArgumentResolved += HandleArgumentResolved;
            controller.CurrentStatementChanged += HandleCurrentStatementChanged;

            Refresh();
        }

        private void OnDestroy()
        {
            if (controller == null)
            {
                return;
            }

            controller.CaseStarted -= Refresh;
            controller.CaseRestarted -= Refresh;
            controller.ArgumentResolved -= HandleArgumentResolved;
            controller.CurrentStatementChanged -= HandleCurrentStatementChanged;
        }

        private void OnEnable()
        {
            checkEvidenceButton?.onClick.AddListener(HandleCheckEvidenceClicked);
        }

        private void OnDisable()
        {
            checkEvidenceButton?.onClick.RemoveListener(HandleCheckEvidenceClicked);
        }

        private void HandleCheckEvidenceClicked()
        {
            // Browse-only from the HUD button - if a statement is
            // active, Open() already defaults to allowing Present via
            // controller.CanInteract, same as opening it from the
            // Statement panel itself.
            evidencePanel?.Open();
        }

        private void HandleArgumentResolved(ResolverResult result)
        {
            Refresh();
        }

        private void HandleCurrentStatementChanged(StatementRuntime statement)
        {
            if (witnessNameText != null)
            {
                witnessNameText.text = controller.CurrentWitness?.Data?.Character?.DisplayName
                    ?? controller.CurrentWitness?.Data?.Id
                    ?? string.Empty;
            }

            if (testimonyTitleText != null)
            {
                testimonyTitleText.text = controller.CurrentTestimony?.Data?.Title ?? string.Empty;
            }
        }

        private void Refresh()
        {
            CourtStateRuntime state = controller?.CourtState;

            if (state == null)
            {
                return;
            }

            foreach (StatGaugeView gauge in gauges)
            {
                if (gauge == null)
                {
                    continue;
                }

                bool wasWarning = gauge.IsInWarningState;

                gauge.SetValue(state.GetCourtStat(gauge.Stat));

                if (!wasWarning && gauge.IsInWarningState)
                {
                    GaugeEnteredWarning?.Invoke(gauge);
                }
            }
        }
    }
}
