using System;
using UnityEngine;

namespace Verdict.Data.Presentation
{
    public enum CourtroomCameraShotTargetType
    {
        CurrentWitness,
        CurrentEvidence,
        Judge,
        Defense,
        Prosecutor,
        Audience,
        Custom,
    }

    [Serializable]
    public sealed class CourtroomCameraShotTarget
    {
        [SerializeField]
        private string id = "shot_target";

        [SerializeField]
        private Transform target;

        [SerializeField]
        private CourtroomCameraShotTargetType targetType = CourtroomCameraShotTargetType.Custom;

        [SerializeField]
        private Vector3 offset = Vector3.zero;

        public string Id => id;

        public Transform Target => target;

        public CourtroomCameraShotTargetType TargetType => targetType;

        public Vector3 Offset => offset;

        public void SetTarget(Transform value)
        {
            target = value;
        }
    }
}
