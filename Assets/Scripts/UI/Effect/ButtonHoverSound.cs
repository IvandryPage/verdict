using UnityEngine;
using UnityEngine.EventSystems;
using Verdict;

public sealed class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    [Header("Audio")]
    [SerializeField] private NarrativeAudioController audioController;

    [SerializeField] private string soundId = "ui_hover";

    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.7f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (audioController == null)
        {
            return;
        }

        audioController.PlaySound(soundId, volume);
    }
}
