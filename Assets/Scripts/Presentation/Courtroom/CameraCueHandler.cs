using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verdict.Data.Narrative;

namespace Verdict.Presentation.Courtroom
{
    [Serializable]
    public sealed class CameraPreset
    {
        public string Id;

        [Tooltip("Move target (straight move) or orbit pivot (when Orbit is on).")]
        public Transform Target;

        [Header("Orbit (\"keliling\")")]
        [Tooltip("If on, CameraMove circles around Target instead of moving straight to it - good for a dramatic reveal or restless witness reaction.")]
        public bool Orbit;

        [Tooltip("Degrees to sweep around the pivot over the move duration. Negative sweeps the other way.")]
        public float OrbitDegrees = 90f;

        [Tooltip("Keep the camera looking at the pivot for the whole orbit.")]
        public bool LookAtPivot = true;
    }

    /// <summary>
    /// Example IPresentationEventHandler for camera cues.
    /// CameraMove's Parameter names a preset; Value is duration in
    /// seconds. A preset with Orbit on circles the camera around its
    /// Target instead of moving straight there - that's your "move
    /// camera keliling" cue. CameraShake's Value is intensity.
    /// </summary>
    public sealed class CameraCueHandler : MonoBehaviour, IPresentationEventHandler
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private List<CameraPreset> presets = new();
        [SerializeField] private float defaultMoveDuration = 0.6f;
        [SerializeField] private float defaultOrbitDuration = 2.5f;
        [SerializeField] private float shakeDuration = 0.3f;

        private Coroutine activeRoutine;
        private Vector3 shakeBasePosition;

        public bool CanHandle(NarrativeEventType type)
        {
            return type is NarrativeEventType.CameraMove or NarrativeEventType.CameraShake;
        }

        public void Handle(NarrativeEventData eventData)
        {
            if (cameraTransform == null)
            {
                return;
            }

            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }

            switch (eventData.Type)
            {
                case NarrativeEventType.CameraMove:
                    MoveTo(eventData);
                    break;

                case NarrativeEventType.CameraShake:
                    activeRoutine = StartCoroutine(ShakeRoutine(eventData.Value > 0f ? eventData.Value : 0.2f));
                    break;
            }
        }

        private void MoveTo(NarrativeEventData eventData)
        {
            CameraPreset preset = presets.FirstOrDefault(p => p.Id == eventData.Parameter);

            if (preset?.Target == null)
            {
                Debug.LogWarning($"{nameof(CameraCueHandler)}: No camera preset registered for id '{eventData.Parameter}'.");
                return;
            }

            if (preset.Orbit)
            {
                float orbitDuration = eventData.Value > 0f ? eventData.Value : defaultOrbitDuration;
                activeRoutine = StartCoroutine(OrbitRoutine(preset, orbitDuration));
                return;
            }

            float duration = eventData.Value > 0f ? eventData.Value : defaultMoveDuration;
            activeRoutine = StartCoroutine(MoveRoutine(preset.Target, duration));
        }

        private IEnumerator MoveRoutine(Transform target, float duration)
        {
            Vector3 startPos = cameraTransform.position;
            Quaternion startRot = cameraTransform.rotation;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

                cameraTransform.position = Vector3.Lerp(startPos, target.position, t);
                cameraTransform.rotation = Quaternion.Slerp(startRot, target.rotation, t);

                yield return null;
            }

            cameraTransform.position = target.position;
            cameraTransform.rotation = target.rotation;

            activeRoutine = null;
        }

        /// <summary>
        /// Circles the camera around preset.Target ("the pivot") by
        /// preset.OrbitDegrees over `duration` seconds, keeping the
        /// camera's current distance from the pivot. This is the "move
        /// camera keliling" cue - use it for a slow reveal around a
        /// witness or the whole courtroom.
        /// </summary>
        private IEnumerator OrbitRoutine(CameraPreset preset, float duration)
        {
            Transform pivot = preset.Target;
            Vector3 offset = cameraTransform.position - pivot.position;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float currentAngle = preset.OrbitDegrees * t;

                Quaternion rotation = Quaternion.Euler(0f, currentAngle, 0f);
                cameraTransform.position = pivot.position + rotation * offset;

                if (preset.LookAtPivot)
                {
                    cameraTransform.LookAt(pivot.position);
                }

                yield return null;
            }

            Quaternion finalRotation = Quaternion.Euler(0f, preset.OrbitDegrees, 0f);
            cameraTransform.position = pivot.position + finalRotation * offset;

            if (preset.LookAtPivot)
            {
                cameraTransform.LookAt(pivot.position);
            }

            activeRoutine = null;
        }

        private IEnumerator ShakeRoutine(float intensity)
        {
            shakeBasePosition = cameraTransform.localPosition;
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float falloff = 1f - elapsed / shakeDuration;

                Vector2 offset = UnityEngine.Random.insideUnitCircle * intensity * falloff;
                cameraTransform.localPosition = shakeBasePosition + new Vector3(offset.x, offset.y, 0f);

                yield return null;
            }

            cameraTransform.localPosition = shakeBasePosition;
            activeRoutine = null;
        }
    }
}
