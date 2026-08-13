using DungeonGame.Stats;

namespace DungeonGame
{
    public class Combat // new for each individual combat
    {
        private bool hasHealed = false;
        public void FightEnemy(Enemy enemy, PlayerData Player)
        {
            // grab accessory and armor buffs
            Dictionary<string, int> armorAccessoryBenefits = Player.GrabAccessoryAndArmorBenefits();
            Random rand = new();
            Console.WriteLine($"You encounter a {enemy.Name}!\nWhat will you do?\n");
            while (enemy.Health > 0)
            {
                //player attack
                int totalWeapons = Items.WeaponList.Count;
                for (int i = 0; i < totalWeapons; i++)
                {
                    Item weapon = Items.WeaponList[i];
                    Console.WriteLine($"[{i}] {weapon.Name}: {weapon.Description}");
                }
                Console.WriteLine($"[{totalWeapons}] Healing Potion: Heals 50 hp. Usable once per fight.");
                int input = Convert.ToInt32(Console.ReadLine());
                while (input > Items.WeaponList.Count || input < 0)
                {
                    Console.WriteLine("Not an option!");
                    input = Convert.ToInt32(Console.ReadLine());
                }
                if(input == Items.WeaponList.Count) // use healing potion
                {
                    TryHeal(Player);
                    input = Convert.ToInt32(Console.ReadLine());
                    while (input > Items.WeaponList.Count - 1 || input < 0)
                    {
                        Console.WriteLine("Not an option!" + (input == Items.WeaponList.Count ? " Already healed!" : ""));
                        input = Convert.ToInt32(Console.ReadLine());
                    }
                }
                Console.Clear();
                Weapon selectedWeapon = Items.WeaponList[input];
                Player.IncreaseWeaponUse(selectedWeapon);
                int damage = selectedWeapon.Damage + (int)(selectedWeapon.Damage * ((Player.WeaponUsesAndLevels[selectedWeapon.Name].Level - 1) / 10.0));
                if (armorAccessoryBenefits.TryGetValue("Damage", out int extraDmg))
                    damage += (int)(damage * (extraDmg / 100.0));
                enemy.Health -= damage;
                if (enemy.Health < 0)
                {
                    Console.WriteLine("You defeated the " + enemy.Name + "!\n");
                    break;
                }
                foreach (string effect in selectedWeapon.OnHitEffects)
                    if (effect != "Dodge")
                        enemy.Debuffs.Add(effect);
                Console.WriteLine("You swing your " + selectedWeapon.Name + ", dealing " + damage + " damage. The " + enemy.Name + " now has " + enemy.Health + " hitpoints.\n");
                //dodge chance
                if (selectedWeapon.OnHitEffects.Contains("Dodge") && rand.Next(5) == 0)
                    Console.WriteLine("You attack so fast, the enemy can't attack you back!\n");
                else if (armorAccessoryBenefits.TryGetValue("Dodge", out int dodge) && rand.Next(100) < dodge)
                    Console.WriteLine("Because of your mobility accessories and armor, you're able to dodge the enemy's attack!");
                //enemy attack
                else
                {
                    int damageTaken = rand.Next(enemy.Damage);
                    damageTaken = (int)(enemy.Debuffs.Contains("Bleed") ? damageTaken * 0.75 : damageTaken);
                    if (armorAccessoryBenefits.TryGetValue("Defense", out int defense))
                        damageTaken = Math.Max(damageTaken - defense, 0); // enemies cant deal negative damage
                    Player.AffectHealth(-damageTaken);
                    Console.Write($"The {enemy.Name} attacks you! It deals {damageTaken} damage. You now have {Player.PlayerHealth}/{Player.MaxPlayerHealth} health remaining.\n");
                    if (Player.PlayerHealth <= 0)
                    {
                        Console.Clear();
                        Console.Write($"You were slain! Final Score: {Player.DungeonLevel}");
                        if (Player.DungeonLevel > Player.DungeonLevelHighScore)
                        {
                            Console.Write($" [New Highscore! Old Score: {Player.DungeonLevelHighScore}]");
                            Player.DungeonLevelHighScore = Player.DungeonLevel;
                        }
                        Console.WriteLine(); // newline
                        Console.WriteLine("Don't give up though! You can restart at dungeon level 1, keeping your items you have found so far along with your weapon levels!");
                        Console.WriteLine("Press any key to restart.");
                        Console.ReadLine();
                        enemy.Health = -1;
                        enemy.Debuffs.Clear();
                        Player.PlayerHealth = 150;
                        Player.DungeonLevel = 1;
                    }
                }
                foreach (string d in enemy.Debuffs) // do debuff damage after enemy has attacked
                {
                    if (d == "Fire")
                    {
                        enemy.Health -= 7;
                        Console.WriteLine($"The enemy also takes 7 burn damage, leaving it at {enemy.Health}.");
                    }
                    else if (d == "Poison")
                    {
                        enemy.Health -= 10;
                        Console.WriteLine($"The enemy also takes 10 poison damage, leaving it at {enemy.Health}.");
                    }
                    else if (d == "Bleed")
                        Console.WriteLine("The enemy is bleeding, which means it can only deal 75% of its damage!");
                }
                Console.WriteLine();
            }
        }
        private void TryHeal(PlayerData Player)
        {
            if (!hasHealed)
            {
                if (Player.HealingPotionAmount == 0)
                    Console.WriteLine("You don't have any healing potions!");
                else
                {
                    Player.AffectHealth(50);
                    Console.WriteLine("You have been healed! Your hitpoints are now " + Player.PlayerHealth + ".");
                    Player.HealingPotionAmount--;
                    Console.WriteLine("You now have " + Player.HealingPotionAmount + " healing potions.");
                    hasHealed = true;
                }
            }
            else
                Console.WriteLine("Cannot heal, you have already healed!");
        }
    }
}
