using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Data.Narrative;
using Verdict.Editor.CaseEditor.Theme;

namespace Verdict.Editor.NarrativeEditor
{
    /// <summary>
    /// Visual node for a single NarrativeNodeData. Builds different inline
    /// fields and output ports depending on the node's concrete type, but
    /// is otherwise one class - the polymorphism lives in the data, not
    /// in a Node subclass per type.
    /// </summary>
    public sealed class NarrativeNodeView : Node
    {
        private readonly NarrativeEditService editService;
        private readonly NarrativeGraphContext context;

        private readonly List<(string Key, Port Port)> outputSlots = new();

        private NodeStyle currentStyle;
        private VisualElement choiceRowsContainer;

        public NarrativeNodeData Data { get; }

        public Port InputPort { get; private set; }

        public IReadOnlyList<(string Key, Port Port)> OutputSlots => outputSlots;

        public bool IsStartNode { get; private set; }

        internal bool SuppressSelectedEvent { get; set; }

        public event Action<NarrativeNodeView> Selected;

        public NarrativeNodeView(
            NarrativeNodeData data,
            NarrativeEditService editService,
            NarrativeGraphContext context)
        {
            Data = data;
            this.editService = editService;
            this.context = context;

            capabilities |=
                Capabilities.Selectable |
                Capabilities.Movable |
                Capabilities.Deletable |
                Capabilities.Ascendable;

            title = GetTitle(data);

            style.minWidth = 220;

            BuildInputPort();
            BuildBody();

            RefreshExpandedState();
            RefreshPorts();
        }

        public void SetIsStartNode(bool isStart)
        {
            IsStartNode = isStart;
            ApplyStyle(NarrativeNodeColors.GetStyle(Data));
        }

        public Port FindOutputPort(string key)
        {
            foreach ((string Key, Port Port) slot in outputSlots)
            {
                if (slot.Key == key)
                {
                    return slot.Port;
                }
            }

            return null;
        }

        private void BuildInputPort()
        {
            InputPort = InstantiatePort(
                Orientation.Horizontal,
                Direction.Input,
                Port.Capacity.Multi,
                typeof(bool));

            InputPort.portName = "In";
            inputContainer.Add(InputPort);
        }

        private Port AddOutputPort(string key, string label)
        {
            Port port = InstantiatePort(
                Orientation.Horizontal,
                Direction.Output,
                Port.Capacity.Single,
                typeof(bool));

            port.portName = label;
            outputContainer.Add(port);
            outputSlots.Add((key, port));

            return port;
        }

        private void BuildBody()
        {
            switch (Data)
            {
                case DialogueNodeData dialogueNode:
                    BuildDialogueBody(dialogueNode);
                    break;

                case StatementNodeData statementNode:
                    BuildStatementBody(statementNode);
                    break;

                case ChoiceNodeData choiceNode:
                    BuildChoiceBody(choiceNode);
                    break;

                case ConditionNodeData conditionNode:
                    BuildConditionBody(conditionNode);
                    break;

                case JumpNodeData jumpNode:
                    BuildJumpBody(jumpNode);
                    break;

                case EndNodeData endNode:
                    BuildEndBody(endNode);
                    break;

                case GameplayNodeData gameplayNode:
                    BuildGameplayBody(gameplayNode);
                    break;
            }
        }

        // ---------------------------------------------------------------
        // Dialogue
        // ---------------------------------------------------------------

        private void BuildDialogueBody(DialogueNodeData node)
        {
            Label summary = new(GetEntrySummary(node))
            {
                style = { whiteSpace = WhiteSpace.Normal, marginTop = 4, marginBottom = 4, fontSize = 11 }
            };

            extensionContainer.Add(summary);

            Button editButton = new(() =>
            {
                DialogueEntryEditorWindow.Open(node, editService, () =>
                {
                    summary.text = GetEntrySummary(node);
                });
            })
            { text = "Edit Entries..." };

            extensionContainer.Add(editButton);

            AddOutputPort(NarrativeOutputKeys.Next, "Next");
        }

        private static string GetEntrySummary(DialogueNodeData node)
        {
            int count = node.Entries.Count;

            if (count == 0)
            {
                return "<no entries>";
            }

            int lines = 0;
            int events = 0;

            foreach (NarrativeDialogueEntryData entry in node.Entries)
            {
                if (entry.Type == NarrativeDialogueEntryType.Line) lines++;
                else events++;
            }

            return $"{lines} line(s), {events} event(s)";
        }

        // ---------------------------------------------------------------
        // Statement
        // ---------------------------------------------------------------

