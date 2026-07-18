using System.Collections.Generic;
using Verdict.Data.Cases;
using Verdict.Runtime.Dialogue;

namespace Verdict.Runtime
{
    public sealed class CaseRuntime
    {
        public CaseRuntime(
            CaseData data,
            IReadOnlyList<EvidenceRuntime> evidence,
            IReadOnlyList<WitnessRuntime> witnesses,
            CourtStateRuntime courtState,
            IReadOnlyDictionary<string, StatementRuntime> statementsById,
            IReadOnlyDictionary<string, TestimonyRuntime> testimoniesById,
            IReadOnlyDictionary<string, WitnessRuntime> witnessesById)
        {
            Data = data;
            Evidence = evidence;
            Witnesses = witnesses;
            CourtState = courtState;
            StatementsById = statementsById;
            TestimoniesById = testimoniesById;
            WitnessesById = witnessesById;
        }

        public CaseData Data { get; }

        public IReadOnlyList<EvidenceRuntime> Evidence { get; }

        public IReadOnlyList<WitnessRuntime> Witnesses { get; }

        public CourtStateRuntime CourtState { get; }

        public IReadOnlyDictionary<string, StatementRuntime> StatementsById { get; }

        public IReadOnlyDictionary<string, TestimonyRuntime> TestimoniesById { get; }

        public IReadOnlyDictionary<string, WitnessRuntime> WitnessesById { get; }


        public bool TryGetStatement(string statementId, out StatementRuntime statement)
        {
            if (string.IsNullOrWhiteSpace(statementId))
            {
                statement = null;
                return false;
            }

            return StatementsById.TryGetValue(statementId, out statement);
        }

        public bool TryGetTestimony(string testimonyId, out TestimonyRuntime testimony)
        {
            if (string.IsNullOrWhiteSpace(testimonyId))
            {
                testimony = null;
                return false;
            }

            return TestimoniesById.TryGetValue(testimonyId, out testimony);
        }

        public bool TryGetWitness(string witnessId, out WitnessRuntime witness)
        {
            if (string.IsNullOrWhiteSpace(witnessId))
            {
                witness = null;
                return false;
            }

            return WitnessesById.TryGetValue(witnessId, out witness);
        }
    }
}
