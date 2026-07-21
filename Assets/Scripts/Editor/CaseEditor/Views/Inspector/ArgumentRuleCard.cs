using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Editor.CaseEditor.Authoring;
using Verdict.Editor.CaseEditor.Service;

namespace Verdict.Editor.CaseEditor.Inspector
{
    /// <summary>
    /// Action + Conditions (all must pass) + Success/Failure effects for
    /// a single ArgumentRule. Conditions render with Unity's default
    /// SerializeReference drawing - it already shows the right fields for
    /// whichever concrete condition type is stored at each slot.
    /// </summary>
    public sealed class ArgumentRuleCard : VisualElement
    {
        private readonly EditorSession session;
        private readonly CaseEditService editService;

        private readonly StatementContext context;
        private readonly ClaimData claim;
        private readonly ArgumentRuleData rule;

        private VisualElement conditionsContainer;

        public ArgumentRuleCard(
            EditorSession session,
            CaseEditService editService,
            StatementContext context,
            ClaimData claim,
            ArgumentRuleData rule)
        {
            this.session = session;
            this.editService = editService;

            this.context = context;
            this.claim = claim;
            this.rule = rule;

            style.marginTop = 8;
            style.paddingLeft = 8;
            style.paddingRight = 8;
            style.paddingTop = 6;
            style.paddingBottom = 6;

            style.borderTopWidth = 1;
            style.borderBottomWidth = 1;
            style.borderLeftWidth = 1;
            style.borderRightWidth = 1;

            DrawHeader();

            DrawAction();

            DrawConditions();

            DrawSuccess();

            DrawFailure();
        }

        private void DrawHeader()
        {
            Toolbar toolbar = new();

            toolbar.Add(
                new Label("Argument Rule"));

            toolbar.Add(new ToolbarSpacer());

            toolbar.Add(
                new ToolbarButton(() =>
                {
                    editService.DeleteArgumentRule(
                        claim,
                        rule);
                })
                {
                    text = "Delete"
                });

            Add(toolbar);
        }

        private void DrawAction()
        {
            SerializedObject so =
                new SerializedObject(context.Case);

            SerializedProperty ruleProperty =
                so.FindProperty(
                    context.ArgumentRulePropertyPath(
                        claim,
                        rule));

            if (ruleProperty == null)
            {
                Add(new Label("Argument Rule not found."));
                return;
            }

            SerializedProperty actionProperty =
                ruleProperty.FindPropertyRelative("action");

            PropertyField actionField = new(actionProperty, "Action");

            actionField.Bind(so);

            Add(actionField);
        }

        private void DrawConditions()
        {
            Foldout foldout = new()
            {
                text = "Conditions (all must pass)",
                value = true
            };

            conditionsContainer = new VisualElement();
            foldout.Add(conditionsContainer);

            RefreshConditions();

            foldout.Add(BuildAddConditionMenu());

            Add(foldout);
        }

        private void RefreshConditions()
        {
            conditionsContainer.Clear();

            SerializedObject so =
                new SerializedObject(context.Case);

            SerializedProperty ruleProperty =
                so.FindProperty(
                    context.ArgumentRulePropertyPath(
                        claim,
                        rule));

            if (ruleProperty == null)
            {
                return;
            }

            SerializedProperty conditionsProperty =
                ruleProperty.FindPropertyRelative("conditions");

            for (int i = 0; i < rule.Conditions.Count; i++)
            {
                ArgumentConditionData condition = rule.Conditions[i];

                VisualElement row = new()
                {
                    style = { flexDirection = FlexDirection.Row, marginBottom = 4 }
                };

                SerializedProperty elementProperty =
                    conditionsProperty.GetArrayElementAtIndex(i);

                PropertyField conditionField = new(elementProperty)
                {
                    style = { flexGrow = 1 }
                };

                conditionField.Bind(so);

                Button removeButton = new(() =>
                {
                    editService.RemoveCondition(rule, condition);
                    RefreshConditions();
                })
                { text = "-" };

                row.Add(conditionField);
                row.Add(removeButton);

                conditionsContainer.Add(row);
            }
        }

