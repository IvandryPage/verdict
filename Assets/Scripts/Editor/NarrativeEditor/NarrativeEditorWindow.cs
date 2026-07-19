using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Systems.Validation;

namespace Verdict.Editor.NarrativeEditor
{
    /// <summary>
    /// Standalone editor for authoring a case's branching narrative graph:
    /// dialogue, statement pause points, choices, conditions, jumps and
    /// endings. Opens for whichever CaseData is selected in the Project
    /// window, or pick one from the Case field.
    /// </summary>
    public sealed class NarrativeEditorWindow : EditorWindow
    {
        private ObjectField caseField;
        private VisualElement graphContainer;
        private Label statusLabel;

        private NarrativeGraphView graphView;
        private NarrativeEditService editService;
        private NarrativeGraphContext context;
        private CaseData caseData;

        private Vector2 nextSpawnPosition = new(100, 100);

        [MenuItem("Verdict/Narrative Graph Editor")]
        public static void Open()
        {
            NarrativeEditorWindow window = GetWindow<NarrativeEditorWindow>();
            window.titleContent = new GUIContent("Narrative Graph");
            window.minSize = new Vector2(960, 620);
        }

        private void CreateGUI()
        {
            rootVisualElement.style.flexDirection = FlexDirection.Column;

            BuildToolbar();

            graphContainer = new VisualElement { style = { flexGrow = 1 } };
            rootVisualElement.Add(graphContainer);

            if (Selection.activeObject is CaseData selected)
            {
                LoadCase(selected);
            }
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject is CaseData selected && selected != caseData)
            {
                LoadCase(selected);
            }
        }

        private void BuildToolbar()
        {
            Toolbar toolbar = new();

            caseField = new ObjectField("Case")
            {
                objectType = typeof(CaseData),
                allowSceneObjects = false
            };

            caseField.style.minWidth = 260;
            caseField.RegisterValueChangedCallback(evt => LoadCase(evt.newValue as CaseData));

            toolbar.Add(caseField);
            toolbar.Add(new ToolbarSpacer());

            toolbar.Add(CreateNodeButton("+ Dialogue", () => editService.CreateDialogueNode(NextSpawnPosition())));
            toolbar.Add(CreateNodeButton("+ Statement", () => editService.CreateStatementNode(NextSpawnPosition())));
            toolbar.Add(CreateNodeButton("+ Choice", () => editService.CreateChoiceNode(NextSpawnPosition())));
            toolbar.Add(CreateNodeButton("+ Condition", () => editService.CreateConditionNode(NextSpawnPosition())));
            toolbar.Add(CreateNodeButton("+ Jump", () => editService.CreateJumpNode(NextSpawnPosition())));
            toolbar.Add(CreateNodeButton("+ End", () => editService.CreateEndNode(NextSpawnPosition())));
            toolbar.Add(CreateNodeButton("+ Gameplay", () => editService.CreateGameplayNode(NextSpawnPosition())));

            toolbar.Add(new ToolbarSpacer());
            toolbar.Add(new Button(Validate) { text = "Validate" });

            toolbar.Add(new ToolbarSpacer());
            statusLabel = new Label { style = { unityTextAlign = TextAnchor.MiddleLeft, marginLeft = 8 } };
            toolbar.Add(statusLabel);

            rootVisualElement.Add(toolbar);
        }

        private Button CreateNodeButton(string label, System.Action create)
        {
            return new Button(() =>
            {
                if (editService == null)
                {
                    EditorUtility.DisplayDialog("No case loaded", "Pick a Case above first.", "OK");
                    return;
                }

                create();
            })
            { text = label };
        }

        private Vector2 NextSpawnPosition()
        {
            Vector2 position = nextSpawnPosition;
            nextSpawnPosition += new Vector2(40, 40);
            return position;
        }

        private void LoadCase(CaseData data)
        {
            caseData = data;
            caseField.SetValueWithoutNotify(data);

            graphContainer.Clear();
            graphView = null;
            editService = null;

            if (data == null)
            {
                statusLabel.text = string.Empty;
                return;
            }

            editService = new NarrativeEditService(data);
            editService.GraphModified += Rebuild;

            graphView = new NarrativeGraphView(editService);
            graphView.StretchToParentSize();
            graphContainer.Add(graphView);

            MiniMap miniMap = new() { anchored = true };
            miniMap.SetPosition(new Rect(10, 30, 200, 140));
            graphView.Add(miniMap);

            Rebuild();
        }

        private void Rebuild()
        {
            if (caseData == null || graphView == null)
            {
                return;
            }

            context = new NarrativeGraphContext(caseData);
            NarrativeGraphBuilder.Build(graphView, caseData, context);
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            int nodeCount = caseData.Narrative?.Nodes.Count ?? 0;
            bool hasStart = !string.IsNullOrWhiteSpace(caseData.Narrative?.StartNodeId);

            statusLabel.text = hasStart
                ? $"{nodeCount} node(s)"
                : $"{nodeCount} node(s) - no start node set (right-click a node)";
        }

        private void Validate()
        {
            if (caseData == null)
            {
                return;
            }

            ValidationResult result = RuntimeValidator.Validate(caseData);

            var narrativeIssues = result.GetIssues(ValidationScope.Narrative).ToList();

            if (narrativeIssues.Count == 0)
            {
                EditorUtility.DisplayDialog("Narrative Validation", "No issues found.", "OK");
                return;
            }

            StringBuilder summary = new();

            foreach (ValidationIssue issue in narrativeIssues)
            {
                string line = $"[{issue.Severity}] {issue.Message}";
                summary.AppendLine(line);

                if (issue.Severity == ValidationSeverity.Error)
                {
                    Debug.LogError(line);
                }
                else
                {
                    Debug.LogWarning(line);
                }
            }

            EditorUtility.DisplayDialog(
                "Narrative Validation",
                summary.ToString(),
                "OK");
        }
    }
}
