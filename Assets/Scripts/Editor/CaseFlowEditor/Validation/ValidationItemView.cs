using System;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Systems.Validation;

namespace Verdict.Editor.CaseFlow.Validation
{
    public sealed class ValidationItemView : VisualElement
    {
        public ValidationIssue Issue { get; }

        public event Action<ValidationIssue> Clicked;

        public ValidationItemView(
            ValidationIssue issue)
        {
            Issue = issue;

            style.marginBottom = 4;

            style.paddingLeft = 8;
            style.paddingRight = 8;
            style.paddingTop = 6;
            style.paddingBottom = 6;

            style.borderLeftWidth = 4;

            style.borderTopWidth = 1;
            style.borderBottomWidth = 1;
            style.borderRightWidth = 1;

            style.unityTextAlign =
                TextAnchor.MiddleLeft;

            Label severity =
                new(GetSeverityText(issue));

            severity.style.unityFontStyleAndWeight =
                FontStyle.Bold;

            Add(severity);

            if (!string.IsNullOrWhiteSpace(issue.ContextId))
            {
                Label context =
                    new($"Statement : {issue.ContextId}");

                context.style.color =
                    new Color(.7f, .7f, .7f);

                context.style.fontSize = 11;

                Add(context);
            }

            Label message = new(
                issue.ContextId == null
                    ? $"[{issue.Scope}] {issue.Message}"
                    : $"[Statement {issue.ContextId}] {issue.Message}");

            Add(message);

            ApplyTheme(issue);

            RegisterCallback<ClickEvent>(_ =>
            {
                Clicked?.Invoke(issue);
            });

            RegisterCallback<MouseEnterEvent>(_ =>
            {
                style.backgroundColor =
                    new Color(.25f, .25f, .25f);
            });

            RegisterCallback<MouseLeaveEvent>(_ =>
            {
                style.backgroundColor =
                    StyleKeyword.Null;
            });
        }

        private string GetSeverityText(ValidationIssue issue)
        {
            return issue.Severity switch
            {
                ValidationSeverity.Error => "[Error]",
                ValidationSeverity.Warning => "[Warning]",
                ValidationSeverity.Info => "[Info]",
                _ => issue.Severity.ToString()
            };
        }
        private void ApplyTheme(
            ValidationIssue issue)
        {
            switch (issue.Severity)
            {
                case ValidationSeverity.Error:

                    style.borderLeftColor =
                        UnityEngine.Color.red;

                    break;

                case ValidationSeverity.Warning:

                    style.borderLeftColor =
                        new UnityEngine.Color(
                            1f,
                            .65f,
                            0f);

                    break;

                default:

                    style.borderLeftColor =
                        UnityEngine.Color.cyan;

                    break;
            }
        }
    }
}
