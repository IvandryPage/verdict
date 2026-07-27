using System.Collections.Generic;
using UnityEngine;
using Verdict.Data.Narrative;
using Verdict.Systems;

namespace Verdict.Presentation.Courtroom
{
    /// <summary>
    /// Same pattern as PresentationEventRouter, for GameplayEventTriggered
    /// instead. Keeping this separate from the presentation router is
    /// deliberate - Gameplay nodes are for your own gameplay systems
    /// (minigames, unlocks, checkpoints), not camera/audio/VFX.
    /// </summary>
    public sealed class GameplayEventRouter : MonoBehaviour
    {
        [Header("Bootstrap")]
        [SerializeField] private CourtroomBootstrap bootstrap;

        [Header("Handlers")]
        [SerializeField] private List<MonoBehaviour> handlerBehaviours = new();

        private readonly List<IGameplayEventHandler> handlers = new();

        private CourtroomController controller;

        private void Awake()
        {
            if (bootstrap == null)
            {
                bootstrap = FindFirstObjectByType<CourtroomBootstrap>();
            }

            foreach (MonoBehaviour behaviour in handlerBehaviours)
            {
                if (behaviour is IGameplayEventHandler handler)
                {
                    handlers.Add(handler);
                }
                else if (behaviour != null)
                {
                    Debug.LogWarning(
                        $"{nameof(GameplayEventRouter)}: '{behaviour.name}' does not implement IGameplayEventHandler and will be ignored.",
                        behaviour);
                }
            }
        }

        private void Start()
        {
            if (bootstrap == null || bootstrap.Controller == null)
            {
                Debug.LogError($"{nameof(GameplayEventRouter)}: Missing CourtroomBootstrap or Controller.");
                enabled = false;
                return;
            }

            controller = bootstrap.Controller;
            controller.GameplayEventTriggered += HandleEvent;
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.GameplayEventTriggered -= HandleEvent;
            }
        }

        private void HandleEvent(GameplayNodeData node)
        {
            if (node == null)
            {
                return;
            }

            bool handled = false;

            foreach (IGameplayEventHandler handler in handlers)
            {
                if (handler.CanHandle(node.Category))
                {
                    handler.Handle(node);
                    handled = true;
                }
            }

            if (!handled)
            {
                Debug.Log(
                    $"{nameof(GameplayEventRouter)}: No handler registered for category '{node.Category}' " +
                    $"(EventId='{node.GameplayEventId}'). This is a hook, not an error.");
            }
        }
    }
}
