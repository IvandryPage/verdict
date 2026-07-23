using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Editor.CaseEditor.Service;

namespace Verdict.Editor.CaseEditor.Inspector
{
    public sealed class StatementCard : VisualElement
    {
        private readonly EditorSession session;
        private readonly CaseEditService editService;
        private readonly StatementContext context;

        public StatementCard(
            EditorSession session,
            CaseEditService editService,
            StatementContext context)
        {
            this.session = session;
            this.editService = editService;
            this.context = context;

            style.marginBottom = 12;
            style.paddingLeft = 8;
            style.paddingRight = 8;
            style.paddingTop = 8;
            style.paddingBottom = 8;

            Build();
        }

        private void Build()
        {
            Add(new Label("Statement")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 14
                }
            });

            DrawHeader();
            DrawProperties();
            DrawButtons();
        }

        private void DrawHeader()
        {
            Add(new Label($"Witness : {context.Witness.Id}"));
            Add(new Label($"Testimony : {context.Testimony.Title}"));
            Add(new Label($"Statement : {context.Statement.Id}"));

            Add(new VisualElement
            {
                style =
                {
                    height = 1,
                    marginTop = 6,
                    marginBottom = 6
                }
            });
        }

        private void DrawProperties()
        {
            SerializedObject so =
                new SerializedObject(context.Case);

            SerializedProperty statementProperty =
                so.FindProperty(
                    context.StatementPropertyPath);

            if (statementProperty == null)
            {
                Add(new Label("Statement property not found."));
                return;
            }

            SerializedProperty textProperty =
                statementProperty.FindPropertyRelative("text");

            SerializedProperty visibleProperty =
                statementProperty.FindPropertyRelative("initiallyVisible");

            SerializedProperty nextProperty =
                statementProperty.FindPropertyRelative("nextStatementId");

            PropertyField textField =
                new(textProperty);

            PropertyField visibleField =
                new(visibleProperty);

            VisualElement nextField =
                CreateStatementDropdown(
                    so,
                    nextProperty,
                    context.Case);

            textField.Bind(so);
            visibleField.Bind(so);

            Add(textField);
            Add(visibleField);
            Add(nextField);
        }

        private VisualElement CreateStatementDropdown(
            SerializedObject serializedObject,
            SerializedProperty statementProperty,
            CaseData caseData)
        {
            VisualElement container =
                new VisualElement();

            container.Add(new Label("Next Statement")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 2
                }
            });

            List<ReferenceEntry> options =
                new()
                {
                    new ReferenceEntry(
                        string.Empty,
                        "<None>")
                };

            foreach (WitnessData witness in caseData.Witnesses)
            {
                foreach (TestimonyData testimony in witness.Testimonies)
                {
                    foreach (StatementData statement in testimony.Statements)
                    {
                        string label =
                            string.IsNullOrWhiteSpace(
                                statement.Text)
                                ? statement.Id
                                : $"{statement.Id} — {GetPreviewText(statement.Text)}";

                        options.Add(
                            new ReferenceEntry(
                                statement.Id,
                                label));
                    }
                }
            }

            int selectedIndex =
                Math.Max(
                    0,
                    options.FindIndex(
                        option => option.Id ==
                            statementProperty.stringValue));

            PopupField<ReferenceEntry> popup =
                new PopupField<ReferenceEntry>(
                    options,
                    selectedIndex,
                    entry => entry.Label,
                    entry => entry.Label);

            popup.RegisterValueChangedCallback(
                evt =>
                {
                    statementProperty.stringValue =
                        evt.newValue?.Id ?? string.Empty;
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(context.Case);
                });

            container.Add(popup);
            return container;
        }

        private static string GetPreviewText(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            const int maxLength = 40;
            string oneLine =
                text.Replace('\n', ' ');

            return oneLine.Length <= maxLength
                ? oneLine
                : oneLine[..maxLength] + "...";
        }

        private sealed class ReferenceEntry
        {
            public string Id { get; }
            public string Label { get; }

            public ReferenceEntry(
                string id,
                string label)
            {
                Id = id;
                Label = label;
            }
        }

        private void DrawButtons()
        {
            Button add =
                new Button(() =>
                {
                    editService.CreateStatementAfter(context);
                })
                {
                    text = "Add Statement After"
                };

            Button delete =
                new Button(() =>
                {
                    editService.DeleteStatement(context);
                })
                {
                    text = "Delete Statement"
                };

            Add(add);
            Add(delete);
        }
    }
}
