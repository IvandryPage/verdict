using System;
using UnityEditor;
using Verdict.Data.Cases;
using Verdict.Editor.CaseFlow.Authoring;

namespace Verdict.Editor.CaseFlow.Service
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

        private static T Execute<T>(
                    string operation,
                    Func<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                return action();
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Failed to execute '{operation}'.",
                    exception);
            }
        }

        private static void Execute(
            string operation,
            Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                action();
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

            return Execute(
                "Create Statement",
                () =>
                {
                    StatementData statement =
                        AuthoringFactory.CreateStatement();

                    if (index < 0)
                    {
                        testimony.AddStatement(statement);
                    }
                    else
                    {
                        testimony.InsertStatement(index, statement);
                    }

                    NotifyModified();

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

            return Execute(
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

    }
}
