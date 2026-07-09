using System;
using UnityEngine;

namespace Verdict.Data.Cases
{
    [Serializable]
    public class FactOperand
    {
        [SerializeField] private FactOperandType type;
        [SerializeField] private string value;

        public FactOperandType Type => type;
        public string Value => value;
    }
}
