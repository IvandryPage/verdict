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
    public sealed class ClaimCard : VisualElement
    {
        private readonly EditorSession session;
        private readonly CaseEditService editService;
        private readonly StatementContext context;
        private readonly ClaimData claim;

        public ClaimCard(
            EditorSession session,
            CaseEditService editService,
            StatementContext context,
            ClaimData claim)
        {
            this.session = session;
            this.editService = editService;
            this.context = context;
            this.claim = claim;

            style.marginTop = 10;
            style.marginBottom = 10;

            style.paddingLeft = 8;
            style.paddingRight = 8;
            style.paddingTop = 8;
            style.paddingBottom = 8;

            Build();
        }

        private void Build()
        {
            Add(new Label("Claim")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 13
                }
            });

            DrawProperty();

            DrawRules();

            DrawButtons();
        }

        private void DrawProperty()
        {
            SerializedObject so =
                new SerializedObject(context.Case);

            SerializedProperty claimProperty =
                so.FindProperty(context.ClaimPropertyPath(claim));

            if (claimProperty == null)
            {
                Add(new Label("Claim property not found."));
                return;
            }

            SerializedProperty factProperty =
                claimProperty.FindPropertyRelative(
                    "factId");

            SerializedProperty truthProperty =
                claimProperty.FindPropertyRelative(
                    "isTrue");

            VisualElement factField =
                CreateFactDropdown(
                    so,
                    factProperty,
                    context.Case);

            PropertyField truthField =
                new PropertyField(truthProperty);

            truthField.Bind(so);

            truthField.RegisterValueChangeCallback(_ =>
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(context.Case);
            });

            Add(factField);
            Add(truthField);
        }

        private VisualElement CreateFactDropdown(
            SerializedObject serializedObject,
            SerializedProperty factProperty,
            CaseData caseData)
        {
            VisualElement container =
                new VisualElement();

            container.Add(new Label("Fact")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 2
                }
            });

            IReadOnlyList<FactData> facts =
                caseData?.Truth?.Facts ??
                Array.Empty<FactData>();

            List<ReferenceEntry> options =
                facts.Select(f =>
                    new ReferenceEntry(
                        f.Id,
                        GetFactLabel(f)))
                .ToList();

            options.Insert(
                0,
                new ReferenceEntry(
                    string.Empty,
                    "<None>"));

            int selectedIndex =
                Math.Max(
                    0,
                    options.FindIndex(
                        option => option.Id ==
                            factProperty.stringValue));

            PopupField<ReferenceEntry> popup =
                new PopupField<ReferenceEntry>(
                    options,
                    selectedIndex,
                    entry => entry.Label,
                    entry => entry.Label);

            popup.RegisterValueChangedCallback(
                evt =>
                {
                    factProperty.stringValue =
                        evt.newValue?.Id ?? string.Empty;
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(context.Case);
                });

            container.Add(popup);
            return container;
        }

        private static string GetFactLabel(
            FactData fact)
        {
            if (fact == null)
                return string.Empty;

            string subject = fact.Subject?.ToString() ?? "?";
            string predicate = fact.Predicate.ToString();
            string @object = fact.Object?.ToString() ?? "?";

            return string.IsNullOrWhiteSpace(fact.Id)
                ? $"{subject} {predicate} {@object}"
                : $"{fact.Id} — {subject} {predicate} {@object}";
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

        private void DrawRules()
        {
            foreach (ArgumentRuleData rule in claim.ArgumentRules)
            {
                Add(
                    new ArgumentRuleCard(
                        session,
                        editService,
                        context,
                        claim,
                        rule));
            }
        }

        private void DrawButtons()
        {
            Button addRule =
                new Button(() =>
                {
                    editService.CreateArgumentRule(claim);
                })
                {
                    text = "Add Argument Rule"
                };

            Button delete =
                new Button(() =>
                {
                    editService.DeleteClaim(
                        context,
                        claim);
                })
                {
                    text = "Delete Claim"
                };

            Add(addRule);
            Add(delete);
        }
    }
}
