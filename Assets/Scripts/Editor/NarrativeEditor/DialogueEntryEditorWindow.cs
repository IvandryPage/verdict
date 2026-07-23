using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Verdict.Data.Characters;
using Verdict.Data.Narrative;

namespace Verdict.Editor.NarrativeEditor
{
    /// <summary>
    /// Editing a dialogue node's line/event sequence needs more room than
    /// fits inline in a GraphView node, so - like the reference template
    /// this editor is based on - it opens in its own window.
    /// </summary>
    public sealed class DialogueEntryEditorWindow : EditorWindow
    {
        private const float RowPadding = 4f;
        private const float LineHeight = 18f;

        private ReorderableList list;
        private DialogueNodeData node;
        private NarrativeEditService editService;
        private Action onChanged;
        private Vector2 scrollPosition;

        public static void Open(
            DialogueNodeData node,
            NarrativeEditService editService,
            Action onChanged)
        {
            DialogueEntryEditorWindow window = GetWindow<DialogueEntryEditorWindow>();
            window.titleContent = new GUIContent("Dialogue Entries");
            window.minSize = new Vector2(420, 320);
            window.Bind(node, editService, onChanged);
            window.Show();
        }

        private void Bind(
            DialogueNodeData targetNode,
            NarrativeEditService service,
            Action changedCallback)
        {
            node = targetNode;
            editService = service;
            onChanged = changedCallback;

            BuildList();
        }

        private void BuildList()
        {
            List<NarrativeDialogueEntryData> entries = new(node.Entries);

            list = new ReorderableList(entries, typeof(NarrativeDialogueEntryData), true, true, true, true)
            {
                drawHeaderCallback = rect =>
                    EditorGUI.LabelField(rect, "Entries (played in order)"),

                drawElementCallback = (rect, index, _, _) =>
                {
                    EditorGUI.BeginChangeCheck();

                    DrawEntry(rect, entries[index]);

                    if (EditorGUI.EndChangeCheck())
                    {
                        editService.RecordDialogueEntryEdit();
                        onChanged?.Invoke();
                    }
                },

                elementHeightCallback = index => GetEntryHeight(entries[index])
            };

            list.onAddCallback = _ =>
            {
                var entry = new NarrativeDialogueEntryData
                {
                    Type = NarrativeDialogueEntryType.Line,
                    Line = new NarrativeLineData()
                };

                editService.AddDialogueEntry(node, entry);
                RefreshFromNode();
            };

            list.onRemoveCallback = reorderableList =>
            {
                editService.RemoveDialogueEntryAt(node, reorderableList.index);
                RefreshFromNode();
            };

            list.onReorderCallbackWithDetails = (_, oldIndex, newIndex) =>
            {
                editService.MoveDialogueEntry(node, oldIndex, newIndex);
                RefreshFromNode();
            };
        }

        private void RefreshFromNode()
        {
            BuildList();
            onChanged?.Invoke();
            Repaint();
        }

        private void OnGUI()
        {
            if (node == null)
            {
                EditorGUILayout.HelpBox("No dialogue node selected.", MessageType.Info);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            list?.DoLayoutList();
            EditorGUILayout.EndScrollView();
        }

        private static float GetEntryHeight(NarrativeDialogueEntryData entry)
        {
            return entry.Type == NarrativeDialogueEntryType.Line
                ? LineHeight * 9 + RowPadding * 7
                : LineHeight * 4 + RowPadding * 4;
        }

        private static void DrawEntry(Rect rect, NarrativeDialogueEntryData entry)
        {
            float y = rect.y + RowPadding;
            float width = rect.width;

            entry.Type = (NarrativeDialogueEntryType)EditorGUI.EnumPopup(
                new Rect(rect.x, y, width, LineHeight), "Type", entry.Type);
            y += LineHeight + RowPadding;

            if (entry.Type == NarrativeDialogueEntryType.Line)
            {
                entry.Line ??= new NarrativeLineData();

                entry.Line.SpeakerType = (NarrativeSpeakerType)EditorGUI.EnumPopup(
                    new Rect(rect.x, y, width, LineHeight), "Speaker Type", entry.Line.SpeakerType);
                y += LineHeight + RowPadding;

                entry.Line.Speaker = (CharacterData)EditorGUI.ObjectField(
                    new Rect(rect.x, y, width, LineHeight), "Character", entry.Line.Speaker, typeof(CharacterData), false);
                y += LineHeight + RowPadding;

                entry.Line.Emotion = (CharacterEmotion)EditorGUI.EnumPopup(
                    new Rect(rect.x, y, width, LineHeight), "Emotion", entry.Line.Emotion);
                y += LineHeight + RowPadding;

                entry.Line.WaitMode = (NarrativeWaitMode)EditorGUI.EnumPopup(
                    new Rect(rect.x, y, width, LineHeight), "Wait Mode", entry.Line.WaitMode);
                y += LineHeight + RowPadding;

                if (entry.Line.WaitMode == NarrativeWaitMode.Auto)
                {
                    entry.Line.AutoAdvanceDelay = EditorGUI.FloatField(
                        new Rect(rect.x, y, width, LineHeight), "Auto Delay (s)", entry.Line.AutoAdvanceDelay);
                    y += LineHeight + RowPadding;
                }

                EditorGUI.LabelField(new Rect(rect.x, y, width, LineHeight), "Text");
                y += LineHeight;

                entry.Line.Text = EditorGUI.TextArea(
                    new Rect(rect.x, y, width, LineHeight * 2), entry.Line.Text);
            }
            else
            {
                entry.Event ??= new NarrativeEventData();

                entry.Event.Type = (NarrativeEventType)EditorGUI.EnumPopup(
                    new Rect(rect.x, y, width, LineHeight), "Event Type", entry.Event.Type);
                y += LineHeight + RowPadding;

                entry.Event.Parameter = EditorGUI.TextField(
                    new Rect(rect.x, y, width, LineHeight), GetParameterLabel(entry.Event.Type), entry.Event.Parameter);
                y += LineHeight + RowPadding;

                entry.Event.Value = EditorGUI.FloatField(
                    new Rect(rect.x, y, width, LineHeight), GetValueLabel(entry.Event.Type), entry.Event.Value);
            }
        }

        private static string GetParameterLabel(NarrativeEventType type)
        {
            return type switch
            {
                NarrativeEventType.PlayMusic => "Track Id",
                NarrativeEventType.StopMusic => "Track Id (optional)",
                NarrativeEventType.PlaySound => "SFX Clip Id",
                NarrativeEventType.CameraMove => "Camera Preset",
                NarrativeEventType.CameraShake => "Shake Preset",
                NarrativeEventType.ScreenFade => "Fade Color (e.g. black)",
                NarrativeEventType.ChangeBackground => "Background Id",
                _ => "Parameter"
            };
        }

        private static string GetValueLabel(NarrativeEventType type)
        {
            return type switch
            {
                NarrativeEventType.PlayMusic => "Volume (0-1)",
                NarrativeEventType.CameraMove => "Duration (s)",
                NarrativeEventType.CameraShake => "Intensity",
                NarrativeEventType.ScreenFade => "Duration (s)",
                _ => "Value"
            };
        }

        private void OnDisable()
        {
            if (node != null)
            {
                editService?.RecordDialogueEntryEdit();
            }
        }
    }
}
