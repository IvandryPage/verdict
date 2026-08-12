using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verdict.Systems.Presentation;
using Verdict.Data.Presentation;

namespace Verdict.Presentation
{
    /// <summary>
    /// Unity-facing adapter that activates a camera GameObject from a list.
    /// It prioritizes narrative speaker context, then falls back to cue-based
    /// presentation shots.
    /// </summary>
    public sealed class CourtroomCameraRig : MonoBehaviour
    {
        [Serializable]
        private sealed class CameraEntry
        {
            public CourtroomCameraCue cue = CourtroomCameraCue.Default;
            public string speakerId;
            public GameObject cameraObject;
        }

        [Header("Controller")]
        [SerializeField]
        private CourtroomCameraController cameraController;

        [Header("Cameras")]
        [SerializeField]
        private List<CameraEntry> cameraEntries = new();

        [SerializeField]
        private bool autoBind = true;

        private void Awake()
        {
            if (autoBind)
            {
                Bind(cameraController);
            }
        }

        private void OnDestroy()
        {
            if (cameraController != null)
            {
                cameraController.CameraRequested -= HandleCameraRequested;
            }
        }

        public void Bind(CourtroomCameraController controller)
        {
            if (cameraController != null)
            {
                cameraController.CameraRequested -= HandleCameraRequested;
            }

            cameraController = controller;

            if (cameraController == null)
            {
                return;
            }

            cameraController.CameraRequested += HandleCameraRequested;
            ApplyRequest(new CourtroomCameraRequest(CourtroomCameraCue.Default, null, default));
        }

        private void HandleCameraRequested(CourtroomCameraRequest request)
        {
            ApplyRequest(request);
        }

        private void ApplyRequest(CourtroomCameraRequest request)
        {
            if (cameraEntries == null || cameraEntries.Count == 0)
            {
                return;
            }

            CameraEntry selected = null;

            if (!string.IsNullOrWhiteSpace(request.SpeakerId))
            {
                selected = cameraEntries
                    .FirstOrDefault(entry => entry != null && entry.cameraObject != null &&
                                             !string.IsNullOrWhiteSpace(entry.speakerId) &&
                                             entry.speakerId == request.SpeakerId);
            }

            if (selected == null)
            {
                selected = cameraEntries
                    .FirstOrDefault(entry => entry != null && entry.cameraObject != null && entry.cue == request.Cue);
            }

            if (selected == null)
            {
                selected = cameraEntries
                    .FirstOrDefault(entry => entry != null && entry.cameraObject != null && entry.cue == CourtroomCameraCue.Default);
            }

            if (selected == null)
            {
                selected = cameraEntries
                    .FirstOrDefault(entry => entry != null && entry.cameraObject != null);
            }

            if (selected == null)
            {
                return;
            }

            foreach (CameraEntry entry in cameraEntries)
            {
                if (entry == null || entry.cameraObject == null)
                {
                    continue;
                }

                entry.cameraObject.SetActive(entry == selected);
            }
        }
    }
}
