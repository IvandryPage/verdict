using System;
using UnityEngine;
using Verdict.Data.Narrative;
using Verdict.Data.Presentation;
using Verdict.Systems.Presentation;

namespace Verdict.Systems
{
    public sealed class CourtroomEventResolver
    {
        private readonly CourtroomController courtroomController;
        private readonly CourtroomCameraController courtroomCameraController;
        private readonly global::Verdict.NarrativeAudioController narrativeAudioController;

        public event Action<NarrativeEventData> PresentationEventReceived;
        public event Action<GameplayNodeData> GameplayEventReceived;

        public CourtroomEventResolver(
            CourtroomController courtroomController,
            CourtroomCameraController courtroomCameraController = null,
            global::Verdict.NarrativeAudioController narrativeAudioController = null)
        {
            this.courtroomController = courtroomController ??
                throw new ArgumentNullException(nameof(courtroomController));

            this.courtroomCameraController = courtroomCameraController;
            this.narrativeAudioController = narrativeAudioController;
        }

        public void Bind()
        {
            courtroomController.PresentationEventTriggered +=
                HandlePresentationEvent;

            courtroomController.GameplayEventTriggered +=
                HandleGameplayNodeReached;
        }

        public void Unbind()
        {
            courtroomController.PresentationEventTriggered -=
                HandlePresentationEvent;

            courtroomController.GameplayEventTriggered -=
                HandleGameplayNodeReached;
        }

        private void HandlePresentationEvent(
            NarrativeEventData eventData)
        {
            if (eventData == null)
            {
                return;
            }

            ResolvePresentationEvent(eventData);
            PresentationEventReceived?.Invoke(eventData);
        }

        private void HandleGameplayNodeReached(
            GameplayNodeData node)
        {
            if (node == null)
            {
                return;
            }

            ResolveGameplayEvent(node);
            GameplayEventReceived?.Invoke(node);
        }

        private void ResolvePresentationEvent(
            NarrativeEventData eventData)
        {
            switch (eventData.Type)
            {
                case NarrativeEventType.CameraMove:
                    if (TryParseCameraCue(eventData.Parameter,
                        out CourtroomCameraCue cue))
                    {
                        courtroomCameraController?.Trigger(
                            new CourtroomCameraRequest(
                                cue,
                                null,
                                NarrativeSpeakerType.System));
                    }
                    else
                    {
                        Debug.Log(
                            $"[EventResolver] Unknown camera cue: {eventData.Parameter}");
                    }
                    break;

                case NarrativeEventType.CameraShake:
                    courtroomCameraController?.Trigger(
                        new CourtroomCameraRequest(
                            CourtroomCameraCue.Reaction,
                            null,
                            NarrativeSpeakerType.System));
                    break;

                case NarrativeEventType.PlayMusic:
                    if (narrativeAudioController != null)
                    {
                        narrativeAudioController.PlayMusic(
                            eventData.Parameter,
                            Mathf.Clamp01(eventData.Value));
                    }
                    else
                    {
                        Debug.Log(
                            $"[EventResolver] Play music: {eventData.Parameter}");
                    }
                    break;

                case NarrativeEventType.StopMusic:
                    if (narrativeAudioController != null)
                    {
                        narrativeAudioController.StopMusic(
                            eventData.Parameter);
                    }
                    else
                    {
                        Debug.Log(
                            $"[EventResolver] Stop music: {eventData.Parameter}");
                    }
                    break;

                case NarrativeEventType.PlaySound:
                    if (narrativeAudioController != null)
                    {
                        narrativeAudioController.PlaySound(
                            eventData.Parameter,
                            Mathf.Clamp01(eventData.Value));
                    }
                    else
                    {
                        Debug.Log(
                            $"[EventResolver] Play sound: {eventData.Parameter}");
                    }
                    break;

                case NarrativeEventType.ScreenFade:
                    Debug.Log(
                        $"[EventResolver] Screen fade: {eventData.Parameter} ({eventData.Value})");
                    break;

                case NarrativeEventType.ChangeBackground:
                    Debug.Log(
                        $"[EventResolver] Change background: {eventData.Parameter}");
                    break;

                case NarrativeEventType.None:
                default:
                    break;
            }
        }

