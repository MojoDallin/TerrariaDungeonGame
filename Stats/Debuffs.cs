using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonGame.Stats
{
    public class Debuff
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int TurnsLeft { get; set; }
        public Action OnTurnEffect { get; set; }
        public Debuff(string name, string description, int turnsLeft, Action onTurnEffect)
        {
            Name = name;
            Description = description;
            TurnsLeft = turnsLeft;
            OnTurnEffect = onTurnEffect;
            OnTurnEffect += () => TurnsLeft--;
        }
    }
}
