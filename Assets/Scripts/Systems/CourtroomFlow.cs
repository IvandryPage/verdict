using System;
using Verdict.Runtime;

namespace Verdict.Systems
{
    public sealed class CourtroomFlow
    {
        private readonly CaseRuntime runtime;

        private int witnessIndex;
        private int testimonyIndex;
        private int statementIndex;

        public CourtroomFlow(CaseRuntime runtime)
        {
            this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public CaseRuntime Runtime => runtime;

        public WitnessRuntime CurrentWitness =>
            runtime.Witnesses[witnessIndex];

        public TestimonyRuntime CurrentTestimony =>
            CurrentWitness.Testimonies[testimonyIndex];

        public StatementRuntime CurrentStatement =>
            CurrentTestimony.Statements[statementIndex];

        public bool IsFirstWitness => witnessIndex == 0;

        public bool IsLastWitness =>
            witnessIndex == runtime.Witnesses.Count - 1;

        public bool IsFirstTestimony => testimonyIndex == 0;

        public bool IsLastTestimony =>
            testimonyIndex == CurrentWitness.Testimonies.Count - 1;

        public bool IsFirstStatement => statementIndex == 0;

        public bool IsLastStatement =>
            statementIndex == CurrentTestimony.Statements.Count - 1;

        public bool CanMoveNextStatement => !IsLastStatement;

        public bool CanMovePreviousStatement => !IsFirstStatement;

        public bool CanMoveNextTestimony => !IsLastTestimony;

        public bool CanMovePreviousTestimony => !IsFirstTestimony;

        public bool CanMoveNextWitness => !IsLastWitness;

        public bool CanMovePreviousWitness => !IsFirstWitness;

        public bool IsComplete =>
            IsLastWitness &&
            IsLastTestimony &&
            IsLastStatement;

        public bool MoveNextStatement()
        {
            if (!CanMoveNextStatement)
            {
                return false;
            }

            statementIndex++;
            return true;
        }

        public bool MovePreviousStatement()
        {
            if (!CanMovePreviousStatement)
            {
                return false;
            }

            statementIndex--;
            return true;
        }

        public bool MoveNextTestimony()
        {
            if (!CanMoveNextTestimony)
            {
                return false;
            }

            testimonyIndex++;
            statementIndex = 0;

            return true;
        }

        public bool MovePreviousTestimony()
        {
            if (!CanMovePreviousTestimony)
            {
                return false;
            }

            testimonyIndex--;
            statementIndex = 0;

            return true;
        }

        public bool MoveNextWitness()
        {
            if (!CanMoveNextWitness)
            {
                return false;
            }

            witnessIndex++;
            testimonyIndex = 0;
            statementIndex = 0;

            return true;
        }

        public bool MovePreviousWitness()
        {
            if (!CanMovePreviousWitness)
            {
                return false;
            }

            witnessIndex--;
            testimonyIndex = 0;
            statementIndex = 0;

            return true;
        }

        public bool MoveNext()
        {
            if (MoveNextStatement())
            {
                return true;
            }

            if (MoveNextTestimony())
            {
                return true;
            }

            if (MoveNextWitness())
            {
                return true;
            }

            return false;
        }

        public void Reset()
        {
            witnessIndex = 0;
            testimonyIndex = 0;
            statementIndex = 0;
        }
    }
}
