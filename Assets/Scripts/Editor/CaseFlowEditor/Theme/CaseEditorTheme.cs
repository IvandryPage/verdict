using UnityEngine;

namespace Verdict.Editor.CaseFlow.Theme
{
    public static class CaseEditorTheme
    {
        public static NodeStyle GetNodeStyle(
            NodeColor color)
        {
            return color switch
            {
                NodeColor.Entry =>
                    new NodeStyle(
                        Hex("#1B5E20"),
                        Hex("#43A047"),
                        Hex("#E8F5E9")),

                NodeColor.Hidden =>
                    new NodeStyle(
                        Hex("#2B2E36"),
                        Hex("#61656D"),
                        Hex("#E0E0E0")),

                NodeColor.Warning =>
                    new NodeStyle(
                        Hex("#4E342E"),
                        Hex("#F9A825"),
                        Hex("#FFF8E1")),

                NodeColor.Error =>
                    new NodeStyle(
                        Hex("#B71C1C"),
                        Hex("#D32F2F"),
                        Hex("#FFEBEE")),

                _ =>
                    new NodeStyle(
                        Hex("#23232A"),
                        Hex("#616161"),
                        Hex("#E8EAF6"))
            };
        }

        public static EdgeStyle GetEdgeStyle(
            EdgeColor color)
        {
            return color switch
            {
                EdgeColor.Reveal =>
                    new EdgeStyle(
                        Hex("#42A5F5"),
                        3f),

                EdgeColor.Jump =>
                    new EdgeStyle(
                        Hex("#7C4DFF"),
                        3f),

                EdgeColor.Warning =>
                    new EdgeStyle(
                        Hex("#F9A825"),
                        3f),

                EdgeColor.Error =>
                    new EdgeStyle(
                        Hex("#EF5350"),
                        4f),

                _ =>
                    new EdgeStyle(
                        Hex("#9E9E9E"),
                        2f)
            };
        }

        private static Color Hex(
            string html)
        {
            ColorUtility.TryParseHtmlString(
                html,
                out Color color);

            return color;
        }
    }
}
