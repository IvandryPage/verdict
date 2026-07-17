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
            style.minHeight = 40;

            Label title = new("Validation");

            title.style.unityFontStyleAndWeight =
                FontStyle.Bold;

            title.style.fontSize = 14;

            title.style.marginBottom = 6;

            Add(title);

            scroll = new ScrollView();

            scroll.style.flexGrow = 1;

            Add(scroll);
        }

        public void Show(
            ValidationResult result)
        {
            scroll.Clear();

            DrawGroup(
                "Errors",
                ValidationSeverity.Error,
                result);

            DrawGroup(
                "Warnings",
                ValidationSeverity.Warning,
                result);

            DrawGroup(
                "Info",
                ValidationSeverity.Info,
                result);

            if (result.Issues.Count == 0)
            {
                Label ok =
                    new("No validation issues.");

                ok.style.unityFontStyleAndWeight =
                    FontStyle.Bold;

                ok.style.unityTextAlign =
                    TextAnchor.MiddleCenter;

                ok.style.marginTop = 12;

                scroll.Add(ok);
            }
        }

        private void DrawGroup(
            string title,
            ValidationSeverity severity,
            ValidationResult result)
        {
            int count = 0;

            foreach (ValidationIssue issue in result.Issues)
            {
                if (issue.Severity == severity)
                    count++;
            }

            if (count == 0)
                return;

            Foldout foldout =
                new Foldout()
                {
                    text = $"{title} ({count})",
                    value = true
                };

            foreach (ValidationIssue issue in result.Issues)
            {
                if (issue.Severity != severity)
                    continue;

                ValidationItemView item =
                    new(issue);

                item.Clicked += HandleClicked;

                foldout.Add(item);
            }

            scroll.Add(foldout);
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
