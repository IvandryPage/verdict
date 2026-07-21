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
using Verdict.Editor.CaseEditor.Service;

namespace Verdict.Editor.CaseEditor.Inspector
{
    /// <summary>
    /// One CourtStateEffectData. The Inspector reshapes itself around the
    /// selected effect type - only the fields that effect actually uses
    /// are shown, and TargetType is derived automatically from the
    /// effect (the user never has to keep two dropdowns in sync).
    /// </summary>
    public sealed class EffectCard : VisualElement
    {
        private readonly EditorSession session;
        private readonly CaseEditService editService;

        private readonly StatementContext context;
        private readonly ClaimData claim;
        private readonly ArgumentRuleData rule;
        private readonly CourtStateEffectData effect;

        private readonly bool isSuccess;

        private VisualElement fieldsContainer;

        public EffectCard(
            EditorSession session,
            CaseEditService editService,
            StatementContext context,
            ClaimData claim,
            ArgumentRuleData rule,
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

            style.borderLeftWidth = 2;
            style.borderLeftColor = new Color(.35f, .35f, .35f);

            Build();
        }

        private void Build()
        {
            fieldsContainer = new VisualElement();

            Add(fieldsContainer);

            DrawFields();

            DrawButtons();
        }

        private string PropertyPath =>
            isSuccess
                ? context.SuccessEffectPropertyPath(claim, rule, effect)
                : context.FailureEffectPropertyPath(claim, rule, effect);

        private void DrawFields()
        {
            fieldsContainer.Clear();

            SerializedObject so = new(context.Case);

            SerializedProperty property = so.FindProperty(PropertyPath);

            if (property == null)
            {
                fieldsContainer.Add(new Label("Effect property not found."));
                return;
            }

            SerializedProperty effectProperty = property.FindPropertyRelative("effect");
            SerializedProperty targetTypeProperty = property.FindPropertyRelative("targetType");
            SerializedProperty targetIdProperty = property.FindPropertyRelative("targetId");
            SerializedProperty courtStatProperty = property.FindPropertyRelative("courtStat");
            SerializedProperty operationProperty = property.FindPropertyRelative("operation");
            SerializedProperty characterStatProperty = property.FindPropertyRelative("characterStat");
            SerializedProperty valueProperty = property.FindPropertyRelative("value");

            EnumField effectField = new("Effect", (CourtStateEffect)effectProperty.enumValueIndex);

            fieldsContainer.Add(effectField);

            effectField.RegisterValueChangedCallback(evt =>
            {
                CourtStateEffect newEffect = (CourtStateEffect)evt.newValue;

                effectProperty.enumValueIndex = (int)newEffect;

                EffectTargetType impliedTarget = GetImpliedTargetType(newEffect);

                if ((EffectTargetType)targetTypeProperty.enumValueIndex != impliedTarget)
                {
                    targetIdProperty.stringValue = string.Empty;
                }

                targetTypeProperty.enumValueIndex = (int)impliedTarget;

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(context.Case);

                DrawFields();
            });

            CourtStateEffect currentEffect = (CourtStateEffect)effectProperty.enumValueIndex;

            switch (currentEffect)
            {
                case CourtStateEffect.RevealStatement:
                case CourtStateEffect.JumpStatement:
                    DrawTargetDropdown(targetIdProperty, EffectTargetType.Statement, "Target Statement");
                    break;

                case CourtStateEffect.RevealTestimony:
                case CourtStateEffect.JumpTestimony:
                    DrawTargetDropdown(targetIdProperty, EffectTargetType.Testimony, "Target Testimony");
                    break;

                case CourtStateEffect.RevealWitness:
                case CourtStateEffect.JumpWitness:
                    DrawTargetDropdown(targetIdProperty, EffectTargetType.Witness, "Target Witness");
                    break;

                case CourtStateEffect.UnlockEvidence:
                    DrawTargetDropdown(targetIdProperty, EffectTargetType.Evidence, "Evidence to Unlock");
                    break;

                case CourtStateEffect.ModifyCourtStat:
                    DrawStatFields(so, courtStatProperty, operationProperty, valueProperty);
                    break;

                case CourtStateEffect.ModifyCharacterStat:
                    DrawTargetDropdown(targetIdProperty, EffectTargetType.Character, "Character");
                    DrawStatFields(so, characterStatProperty, operationProperty, valueProperty);
                    break;

                case CourtStateEffect.None:
                    fieldsContainer.Add(new Label("Pick an effect type above.")
                    {
                        style = { color = new Color(.6f, .6f, .6f) }
                    });
                    break;
            }
        }

        private static EffectTargetType GetImpliedTargetType(CourtStateEffect effect)
        {
            return effect switch
            {
                CourtStateEffect.RevealStatement or CourtStateEffect.JumpStatement => EffectTargetType.Statement,
                CourtStateEffect.RevealTestimony or CourtStateEffect.JumpTestimony => EffectTargetType.Testimony,
                CourtStateEffect.RevealWitness or CourtStateEffect.JumpWitness => EffectTargetType.Witness,
                CourtStateEffect.UnlockEvidence => EffectTargetType.Evidence,
                CourtStateEffect.ModifyCharacterStat => EffectTargetType.Character,
                _ => EffectTargetType.None
            };
        }

