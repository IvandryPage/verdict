using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Verdict.Data.Evidence;
using Verdict.Input;
using Verdict.Presentation.Courtroom;
using Verdict.Runtime;
using Verdict.Systems;

namespace Verdict.Presentation.Evidence
{
    /// <summary>
    /// Evidence inventory. Opens over the Statement panel, lets the
    /// player look at unlocked evidence, then confirm presenting one.
    /// Two-step (select -&gt; confirm) on purpose - it mirrors how the
    /// genre builds a small beat of tension before committing to an
    /// argument, rather than a single blind click.
    /// </summary>
    public sealed class EvidencePanel : MonoBehaviour
    {
        [Header("Bootstrap")]
        [SerializeField] private CourtroomBootstrap bootstrap;

        [Header("Root")]
        [SerializeField] private CanvasGroup root;

        [Header("Grid")]
        [SerializeField] private RectTransform slotContainer;
        [SerializeField] private EvidenceSlotView slotPrefab;

        [Header("Detail / Confirm")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image artworkImage;
        [SerializeField] private TMP_Text detailNameText;
        [SerializeField] private TMP_Text detailDescriptionText;
        [SerializeField] private Button presentButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button closeButton;

        private CourtroomController controller;
        private readonly List<EvidenceSlotView> spawned = new();
        private EvidenceData selected;

        private VerdictInputActions inputActions;

        private void Awake()
        {
            if (bootstrap == null)
            {
                bootstrap = FindFirstObjectByType<CourtroomBootstrap>();
            }

            inputActions = new VerdictInputActions();
        }

        private void Start()
        {
            if (bootstrap == null || bootstrap.Controller == null)
            {
                Debug.LogError($"{nameof(EvidencePanel)}: Missing CourtroomBootstrap or Controller.");
                enabled = false;
                return;
            }

            controller = bootstrap.Controller;
            controller.ArgumentResolved += HandleArgumentResolved;

            SetVisible(false);
            ShowGrid();
        }

        private void OnEnable()
        {
            presentButton?.onClick.AddListener(HandlePresentClicked);
            backButton?.onClick.AddListener(ShowGrid);
            closeButton?.onClick.AddListener(Close);

            inputActions.UI.Enable();
            inputActions.UI.Cancel.performed += HandleCancelPerformed;
        }

        private void OnDisable()
        {
            presentButton?.onClick.RemoveListener(HandlePresentClicked);
            backButton?.onClick.RemoveListener(ShowGrid);
            closeButton?.onClick.RemoveListener(Close);

            inputActions.UI.Cancel.performed -= HandleCancelPerformed;
            inputActions.UI.Disable();
        }

        private void OnDestroy()
        {
            inputActions?.Dispose();

            if (controller != null)
            {
                controller.ArgumentResolved -= HandleArgumentResolved;
            }
        }

        private void HandleCancelPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (root == null || root.alpha <= 0f)
            {
                return;
            }

            // On the detail view, Cancel backs out to the grid first;
            // pressing it again (or from the grid) closes the panel -
            // same "cancel" a Back button would give you.
            if (detailPanel != null && detailPanel.activeSelf)
            {
                ShowGrid();
            }
            else
            {
                Close();
            }
        }

        private bool allowPresenting = true;

        public void Open(bool? allowPresenting = null)
        {
            this.allowPresenting = allowPresenting ?? controller?.CanInteract ?? false;

            RebuildGrid();
            ShowGrid();
            SetVisible(true);
        }

        public void Close()
        {
            SetVisible(false);
        }

        private void HandleArgumentResolved(ResolverResult result)
        {
            // Whatever the outcome, presenting evidence is a one-shot
            // action - close so the Statement panel (or whatever comes
            // next) can take over.
            Close();
        }

        private void RebuildGrid()
        {
            foreach (EvidenceSlotView view in spawned)
            {
                if (view != null)
                {
                    Destroy(view.gameObject);
                }
            }

            spawned.Clear();

            if (slotPrefab == null || slotContainer == null || controller?.CurrentStatement == null)
            {
                return;
            }

            CaseRuntime runtime = FindCaseRuntime();

            if (runtime == null)
            {
                return;
            }

            IEnumerable<EvidenceData> unlocked = runtime.Evidence
                .Where(e => e.IsUnlocked)
                .Select(e => e.Data);

            foreach (EvidenceData evidence in unlocked)
            {
                EvidenceSlotView view = Instantiate(slotPrefab, slotContainer);
                view.Bind(evidence);
                view.Clicked += HandleSlotClicked;
                spawned.Add(view);
            }
        }

        private CaseRuntime FindCaseRuntime()
        {
            return bootstrap != null ? bootstrap.Runtime : null;
        }

        private void HandleSlotClicked(EvidenceData evidence)
        {
            selected = evidence;
            ShowDetail();
        }

        private void ShowGrid()
        {
            selected = null;

            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }

            if (slotContainer != null)
            {
                slotContainer.gameObject.SetActive(true);
            }
        }

        private void ShowDetail()
        {
            if (selected == null)
            {
                return;
            }

            if (slotContainer != null)
            {
                slotContainer.gameObject.SetActive(false);
            }

            if (detailPanel != null)
            {
                detailPanel.SetActive(true);
            }

            if (artworkImage != null)
            {
                Sprite art = selected.Artwork != null ? selected.Artwork : selected.Icon;
                artworkImage.sprite = art;
                artworkImage.enabled = art != null;
            }

            if (detailNameText != null)
            {
                detailNameText.text = selected.DisplayName;
            }

            if (detailDescriptionText != null)
            {
                detailDescriptionText.text = selected.Description;
            }

            if (presentButton != null)
            {
                presentButton.gameObject.SetActive(allowPresenting);
            }
        }

        private void HandlePresentClicked()
        {
            if (selected == null || controller == null || !allowPresenting)
            {
                return;
            }

            controller.PresentEvidence(selected);
        }

        private void SetVisible(bool visible)
        {
            if (root == null)
            {
                return;
            }

            root.alpha = visible ? 1f : 0f;
            root.interactable = visible;
            root.blocksRaycasts = visible;
        }
    }
}
