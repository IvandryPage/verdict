namespace Verdict.Data.Narrative
{
    /// <summary>
    /// Pure audio-visual/presentation cues fired while a DialogueNodeData
    /// plays - camera, music, sound, screen effects. Nothing here is
    /// related to player interaction or gameplay state; that's what
    /// StatementNodeData/ChoiceNodeData/GameplayNodeData are for.
    /// </summary>
    public enum NarrativeEventType
    {
        None,

        PlayMusic,

        StopMusic,

        PlaySound,

        CameraMove,

        CameraShake,

        ScreenFade,

        ChangeBackground
    }
}
