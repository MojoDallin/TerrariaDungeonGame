namespace DungeonGame.Stats
{
    public static class Items
    {
        public static List<Weapon> WeaponList =
        [
            new Weapon("Fiery Greatsword", "A big sword with decent damage that can hit multiple enemies, and set them on fire.", -1, 35, ["Fire"]),
            new Weapon("Muramasa", "A small, fast-swinging sword that can sometimes allow you to dodge enemy hits.", -1, 20, ["Dodge"]),
            new Weapon("Blade of Grass", "A medium sized sword that can hit multiple enemies, as well as poisoning them.", -1, 25, ["Poison"]),
            new Weapon("Blood Butcherer", "A large sword that hits one enemy, but can cause the enemy to bleed.", -1, 30, ["Bleed"])
        ];

        public static List<string> EffectTypes =
        [
        "Dodge",
        "Damage",
        "Defense"
        ];

        public static List<Accessory> AccessoryList =
        [
            new Accessory("Cloud in a Bottle", "This cloud in a bottle allows you to double jump! You have an extra 5% dodge chance.", 15, [(EffectTypes[0], 5)]),
            new Accessory("Hermes Boots", "These boots allow you to run super fast! You have an extra 5% dodge chance.", 25, [(EffectTypes[0], 5)]),
            new Accessory("Shark Tooth Necklace", "This necklace made of shark teeth gives you a nice 10% extra damage on all attacks.", 10, [(EffectTypes[1], 10)]),
            new Accessory("Hard Shackles", "These hard shackles lightly protect you, shielding you of 2 damage every time you're hit AND giving you 5% extra damage.", 5, [(EffectTypes[2], 2), (EffectTypes[1], 5)])
        ];
    }

    // actual classes
    public abstract class Item()
    {
        public abstract string Name { get; set; }
        public abstract string Description { get; set; }
        public abstract int MinimumDepthLevelToFind { get; set; } // -1 means you start with it (like the 4 nights edge components)
        public override bool Equals(object? obj) => obj is Item item && Name == item.Name;
        public override int GetHashCode() => Name.GetHashCode();
    }
    public class Weapon(string name, string description, int level, int damage, List<string> effects) : Item
    {
        public override string Name { get; set; } = name;
        public override string Description { get; set; } = description;
        public override int MinimumDepthLevelToFind { get; set; } = level;
        public int Damage { get; set; } = damage;
        public List<string> OnHitEffects { get; set; } = effects;
    }
    public class Accessory(string name, string description, int level, List<(string, int)> accessoryBuffs) : Item
    {
        public override string Name { get; set; } = name;
        public override string Description { get; set; } = description;
        public override int MinimumDepthLevelToFind { get; set; } = level;
        public List<(string, int)> AccessoryTypeAndValue { get; set; } = accessoryBuffs;
    }
    public class Armor(string name, string description, string EquipmentType, int EquipmentSlot, List<Tuple<string, int>> ArmorEffects) : Item
    {
        public override string Name { get; set; } = name;
        public override string Description { get; set; } = description;
        public override int MinimumDepthLevelToFind { get; set; } = 0;
        public string EquipmentType { get; set; } = EquipmentType; // copper iron silver gold etc
        public int EquipmentSlot { get; set; } = EquipmentSlot; // 0 for head, 1 for torso, 2 for legs
        public List<Tuple<string, int>> ArmorEffects { get; set; } = ArmorEffects;
    }
}
