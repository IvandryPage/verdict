using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Verdict.Data.Cases;

namespace Verdict.UI.Narrative
{
    public sealed class ChoiceOptionView : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [SerializeField]
        private TMP_Text label;

        private PlayerAction action;
        private Action<PlayerAction> callback;
        private int choiceIndex;
        private Action<int> choiceCallback;

        public void SetText(string text)
        {
            if (label != null)
            {
                label.text = text;
            }
        }

        public void SetAction(
            PlayerAction action,
            Action<PlayerAction> callback)
        {
            this.action = action;
            this.callback = callback;
            this.choiceCallback = null;

            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(
                HandleClicked);
        }

        public void SetChoice(
            string text,
            int choiceIndex,
            Action<int> callback)
        {
            SetText(text);

            this.choiceIndex = choiceIndex;
            this.choiceCallback = callback;
            this.callback = null;

            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(
                HandleClicked);
        }

        private void HandleClicked()
        {
            if (choiceCallback != null)
            {
                choiceCallback(choiceIndex);
                return;
            }

            callback?.Invoke(action);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }
}

