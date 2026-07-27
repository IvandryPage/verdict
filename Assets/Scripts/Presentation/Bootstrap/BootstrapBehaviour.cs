using UnityEngine;
using UnityEngine.SceneManagement;

namespace Verdict.Presentation.Bootstrap
{
    /// <summary>
    /// Entry point of the application.
    /// Responsible for initializing global systems
    /// before loading the first playable scene.
    /// </summary>
    public sealed class BootstrapBehaviour : MonoBehaviour
    {
        [Header("Startup")]
        [SerializeField]
        private string firstScene = "01_MainMenu";

        private async void Start()
        {
            DontDestroyOnLoad(gameObject);

            await Initialize();

            await LoadFirstScene();
        }

        private async Awaitable Initialize()
        {
            Debug.Log("———————Bootstrap—————————");
            // GameSettings reads straight from PlayerPrefs on first
            // access, so there's no explicit "load" step for it.
            // TODO
            // Initialize Input
            // Initialize Audio
            // Initialize Save System

            await Awaitable.NextFrameAsync();
        }

        private async Awaitable LoadFirstScene()
        {
            Debug.Log($"Loading Scene : {firstScene}");

            AsyncOperation operation =
                SceneManager.LoadSceneAsync(firstScene);

            while (!operation.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
        }
    }
}
