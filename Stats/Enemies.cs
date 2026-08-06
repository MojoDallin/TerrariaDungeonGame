using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonGame.Stats
{
    public class Enemy(string name, string description, int health, int damage)
    {
        public string Name { get; set; } = name;
        public string Description { get; set; } = description;
        public int Health { get; set; } = health;
        public int Damage { get; set; } = damage;
        public HashSet<string> Debuffs { get; set; } = [];
    }
    public static class Enemies
    {
        public static List<Enemy> EnemyList =
        [
            new Enemy("Slime", "desc", 30, 10),
            new Enemy("Zombie", "desc2", 70, 20),
            new Enemy("Demon Eye", "desc3", 50, 30)
        ];
    }
}
