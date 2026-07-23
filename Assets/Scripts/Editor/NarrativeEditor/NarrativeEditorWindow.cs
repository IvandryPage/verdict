using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Data.Narrative;
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
            toolbar.Add(new Button(Simulate) { text = "▶ Simulate" });

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
        private void Simulate()
        {
            if (caseData == null)
            {
                return;
            }

            NarrativeGraphData graph = caseData.Narrative;

            if (graph == null || string.IsNullOrWhiteSpace(graph.StartNodeId))
            {
                EditorUtility.DisplayDialog("Simulate", "No StartNodeId set - nothing to trace.", "OK");
                return;
            }

            Dictionary<string, NarrativeNodeData> nodesById = graph.Nodes
                .Where(n => !string.IsNullOrWhiteSpace(n.NodeId))
                .ToDictionary(n => n.NodeId);

            StringBuilder trace = new();
            trace.Append("Start");

            HashSet<string> visited = new();
            string currentId = graph.StartNodeId;
            int guard = 0;

            while (!string.IsNullOrWhiteSpace(currentId) && guard++ < 500)
            {
                if (!nodesById.TryGetValue(currentId, out NarrativeNodeData node))
                {
                    trace.Append($"\n↓\n<missing node '{currentId}'>");
                    break;
                }

                trace.Append($"\n↓\n{DescribeNode(node)}");

                if (!visited.Add(currentId))
                {
                    trace.Append("\n↓\n<loop back to a visited node - stopping trace>");
                    break;
                }

                currentId = GetDefaultNextId(node);
            }

            if (string.IsNullOrWhiteSpace(currentId) && guard < 500)
            {
                trace.Append("\n↓\nEnd of branch (no outgoing connection)");
            }

            EditorUtility.DisplayDialog(
                "Simulate (dry-run trace, no game running)",
                trace.ToString(),
                "OK");
        }

        /// <summary>
        /// Static preview only - Condition nodes always follow True,
        /// Choice nodes always follow the first choice. This traces one
        /// possible path through the graph, it does not evaluate any
        /// real CourtState or claim.
        /// </summary>
        private static string GetDefaultNextId(NarrativeNodeData node)
        {
            return node switch
            {
                DialogueNodeData n => n.NextNodeId,
                StatementNodeData n => n.NextNodeId,
                JumpNodeData n => n.TargetNodeId,
                GameplayNodeData n => n.NextNodeId,
                ConditionNodeData n => n.TrueNodeId,
                ChoiceNodeData n => n.Choices.Count > 0 ? n.Choices[0].NextNodeId : null,
                _ => null
            };
        }

        private static string DescribeNode(NarrativeNodeData node)
        {
            return node switch
            {
                DialogueNodeData => $"Dialogue ({ShortId(node.NodeId)})",
                StatementNodeData statementNode => $"Statement ({ShortId(statementNode.StatementId)})",
                ChoiceNodeData choiceNode => $"Choice ({choiceNode.Choices.Count} option(s), following #1)",
                ConditionNodeData => "Branch (following True)",
                JumpNodeData => $"Jump ({ShortId(node.NodeId)})",
                EndNodeData endNode => string.IsNullOrWhiteSpace(endNode.EndingId)
                    ? "End"
                    : $"End ({endNode.EndingId})",
                GameplayNodeData gameplayNode => $"Gameplay ({gameplayNode.GameplayEventId})",
                _ => node.NodeId
            };
        }

        private static string ShortId(string id) =>
            string.IsNullOrWhiteSpace(id) ? "<none>" : (id.Length <= 6 ? id : id[..6]);
    }
}