        private void BuildStatementBody(StatementNodeData node)
        {
            List<NarrativeOption> options = new() { new NarrativeOption(string.Empty, "<none>") };
            options.AddRange(context.Statements);

            int selectedIndex = Mathf.Max(0, options.FindIndex(o => o.Id == node.StatementId));

            PopupField<NarrativeOption> popup = new(
                options,
                selectedIndex,
                entry => entry.Label,
                entry => entry.Label);

            popup.RegisterValueChangedCallback(evt =>
            {
                editService.SetStatementId(node, evt.newValue.Id);
            });

            popup.style.whiteSpace = WhiteSpace.Normal;

            extensionContainer.Add(popup);

            AddOutputPort(NarrativeOutputKeys.Next, "Next");
        }

        // ---------------------------------------------------------------
        // Choice
        // ---------------------------------------------------------------

        private void BuildChoiceBody(ChoiceNodeData node)
        {
            choiceRowsContainer = new VisualElement();
            extensionContainer.Add(choiceRowsContainer);

            RebuildChoiceRows(node);

            Button addButton = new(() =>
            {
                editService.AddChoice(node);
                RebuildChoicePorts(node);
            })
            { text = "+ Add Choice" };

            extensionContainer.Add(addButton);
        }

        /// <summary>
        /// Rebuilds just this node's choice rows/ports in place - not a
        /// full graph rebuild - so in-progress edits elsewhere in the
        /// graph aren't disturbed.
        /// </summary>
        public void RebuildChoicePorts(ChoiceNodeData node)
        {
            NarrativeGraphView graphView = this.GetFirstAncestorOfType<NarrativeGraphView>();

            foreach ((string Key, Port Port) slot in outputSlots)
            {
                graphView?.DisconnectAndRemovePort(this, slot.Port);
            }

            outputSlots.Clear();

            RebuildChoiceRows(node);

            RefreshPorts();
            RefreshExpandedState();

            graphView?.ReconnectNode(this);
        }

        private void RebuildChoiceRows(ChoiceNodeData node)
        {
            choiceRowsContainer.Clear();

            for (int i = 0; i < node.Choices.Count; i++)
            {
                NarrativeChoiceOptionData choice = node.Choices[i];

                VisualElement row = new() { style = { flexDirection = FlexDirection.Row, marginBottom = 2 } };

                TextField textField = new() { value = choice.Text };
                textField.style.flexGrow = 1;

                textField.RegisterValueChangedCallback(evt =>
                    editService.SetChoiceText(choice, evt.newValue));

                Button removeButton = new(() =>
                {
                    editService.RemoveChoice(node, choice);
                    RebuildChoicePorts(node);
                })
                { text = "-" };

                row.Add(textField);
                row.Add(removeButton);

                choiceRowsContainer.Add(row);

                AddOutputPort(choice.ChoiceId, $"#{i + 1}");
            }
        }

        // ---------------------------------------------------------------
        // Condition
        // ---------------------------------------------------------------

        private void BuildConditionBody(ConditionNodeData node)
        {
            EnumField modeField = new("Mode", node.Condition.Mode);
            extensionContainer.Add(modeField);

            VisualElement courtStatGroup = new();
            VisualElement claimGroup = new();

            extensionContainer.Add(courtStatGroup);
            extensionContainer.Add(claimGroup);

            BuildCourtStatConditionFields(node, courtStatGroup);
            BuildClaimConditionFields(node, claimGroup);

            void RefreshVisibility()
            {
                bool isCourtStat = node.Condition.Mode == NarrativeConditionMode.CourtStat;

                courtStatGroup.style.display = isCourtStat ? DisplayStyle.Flex : DisplayStyle.None;
                claimGroup.style.display = isCourtStat ? DisplayStyle.None : DisplayStyle.Flex;
            }

            modeField.RegisterValueChangedCallback(evt =>
            {
                NarrativeConditionMode newMode = (NarrativeConditionMode)evt.newValue;

                if (newMode == NarrativeConditionMode.CourtStat)
                {
                    editService.SetCourtStatCondition(
                        node,
                        node.Condition.Stat,
                        node.Condition.Operator,
                        node.Condition.Value);
                }
                else
                {
                    editService.SetClaimResolvedCondition(
                        node,
                        node.Condition.ClaimId,
                        node.Condition.RequireSuccessful);
                }

                RefreshVisibility();
            });

            RefreshVisibility();

            AddOutputPort(NarrativeOutputKeys.ConditionTrue, "True");
            AddOutputPort(NarrativeOutputKeys.ConditionFalse, "False");
        }

        private void BuildCourtStatConditionFields(ConditionNodeData node, VisualElement container)
        {
            EnumField statField = new("Stat", node.Condition.Stat);
            EnumField operatorField = new("Op", node.Condition.Operator);
            IntegerField valueField = new("Value") { value = node.Condition.Value };

            void PushCondition()
            {
                editService.SetCourtStatCondition(
                    node,
                    (CourtStat)statField.value,
                    (NarrativeComparisonOperator)operatorField.value,
                    valueField.value);
            }

            statField.RegisterValueChangedCallback(_ => PushCondition());
            operatorField.RegisterValueChangedCallback(_ => PushCondition());
            valueField.RegisterValueChangedCallback(_ => PushCondition());

            container.Add(statField);
            container.Add(operatorField);
            container.Add(valueField);
        }

