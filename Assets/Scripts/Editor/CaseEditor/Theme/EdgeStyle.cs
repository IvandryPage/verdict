using UnityEngine;

namespace Verdict.Editor.CaseEditor.Theme
{
    public sealed class EdgeStyle
    {
        public Color Color { get; }
        public float Width {get;}

        public EdgeStyle(
            Color color,
            float width
        )
        {
            Color = color;
            Width = width;
        }
    }
}
