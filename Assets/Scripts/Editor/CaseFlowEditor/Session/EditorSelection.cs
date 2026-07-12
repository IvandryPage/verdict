using System;
using UnityEngine;

namespace Verdict.Editor.CaseFlow
{
    public sealed class EditorSelection
    {
        public event Action SelectionChanged;

        public object Selected { get; private set; }

        public void Select<T>(T value)
            where T : class
        {
            Debug.Log($"Selecting {value}");
            if (ReferenceEquals(Selected, value))
                return;

            Selected = value;
            Debug.Log("Selected changed!");
            SelectionChanged?.Invoke();
        }

        public T Get<T>()
            where T : class
        {
            return Selected as T;
        }

        public void Clear()
        {
            Selected = null;

            SelectionChanged?.Invoke();
        }
    }
}
