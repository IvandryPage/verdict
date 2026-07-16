using System;
using Verdict.Data.Cases;

namespace Verdict.Editor.CaseFlow
{
    /// <summary>
    /// Represents the current authoring context inside the Case Flow Editor.
    /// </summary>
    public sealed class EditorSelection
    {
        public event Action SelectionChanged;

        public CaseData Case { get; private set; }

        public StatementContext StatementContext { get; private set; }

        public bool HasCase => Case != null;

        public bool HasWitness => Witness != null;

        public bool HasTestimony => Testimony != null;

        public bool HasStatement =>
            StatementContext != null;

        public WitnessData Witness =>
            StatementContext?.Witness;

        public TestimonyData Testimony =>
            StatementContext?.Testimony;

        public StatementData Statement =>
            StatementContext?.Statement;


        public void SelectCase(CaseData caseData)
        {
            if (ReferenceEquals(Case, caseData) &&
                StatementContext == null)
            {
                return;
            }

            Case = caseData;
            StatementContext = null;

            NotifySelectionChanged();
        }

        public void SelectStatement(
            StatementContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (ReferenceEquals(StatementContext, context))
                return;

            Case = context.Case;
            StatementContext = context;

            NotifySelectionChanged();
        }

        public void Clear()
        {
            if (Case == null &&
                StatementContext == null)
            {
                return;
            }

            Case = null;
            StatementContext = null;

            NotifySelectionChanged();
        }

        private void NotifySelectionChanged()
        {
            SelectionChanged?.Invoke();
        }
    }
}