        private VisualElement BuildAddConditionMenu()
        {
            Button addButton = new() { text = "+ Add Condition" };

            addButton.clicked += () =>
            {
                GenericMenu menu = new();

                menu.AddItem(new GUIContent("Action"), false, () => AddCondition(AuthoringFactory.CreateActionCondition));
                menu.AddItem(new GUIContent("Evidence"), false, () => AddCondition(AuthoringFactory.CreateEvidenceCondition));
                menu.AddItem(new GUIContent("Fact"), false, () => AddCondition(AuthoringFactory.CreateFactCondition));
                menu.AddItem(new GUIContent("Court State"), false, () => AddCondition(AuthoringFactory.CreateCourtStateCondition));
                menu.AddItem(new GUIContent("Character"), false, () => AddCondition(AuthoringFactory.CreateCharacterCondition));
                menu.AddItem(new GUIContent("Claim"), false, () => AddCondition(AuthoringFactory.CreateClaimCondition));
                menu.AddItem(new GUIContent("Argument"), false, () => AddCondition(AuthoringFactory.CreateArgumentContextCondition));

                menu.ShowAsContext();
            };

            return addButton;
        }

        private void AddCondition(Func<ArgumentConditionData> factory)
        {
            editService.AddCondition(rule, factory);
            RefreshConditions();
        }

        private void DrawSuccess()
        {
            Foldout foldout = new()
            {
                text = "On Success",
                value = true
            };

            foreach (CourtStateEffectData effect in rule.SuccessEffects)
            {
                foldout.Add(
                    new EffectCard(
                        session,
                        editService,
                        context,
                        claim,
                        rule,
                        effect,
                        true));
            }

            foldout.Add(
                BuildAddEffectButton(type => editService.CreateSuccessEffect(rule, type)));

            Add(foldout);
        }

        private void DrawFailure()
        {
            Foldout foldout = new()
            {
                text = "On Failure",
                value = false
            };

            foreach (CourtStateEffectData effect in rule.FailureEffects)
            {
                foldout.Add(
                    new EffectCard(
                        session,
                        editService,
                        context,
                        claim,
                        rule,
                        effect,
                        false));
            }

            foldout.Add(
                BuildAddEffectButton(type => editService.CreateFailureEffect(rule, type)));

            Add(foldout);
        }

        private static Button BuildAddEffectButton(Action<CourtStateEffect> create)
        {
            Button addButton = new() { text = "+ Effect" };

            addButton.clicked += () =>
            {
                GenericMenu menu = new();

                menu.AddItem(new GUIContent("Reveal Statement"), false, () => create(CourtStateEffect.RevealStatement));
                menu.AddItem(new GUIContent("Reveal Testimony"), false, () => create(CourtStateEffect.RevealTestimony));
                menu.AddItem(new GUIContent("Reveal Witness"), false, () => create(CourtStateEffect.RevealWitness));
                menu.AddItem(new GUIContent("Unlock Evidence"), false, () => create(CourtStateEffect.UnlockEvidence));
                menu.AddItem(new GUIContent("Modify Court Stat"), false, () => create(CourtStateEffect.ModifyCourtStat));
                menu.AddItem(new GUIContent("Modify Character Stat"), false, () => create(CourtStateEffect.ModifyCharacterStat));
                menu.AddItem(new GUIContent("Jump Statement"), false, () => create(CourtStateEffect.JumpStatement));
                menu.AddItem(new GUIContent("Jump Testimony"), false, () => create(CourtStateEffect.JumpTestimony));
                menu.AddItem(new GUIContent("Jump Witness"), false, () => create(CourtStateEffect.JumpWitness));

                menu.ShowAsContext();
            };

            return addButton;
        }
    }
}