        private void DrawStatFields(
            SerializedObject so,
            SerializedProperty statProperty,
            SerializedProperty operationProperty,
            SerializedProperty valueProperty)
        {
            PropertyField statField = new(statProperty);
            PropertyField operationField = new(operationProperty);
            PropertyField valueField = new(valueProperty);

            statField.Bind(so);
            operationField.Bind(so);
            valueField.Bind(so);

            fieldsContainer.Add(statField);
            fieldsContainer.Add(operationField);
            fieldsContainer.Add(valueField);
        }

        private void DrawTargetDropdown(
            SerializedProperty targetIdProperty,
            EffectTargetType targetType,
            string label)
        {
            List<ReferenceEntry> options = GetTargetOptions(context.Case, targetType);

            VisualElement container = new();

            container.Add(new Label(label)
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 2 }
            });

            List<ReferenceEntry> withNone = new() { new ReferenceEntry(string.Empty, "<None>") };
            withNone.AddRange(options);

            if (options.Count == 0)
            {
                container.Add(new Label("No available targets of this type yet."));
            }

            int selectedIndex = Math.Max(
                0,
                withNone.FindIndex(entry => entry.Id == targetIdProperty.stringValue));

            PopupField<ReferenceEntry> popup = new(
                withNone,
                selectedIndex,
                entry => entry.Label,
                entry => entry.Label);

            popup.RegisterValueChangedCallback(evt =>
            {
                targetIdProperty.stringValue = evt.newValue?.Id ?? string.Empty;
                targetIdProperty.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(context.Case);
            });

            container.Add(popup);
            fieldsContainer.Add(container);
        }

        private static List<ReferenceEntry> GetTargetOptions(
            CaseData caseData,
            EffectTargetType targetType)
        {
            List<ReferenceEntry> options = new();

            if (caseData == null)
                return options;

            switch (targetType)
            {
                case EffectTargetType.Statement:
                    foreach (WitnessData witness in caseData.Witnesses)
                        foreach (TestimonyData testimony in witness.Testimonies)
                            foreach (StatementData statement in testimony.Statements)
                                options.Add(new ReferenceEntry(statement.Id, FormatStatementLabel(statement)));
                    break;

                case EffectTargetType.Testimony:
                    foreach (WitnessData witness in caseData.Witnesses)
                        foreach (TestimonyData testimony in witness.Testimonies)
                            options.Add(new ReferenceEntry(testimony.Id, FormatTestimonyLabel(testimony)));
                    break;

                case EffectTargetType.Witness:
                    foreach (WitnessData witness in caseData.Witnesses)
                        options.Add(new ReferenceEntry(witness.Id, FormatWitnessLabel(witness)));
                    break;

                case EffectTargetType.Evidence:
                    foreach (EvidenceEntryData evidence in caseData.Evidence)
                    {
                        if (evidence.Evidence == null)
                            continue;
                        options.Add(new ReferenceEntry(evidence.Evidence.Id, FormatEvidenceLabel(evidence.Evidence)));
                    }
                    break;

                case EffectTargetType.Character:
                    AddCharacterOption(caseData.Judge, options);
                    AddCharacterOption(caseData.Prosecutor, options);
                    AddCharacterOption(caseData.DefenseLawyer, options);
                    foreach (CharacterOverrideData overrideData in caseData.CharacterOverrides)
                        AddCharacterOption(overrideData.Character, options);

                    foreach (WitnessData witness in caseData.Witnesses)
                        AddCharacterOption(witness.Character, options);
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

            options.Add(new ReferenceEntry(character.Id, FormatCharacterLabel(character)));
        }

        private static string FormatStatementLabel(StatementData statement)
        {
            string preview = string.IsNullOrWhiteSpace(statement.Text)
                ? "<empty>"
                : Shorten(statement.Text, 35);

            return $"{ShortId(statement.Id)} — {preview}";
        }

        private static string FormatTestimonyLabel(TestimonyData testimony) =>
            $"{ShortId(testimony.Id)} — {testimony.Title}";

        private static string FormatWitnessLabel(WitnessData witness) =>
            $"{ShortId(witness.Id)} — {HierarchyDisplayUtility.GetWitnessName(witness)}";

        private static string FormatEvidenceLabel(EvidenceData evidence) =>
            $"{ShortId(evidence.Id)} — {evidence.name}";

        private static string FormatCharacterLabel(CharacterData character)
        {
            string name = string.IsNullOrWhiteSpace(character.DisplayName)
                ? character.Id
                : character.DisplayName;

            return $"{ShortId(character.Id)} — {name}";
        }

        private static string ShortId(string id) =>
            string.IsNullOrWhiteSpace(id) ? "<none>" : (id.Length <= 5 ? id : id[..5]);

        private static string Shorten(string text, int maxLength)
        {
            string clean = text.Replace("\n", " ");
            return clean.Length <= maxLength ? clean : clean[..maxLength] + "...";
        }

        private sealed class ReferenceEntry
        {
            public string Id { get; }
            public string Label { get; }

            public ReferenceEntry(string id, string label)
            {
                Id = id;
                Label = label;
            }
        }

        private void DrawButtons()
        {
            Button delete = new(() =>
            {
                if (isSuccess)
                    editService.DeleteSuccessEffect(rule, effect);
                else
                    editService.DeleteFailureEffect(rule, effect);
            })
            { text = "Delete Effect" };

            Add(delete);
        }
    }
}
