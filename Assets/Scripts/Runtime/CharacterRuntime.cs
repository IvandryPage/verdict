using System;
using System.Collections.Generic;
using Verdict.Data.Cases;
using Verdict.Data.Characters;

namespace Verdict.Runtime
{
    public sealed class CharacterRuntime
    {
        private readonly Dictionary<CharacterStat, int> characterStats = new();

        public CharacterRuntime(CharacterData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            InitializeDefaults();
        }

        public CharacterData Data { get; }

        public int Credibility
        {
            get => GetCharacterStat(CharacterStat.Credibility);
            set => characterStats[CharacterStat.Credibility] = value;
        }

        public int GetCharacterStat(CharacterStat stat)
        {
            return characterStats.TryGetValue(stat, out int value)
                ? value
                : 0;
        }

        public void ModifyCharacterStat(CharacterStat stat, int value, StatOperation operation = StatOperation.Add)
        {
            if (operation == StatOperation.Add && value == 0)
            {
                return;
            }

            int current = GetCharacterStat(stat);
            long next = current;

            switch (operation)
            {
                case StatOperation.Set:
                    next = value;
                    break;
                case StatOperation.Add:
                    next = (long)current + value;
                    break;
                case StatOperation.Subtract:
                    next = (long)current - value;
                    break;
                case StatOperation.Multiply:
                    next = (long)current * value;
                    break;
                case StatOperation.Divide:
                    if (value == 0)
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(value),
                            "Division by zero is not allowed for stat operations.");
                    }

                    next = current / value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(operation),
                        operation,
                        null);
            }

            if (next < 0)
            {
                next = 0;
            }

            characterStats[stat] = (int)next;
        }

        private void InitializeDefaults()
        {
            characterStats[CharacterStat.Credibility] = Data.DefaultCredibility;
            characterStats[CharacterStat.Trust] = Data.DefaultTrust;
            characterStats[CharacterStat.Stress] = Data.DefaultStress;
            characterStats[CharacterStat.Fear] = Data.DefaultFear;
            characterStats[CharacterStat.Cooperation] = Data.DefaultCooperation;
            characterStats[CharacterStat.Affinity] = Data.DefaultAffinity;
        }
    }
}
