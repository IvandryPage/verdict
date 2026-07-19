using System;
using UnityEditor;
using UnityEngine;
using Verdict.Data.Cases;
using Verdict.Editor.CaseEditor.Authoring;
using Verdict.Systems.Validation.Graph;

namespace Verdict.Editor.CaseEditor.Service
{
    public sealed class CaseEditService
    {
        private readonly EditorSession session;

        public event Action CaseModified;

        public CaseEditService(EditorSession session)
        {
            this.session = session;
        }

        private CaseData CurrentCase =>
            session.CurrentCase;

        private void EnsureCaseLoaded()
        {
            if (CurrentCase == null)
                throw new InvalidOperationException(
                    "No CaseData is currently loaded.");
        }

        private void MarkDirty()
        {
            EditorUtility.SetDirty(CurrentCase);
        }

        private void NotifyModified()
        {
            MarkDirty();

            CaseModified?.Invoke();
        }

        private T ExecuteMutation<T>(
            string operation,
            Func<T> action)
        {
            EnsureCaseLoaded();

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                Undo.RecordObject(CurrentCase, operation);

                T result = action();

                EditorUtility.SetDirty(CurrentCase);

                CaseModified?.Invoke();

                return result;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Failed to execute '{operation}'.",
                    exception);
            }
        }

        private void ExecuteMutation(
            string operation,
            Action action)
        {
            EnsureCaseLoaded();

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                Undo.RecordObject(CurrentCase, operation);

                action();

                EditorUtility.SetDirty(CurrentCase);

                CaseModified?.Invoke();
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Failed to execute '{operation}'.",
                    exception);
            }
        }

        public StatementData CreateStatementAfter(
            StatementContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return CreateStatement(
                context.Testimony,
                context.StatementIndex + 1);
        }

        public StatementData CreateStatement(
            TestimonyData testimony,
            int index = -1)
        {
            if (testimony == null)
                throw new ArgumentNullException(nameof(testimony));

            return ExecuteMutation(
                "Create Statement",
                () =>
                {
                    StatementData statement =
                        AuthoringFactory.CreateStatement();

                    if (index < 0)
                        testimony.AddStatement(statement);
                    else
                        testimony.InsertStatement(index, statement);

                    return statement;
                });
        }

        public bool DeleteStatement(
            TestimonyData testimony,
            StatementData statement)
        {
            if (testimony == null)
                throw new ArgumentNullException(nameof(testimony));

            if (statement == null)
                throw new ArgumentNullException(nameof(statement));

            return ExecuteMutation(
                "Delete Statement",
                () => testimony.RemoveStatement(statement));
        }

        public bool DeleteStatement(
            StatementContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return DeleteStatement(
                context.Testimony,
                context.Statement);
        }

        public WitnessData CreateWitness()
        {
            return ExecuteMutation(
                "Create Witness",
                () =>
                {
                    WitnessData witness =
                        AuthoringFactory.CreateWitness();

                    session.CurrentCase.AddWitness(
                        witness);

                    return witness;
                });
        }

        public bool DeleteWitness(
            WitnessData witness)
        {
            if (witness == null)
                throw new ArgumentNullException(nameof(witness));


            return ExecuteMutation(
                "Delete Witness",
                () =>
                {
                    return session.CurrentCase.RemoveWitness(
                        witness);
                });
        }

        public TestimonyData CreateTestimony(
            WitnessData witness)
        {
            if (witness == null)
                throw new ArgumentNullException(nameof(witness));

            return ExecuteMutation(
                "Create Testimony",
                () =>
                {
                    TestimonyData testimony =
                        AuthoringFactory.CreateTestimony();

                    witness.AddTestimony(testimony);

                    return testimony;
                });
        }

        public bool DeleteTestimony(
            WitnessData witness,
            TestimonyData testimony)
        {
            if (witness == null)
                throw new ArgumentNullException(nameof(witness));

            if (testimony == null)
                throw new ArgumentNullException(nameof(testimony));

            return ExecuteMutation(
                "Delete Testimony",
                () => witness.RemoveTestimony(testimony));
        }

        public void ConnectStatements(
            StatementContext from,
            StatementContext to)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            if (to == null)
                throw new ArgumentNullException(nameof(to));

            ExecuteMutation(
                "Connect Statement",
                () =>
                {
                    from.Statement.SetNextStatement(to.Statement.Id);
                });
        }

        public void DisconnectStatements(
            StatementContext from,
            StatementContext to)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            if (to == null)
                throw new ArgumentNullException(nameof(to));

            ExecuteMutation(
                "Disconnect Statement",
                () =>
                {
                    if (from.Statement.NextStatementId == to.Statement.Id)
                    {
                        from.Statement.SetNextStatement(null);
                    }
                });
        }

        public ClaimData CreateClaim(
            StatementContext context)
        {

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return ExecuteMutation(
                "Create Claim",
                () =>
                {
                    ClaimData claim =
                        AuthoringFactory.CreateClaim();

                    context.Statement.AddClaim(claim);

                    return claim;
                });
        }

        public bool DeleteClaim(
            StatementContext context,
            ClaimData claim)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            return ExecuteMutation(
                "Delete Claim",
                () =>
                {
                    return context.Statement.RemoveClaim(claim);
                });
        }

        public ArgumentRuleData CreateArgumentRule(
            ClaimData claim)
        {
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            return ExecuteMutation(
                "Create Argument Rule",
                () =>
                {
                    ArgumentRuleData rule =
                        AuthoringFactory.CreateArgumentRule();

                    claim.AddArgumentRule(rule);

                    return rule;
                });
        }

        public bool DeleteArgumentRule(
            ClaimData claim,
            ArgumentRuleData rule)
        {
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            return ExecuteMutation(
                "Delete Argument Rule",
                () =>
                {
                    return claim.RemoveArgumentRule(rule);
                });
        }

        public ArgumentConditionData AddCondition(
            ArgumentRuleData rule,
            Func<ArgumentConditionData> factory)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            return ExecuteMutation(
                "Add Condition",
                () =>
                {
                    ArgumentConditionData condition = factory();

                    rule.AddCondition(condition);

                    return condition;
                });
        }

        public bool RemoveCondition(
            ArgumentRuleData rule,
            ArgumentConditionData condition)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            return ExecuteMutation(
                "Remove Condition",
                () =>
                {
                    return rule.RemoveCondition(condition);
                });
        }

        public CourtStateEffectData CreateSuccessEffect(
            ArgumentRuleData rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            return ExecuteMutation(
                "Create Success Effect",
                () =>
                {
                    CourtStateEffectData effect =
                        AuthoringFactory.CreateEffect();

                    rule.AddSuccessEffect(effect);

                    return effect;
                });
        }

        public bool DeleteSuccessEffect(
            ArgumentRuleData rule,
            CourtStateEffectData effect)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            return ExecuteMutation(
                "Delete Success Effect",
                () =>
                {
                    return rule.RemoveSuccessEffect(effect);
                });
        }

        public CourtStateEffectData CreateFailureEffect(
            ArgumentRuleData rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            return ExecuteMutation(
                "Create Failure Effect",
                () =>
                {
                    CourtStateEffectData effect =
                        AuthoringFactory.CreateEffect();

                    rule.AddFailureEffect(effect);

                    return effect;
                });
        }

        public bool DeleteFailureEffect(
            ArgumentRuleData rule,
            CourtStateEffectData effect)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            return ExecuteMutation(
                "Delete Failure Effect",
                () =>
                {
                    return rule.RemoveFailureEffect(effect);
                });
        }
    }
}
