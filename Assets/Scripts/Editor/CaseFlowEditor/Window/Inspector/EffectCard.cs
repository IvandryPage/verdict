using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
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

            PropertyField targetTypeField =
                new(property.FindPropertyRelative("targetType"));

            PropertyField targetIdField =
                new(property.FindPropertyRelative("targetId"));

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
            targetIdField.Bind(so);
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
