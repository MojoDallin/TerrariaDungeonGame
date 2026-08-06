using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonGame.Stats
{
    public class Enemy
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Health { get; set; }
        public int Damage { get; set; }
        public List<Debuff> Debuffs { get; set; } = new();
        public Enemy(string name, string description, int health, int damage)
        {
            Name = name;
            Description = description;
            Health = health;
            Damage = damage;
        }
    }
}
