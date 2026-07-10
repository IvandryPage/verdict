using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Runtime
{
    public sealed class WitnessRuntime
    {
        private readonly Dictionary<CharacterStat, int> characterStats = new();

        public WitnessRuntime(
            WitnessData data,
            IReadOnlyList<TestimonyRuntime> testimonies)
        {
            Data = data;
            Testimonies = testimonies;

            // Initialize commonly used character stats with sensible defaults.
            characterStats[CharacterStat.Credibility] = 100;
        }

        public WitnessData Data { get; }

        public IReadOnlyList<TestimonyRuntime> Testimonies { get; }

        // Compatibility: Credibility used to be a float. Keep the property but back it
        // by the new character stat dictionary so new effects can operate on the same value.
        public float Credibility
        {
            get => characterStats.TryGetValue(CharacterStat.Credibility, out int v) ? v : 100f;
            set => characterStats[CharacterStat.Credibility] = (int)value;
        }

        public int GetCharacterStat(CharacterStat stat)
        {
            return characterStats.TryGetValue(stat, out int v) ? v : 0;
        }

        public void ModifyCharacterStat(CharacterStat stat, int delta)
        {
            if (delta == 0)
            {
                return;
            }

            int current = GetCharacterStat(stat);
            long next = (long)current + delta;
            if (next < 0)
            {
                next = 0;
            }

            characterStats[stat] = (int)next;
        }
    }
}
