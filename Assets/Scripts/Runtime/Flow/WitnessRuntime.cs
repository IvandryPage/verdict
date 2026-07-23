using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Runtime
{
    public sealed class WitnessRuntime
    {
        public WitnessRuntime(
            WitnessData data,
            IReadOnlyList<TestimonyRuntime> testimonies,
            CharacterRuntime characterRuntime)
        {
            Data = data;
            Testimonies = testimonies;
            Character = characterRuntime;
        }

        public WitnessData Data { get; }

        public IReadOnlyList<TestimonyRuntime> Testimonies { get; }

        public CharacterRuntime Character { get; }

        public bool IsVisible { get; set; }

        // Compatibility: Credibility remains available at the witness level.
        public float Credibility
        {
            get => Character.Credibility;
            set => Character.Credibility = (int)value;
        }

        public int GetCharacterStat(CharacterStat stat)
        {
            return Character.GetCharacterStat(stat);
        }

        public void ModifyCharacterStat(CharacterStat stat, int value, StatOperation operation = StatOperation.Add)
        {
            Character.ModifyCharacterStat(stat, value, operation);
        }
    }
}
