using DungeonGame.Stats;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace DungeonGame
{
    public class CustomEnemyItemCreator
    {
        JsonSerializerOptions opts = new() { WriteIndented = true };
        public void CreateEnemy()
        {
            Console.Clear();
            Console.WriteLine("Creating a custom enemy...");
            Console.Write("Enter the name of the enemy: ");
            string name = Console.ReadLine()!;
            Console.Write("Enter the health of the enemy: ");
            int hp = Int32.Parse(Console.ReadLine()!);
            Console.Write("Enter the damage of the enemy: ");
            int damage = Int32.Parse(Console.ReadLine()!);
            Enemy customEnemy = new(name, "custom", hp, damage);
            string fileName = name.ToLower().Replace(" ", "_") + ".tdce"; // Terraria Dungeon Custom Enemy
            File.WriteAllText(fileName, JsonSerializer.Serialize(customEnemy, opts));
            Console.WriteLine($"Successfully saved custom enemy as: {fileName}!\n");
            Thread.Sleep(2000);
        }
        public void CreateWeapon()
        {
            Console.Clear();
            Console.WriteLine("Creating a custom weapon...");
            Console.Write("Enter the name of the weapon: ");
            string name = Console.ReadLine()!;
            Console.Write("Enter the description of the weapon: ");
            string desc = Console.ReadLine()!;
            Console.Write("Enter the damage of the weapon: ");
            int damage = Int32.Parse(Console.ReadLine()!);
            Console.Write("Enter the special effects (Fire, Dodge, Poison, Bleed; multiple are supported, separate by comma): ");
            string[] effects = Console.ReadLine()!.Split(',');
            for (int i = 0; i < effects.Length; i++)
                effects[i] = effects[i].Trim(); // get rid of whitespace on ends
            Weapon customItem = new(name, desc, -1, damage, [.. effects]);
            string fileName = name.ToLower().Replace(" ", "_") + ".tdcw"; // Terraria Dungeon Custom Weapon
            File.WriteAllText(fileName, JsonSerializer.Serialize(customItem, opts));
            Console.WriteLine($"Successfully saved custom item as: {fileName}!");
            Thread.Sleep(2000);
        }
    }
}
