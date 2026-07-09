using System;
using UnityEngine;

namespace Verdict.Data.Cases
{
    [Serializable]
    public sealed class CourtStateEffectData
    {
        [SerializeField] private CourtStateEffect effect;

        [Min(0)]
        [SerializeField] private int value;

        [Tooltip("Optional target identifier (Statement ID, Testimony ID, Ending ID, etc.).")]
        [SerializeField] private string targetId;

        public CourtStateEffect Effect => effect;

        public int Value => value;

        public string TargetId => targetId;
    }
}
