using Verdict.Data.Cases;

namespace Verdict.Editor.CaseEditor
{
    public sealed class StatementContext
    {
        public CaseData Case { get; }

        public WitnessData Witness { get; }

        public TestimonyData Testimony { get; }

        public StatementData Statement { get; }


        public int WitnessIndex { get; }

        public int TestimonyIndex { get; }

        public int StatementIndex { get; }


        public StatementContext(
            CaseData caseData,
            WitnessData witness,
            TestimonyData testimony,
            StatementData statement,
            int witnessIndex,
            int testimonyIndex,
            int statementIndex)
        {
            Case = caseData;

            Witness = witness;
            Testimony = testimony;
            Statement = statement;

            WitnessIndex = witnessIndex;
            TestimonyIndex = testimonyIndex;
            StatementIndex = statementIndex;
        }


        // witnesses[x].testimonies[y].statements[z]
        public string StatementPropertyPath =>
            $"witnesses.Array.data[{WitnessIndex}]." +
            $"testimonies.Array.data[{TestimonyIndex}]." +
            $"statements.Array.data[{StatementIndex}]";


        public string ClaimPropertyPath(
            ClaimData claim)
        {
            int index =
                FindIndex(
                    Statement.Claims,
                    claim);


            if (index < 0)
                return null;


            return
                $"{StatementPropertyPath}." +
                $"claims.Array.data[{index}]";
        }


        public string ArgumentRulePropertyPath(
            ClaimData claim,
            ArgumentRuleData rule)
        {
            string claimPath =
                ClaimPropertyPath(claim);


            if (claimPath == null)
                return null;


            int index =
                FindIndex(
                    claim.ArgumentRules,
                    rule);


            if (index < 0)
                return null;


            return
                $"{claimPath}." +
                $"evaluationRules.Array.data[{index}]";
        }


        public string SuccessEffectPropertyPath(
            ClaimData claim,
            ArgumentRuleData rule,
            CourtStateEffectData effect)
        {
            string rulePath =
                ArgumentRulePropertyPath(
                    claim,
                    rule);


            if (rulePath == null)
                return null;


            int index =
                FindIndex(
                    rule.SuccessEffects,
                    effect);


            if (index < 0)
                return null;


            return
                $"{rulePath}." +
                $"successEffects.Array.data[{index}]";
        }


        public string FailureEffectPropertyPath(
            ClaimData claim,
            ArgumentRuleData rule,
            CourtStateEffectData effect)
        {
            string rulePath =
                ArgumentRulePropertyPath(
                    claim,
                    rule);


            if (rulePath == null)
                return null;


            int index =
                FindIndex(
                    rule.FailureEffects,
                    effect);


            if (index < 0)
                return null;


            return
                $"{rulePath}." +
                $"failureEffects.Array.data[{index}]";
        }



        private int FindIndex<T>(
            System.Collections.Generic.IReadOnlyList<T> list,
            T target)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(
                    list[i],
                    target))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
