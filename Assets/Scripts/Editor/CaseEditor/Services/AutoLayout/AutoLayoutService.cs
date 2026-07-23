using System.Collections.Generic;
using UnityEngine;
using Verdict.Editor.CaseEditor.Layout;
using Verdict.Systems.Validation.Graph;

namespace Verdict.Editor.CaseEditor.Service
{
    public sealed class AutoLayoutService
    {
        private readonly LayerBuilder layerBuilder =
            new();

        private readonly CoordinateBuilder coordinateBuilder =
            new();

        private readonly LayoutSettings settings =
            new();

        public Dictionary<string, Vector2> Calculate(
            FlowGraph graph)
        {
            Dictionary<string, int> layers =
                layerBuilder.Build(graph);

            return coordinateBuilder.Build(
                graph,
                layers,
                settings);
        }
    }
}
