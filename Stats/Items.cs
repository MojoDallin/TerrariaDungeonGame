namespace DungeonGame.Stats
{
    public class Item(string name, string description, int damage, List<string> onHitEffects)
    {
        public string Name { get; set; } = name;
        public string Description { get; set; } = description;
        public int Damage { get; set; } = damage;
        public List<string> OnHitEffects { get; set; } = onHitEffects;
    }

    public static class Items
    {
        public static List<Item> WeaponList =
        [
            new Item("Fiery Greatsword", "A big sword with decent damage that can hit multiple enemies, and set them on fire.", 35, ["Fire"]),
            new Item("Muramasa", "A small, fast-swinging sword that can sometimes allow you to dodge enemy hits.", 20, ["Dodge"]),
            new Item("Blade of Grass", "A medium sized sword that can hit multiple enemies, as well as poisoning them.", 25, ["Poison"]),
            new Item("Blood Butcherer", "A large sword that hits one enemy, but can cause the enemy to bleed.", 30, ["Bleed"])
        ];
        private readonly static Random Random = new();

        // on hit functions
        private static void ApplyDebuff()
        {

        }
    }
}
