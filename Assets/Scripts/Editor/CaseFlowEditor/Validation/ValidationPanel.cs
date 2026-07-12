using System;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Systems.Validation;

namespace Verdict.Editor.CaseFlow.Validation
{
    public sealed class ValidationPanel
        : VisualElement
    {
        private readonly ScrollView scroll;

        public event Action<ValidationIssue> IssueSelected;

        public ValidationPanel()
        {
            style.flexGrow = 1;
            style.minHeight = 100;
            scroll = new ScrollView();
            scroll.style.flexGrow = 1;

            Add(new Label("validation Panel"));
            Add(scroll);
        }

        public void Show(ValidationResult result)
        {
            scroll.Clear();

            foreach (ValidationIssue issue in result.Issues)
            {
                ValidationItemView item = new(issue);

                item.Clicked += HandleClicked;

                scroll.Add(item);
            }
        }

        public void ClearIssues()
        {
            scroll.Clear();
        }

        private void HandleClicked(
            ValidationIssue issue)
        {
            IssueSelected?.Invoke(issue);
        }
    }
}
