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
    bool loadData = true;
    switch (option)
    {
        case 1:
            if (File.Exists("player_data.json"))
            {
                string txt = File.ReadAllText("player_data.json");
                Player = JsonSerializer.Deserialize<PlayerData>(txt, opts)!;
            }
            else
                File.WriteAllText("player_data.json", JsonSerializer.Serialize(Player, opts));
            loadData = false;
            break;
        case 0:
            File.WriteAllText("player_data.json", JsonSerializer.Serialize(Player, opts)); // reset data
            break;
        case 2:
            string[] tdcFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.tdc?", SearchOption.AllDirectories); // ? allows it to search for tdce and tdci
            if (tdcFiles.Length < 1)
                Console.WriteLine("Couldn't find any custom files! Did you place them in the correct directory?");
            else
            {
                Console.WriteLine($"Found {tdcFiles.Length} custom files:");
                for(int index = 0; index < tdcFiles.Length; index++)
                    Console.WriteLine($"[{index}] {Path.GetFileName(tdcFiles[index])}");
                Console.WriteLine("Type the numbers (separated by space) of the ones you would like to use (ex. 0 1 2 3; alternatively, type ALL for everything):");
                List<string> options = [.. Console.ReadLine()!.Split(" ")];
                if (options[0].Equals("ALL", StringComparison.CurrentCultureIgnoreCase))
                {
                    options.Clear();
                    for (int index = 0; index < tdcFiles.Length; index++)
                        options.Add(index.ToString());
                }
                foreach(string opt in options)
                {
                    int index = Int32.Parse(opt);
                    string json = File.ReadAllText(tdcFiles[index]);
                    if (tdcFiles[index].Contains("tdce")) // enemy
                        Enemies.EnemyList.Add(JsonSerializer.Deserialize<Enemy>(json, opts)!);
                    else if (tdcFiles[index].Contains("tdcw")) // weapon
                        Items.WeaponList.Add(JsonSerializer.Deserialize<Weapon>(json, opts)!);
                    else if (tdcFiles[index].Contains("tdca")) // accessory
                        Items.AccessoryList.Add(JsonSerializer.Deserialize<Accessory>(json, opts)!);
                }
            }
            break;
        case 3:
            CustomEnemyItemCreator creator = new();
            Console.Clear();
            Console.WriteLine("Choose what you would like to create...");
            Console.WriteLine("[1] Weapon");
            Console.WriteLine("[2] Accessory");
            Console.WriteLine("[3] Enemy");
            switch (Console.ReadLine())
            {
                case "1":
                    creator.CreateWeapon();
                    break;
                case "3":
                    creator.CreateEnemy();
                    break;
            }
            break;
    }
    if (loadData)
        LoadData(1);
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
    Console.WriteLine("Welcome to the Terraria Dungeon! Options:");
    Console.WriteLine("[1] Start the game, where you previously left off.");
    Console.WriteLine("[2] Use custom items/enemies yourself or others have made!");
    Console.WriteLine("[3] Create a custom item/enemy to use/encounter in your runs!");
    Console.WriteLine("[0] Start the game, but erase your data instead.");
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
        Enemy enemy = Enemies.EnemyList[rand.Next(Enemies.EnemyList.Count)];
        enemy = new(enemy.Name, enemy.Description, enemy.Health, enemy.Damage);
        double multiplier = 1 + Player.DungeonLevel / 20.0; // increase stats
        enemy.Health = (int)(enemy.Health * multiplier);
        enemy.Damage = (int)(enemy.Damage * multiplier);
        new Combat().FightEnemy(enemy, Player);
    }
    SaveGame();
    continueForwards();
    interpretInput(rand.Next(3));
}

void takePath(int direction, string[] directionOptions) => Console.WriteLine("You went " + directionOptions[direction - 1].ToLower() + ".");