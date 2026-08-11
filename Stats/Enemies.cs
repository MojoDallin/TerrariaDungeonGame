using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DungeonGame.Stats
{
    public class Enemy(string name, string description, int health, int damage)
    {
        public string Name { get; set; } = name;
        public string Description { get; set; } = description;
        public int Health { get; set; } = health;
        public int Damage { get; set; } = damage;
        [JsonIgnore] // for custom enemies
        public HashSet<string> Debuffs { get; set; } = [];
    }
    public static class Enemies
    {
        public static List<Enemy> EnemyList =
        [
            new Enemy("Slime", "desc", 35, 15), // name description health damage
            new Enemy("Zombie", "desc2", 50, 20),
            new Enemy("Demon Eye", "desc3", 40, 25)
        ];
    }
}
