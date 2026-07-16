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

            style.marginBottom = 6;

            style.paddingLeft = 10;
            style.paddingRight = 10;
            style.paddingTop = 8;
            style.paddingBottom = 8;

            style.borderLeftWidth = 4;
            style.borderTopWidth = 1;
            style.borderBottomWidth = 1;
            style.borderRightWidth = 1;

            style.borderTopColor = new Color(.25f, .25f, .25f);
            style.borderBottomColor = new Color(.25f, .25f, .25f);
            style.borderRightColor = new Color(.25f, .25f, .25f);

            style.borderTopLeftRadius = 4;
            style.borderTopRightRadius = 4;
            style.borderBottomLeftRadius = 4;
            style.borderBottomRightRadius = 4;

            ApplyTheme(issue);

            //---------------------------------
            // Header
            //---------------------------------

            VisualElement header = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween
                }
            };

            Label severity = new(GetSeverityText(issue));

            severity.style.unityFontStyleAndWeight =
                FontStyle.Bold;

            severity.style.fontSize = 12;

            header.Add(severity);

            if (!string.IsNullOrWhiteSpace(issue.Scope.ToString()))
            {
                Label scope =
                    new(issue.Scope.ToString());

                scope.style.opacity = .6f;
                scope.style.fontSize = 11;

                header.Add(scope);
            }

            Add(header);

            //---------------------------------
            // Context
            //---------------------------------

            if (!string.IsNullOrWhiteSpace(issue.ContextId))
            {
                Label context =
                    new($"Statement: {issue.ContextId}");

                context.style.fontSize = 11;
                context.style.opacity = .65f;

                context.style.marginTop = 2;
                context.style.marginBottom = 4;

                Add(context);
            }

            //---------------------------------
            // Message
            //---------------------------------

            Label message =
                new(issue.Message);

            message.style.whiteSpace =
                WhiteSpace.Normal;

            message.style.marginTop = 2;

            Add(message);

            RegisterCallback<ClickEvent>(_ =>
            {
                Clicked?.Invoke(issue);
            });

            RegisterCallback<MouseEnterEvent>(_ =>
            {
                style.backgroundColor =
                    new Color(.24f, .24f, .24f);
            });

            RegisterCallback<MouseLeaveEvent>(_ =>
            {
                style.backgroundColor =
                    StyleKeyword.Null;
            });
        }

        private static string GetSeverityText(
            ValidationIssue issue)
        {
            return issue.Severity switch
            {
                ValidationSeverity.Error =>
                    "● Error",

                ValidationSeverity.Warning =>
                    "▲ Warning",

                ValidationSeverity.Info =>
                    "ℹ Info",

                _ =>
                    issue.Severity.ToString()
            };
        }

        private void ApplyTheme(
            ValidationIssue issue)
        {
            Color color =
                issue.Severity switch
                {
                    ValidationSeverity.Error =>
                        new Color(.9f, .3f, .3f),

                    ValidationSeverity.Warning =>
                        new Color(1f, .7f, .2f),

                    _ =>
                        new Color(.3f, .8f, 1f)
                };

            style.borderLeftColor = color;
        }
    }
}
