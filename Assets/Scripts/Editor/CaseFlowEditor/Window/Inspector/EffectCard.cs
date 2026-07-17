using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Data.Characters;
using Verdict.Data.Evidence;
using Verdict.Editor.CaseFlow.Service;

namespace Verdict.Editor.CaseFlow.Inspector
{
    public sealed class EffectCard : VisualElement
    {
        private readonly EditorSession session;
        private readonly CaseEditService editService;

        private readonly StatementContext context;
        private readonly ClaimData claim;
        private readonly EvaluationRuleData rule;
        private readonly CourtStateEffectData effect;

        private readonly bool isSuccess;

        public EffectCard(
            EditorSession session,
            CaseEditService editService,
            StatementContext context,
            ClaimData claim,
            EvaluationRuleData rule,
            CourtStateEffectData effect,
            bool isSuccess)
        {
            this.session = session;
            this.editService = editService;

            this.context = context;
            this.claim = claim;
            this.rule = rule;
            this.effect = effect;

            this.isSuccess = isSuccess;

            style.marginTop = 6;
            style.marginBottom = 6;
            style.paddingLeft = 6;
            style.paddingRight = 6;
            style.paddingTop = 6;
            style.paddingBottom = 6;

            Build();
        }

        private void Build()
        {
            DrawProperty();

            DrawButtons();
        }

        private void DrawProperty()
        {
            SerializedObject so =
                new SerializedObject(context.Case);

            string path =
                isSuccess
                    ? context.SuccessEffectPropertyPath(
                        claim,
                        rule,
                        effect)
                    : context.FailureEffectPropertyPath(
                        claim,
                        rule,
                        effect);

            SerializedProperty property =
                so.FindProperty(path);

            if (property == null)
            {
                Add(new Label("Effect property not found."));
                return;
            }

            PropertyField effectField =
                new(property.FindPropertyRelative("effect"));

            SerializedProperty targetTypeProperty =
                property.FindPropertyRelative("targetType");

            SerializedProperty targetIdProperty =
                property.FindPropertyRelative("targetId");

            PropertyField targetTypeField =
                new(targetTypeProperty);

            VisualElement targetIdField =
                CreateTargetIdDropdown(
                    so,
                    targetTypeProperty,
                    targetIdProperty,
                    context.Case);

            PropertyField courtStatField =
                new(property.FindPropertyRelative("courtStat"));

            PropertyField operationField =
                new(property.FindPropertyRelative("operation"));

            PropertyField characterStatField =
                new(property.FindPropertyRelative("characterStat"));

            PropertyField valueField =
                new(property.FindPropertyRelative("value"));

            effectField.Bind(so);
            targetTypeField.Bind(so);
            UpdateTargetDropdown(
                targetTypeProperty,
                targetIdProperty,
                targetIdField,
                context.Case);

            targetTypeField.RegisterValueChangeCallback(
                _ =>
                {
                    so.ApplyModifiedProperties();
                    UpdateTargetDropdown(
                        targetTypeProperty,
                        targetIdProperty,
                        targetIdField,
                        context.Case);
                });
            courtStatField.Bind(so);
            operationField.Bind(so);
            characterStatField.Bind(so);
            valueField.Bind(so);

            Add(effectField);
            Add(targetTypeField);
            Add(targetIdField);
            Add(courtStatField);
            Add(operationField);
            Add(characterStatField);
            Add(valueField);
        }

        private VisualElement CreateTargetIdDropdown(
            SerializedObject serializedObject,
            SerializedProperty targetTypeProperty,
            SerializedProperty targetIdProperty,
            CaseData caseData)
        {
            VisualElement container =
                new VisualElement();

            container.Add(new Label("Target")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 2
                }
            });

            container.Add(new Label("Loading targets...")
            {
                name = "targetDropdownPlaceholder"
            });