        private void BuildClaimConditionFields(ConditionNodeData node, VisualElement container)
        {
            List<NarrativeOption> options = new() { new NarrativeOption(string.Empty, "<none>") };
            options.AddRange(context.Claims);

            int selectedIndex = Mathf.Max(0, options.FindIndex(o => o.Id == node.Condition.ClaimId));

            PopupField<NarrativeOption> claimPopup = new(
                options,
                selectedIndex,
                entry => entry.Label,
                entry => entry.Label)
            {
                style = { whiteSpace = WhiteSpace.Normal }
            };

            Toggle requireSuccessToggle = new("Require Successful")
            {
                value = node.Condition.RequireSuccessful
            };

            claimPopup.RegisterValueChangedCallback(evt =>
                editService.SetClaimResolvedCondition(node, evt.newValue.Id, requireSuccessToggle.value));

            requireSuccessToggle.RegisterValueChangedCallback(evt =>
                editService.SetClaimResolvedCondition(node, node.Condition.ClaimId, evt.newValue));

            container.Add(claimPopup);
            container.Add(requireSuccessToggle);
        }

        // ---------------------------------------------------------------
        // Jump
        // ---------------------------------------------------------------

        private void BuildJumpBody(JumpNodeData node)
        {
            Label hint = new("Drag from the port below to the target node.")
            {
                style = { whiteSpace = WhiteSpace.Normal, fontSize = 10, marginBottom = 4 }
            };

            extensionContainer.Add(hint);

            AddOutputPort(NarrativeOutputKeys.Next, "Target");
        }

        // ---------------------------------------------------------------
        // End
        // ---------------------------------------------------------------

        private void BuildEndBody(EndNodeData node)
        {
            List<NarrativeOption> options = new() { new NarrativeOption(string.Empty, "<no ending>") };
            options.AddRange(context.Endings);

            int selectedIndex = Mathf.Max(0, options.FindIndex(o => o.Id == node.EndingId));

            PopupField<NarrativeOption> popup = new(
                options,
                selectedIndex,
                entry => entry.Label,
                entry => entry.Label);

            popup.RegisterValueChangedCallback(evt =>
            {
                editService.SetEndingId(node, evt.newValue.Id);
            });

            extensionContainer.Add(popup);

            // Terminal node - no output port.
        }

        // ---------------------------------------------------------------
        // Gameplay
        // ---------------------------------------------------------------

        private void BuildGameplayBody(GameplayNodeData node)
        {
            TextField field = new("Event Id") { value = node.GameplayEventId };

            field.RegisterValueChangedCallback(evt =>
                editService.SetGameplayEventId(node, evt.newValue));

            extensionContainer.Add(field);

            AddOutputPort(NarrativeOutputKeys.Next, "Next");
        }

        // ---------------------------------------------------------------
        // Shared
        // ---------------------------------------------------------------

        private static string GetTitle(NarrativeNodeData data)
        {
            string kind = data switch
            {
                DialogueNodeData => "Dialogue",
                StatementNodeData => "Statement",
                ChoiceNodeData => "Choice",
                ConditionNodeData => "Condition",
                JumpNodeData => "Jump",
                EndNodeData => "End",
                GameplayNodeData => "Gameplay",
                _ => "Node"
            };

            string shortId = data.NodeId.Length <= 5 ? data.NodeId : data.NodeId[..5];

            return $"{kind} ({shortId})";
        }

        public void ApplyStyle(NodeStyle style)
        {
            currentStyle = style;

            Color border = IsStartNode ? NarrativeNodeColors.StartHighlight : style.Border;
            float width = IsStartNode ? 3 : 1;

            mainContainer.style.backgroundColor = new StyleColor(style.Background);

            mainContainer.style.borderLeftWidth = width;
            mainContainer.style.borderRightWidth = width;
            mainContainer.style.borderTopWidth = width;
            mainContainer.style.borderBottomWidth = width;

            mainContainer.style.borderLeftColor = new StyleColor(border);
            mainContainer.style.borderRightColor = new StyleColor(border);
            mainContainer.style.borderTopColor = new StyleColor(border);
            mainContainer.style.borderBottomColor = new StyleColor(border);

            titleContainer.style.backgroundColor = new StyleColor(style.Border);
            titleContainer.style.color = new StyleColor(style.Title);
        }

        public override void OnSelected()
        {
            base.OnSelected();

            if (!SuppressSelectedEvent)
            {
                Selected?.Invoke(this);
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            evt.menu.AppendAction(
                "Set as Start Node",
                _ => this.GetFirstAncestorOfType<NarrativeGraphView>()?.SetStartNode(this),
                IsStartNode
                    ? DropdownMenuAction.Status.Disabled
                    : DropdownMenuAction.Status.Normal);
        }
    }
}
