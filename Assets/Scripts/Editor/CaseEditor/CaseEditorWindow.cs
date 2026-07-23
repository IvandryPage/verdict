using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Verdict.Editor.CaseEditor
{
    public sealed class CaseEditorWindow : EditorWindow
    {
        private CaseEditorController controller;

        [MenuItem("Verdict/Case Editor")]
        public static void Open()
        {
            CaseEditorWindow window =
                GetWindow<CaseEditorWindow>();

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
