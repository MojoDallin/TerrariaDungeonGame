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
        public int DungeonLevelHighScore { get; set; } = 1;
        public int HealingPotionAmount { get; set; } = 5;
        public Dictionary<string, WeaponUseAndLevel> WeaponUsesAndLevels { get; set; } = [];
        public List<string> Accessories { get; set; } = [];
        public Armor[] Armor { get; set; } = new Armor[3]; // head torso legs
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
            yield return ("Maximum Depth Reached", DungeonLevelHighScore);
            yield return ("Amount Of Healing Potions", HealingPotionAmount);
            for (int i = 0; i < Accessories.Count; i++)
            {
                Accessory acc = Items.AccessoryList.Find(acc2 => acc2.Name == Accessories[i])!;
                if (i == 0)
                    Console.WriteLine("\n==Accessories==\n");
                yield return (acc.Name, acc.Description);
            }
            for(int i = 0; i < Armor.Length; i++)
            {
                Armor arm = Armor[i];
                if (i == 0)
                    Console.WriteLine("\n==Armor==\n");
                if(arm is not null)
                    yield return (arm.Name, "\n" + arm.Description);
            }
            foreach (KeyValuePair<string, WeaponUseAndLevel> pair in WeaponUsesAndLevels)
            {
                if (pair.Key == "Fiery Greatsword") // always first weapon
                    Console.WriteLine("\n==Weapon Levels==\n");
                yield return (pair.Key, $"Level {pair.Value.Level}");
            }
        }

        public void AffectHealth(int amount) => PlayerHealth = Math.Min(PlayerHealth + amount, MaxPlayerHealth);
        public Dictionary<string, int> GrabAccessoryAndArmorBenefits()
        {
            Dictionary<string, int> returnDict = [];
            foreach(string acc in Accessories)
            {
                Accessory accessory = Items.AccessoryList.Find(acc2 => acc2.Name == acc)!;
                foreach (Tuple<string, int> effect in accessory.AccessoryTypeAndValue)
                    if (!returnDict.TryAdd(effect.Item1, effect.Item2)) // checks if already exists
                        returnDict[effect.Item1] += effect.Item2;
            }
            foreach(Armor armor in Armor)
            {
                foreach (Tuple<string, int> effect in armor.ArmorEffects)
                    if (!returnDict.TryAdd(effect.Item1, effect.Item2))
                        returnDict[effect.Item1] += effect.Item2;
            }
            return returnDict;
        }
    }
    public record WeaponUseAndLevel { public int Use { get; set; } = 0; public int Level { get; set; } = 0; } 
}
