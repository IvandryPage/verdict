using UnityEngine;
using Verdict.Data.Narrative;
using Verdict.Editor.CaseEditor.Theme;

namespace Verdict.Editor.NarrativeEditor
{
    /// <summary>
    /// Per-node-type colors for the narrative graph editor. Kept separate
    /// from CaseEditorTheme (which colors the statement flow graph) so
    /// neither has to know about the other's node kinds.
    /// </summary>
    public static class NarrativeNodeColors
    {
        public static NodeStyle GetStyle(NarrativeNodeData node)
        {
            return node switch
            {
                DialogueNodeData => new NodeStyle(Hex("#1A237E"), Hex("#3F51B5"), Hex("#E8EAF6")),
                StatementNodeData => new NodeStyle(Hex("#1B5E20"), Hex("#43A047"), Hex("#E8F5E9")),
                ChoiceNodeData => new NodeStyle(Hex("#4A148C"), Hex("#8E24AA"), Hex("#F3E5F5")),
                ConditionNodeData => new NodeStyle(Hex("#4E342E"), Hex("#F9A825"), Hex("#FFF8E1")),
                JumpNodeData => new NodeStyle(Hex("#263238"), Hex("#607D8B"), Hex("#ECEFF1")),
                EndNodeData => new NodeStyle(Hex("#B71C1C"), Hex("#D32F2F"), Hex("#FFEBEE")),
                GameplayNodeData => new NodeStyle(Hex("#004D40"), Hex("#00897B"), Hex("#E0F2F1")),
                _ => new NodeStyle(Hex("#23232A"), Hex("#616161"), Hex("#E8EAF6"))
            };
        }

        public static readonly Color StartHighlight = Hex("#FFD54F");

        private static Color Hex(string html)
        {
            ColorUtility.TryParseHtmlString(html, out Color color);
            return color;
        }
    }
}
