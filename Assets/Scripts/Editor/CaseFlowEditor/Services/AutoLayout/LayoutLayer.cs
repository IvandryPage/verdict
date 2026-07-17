using System.Collections.Generic;
using Verdict.Systems.Validation.Graph;

namespace Verdict.Editor.CaseFlow.Layout
{
    public sealed class LayoutLayer
    {
        public int Index;

        public List<FlowGraphNode> Nodes =
            new();
    }
}
