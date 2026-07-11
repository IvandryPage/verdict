using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Verdict.Editor.CaseFlow
{
    public sealed class CaseFlowEditorWindow : EditorWindow
    {
        private CaseEditorController controller;

        [MenuItem("Verdict/Case Flow Editor")]
        public static void Open()
        {
            CaseFlowEditorWindow window =
                GetWindow<CaseFlowEditorWindow>();

            window.titleContent =
                new GUIContent("Case Flow");

            window.minSize =
                new Vector2(1200, 700);
        }

        private void CreateGUI()
        {
            controller = new CaseEditorController(rootVisualElement);
            controller.Initialize();
        }

        private void OnGUI()
        {
            controller?.HandleEvent(Event.current);
        }
    }
}
