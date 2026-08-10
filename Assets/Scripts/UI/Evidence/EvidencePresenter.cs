using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Verdict.Data.Evidence;

namespace Verdict.UI.Evidence
{
    /// <summary>
    /// Presents and manages the evidence selection interface.
    ///
    /// Responsibilities:
    /// - Build the evidence icon grid.
    /// - Open evidence details.
    /// - Track selected evidence.
    /// - Handle select / deselect.
    /// - Handle cancel.
    /// - Submit selected evidence for presentation.
    ///
    /// Does NOT:
    /// - Resolve evidence.
    /// - Decide whether evidence succeeds.
    /// - Modify CourtState.
    /// </summary>
    public sealed class EvidencePresenter : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField]
        private GameObject panel;

        [Header("Evidence List")]
        [SerializeField]
        private Transform evidenceContainer;

        [SerializeField]
        private EvidenceOptionView evidenceOptionPrefab;

        [Header("Evidence Detail")]
        [SerializeField]
        private EvidenceDetailView evidenceDetailView;

        [Header("Buttons")]
        [SerializeField]
        private Button cancelButton;

        [SerializeField]
        private Button presentButton;

        private readonly List<EvidenceOptionView>
            spawnedOptions = new();

        private readonly List<EvidenceData>
            selectedEvidence = new();

        public event Action CancelRequested;

        public event Action<IReadOnlyList<EvidenceData>>
            PresentRequested;

        public IReadOnlyList<EvidenceData>
            SelectedEvidence =>
            selectedEvidence;

        private void Awake()
        {
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(
                    HandleCancel);
            }

            if (presentButton != null)
            {
                presentButton.onClick.AddListener(
                    HandlePresent);
            }

            Hide();
        }

        public void Open(
            IReadOnlyList<EvidenceData> availableEvidence)
        {
            selectedEvidence.Clear();

            if (evidenceDetailView != null)
            {
                evidenceDetailView.Hide();
            }

            BuildEvidenceOptions(
                availableEvidence);

            UpdateOptionVisuals();
            UpdatePresentButton();

            if (panel != null)
            {
                panel.SetActive(true);
            }
        }

        public void Hide()
        {
            if (evidenceDetailView != null)
            {
                evidenceDetailView.Hide();
            }

            ClearOptions();

            selectedEvidence.Clear();

            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void BuildEvidenceOptions(
            IReadOnlyList<EvidenceData> availableEvidence)
        {
            ClearOptions();

            if (availableEvidence == null)
            {
                return;
            }

            foreach (EvidenceData evidence
                in availableEvidence)
            {
                if (evidence == null)
                {
                    continue;
                }

                EvidenceOptionView option =
                    Instantiate(
                        evidenceOptionPrefab,
                        evidenceContainer);

                option.Setup(
                    evidence,
                    HandleEvidenceClicked);

                spawnedOptions.Add(option);
            }
        }

        private void HandleEvidenceClicked(
            EvidenceData evidence)
        {
            if (evidence == null)
            {
                return;
            }

            if (evidenceDetailView == null)
            {
                Debug.LogWarning(
                    "EvidencePresenter: " +
                    "EvidenceDetailView is not assigned.",
                    this);

                return;
            }

            evidenceDetailView.Show(
                evidence,
                selectedEvidence.Contains(evidence),
                HandleSelectToggled,
                CloseDetail);
        }

        private void HandleSelectToggled(
            EvidenceData evidence)
        {
            if (evidence == null)
            {
                return;
            }

            if (selectedEvidence.Contains(evidence))
            {
                selectedEvidence.Remove(evidence);
            }
            else
            {
                selectedEvidence.Add(evidence);
            }

            UpdateOptionVisuals();
            UpdatePresentButton();

            if (evidenceDetailView != null)
            {
                evidenceDetailView.UpdateSelectionState(
                    selectedEvidence.Contains(evidence));
            }
        }

        private void CloseDetail()
        {
            if (evidenceDetailView != null)
            {
                evidenceDetailView.Hide();
            }
        }

        private void UpdateOptionVisuals()
        {
            foreach (EvidenceOptionView option
                in spawnedOptions)
            {
                if (option == null)
                {
                    continue;
                }

                option.SetSelected(
                    selectedEvidence.Contains(
                        option.Evidence));
            }
        }

        private void UpdatePresentButton()
        {
            if (presentButton != null)
            {
                presentButton.interactable =
                    selectedEvidence.Count > 0;
            }
        }

        private void HandleCancel()
        {
            Hide();

            CancelRequested?.Invoke();
        }

        private void HandlePresent()
        {
            if (selectedEvidence.Count == 0)
            {
                return;
            }

            List<EvidenceData> submittedEvidence =
                selectedEvidence.ToList();

            Hide();

            PresentRequested?.Invoke(
                submittedEvidence);
        }

        private void ClearOptions()
        {
            foreach (EvidenceOptionView option
                in spawnedOptions)
            {
                if (option != null)
                {
                    Destroy(option.gameObject);
                }
            }

            spawnedOptions.Clear();
        }

        private void OnDestroy()
        {
            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(
                    HandleCancel);
            }

            if (presentButton != null)
            {
                presentButton.onClick.RemoveListener(
                    HandlePresent);
            }
        }
    }
}
