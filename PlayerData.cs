using DungeonGame.Stats;
using System;
using System.Collections.Generic;
using System.Text;

namespace DungeonGame
{
    public class PlayerData
    {
        public int PlayerHealth { get; set; } = 150; // default
        public int MaxPlayerHealth { get; set; } = 200;
        public int DungeonLevel { get; set; } = 1;
        public int HealingPotionAmount { get; set; } = 5;
        public Dictionary<string, WeaponUseAndLevel> WeaponUsesAndLevels { get; set; } = [];
        public void IncreaseWeaponUse(Item weapon)
        {
            if (WeaponUsesAndLevels[weapon.Name].Level < 11) // cap of 11 levels
            {
                if (WeaponUsesAndLevels[weapon.Name].Use <= WeaponUsesAndLevels[weapon.Name].Level)
                    WeaponUsesAndLevels[weapon.Name].Use++;
                else
                {
                    WeaponUsesAndLevels[weapon.Name].Use = 0;
                    WeaponUsesAndLevels[weapon.Name].Level++;
                    Console.WriteLine($"You've used the {weapon.Name} so much, it's level has increased to {WeaponUsesAndLevels[weapon.Name].Level}!");
                }
            }
        }
        public IEnumerator<(string name, object? value)> GetEnumerator()
        {
            yield return ("Current Health", PlayerHealth);
            yield return ("Maximum Health", MaxPlayerHealth);
            yield return ("Current Depth Level", DungeonLevel);
            yield return ("Amount Of Healing Potions", HealingPotionAmount);
            foreach (KeyValuePair<string, WeaponUseAndLevel> pair in WeaponUsesAndLevels)
            {
                if (pair.Key == "Fiery Greatsword") // always first weapon
                    Console.WriteLine("\n==Weapon Levels==\n");
                yield return (pair.Key, $"Level {pair.Value.Level}");
            }
        }

        public void AffectHealth(int amount) => PlayerHealth = Math.Min(PlayerHealth + amount, MaxPlayerHealth);
    }
    public record WeaponUseAndLevel { public int Use { get; set; } = 0; public int Level { get; set; } = 0; } 
}
