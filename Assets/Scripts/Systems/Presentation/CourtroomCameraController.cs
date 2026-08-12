using System;
using Verdict.Data.Narrative;
using Verdict.Data.Presentation;
using Verdict.Runtime;
using Verdict.Systems;
using Verdict.Systems.Evaluation;

namespace Verdict.Systems.Presentation
{
    public readonly struct CourtroomCameraRequest
    {
        public CourtroomCameraRequest(
            CourtroomCameraCue cue,
            string speakerId,
            NarrativeSpeakerType speakerType)
        {
            Cue = cue;
            SpeakerId = speakerId;
            SpeakerType = speakerType;
        }

        public CourtroomCameraCue Cue { get; }

        public string SpeakerId { get; }

        public NarrativeSpeakerType SpeakerType { get; }
    }

    /// <summary>
    /// Presentation-oriented camera decision layer.
    /// It chooses camera requests from narrative speaker context and
    /// presentation moments instead of raw state labels.
    /// </summary>
    public sealed class CourtroomCameraController
    {
        private readonly CourtroomController courtroomController;

        public event Action<CourtroomCameraRequest> CameraRequested;

        public CourtroomCameraController(
            CourtroomController courtroomController)
        {
            this.courtroomController = courtroomController ??
                throw new ArgumentNullException(nameof(courtroomController));
        }

        public void Bind()
        {
            if (courtroomController == null)
            {
                return;
            }

            courtroomController.NarrativeEntryChanged += HandleNarrativeEntryChanged;
            courtroomController.CurrentStatementChanged += HandleCurrentStatementChanged;
            courtroomController.CourtroomStateChanged += HandleCourtroomStateChanged;
            courtroomController.ArgumentResolved += HandleArgumentResolved;

            Trigger(new CourtroomCameraRequest(
                CourtroomCameraCue.Default,
                null,
                NarrativeSpeakerType.System));
        }

        public void Unbind()
        {
            if (courtroomController == null)
            {
                return;
            }

            courtroomController.NarrativeEntryChanged -= HandleNarrativeEntryChanged;
            courtroomController.CurrentStatementChanged -= HandleCurrentStatementChanged;
            courtroomController.CourtroomStateChanged -= HandleCourtroomStateChanged;
            courtroomController.ArgumentResolved -= HandleArgumentResolved;
        }

        public void Trigger(CourtroomCameraRequest request)
        {
            CameraRequested?.Invoke(request);
        }

        private void HandleNarrativeEntryChanged(NarrativeDialogueEntryData entry)
        {
            if (entry?.Line == null)
            {
                Trigger(new CourtroomCameraRequest(
                    CourtroomCameraCue.Default,
                    null,
                    NarrativeSpeakerType.System));
                return;
            }

            if (entry.Line.SpeakerType == NarrativeSpeakerType.Character &&
                entry.Line.Speaker != null)
            {
                Trigger(new CourtroomCameraRequest(
                    CourtroomCameraCue.Statement,
                    entry.Line.Speaker.Id,
                    entry.Line.SpeakerType));
                return;
            }

            if (entry.Line.SpeakerType == NarrativeSpeakerType.Narrator)
            {
                Trigger(new CourtroomCameraRequest(
                    CourtroomCameraCue.Default,
                    null,
                    NarrativeSpeakerType.Narrator));
                return;
            }

            Trigger(new CourtroomCameraRequest(
                CourtroomCameraCue.Default,
                null,
                entry.Line.SpeakerType));
        }

        private void HandleCurrentStatementChanged(StatementRuntime statement)
        {
            if (courtroomController.CurrentNarrativeEntry != null)
            {
                return;
            }

            Trigger(new CourtroomCameraRequest(
                CourtroomCameraCue.Statement,
                courtroomController.CurrentWitness?.Character?.Data?.Id,
                NarrativeSpeakerType.Character));
        }

        private void HandleCourtroomStateChanged(CourtroomState state)
        {
            switch (state)
            {
                case CourtroomState.EvidenceSelection:
                case CourtroomState.EvidenceInspection:
                    Trigger(new CourtroomCameraRequest(
                        CourtroomCameraCue.Evidence,
                        null,
                        NarrativeSpeakerType.System));
                    break;

                case CourtroomState.Result:
                    Trigger(new CourtroomCameraRequest(
                        CourtroomCameraCue.Verdict,
                        null,
                        NarrativeSpeakerType.System));
                    break;

                default:
                    Trigger(new CourtroomCameraRequest(
                        CourtroomCameraCue.Default,
                        null,
                        NarrativeSpeakerType.System));
                    break;
            }
        }

        private void HandleArgumentResolved(ResolverResult result)
        {
            if (result == null)
            {
                return;
            }

            Trigger(new CourtroomCameraRequest(
                result.IsSuccess
                    ? CourtroomCameraCue.Reaction
                    : CourtroomCameraCue.Witness,
                courtroomController.CurrentWitness?.Character?.Data?.Id,
                NarrativeSpeakerType.Character));
        }
    }
}
