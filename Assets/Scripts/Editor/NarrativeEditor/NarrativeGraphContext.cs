using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Editor.NarrativeEditor
{
    public readonly struct NarrativeOption
    {
        public readonly string Id;
        public readonly string Label;

        public NarrativeOption(string id, string label)
        {
            Id = id;
            Label = label;
        }
    }

    /// <summary>
    /// Everything from CaseData the narrative graph editor needs to
    /// render dropdowns without reaching into CaseData directly from
    /// every node view.
    /// </summary>
    public sealed class NarrativeGraphContext
    {
        public IReadOnlyList<NarrativeOption> Statements { get; }

        public IReadOnlyList<NarrativeOption> Endings { get; }

        public IReadOnlyList<NarrativeOption> Claims { get; }

        public NarrativeGraphContext(CaseData caseData)
        {
            var statements = new List<NarrativeOption>();
            var claims = new List<NarrativeOption>();

            foreach (WitnessData witness in caseData.Witnesses)
            {
                foreach (TestimonyData testimony in witness.Testimonies)
                {
                    foreach (StatementData statement in testimony.Statements)
                    {
                        string preview = string.IsNullOrWhiteSpace(statement.Text)
                            ? "<empty>"
                            : statement.Text.Trim();

                        if (preview.Length > 40)
                        {
                            preview = preview[..40] + "...";
                        }

                        string shortId = statement.Id.Length <= 6
                            ? statement.Id
                            : statement.Id[..6];

                        statements.Add(new NarrativeOption(
                            statement.Id,
                            $"{witness.Id} / {testimony.Title} / {shortId} — {preview}"));

                        foreach (ClaimData claim in statement.Claims)
                        {
                            string claimShortId = claim.Id.Length <= 6
                                ? claim.Id
                                : claim.Id[..6];

                            claims.Add(new NarrativeOption(
                                claim.Id,
                                $"{witness.Id} / {testimony.Title} / {claimShortId} — {claim.FactId}"));
                        }
                    }
                }
            }

            var endings = new List<NarrativeOption>();

            foreach (EndingData ending in caseData.Endings)
            {
                endings.Add(new NarrativeOption(ending.Id, ending.Id));
            }

            Statements = statements;
            Endings = endings;
            Claims = claims;
        }

        public string GetStatementLabel(string statementId)
        {
            if (string.IsNullOrWhiteSpace(statementId))
            {
                return "(none)";
            }

            foreach (NarrativeOption option in Statements)
            {
                if (option.Id == statementId)
                {
                    return option.Label;
                }
            }

            return $"<missing: {statementId}>";
        }
    }
}
