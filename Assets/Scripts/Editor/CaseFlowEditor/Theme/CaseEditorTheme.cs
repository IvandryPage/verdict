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
                        Hex("#2E7D32"),
                        Hex("#66BB6A"),
                        Color.white),

                NodeColor.Hidden =>
                    new NodeStyle(
                        Hex("#4A4A4A"),
                        Hex("#757575"),
                        Color.white),

                NodeColor.Warning =>
                    new NodeStyle(
                        Hex("#A16A00"),
                        Hex("#FBC02D"),
                        Color.white),

                NodeColor.Error =>
                    new NodeStyle(
                        Hex("#7F1D1D"),
                        Hex("#EF5350"),
                        Color.white),

                _ =>
                    new NodeStyle(
                        Hex("#2D2D30"),
                        Hex("#616161"),
                        Color.white)
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
                        Hex("#FB8C00"),
                        3f),

                EdgeColor.Warning =>
                    new EdgeStyle(
                        Hex("#FBC02D"),
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