        private void ResolveGameplayEvent(
            GameplayNodeData node)
        {
            if (string.IsNullOrWhiteSpace(node.GameplayEventId))
            {
                return;
            }

            string payload = node.GameplayEventId.Trim().ToLowerInvariant();

            switch (payload)
            {
                case "press":
                    if (courtroomController.CanInteract)
                    {
                        courtroomController.BeginPress();
                    }
                    break;

                case "question":
                    if (courtroomController.CanInteract)
                    {
                        courtroomController.BeginQuestion();
                    }
                    break;

                case "presentevidence":
                case "open evidence":
                case "begin presentevidence":
                case "begin evidence":
                    if (courtroomController.CanInteract)
                    {
                        courtroomController.BeginPresentEvidence();
                    }
                    break;

                case "bluff":
                    if (courtroomController.CanInteract)
                    {
                        courtroomController.BeginBluff();
                    }
                    break;

                case "threaten":
                    if (courtroomController.CanInteract)
                    {
                        courtroomController.BeginThreaten();
                    }
                    break;

                case "object":
                    if (courtroomController.CanInteract)
                    {
                        courtroomController.BeginObject();
                    }
                    break;

                case "interrupt":
                    if (courtroomController.CanInteract)
                    {
                        courtroomController.BeginInterrupt();
                    }
                    break;

                case "compareevidence":
                case "compare evidence":
                    if (courtroomController.CanInteract)
                    {
                        courtroomController.BeginCompareEvidence();
                    }
                    break;

                case "reviewevidence":
                case "review evidence":
                    courtroomController.ReviewSelectedEvidence();
                    break;

                default:
                    if (node.Category == GameplayEventCategory.UnlockFeature)
                    {
                        if (payload == "unlock evidence" || payload == "unlockevidence")
                        {
                            if (courtroomController.CanInteract)
                            {
                                courtroomController.BeginEvidenceSelection();
                            }

                            return;
                        }

                        if (TryParseUnlockEvidenceEventId(
                            payload,
                            node.GameplayEventId,
                            out string evidenceId))
                        {
                            courtroomController.UnlockEvidence(evidenceId);
                            return;
                        }
                    }

                    Debug.Log(
                        $"[EventResolver] Gameplay event not mapped: {node.GameplayEventId}");
                    break;
            }
        }

        private static bool TryParseUnlockEvidenceEventId(
            string payload,
            string originalEventId,
            out string evidenceId)
        {
            evidenceId = null;

            if (string.IsNullOrWhiteSpace(payload) ||
                string.IsNullOrWhiteSpace(originalEventId))
            {
                return false;
            }

            const string underscorePrefix = "unlock_evidence:";
            const string spacePrefix = "unlock evidence ";
            const string noSpacePrefix = "unlockevidence ";

            int prefixLength = -1;

            if (payload.StartsWith(underscorePrefix))
            {
                prefixLength = underscorePrefix.Length;
            }
            else if (payload.StartsWith(spacePrefix))
            {
                prefixLength = spacePrefix.Length;
            }
            else if (payload.StartsWith(noSpacePrefix))
            {
                prefixLength = noSpacePrefix.Length;
            }

            if (prefixLength < 0)
            {
                return false;
            }

            evidenceId = originalEventId.Substring(prefixLength).Trim();
            return !string.IsNullOrWhiteSpace(evidenceId);
        }

        private static bool TryParseCameraCue(
            string parameter,
            out CourtroomCameraCue cue)
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                cue = CourtroomCameraCue.Default;
                return false;
            }

            return Enum.TryParse(
                parameter,
                true,
                out cue);
        }
    }
}
