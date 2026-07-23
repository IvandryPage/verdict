using UnityEngine;

namespace Verdict.Editor.CaseEditor.Theme
{
    public sealed class NodeStyle
    {
        public Color Background { get; }

        public Color Border { get; }

        public Color Title { get; }

        public NodeStyle(
            Color background,
            Color border,
            Color title)
        {
            Background = background;
            Border = border;
            Title = title;
        }
    }
}
