namespace DungeonGame.Stats
{
    public class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Level { get; set; }
        public int Damage { get; set; }
        public Action<List<Enemy>>? OnHitEffect { get; set; }

        public Item(string name, string description, int damage)
        {
            Name = name;
            Description = description;
            Level = 1; // start at 1 (max is 10)
            Damage = damage;
        }
        public void LevelUp()
        {
            if (Level < 10)
                Level++;
        }
    }

    public static class Items
    {
        public static List<Item> WeaponList = new();
        private readonly static Random Random = new();
        public static void Initialize()
        {
            WeaponList.Add(new Item("Fiery Greatsword", "A big sword with decent damage that can hit multiple enemies, and set them on fire.", 35));
            WeaponList[0].OnHitEffect = (enemies) => MultiHit(enemies, 0, WeaponList[0], ApplyDebuff); // 0 means hit all
            WeaponList.Add(new Item("Muramasa", "A small, fast-swinging sword that can sometimes allow you to dodge enemy hits.", 25));
        }

        // on hit functions
        private static void MultiHit(List<Enemy> enemies, int amount, Item item, Action? action = null)
        {
            if (amount > 0) // 0 means hit all
                for (int i = 0; i < amount; i++)
                    enemies.RemoveAt(Random.Next(enemies.Count)); // this removes random enemies so the multihit only hits some not all
            foreach (Enemy enemy in enemies)
                enemy.Health -= Random.Next(item.Damage - item.Damage/10, item.Damage + item.Damage/10); // 10% in either direction of the damage value
            if (action is not null)
                action();

        }
        private static Dictionary<string, string> DebuffEffects = new()
        {
            { "Fire", "Enemy takes 10 damage per turn" },
            { "Poison", "Enemy takes 7 damage per turn" },
            { "Bleed", "Enemy takes 5 damage per turn" }
        };
        private static void ApplyDebuff()
        {

        }
    }
}
