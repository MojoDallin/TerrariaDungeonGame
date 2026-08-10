using DungeonGame.Stats;
using System;
using System.Collections.Generic;
using System.Text;

namespace DungeonGame
{
    public class RandomChanceRolls()
    {
        private readonly Random rand = new();

        public void RollForMaxLifeIncreaseOrHealthPotion(PlayerData Player)
        {
            if (rand.Next(20) <= 0 + (Player.DungeonLevel / 20)) // 5% for crystal or fruit, with an extra 5% every 20 levels
            {
                string item = "";
                if (Player.MaxPlayerHealth < 400) // life crystal
                {
                    Player.MaxPlayerHealth += 20;
                    Player.AffectHealth(20);
                    item = "Crystal";
                }
                else if (Player.MaxPlayerHealth < 500 && Player.DungeonLevel > 30)
                {
                    Player.MaxPlayerHealth += 5;
                    Player.AffectHealth(5);
                    item = "Fruit";
                }
                if (item.Length > 3)
                    Console.WriteLine($"Woah! You found a Life {item}! Your maximum health has increased to {Player.MaxPlayerHealth}!");
            }
            else if (rand.Next(5) == 0) // 20%
            {
                Console.WriteLine("You found a healing potion!");
                Player.HealingPotionAmount++;
            }
        }

        public void RollForAccessory(PlayerData Player)
        {
            List<Accessory> accessoriesNotFound = [.. Items.AccessoryList]; // clone
            foreach (string acc in Player.Accessories)
                accessoriesNotFound.Remove(Items.AccessoryList.Find(acc2 => acc2.Name == acc)!);
            if (rand.Next(100) < accessoriesNotFound.Count * 5) // 5% for each accessory
            {
                accessoriesNotFound.RemoveAll(acc => acc.MinimumDepthLevelToFind > Player.DungeonLevel); // remove those too shallow to be found
                if (accessoriesNotFound.Count > 0)
                {
                    Accessory found = accessoriesNotFound[rand.Next(accessoriesNotFound.Count)];
                    Player.Accessories.Add(found.Name);
                    Console.WriteLine($"Awesome! You found {found.Name}! {found.Description}");
                }
            }
        }
        public void RollForArmor(PlayerData Player)
        {
            if (rand.Next(20) <= 0 + (Player.DungeonLevel / 15)) // 5% with an extra 5% every 15 levels
            {
                string[] rarities = ["Copper", "Iron", "Silver", "Gold"];
                int rarity = rand.Next(rarities.Length); // worst pony of all time
                for (int i = 0; i < Player.DungeonLevel / 15; i++)
                { // reroll (basically higher rarity chance for higher level)
                    int newRarity = rand.Next(rarities.Length);
                    if (newRarity > rarity)
                        rarity = newRarity;
                    if (newRarity == rarities.Length - 1)
                        break;
                }
                string type = rarities[rarity];
                int armorType = rand.Next(3); // one of 3 armor pieces
                string[] slots = ["Helmet", "Chestplate", "Leggings"];
                List<string> EffectTypesCopy = [.. Items.EffectTypes];
                Armor armor = new($"{type} {slots[armorType]}", "", type, armorType, []);
                int effectAmount = Math.Min(rand.Next(1, 1 + Player.DungeonLevel / 20), EffectTypesCopy.Count);
                for (int i = 0; i < effectAmount; i++) // atleast 1 effect, with a possibility for another one each 20 levels
                {
                    int value = rand.Next(1, 5 * (rarity + 1)); // atleast a 5% effect, with upwards of 20%!!
                    int index = rand.Next(EffectTypesCopy.Count);
                    string effect = EffectTypesCopy[index];
                    armor.ArmorEffects.Add(Tuple.Create(effect, value));
                    armor.Description += $"+{value} {effect}\n";
                    EffectTypesCopy.RemoveAt(index);
                }
                armor.Description = armor.Description[..^1]; // get rid of newline
                Console.WriteLine($"Sweet! You found a {armor.Name}! It grants you:\n{armor.Description}");
                if (Player.Armor[armorType] is not null)
                {
                    Console.WriteLine($"You currently have a {Player.Armor[armorType].Name} equipped, which grants you:\n{Player.Armor[armorType].Description}");
                    Console.WriteLine("Press 1 to replace it, or press 2 to keep your old armor.");
                    string? input = Console.ReadLine();
                    if (input is not null && input.Contains('1'))
                        Player.Armor[armorType] = armor;
                }
                else
                {
                    Console.WriteLine("You currently have nothing equipped, so it has been automatically equipped!");
                    Player.Armor[armorType] = armor;
                }
            }
        }
    }
}
