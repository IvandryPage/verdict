using System;
using UnityEngine;
using Verdict.Data.Narrative;
using Verdict.Data.Presentation;
using Verdict.Systems.Presentation;
using Verdict.UI.Narrative;

namespace Verdict.Systems
{
    public sealed class CourtroomEventResolver
    {
        private readonly CourtroomController courtroomController;
        private readonly CourtroomCameraController courtroomCameraController;
        private readonly global::Verdict.NarrativeAudioController narrativeAudioController;
        private readonly ChapterPresenter chapterPresenter;
        private readonly ScreenFadePresenter screenFadePresenter;

        public event Action<NarrativeEventData> PresentationEventReceived;
        public event Action<GameplayNodeData> GameplayEventReceived;

        public CourtroomEventResolver(
            CourtroomController courtroomController,
            CourtroomCameraController courtroomCameraController = null,
            global::Verdict.NarrativeAudioController narrativeAudioController = null,
            ChapterPresenter chapterPresenter = null,
            ScreenFadePresenter screenFadePresenter = null)
        {
            this.courtroomController = courtroomController ??
                throw new ArgumentNullException(nameof(courtroomController));

            this.courtroomCameraController = courtroomCameraController;
            this.narrativeAudioController = narrativeAudioController;
            this.chapterPresenter = chapterPresenter;
            this.screenFadePresenter = screenFadePresenter;
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
                    screenFadePresenter?.Trigger(
                        eventData.Parameter,
                        eventData.Value);
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

            string originalEventId = node.GameplayEventId.Trim();
            string payload = originalEventId.ToLowerInvariant();

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
                    if (TryParseChapterEventId(
                        payload,
                        originalEventId,
                        out string chapterTitle))
                    {
                        chapterPresenter?.ShowChapter(chapterTitle);
                        return;
                    }

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

        private static bool TryParseChapterEventId(
            string payload,
            string originalEventId,
            out string chapterTitle)
        {
            chapterTitle = null;

            if (string.IsNullOrWhiteSpace(payload) ||
                string.IsNullOrWhiteSpace(originalEventId))
            {
                return false;
            }

            const string chapterPrefix = "chapter:";
            const string chapterLabelPrefix = "chapter ";
            const string chapterUnderscorePrefix = "chapter_";

            string trimmed = payload.Trim();
            string originalTrimmed = originalEventId.Trim();

            if (trimmed.StartsWith(chapterPrefix, StringComparison.Ordinal))
            {
                chapterTitle = originalTrimmed.Substring(chapterPrefix.Length).Trim();
                return !string.IsNullOrWhiteSpace(chapterTitle);
            }

            if (trimmed.StartsWith(chapterLabelPrefix, StringComparison.Ordinal))
            {
                chapterTitle = originalTrimmed.Substring(chapterLabelPrefix.Length).Trim();
                return !string.IsNullOrWhiteSpace(chapterTitle);
            }

            if (trimmed.StartsWith(chapterUnderscorePrefix, StringComparison.Ordinal))
            {
                int separatorIndex = originalTrimmed.IndexOf(':');
                if (separatorIndex >= 0)
                {
                    chapterTitle = originalTrimmed.Substring(separatorIndex + 1).Trim();
                    return !string.IsNullOrWhiteSpace(chapterTitle);
                }

                chapterTitle = originalTrimmed.Substring(chapterUnderscorePrefix.Length).Trim();
                return !string.IsNullOrWhiteSpace(chapterTitle);
            }

            return false;
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
