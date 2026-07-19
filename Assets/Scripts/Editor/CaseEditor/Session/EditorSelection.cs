using System;
using UnityEngine;
using Verdict.Data.Cases;

namespace Verdict.Editor.CaseEditor
{
    public sealed class EditorSelection
    {
        public event Action SelectionChanged;


        public CaseData Case { get; private set; }


        public WitnessContext WitnessContext { get; private set; }

        public TestimonyContext TestimonyContext { get; private set; }

        public StatementContext StatementContext { get; private set; }



        public bool HasCase =>
            Case != null;


        public bool HasWitness =>
            WitnessContext != null;


        public bool HasTestimony =>
            TestimonyContext != null;


        public bool HasStatement =>
            StatementContext != null;



        public WitnessData Witness =>
            WitnessContext?.Witness;


        public TestimonyData Testimony =>
            TestimonyContext?.Testimony;


        public StatementData Statement =>
            StatementContext?.Statement;



        public void SelectCase(
            CaseData caseData)
        {
            Case = caseData;

            WitnessContext = null;
            TestimonyContext = null;
            StatementContext = null;

            Notify();
        }



        public void SelectWitness(
            WitnessContext context)
        {
            Case =
                context.Case;

            WitnessContext =
                context;

            TestimonyContext = null;
            StatementContext = null;

            Notify();
        }



        public void SelectTestimony(
            TestimonyContext context)
        {
            Case =
                context.Case;

            WitnessContext = null;

            TestimonyContext =
                context;

            StatementContext = null;

            Notify();
        }



        public void SelectStatement(
            StatementContext context)
        {
            Case =
                context.Case;


            WitnessContext = null;

            TestimonyContext = null;


            StatementContext =
                context;


            Notify();
        }



        public void Clear()
        {
            Case = null;

            WitnessContext = null;

            TestimonyContext = null;

            StatementContext = null;

            Notify();
        }



        private void Notify()
        {
            SelectionChanged?.Invoke();
        }
    }
}
