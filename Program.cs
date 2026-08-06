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
void continueForwards() => Thread.Sleep(1000);
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

        if(rand.Next(20) == 0) // 5% for crystal or fruit
        {
            string item = "";
            if (Player.MaxPlayerHealth < 400) // life crystal
            {
                Player.MaxPlayerHealth += 20;
                Player.AffectHealth(20);
                item = "Crystal";
            }
            else if (Player.DungeonLevel > 30)
            {
                Player.MaxPlayerHealth += 5;
                Player.AffectHealth(5);
                item = "Fruit";
            }
            if(item.Length > 3)
                Console.WriteLine($"Woah! You found a Life {item}! Your maximum health has increased to {Player.MaxPlayerHealth}!");
        }
        else if (rand.Next(5) == 0) // 20%
        {
            Console.WriteLine("You found a healing potion!");
            Player.HealingPotionAmount++;
        }
        // accessories
        List<Accessory> accessoriesNotFound = [..Items.AccessoryList]; // clone
        foreach (string acc in Player.Accessories)
            accessoriesNotFound.Remove(Items.AccessoryList.Find(acc2 => acc2.Name == acc)!);
        if(rand.Next(100) < accessoriesNotFound.Count * 5) // 5% for each accessory
        {
            accessoriesNotFound.RemoveAll(acc => acc.MinimumDepthLevelToFind > Player.DungeonLevel); // remove those too shallow to be found
            if(accessoriesNotFound.Count > 0)
            {
                Accessory found = accessoriesNotFound[rand.Next(accessoriesNotFound.Count)];
                Player.Accessories.Add(found.Name);
                Console.WriteLine($"Awesome! You found {found.Name}! {found.Description}");
            }
        }
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
        int dodgeChance = 0;
        int extraDamagePercentage = 0;
        int defense = 0;
        foreach (string acc in Player.Accessories)
        {
            Accessory accessory = Items.AccessoryList.Find(acc2 => acc2.Name == acc)!;
            foreach ((string, int) effect in accessory.AccessoryTypeAndValue)
            {
                int val = effect.Item2;
                switch (effect.Item1)
                {
                    case "Dodge":
                        dodgeChance += val;
                        break;
                    case "Damage":
                        extraDamagePercentage += val;
                        break;
                    case "Defense":
                        defense += val;
                        break;
                }
            }
        }
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
            damage += (int)(damage * (extraDamagePercentage/100.0));
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
            else if (rand.Next(100) < dodgeChance)
                Console.WriteLine("Because of your mobility accessories, you dodge the enemy's attack!");
            //enemy attack
            else
            {
                int damageTaken = rand.Next(enemy.Damage);
                damageTaken = (int)(enemy.Debuffs.Contains("Bleeding") ? damageTaken * 0.75 : damageTaken);
                damageTaken -= defense;
                Player.AffectHealth(-damageTaken);
                Console.Write("The " + enemy.Name + " attacks you! It deals " + damageTaken + " damage. You now have " + Player.PlayerHealth + " hitpoints.\n");
                if (Player.PlayerHealth <= 0)
                {
                    Console.Clear();
                    Console.WriteLine($"You were slain! Final Score: {Player.DungeonLevel}");
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
