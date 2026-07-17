using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Editor.CaseFlow.Service;

namespace Verdict.Editor.CaseFlow.Inspector
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

            PropertyField factField =
                new PropertyField(factProperty);

            PropertyField truthField =
                new PropertyField(truthProperty);

            factField.Bind(so);
            truthField.Bind(so);

            factField.RegisterValueChangeCallback(_ =>
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(context.Case);
            });

            truthField.RegisterValueChangeCallback(_ =>
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(context.Case);
            });

            Add(factField);
            Add(truthField);
        }

        private void DrawRules()
        {
            foreach (EvaluationRuleData rule in claim.EvaluationRules)
            {
                Add(
                    new EvaluationRuleCard(
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
                    editService.CreateEvaluationRule(claim);
                })
                {
                    text = "Add Evaluation Rule"
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
