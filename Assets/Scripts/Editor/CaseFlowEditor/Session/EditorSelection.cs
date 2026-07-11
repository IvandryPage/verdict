using System;

namespace Verdict.Editor.CaseFlow
{
    public sealed class EditorSelection
    {
        public event Action SelectionChanged;

        public object Selected { get; private set; }

        public void Select<T>(T value)
            where T : class
        {
            if (ReferenceEquals(Selected, value))
                return;

            Selected = value;

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
