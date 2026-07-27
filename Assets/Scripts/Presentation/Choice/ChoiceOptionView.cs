using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Verdict.Presentation.Choice
{
    /// <summary>
    /// One selectable option inside the Choice panel. Put this on a
    /// prefab with a Button and a TMP_Text child.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class ChoiceOptionView : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        private Button button;
        private int index;

        public event Action<int> Clicked;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleClick);
        }

        public void Bind(int optionIndex, string text)
        {
            index = optionIndex;

            if (label != null)
            {
                label.text = text;
            }
        }

        private void HandleClick()
        {
            Clicked?.Invoke(index);
        }
    }
}
