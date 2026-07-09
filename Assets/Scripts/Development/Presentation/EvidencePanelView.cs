using UnityEngine;
using Verdict.Data.Evidence;
using Verdict.Presentation;
using Verdict.Runtime;

namespace Verdict.UI
{
    public sealed class EvidencePanelView : MonoBehaviour
    {
        [SerializeField]
        private EvidenceItemView itemPrefab;

        [SerializeField]
        private Transform content;

        private CourtroomPresenter presenter;

        private EvidenceItemView selectedItem;

        public void Initialize(
            CourtroomPresenter presenter,
            CaseRuntime runtime)
        {
            this.presenter = presenter;

            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            foreach (EvidenceRuntime evidence in runtime.Evidence)
            {
                EvidenceItemView item =
                    Instantiate(
                        itemPrefab,
                        content);

                item.Bind(evidence.Data);

                item.Selected += evidence => SelectItem(item, evidence);
            }
        }

        private void SelectItem(
            EvidenceItemView item,
            EvidenceData evidence)
        {
            if (selectedItem != null)
            {
                selectedItem.SetSelected(false);
            }

            selectedItem = item;

            selectedItem.SetSelected(true);

            presenter.SelectEvidence(evidence);
        }
    }
}