            return container;
        }

        private void UpdateTargetDropdown(
            SerializedProperty targetTypeProperty,
            SerializedProperty targetIdProperty,
            VisualElement container,
            CaseData caseData)
        {
            container.Clear();
            container.Add(new Label("Target")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 2
                }
            });

            EffectTargetType targetType =
                (EffectTargetType)targetTypeProperty.enumValueIndex;

            if (targetType == EffectTargetType.None)
            {
                container.Add(new Label("No target required for this effect."));
                targetIdProperty.stringValue = string.Empty;
                targetIdProperty.serializedObject.ApplyModifiedProperties();
                return;
            }

            List<ReferenceEntry> options =
                GetTargetOptions(caseData, targetType);

            if (options.Count == 0)
            {
                container.Add(new Label("No available targets for the selected target type."));
                return;
            }

            int selectedIndex =
                Math.Max(
                    0,
                    options.FindIndex(
                        entry => entry.Id == targetIdProperty.stringValue));

            PopupField<ReferenceEntry> popup =
                new PopupField<ReferenceEntry>(
                    options,
                    selectedIndex,
                    entry => entry.Label,
                    entry => entry.Label);

            popup.RegisterValueChangedCallback(
                evt =>
                {
                    targetIdProperty.stringValue =
                        evt.newValue?.Id ?? string.Empty;
                    targetIdProperty.serializedObject.ApplyModifiedProperties();
                });

            container.Add(popup);
        }

        private static List<ReferenceEntry> GetTargetOptions(
            CaseData caseData,
            EffectTargetType targetType)
        {
            List<ReferenceEntry> options =
                new();

            if (caseData == null)
                return options;

            switch (targetType)
            {
                case EffectTargetType.Statement:
                    foreach (WitnessData witness in caseData.Witnesses)
                    {
                        foreach (TestimonyData testimony in witness.Testimonies)
                        {
                            foreach (StatementData statement in testimony.Statements)
                            {
                                options.Add(new ReferenceEntry(
                                    statement.Id,
                                    FormatStatementLabel(statement)));
                            }
                        }
                    }
                    break;
                case EffectTargetType.Testimony:
                    foreach (WitnessData witness in caseData.Witnesses)
                    {
                        foreach (TestimonyData testimony in witness.Testimonies)
                        {
                            options.Add(new ReferenceEntry(
                                testimony.Id,
                                FormatTestimonyLabel(testimony)));
                        }
                    }
                    break;
                case EffectTargetType.Witness:
                    foreach (WitnessData witness in caseData.Witnesses)
                    {
                        options.Add(new ReferenceEntry(
                            witness.Id,
                            FormatWitnessLabel(witness)));
                    }
                    break;
                case EffectTargetType.Evidence:
                    foreach (EvidenceEntryData evidence in caseData.Evidence)
                    {
                        if (evidence.Evidence == null)
                            continue;
                        options.Add(new ReferenceEntry(
                            evidence.Evidence.Id,
                            FormatEvidenceLabel(evidence.Evidence)));
                    }
                    break;
                case EffectTargetType.Character:
                    AddCharacterOption(caseData.Judge, options);
                    AddCharacterOption(caseData.Prosecutor, options);
                    AddCharacterOption(caseData.DefenseLawyer, options);
                    foreach (CharacterOverrideData overrideData in caseData.CharacterOverrides)
                    {
                        AddCharacterOption(overrideData.Character, options);
                    }
                    break;
            }

            return options;
        }

        private static void AddCharacterOption(
            CharacterData character,
            List<ReferenceEntry> options)
        {
            if (character == null)
                return;

            if (options.Any(option => option.Id == character.Id))
                return;

            options.Add(new ReferenceEntry(
                character.Id,
                FormatCharacterLabel(character)));
        }

        private static string FormatStatementLabel(
            StatementData statement)
        {
            string preview =
                string.IsNullOrWhiteSpace(statement.Text)
                    ? "<empty>"
                    : Shorten(statement.Text, 35);

            return $"{ShortId(statement.Id)} — {preview}";
        }

        private static string FormatTestimonyLabel(
            TestimonyData testimony)
        {
            return $"{ShortId(testimony.Id)} — {testimony.Title}";
        }

        private static string FormatWitnessLabel(
            WitnessData witness)
        {
            string name =
                HierarchyDisplayUtility.GetWitnessName(witness);
            return $"{ShortId(witness.Id)} — {name}";
        }

        private static string FormatEvidenceLabel(
            EvidenceData evidence)
        {
            return $"{ShortId(evidence.Id)} — {evidence.name}";
        }

        private static string FormatCharacterLabel(
            CharacterData character)
        {
            string name =
                string.IsNullOrWhiteSpace(character.DisplayName)
                    ? character.Id
                    : character.DisplayName;
            return $"{ShortId(character.Id)} — {name}";
        }

        private static string ShortId(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return "<none>";

            return id.Length <= 5 ? id : id[..5];
        }

        private static string Shorten(
            string text,
            int maxLength)
        {
            string clean = text.Replace("\n", " ");
            return clean.Length <= maxLength
                ? clean
                : clean[..maxLength] + "...";
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
            Button delete =
                new Button(() =>
                {
                    if (isSuccess)
                    {
                        editService.DeleteSuccessEffect(
                            rule,
                            effect);
                    }
                    else
                    {
                        editService.DeleteFailureEffect(
                            rule,
                            effect);
                    }
                })
                {
                    text = "Delete Effect"
                };

            Add(delete);
        }
    }
}
