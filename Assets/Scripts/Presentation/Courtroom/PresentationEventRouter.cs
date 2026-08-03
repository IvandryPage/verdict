using System.Collections.Generic;
using UnityEngine;
using Verdict.Data.Narrative;
using Verdict.Systems;

namespace Verdict.Presentation.Courtroom
{
    /// <summary>
    /// Subscribes once to CourtroomController.PresentationEventTriggered
    /// and fans it out to every registered IPresentationEventHandler that
    /// claims the event's type. To add a new kind of cue (say, a
    /// timeline trigger), write a new MonoBehaviour implementing
    /// IPresentationEventHandler, drop it in the handlers list - no
    /// changes needed anywhere in Systems/Runtime/Data.
    /// </summary>
    public sealed class PresentationEventRouter : MonoBehaviour
    {
        [Header("Bootstrap")]
        [SerializeField] private CourtroomBootstrap bootstrap;

        [Header("Handlers")]
        [Tooltip("Any MonoBehaviour implementing IPresentationEventHandler.")]
        [SerializeField] private List<MonoBehaviour> handlerBehaviours = new();

        private readonly List<IPresentationEventHandler> handlers = new();

        private CourtroomController controller;

        private void Awake()
        {
            if (bootstrap == null)
            {
                bootstrap = FindFirstObjectByType<CourtroomBootstrap>();
            }

            foreach (MonoBehaviour behaviour in handlerBehaviours)
            {
                if (behaviour is IPresentationEventHandler handler)
                {
                    handlers.Add(handler);
                }
                else if (behaviour != null)
                {
                    Debug.LogWarning(
                        $"{nameof(PresentationEventRouter)}: '{behaviour.name}' does not implement IPresentationEventHandler and will be ignored.",
                        behaviour);
                }
            }
        }

        private void Start()
        {
            if (bootstrap == null || bootstrap.Controller == null)
            {
                Debug.LogError($"{nameof(PresentationEventRouter)}: Missing CourtroomBootstrap or Controller.");
                enabled = false;
                return;
            }

            controller = bootstrap.Controller;
            controller.PresentationEventTriggered += HandleEvent;
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.PresentationEventTriggered -= HandleEvent;
            }
        }

        private void HandleEvent(NarrativeEventData eventData)
        {
            if (eventData == null || eventData.Type == NarrativeEventType.None)
            {
                return;
            }

            bool handled = false;

            foreach (IPresentationEventHandler handler in handlers)
            {
                if (handler.CanHandle(eventData.Type))
                {
                    handler.Handle(eventData);
                    handled = true;
                }
            }

            if (!handled)
            {
                Debug.Log(
                    $"{nameof(PresentationEventRouter)}: No handler registered for '{eventData.Type}' " +
                    $"(Parameter='{eventData.Parameter}', Value={eventData.Value}). This is a hook, not an error.");
            }
        }
    }
}
