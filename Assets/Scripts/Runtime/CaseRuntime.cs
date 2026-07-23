using System.Collections.Generic;
using Verdict.Data.Cases;

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
            IReadOnlyDictionary<string, WitnessRuntime> witnessesById,
            IReadOnlyDictionary<string, string> statementNodeIds,
            IReadOnlyDictionary<string, ClaimRuntime> claimsById)
        {
            Data = data;
            Evidence = evidence;
            Witnesses = witnesses;
            CourtState = courtState;
            StatementsById = statementsById;
            TestimoniesById = testimoniesById;
            WitnessesById = witnessesById;
            StatementNodeIds = statementNodeIds;
            ClaimsById = claimsById;
        }

        public CaseData Data { get; }

        public IReadOnlyList<EvidenceRuntime> Evidence { get; }

        public IReadOnlyList<WitnessRuntime> Witnesses { get; }

        public CourtStateRuntime CourtState { get; }

        public IReadOnlyDictionary<string, StatementRuntime> StatementsById { get; }

        public IReadOnlyDictionary<string, TestimonyRuntime> TestimoniesById { get; }

        public IReadOnlyDictionary<string, WitnessRuntime> WitnessesById { get; }

        /// <summary>
        /// Maps StatementData.Id -> the NodeId of the StatementNodeData
        /// (if any) bound to it in the case's narrative graph. Built once
        /// by RuntimeFactory from CaseData.Narrative.
        /// </summary>
        public IReadOnlyDictionary<string, string> StatementNodeIds { get; }

        /// <summary>
        /// All claims across every statement, by ClaimData.Id - used by
        /// ClaimConditionData to check another claim's resolution state.
        /// </summary>
        public IReadOnlyDictionary<string, ClaimRuntime> ClaimsById { get; }

        public bool TryGetClaim(string claimId, out ClaimRuntime claim)
        {
            if (string.IsNullOrWhiteSpace(claimId))
            {
                claim = null;
                return false;
            }

            return ClaimsById.TryGetValue(claimId, out claim);
        }

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

        public bool TryGetNodeIdForStatement(string statementId, out string nodeId)
        {
            if (string.IsNullOrWhiteSpace(statementId))
            {
                nodeId = null;
                return false;
            }

            return StatementNodeIds.TryGetValue(statementId, out nodeId);
        }
    }
}
