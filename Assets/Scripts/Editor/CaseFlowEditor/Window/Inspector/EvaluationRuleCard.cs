using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Editor.CaseFlow.Service;

namespace Verdict.Editor.CaseFlow.Inspector
{
    public sealed class EvaluationRuleCard : VisualElement
    {
        private readonly EditorSession session;
        private readonly CaseEditService editService;

        private readonly StatementContext context;
        private readonly ClaimData claim;
        private readonly EvaluationRuleData rule;

        public EvaluationRuleCard(
            EditorSession session,
            CaseEditService editService,
            StatementContext context,
            ClaimData claim,
            EvaluationRuleData rule)
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

            DrawCondition();

            DrawSuccess();

            DrawFailure();
        }

        private void DrawHeader()
        {
            Toolbar toolbar = new();

            toolbar.Add(
                new Label("Evaluation Rule"));

            toolbar.Add(new ToolbarSpacer());

            toolbar.Add(
                new ToolbarButton(() =>
                {
                    editService.DeleteEvaluationRule(
                        claim,
                        rule);
                })
                {
                    text = "Delete"
                });

            Add(toolbar);
        }

        private void DrawCondition()
        {
            Foldout foldout = new()
            {
                text = "Condition",
                value = true
            };

            SerializedObject so =
                new SerializedObject(context.Case);

            SerializedProperty property =
                so.FindProperty(
                    context.EvaluationRulePropertyPath(
                        claim,
                        rule));

            if (property == null)
            {
                foldout.Add(
                    new Label("Evaluation Rule not found."));
                Add(foldout);
                return;
            }

            SerializedProperty typeProperty =
                property.FindPropertyRelative(
                    "evaluationType");

            SerializedProperty evidenceProperty =
                property.FindPropertyRelative(
                    "requiredEvidence");

            PropertyField typeField =
                new(typeProperty);

            PropertyField evidenceField =
                new(evidenceProperty);

            typeField.Bind(so);
            evidenceField.Bind(so);

            foldout.Add(typeField);
            foldout.Add(evidenceField);

            Add(foldout);
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
                new Button(() =>
                {
                    editService.CreateSuccessEffect(rule);
                })
                {
                    text = "+ Effect"
                });

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
                new Button(() =>
                {
                    editService.CreateFailureEffect(rule);
                })
                {
                    text = "+ Effect"
                });

            Add(foldout);
        }
    }
}
