namespace Verdict.Editor.CaseFlow.Hierarchy
{
    public sealed class HierarchyItem
    {
        public int Id;
        public string Name;
        public string Key;
        public HierarchyType Type;

        public WitnessContext Witness;
        public TestimonyContext Testimony;
        public StatementContext Statement;
    }
}
