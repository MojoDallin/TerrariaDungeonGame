//data
using System.Runtime.Serialization;

string[] ENEMIES = {"Slime", "Zombie", "Demon Eye"};
int[] ENEMY_HP = {30, 100, 120};
string[] WEAPONS = {"Fiery Greatsword", "Muramasa", "Blade of Grass", "Blood Butcherer"};
int[] WEAPON_DAMAGE = {35, 30, 20, 25 };
string[] WEAPON_DESCRIPTIONS = { "A big sword with decent damage. Great for multiple enemies with high HP, and sets them on fire.",
    "A small, fast-swinging sword. Great for multiple enemies with low HP. Due to it's speed, enemies can fail to attack you!",
    "A big sword with lower damage that can poison enemies. Good for one enemy with low HP, and poisons them.",
    "A decently-sized sword, great for taking out a single enemy with high HP. Due to it's size, you can sometimes dodge enemies!"};
string[] CONSUMABLES = {"Healing Potion"};
int[] consumableAmounts = { 5 };
int playerHP = 100;
int dungeonLevel = 1;
Random rand = new Random();

void updateStats()
{
    for (int i = 0; i < ENEMY_HP.Length; i++)
        ENEMY_HP[i] *= 1 + (dungeonLevel/10);
}
//execution

void saveAndLoadGame(bool save)
{ // line 1: player hp, line 2: dungeon level
    string PATH = "C:\\Users\\dalli\\source\\repos\\DungeonGame\\Data.txt";
    if (!save)
    {
        string[] data = System.IO.File.ReadAllLines(PATH);
        playerHP = Convert.ToInt32(data[0]);
        dungeonLevel = Convert.ToInt32(data[1]);
    }
    else
        File.WriteAllText(@PATH, playerHP + "\n" + dungeonLevel);
}
void startGame()
{
    Console.WriteLine("Welcome to the Terraria Dungeon! Press 1 to start when you're ready.");
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
    saveAndLoadGame(false);
    interpretInput(1);
}
startGame();
void continueForwards()
{
    System.Threading.Thread.Sleep(rand.Next(0, 3000));
}
void interpretInput(int option)
{
    Console.Clear();
    string[] directions = {"Left", "Forward", "Right"};
    //the split pathways stuff thingies
    if (option == 0)
    {
        int totalDirections = rand.Next(2, 4);
        string[] newDirections = new string[totalDirections];
        Console.WriteLine("You come across a " + totalDirections + "-way. Which direction would you like to go?\n");
        for (int i = 0; i < totalDirections; i++)
        {
            int direction = rand.Next(totalDirections);
            while (directions[direction] == "REMOVED")
                direction = rand.Next(totalDirections);
            Console.WriteLine((i + 1) + ": " + directions[direction] + "\n");
            newDirections[i] = directions[direction];
            directions[direction] = "REMOVED";
        }
        try
        {
            int input = Convert.ToInt32(Console.ReadLine());
            while (input > totalDirections || input < 1)
            {
                Console.WriteLine("Invalid number!");
                input = Convert.ToInt32(Console.ReadLine());
            }
            takePath(input, newDirections);
        }
        catch (Exception exec)
        {
            Console.WriteLine(exec.Message + ", " + exec.StackTrace);
            Console.WriteLine("uH oH! sOmEtHinG wEnT wRoNg! Jokes aside, you're gonna have to restart, sorry. Luckily, this game autosaves and autoloads!");
        }
        if (rand.Next(10) == 0)
        {
            Console.WriteLine("You found a healing potion!");
            consumableAmounts[0]++;
        }
        dungeonLevel++;
    }
    //combat stuff wooooo
    else if (option == 1)
    {
        int enemy = rand.Next(3);
        int enemyHP = ENEMY_HP[enemy];
        bool hasHealed = false;
        bool fireEffect = false;
        bool poisonEffect = false;
        Console.WriteLine("You encounter a " + ENEMIES[enemy] + "!\nWhat will you do?\n");
        while (enemyHP > 0)
        {
            //player attack
            for (int i = 0; i < WEAPONS.Length; i++)
                Console.WriteLine((i + 1) + ": " + WEAPONS[i] + ": " + WEAPON_DESCRIPTIONS[i]);
            Console.WriteLine("5: Healing Potion: Heals 50 hp. Usable once per fight.");
            int input = Convert.ToInt32((Console.ReadLine()));
            while(input == 5)
            {
                if (!hasHealed)
                {
                    if (consumableAmounts[0] == 0)
                        Console.WriteLine("You don't have any healing potions!");
                    else
                    {
                        playerHP += 50;
                        if (playerHP > 200)
                            playerHP = 200;
                        Console.WriteLine("You have been healed! Your hitpoints are now " + playerHP + ".");
                        consumableAmounts[0]--;
                        Console.WriteLine("You now have " + consumableAmounts[0] + " healing potions.");
                        hasHealed = true;
                    }
                }
                else
                    Console.WriteLine("Cannot heal, you have already healed!");
                input = Convert.ToInt32(Console.ReadLine());
            }
            while (input > WEAPONS.Length || input < 1)
            {
                Console.WriteLine("Not an option!");
                input = Convert.ToInt32(Console.ReadLine());
            }
            Console.Clear();
            int damage = WEAPON_DAMAGE[input - 1];
            enemyHP -= damage;
            if (enemyHP < 0)
            {
                Console.WriteLine("You defeated the " + ENEMIES[enemy] + "!\n");
                break;
            }
            if (input == 1)
                fireEffect = true;
            else if (input == 3)
                poisonEffect = true;
            Console.WriteLine("You swing your " + WEAPONS[input - 1] + ", dealing " + damage + " damage. The " + ENEMIES[enemy] + " now has " + enemyHP + " hitpoints.\n");
            //dodge chance
            if (rand.Next(5) == 0)
            {
                if (input == 2)
                    Console.WriteLine("You attack so fast, the enemy can't attack you back!\n");
                else if (input == 4)
                    Console.WriteLine("Your sword is so big, you manage to dodge the enemy's attack!\n");
            }
            //enemy attack
            else
            {
                int damageTaken = rand.Next(20);
                playerHP -= damageTaken;
                Console.Write("The " + ENEMIES[enemy] + " attacks you! It deals " + damageTaken + " damage. You now have " + playerHP + " hitpoints.");
                if (fireEffect)
                {
                    Console.WriteLine("It also takes 5 burn damage.");
                    enemyHP -= 5;
                }
                else if (poisonEffect)
                {
                    Console.WriteLine("It also takes 10 poison damage.");
                    enemyHP -= 10;
                }
                Console.WriteLine("\n");
            }
        }
    }
    saveAndLoadGame(true);
    continueForwards();
    updateStats();
    interpretInput(rand.Next(2));
}

void takePath(int direction, string[] directionOptions) => Console.WriteLine("You went " + directionOptions[direction - 1].ToLower() + ".");
