using System;
using Verdict.Data.Cases;

namespace Verdict.Editor.CaseFlow
{
    public static class HierarchyDisplayUtility
    {
        public static string GetWitnessName(
            WitnessData witness)
        {
            if (witness == null)
                return "<Missing Witness>";

            if (witness.Character != null &&
                !string.IsNullOrWhiteSpace(
                    witness.Character.name))
            {
                return witness.Character.name;
            }

            return "<No Character>";
        }

        public static string GetTestimonyName(
            TestimonyData testimony)
        {
            if (testimony == null)
                return "<Missing Testimony>";

            if (!string.IsNullOrWhiteSpace(
                testimony.Title))
            {
                return testimony.Title;
            }

            return "<Untitled Testimony>";
        }

        public static string GetStatementName(
            StatementData statement)
        {
            if (statement == null)
                return "<Missing Statement>";

            if (string.IsNullOrWhiteSpace(
                statement.Text))
            {
                return "<Empty Statement>";
            }

            return Shorten(
                statement.Text,
                40);
        }

        private static string Shorten(
            string text,
            int maxLength)
        {
            text =
                text.Replace("\n", " ");

            if (text.Length <= maxLength)
                return text;

            return
                text[..maxLength] + "...";
        }
    }
}
