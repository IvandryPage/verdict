using UnityEngine;

namespace Verdict.Data.Presentation
{
    public enum CourtroomCameraCue
    {
        Default,
        Statement,
        Witness,
        Evidence,
        Reaction,
        Verdict,
        Reset,
    }

    [CreateAssetMenu(fileName = "CourtroomCameraShot_", menuName = "Verdict/Presentation/Courtroom Camera Shot")]
    public sealed class CourtroomCameraShotData : ScriptableObject
    {
        [SerializeField]
        private string id = "shot_default";

        [SerializeField]
        private string label = "Default Shot";

        [SerializeField]
        private CourtroomCameraCue cue = CourtroomCameraCue.Statement;

        [SerializeField]
        private float duration = 2.5f;

        [SerializeField]
        private float blendIn = 0.35f;

        [SerializeField]
        private float blendOut = 0.2f;

        [SerializeField]
        private int priority = 10;

        [SerializeField]
        private Transform shotAnchor;

        [SerializeField]
        private Transform lookAtTarget;

        [SerializeField]
        private CourtroomCameraShotTarget shotTarget;

        [SerializeField]
        private bool useLookAtTarget = true;

        public string Id => id;

        public string Label => label;

        public CourtroomCameraCue Cue => cue;

        public float Duration => duration;

        public float BlendIn => blendIn;

        public float BlendOut => blendOut;

        public int Priority => priority;

        public Transform ShotAnchor => shotAnchor;

        public Transform LookAtTarget => lookAtTarget;

        public CourtroomCameraShotTarget ShotTarget => shotTarget;

        public bool UseLookAtTarget => useLookAtTarget;
    }
}
