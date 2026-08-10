//data
using DungeonGame;
using DungeonGame.Stats;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;

PlayerData Player = new();
Random rand = new();
JsonSerializerOptions opts = new() { WriteIndented = true };
RandomChanceRolls Events = new();

//execution

void LoadData(int option)
{
    if (option == 1)
    {
        if (File.Exists("player_data.json"))
        {
            string txt = File.ReadAllText("player_data.json");
            Player = JsonSerializer.Deserialize<PlayerData>(txt, opts)!;
        }
        else
            File.WriteAllText("player_data.json", JsonSerializer.Serialize(Player, opts));
    }
    else if(option == 0)
        File.WriteAllText("player_data.json", JsonSerializer.Serialize(Player, opts)); // reset data
    foreach (Item weapon in Items.WeaponList) // init
        if (!Player.WeaponUsesAndLevels.ContainsKey(weapon.Name) && weapon.MinimumDepthLevelToFind < 0) // < 0 is basically == -1, which denotes a starter item
            Player.WeaponUsesAndLevels.Add(weapon.Name, new WeaponUseAndLevel()); // (uses, level)
}

void SaveGame()
{
    string txt = JsonSerializer.Serialize(Player, opts);
    File.WriteAllText("player_data.json", txt);
}
void startGame()
{
    Console.WriteLine("Welcome to the Terraria Dungeon! Press 1 to start when you're ready. Alternatively, press 0 to reset your data.");
    int input;
    try
    {
        input = Convert.ToInt32(Console.ReadLine());
        Console.Clear();
    }
    catch (FormatException)
    {
        Console.WriteLine("Input not recognized, please re-enter!");
        startGame();
    }
    catch (Exception)
    {
        Console.WriteLine("Something went wrong! Oops.");
        startGame();
    }
    LoadData(input);
    interpretInput(3); // always path on launch
}
startGame();
void continueForwards() => Thread.Sleep(500);
void interpretInput(int option)
{
    Console.Clear();
    string[] directions = ["Left", "Forward", "Right"];
    //the split pathways stuff thingies
    if (option == 0 || option == 1)
    {
        int totalDirections = rand.Next(2, 4);
        string[] newDirections = new string[totalDirections];
        Console.WriteLine("You come across a " + totalDirections + "-way. Which direction would you like to go?\n");
        for (int i = 0; i < totalDirections; i++)
        {
            int direction = rand.Next(totalDirections);
            while (directions[direction] == "REMOVED")
                direction = rand.Next(totalDirections);
            Console.WriteLine($"[{i + 1}] {directions[direction]}\n");
            newDirections[i] = directions[direction];
            directions[direction] = "REMOVED";
        }
        Console.WriteLine($"[{totalDirections + 1}] View Player Information");
        try
        {
            int input = Convert.ToInt32(Console.ReadLine());
            while (input > totalDirections + 1 || input < 1)
            {
                Console.WriteLine("Invalid number!");
                input = Convert.ToInt32(Console.ReadLine());
            }
            if(input == totalDirections + 1)
            {
                Console.WriteLine("==Player Information==\n");
                foreach (var (name, value) in Player)
                    Console.WriteLine($"{name}: {value}");
                input = Convert.ToInt32(Console.ReadLine());

            }
            Player.AffectHealth(5); // heal by 5 each time you pass a "room"
            takePath(input, newDirections);
        }
        catch (Exception exec)
        {
            Console.WriteLine(exec.Message + ", " + exec.StackTrace);
            Console.WriteLine("uH oH! sOmEtHinG wEnT wRoNg! Jokes aside, you're gonna have to restart, sorry. Luckily, this game autosaves and autoloads!");
        }
        // max hp/health potion
        Events.RollForMaxLifeIncreaseOrHealthPotion(Player);
        // accessories
        Events.RollForAccessory(Player);
        // armor
        Events.RollForArmor(Player);
        Player.DungeonLevel++;
    }
    //combat stuff wooooo
    else if (option == 2) // 33% for combat
    {
        Enemy enemy = Enemies.EnemyList[rand.Next(3)];
        enemy = new(enemy.Name, enemy.Description, enemy.Health, enemy.Damage);
        double multiplier = Player.DungeonLevel / 10.0; // increase stats
        enemy.Health = (int)(enemy.Health * multiplier);
        enemy.Damage = (int)(enemy.Damage * multiplier);
        bool hasHealed = false;
        // accessory and armor benefits
        Dictionary<string, int> armorAccessoryBenefits = Player.GrabAccessoryAndArmorBenefits();
        Console.WriteLine("You encounter a " + enemy.Name + "!\nWhat will you do?\n");
        while (enemy.Health > 0)
        {
            //player attack
            for (int i = 0; i < Items.WeaponList.Count; i++)
            {
                Item weapon = Items.WeaponList[i];
                Console.WriteLine($"[{i + 1}] {weapon.Name}: {weapon.Description}");
            }
            Console.WriteLine("[5] Healing Potion: Heals 50 hp. Usable once per fight.");
            int input = Convert.ToInt32(Console.ReadLine());
            while(input == 5)
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
                input = Convert.ToInt32(Console.ReadLine());
            }
            while (input > Items.WeaponList.Count + 1 || input < 1)
            {
                Console.WriteLine("Not an option!");
                input = Convert.ToInt32(Console.ReadLine());
            }
            Console.Clear();
            Weapon selectedWeapon = Items.WeaponList[input - 1];
            Player.IncreaseWeaponUse(selectedWeapon);
            int damage = selectedWeapon.Damage + (int)(selectedWeapon.Damage * ((Player.WeaponUsesAndLevels[selectedWeapon.Name].Level - 1)/10.0));
            if(armorAccessoryBenefits.TryGetValue("Damage", out int extraDmg))
                damage += (int)(damage * (extraDmg/100.0));
            enemy.Health -= damage;
            if (enemy.Health < 0)
            {
                Console.WriteLine("You defeated the " + enemy.Name + "!\n");
                break;
            }
            if (input == 1)
                enemy.Debuffs.Add("Fire");
            else if (input == 3)
                enemy.Debuffs.Add("Poison");
            else if (input == 4) // blood butcherer
                enemy.Debuffs.Add("Bleeding");
            Console.WriteLine("You swing your " + selectedWeapon.Name + ", dealing " + damage + " damage. The " + enemy.Name + " now has " + enemy.Health + " hitpoints.\n");
            //dodge chance
            if (input == 2 && rand.Next(5) == 0)
                Console.WriteLine("You attack so fast, the enemy can't attack you back!\n");
            else if (armorAccessoryBenefits.TryGetValue("Dodge", out int dodge) && rand.Next(100) < dodge)
                Console.WriteLine("Because of your mobility accessories and armor, you're able to dodge the enemy's attack!");
            //enemy attack
            else
            {
                int damageTaken = rand.Next(enemy.Damage);
                damageTaken = (int)(enemy.Debuffs.Contains("Bleeding") ? damageTaken * 0.75 : damageTaken);
                if (armorAccessoryBenefits.TryGetValue("Defense", out int defense))
                    damageTaken = Math.Max(damageTaken - defense, 0); // enemies cant deal negative damage
                Player.AffectHealth(-damageTaken);
                Console.Write("The " + enemy.Name + " attacks you! It deals " + damageTaken + " damage. You now have " + Player.PlayerHealth + " hitpoints.\n");
                if (Player.PlayerHealth <= 0)
                {
                    Console.Clear();
                    Console.Write($"You were slain! Final Score: {Player.DungeonLevel}");
                    if(Player.DungeonLevel > Player.DungeonLevelHighScore)
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
            foreach(string d in enemy.Debuffs)
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
                else if (d == "Bleeding")
                    Console.WriteLine("The enemy is bleeding, which means it can only deal 75% of its damage!");
            }
            Console.WriteLine();
        }
    }
    SaveGame();
    continueForwards();
    interpretInput(rand.Next(3));
}

void takePath(int direction, string[] directionOptions) => Console.WriteLine("You went " + directionOptions[direction - 1].ToLower() + ".");