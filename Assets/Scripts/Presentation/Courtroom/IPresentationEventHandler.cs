using Verdict.Data.Narrative;

namespace Verdict.Presentation.Courtroom
{
    /// <summary>
    /// Implement this on any MonoBehaviour to react to a Dialogue node's
    /// Event entries (camera, music, sound, screen effects). Register
    /// your handler on PresentationEventRouter - adding a new kind of
    /// cue never requires touching CourtroomController, NarrativeRunner,
    /// or any other engine-core file.
    /// </summary>
    public interface IPresentationEventHandler
    {
        bool CanHandle(NarrativeEventType type);

        void Handle(NarrativeEventData eventData);
    }
}
