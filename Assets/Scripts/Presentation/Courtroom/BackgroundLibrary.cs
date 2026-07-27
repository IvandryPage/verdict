using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verdict.Presentation.Courtroom
{
    [Serializable]
    public sealed class BackgroundEntry
    {
        public string Id;
        public Sprite Background;
    }

    [CreateAssetMenu(fileName = "BackgroundLibrary", menuName = "Verdict/Presentation/Background Library")]
    public sealed class BackgroundLibrary : ScriptableObject
    {
        [SerializeField] private List<BackgroundEntry> entries = new();

        public bool TryGetBackground(string id, out Sprite background)
        {
            BackgroundEntry entry = entries.FirstOrDefault(e => e.Id == id);
            background = entry?.Background;
            return background != null;
        }
    }
}
