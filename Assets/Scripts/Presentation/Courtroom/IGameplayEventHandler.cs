using Verdict.Data.Narrative;

namespace Verdict.Presentation.Courtroom
{
    /// <summary>
    /// Implement this on any MonoBehaviour to react to a Gameplay node
    /// (unlock a feature, start a minigame, mark a checkpoint - whatever
    /// your project defines). Register on GameplayEventRouter.
    /// </summary>
    public interface IGameplayEventHandler
    {
        bool CanHandle(GameplayEventCategory category);

        void Handle(GameplayNodeData node);
    }
}
