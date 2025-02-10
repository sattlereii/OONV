using System;
using System.Collections.Generic;

namespace Hra
{
    class Room
    {
        public string Name { get; }
        public string Question { get; }
        public int CorrectAnswer { get; }
        public List<Room> Neighbors { get; }
        public bool Deadly { get; }
        public bool HasKey { get; set; }
        public bool Visited { get; set; }
        public string EnemyName { get; }
        public string DeathMessage { get; }
        public IEnemyStrategy EnemyStrategy { get; }

        public Room(string name, string question, int correctAnswer, string enemyName = null, bool deadly = false, string deathMessage = null, IEnemyStrategy enemyStrategy = null)
        {
            Name = name;
            Question = question;
            CorrectAnswer = correctAnswer;
            Neighbors = new List<Room>();
            Deadly = deadly;
            HasKey = false;
            Visited = false;
            EnemyName = enemyName;
            DeathMessage = deathMessage;
            EnemyStrategy = enemyStrategy ?? new AggressiveEnemy();
        }
    }

    interface IEnemyStrategy
    {
        void Attack();
    }

    class AggressiveEnemy : IEnemyStrategy
    {
        public void Attack() => Console.WriteLine("Nepřítel útočí agresivně!");
    }

    class DefensiveEnemy : IEnemyStrategy
    {
        public void Attack() => Console.WriteLine("Nepřítel se brání a čeká.");
    }

    abstract class CommandHandler
    {
        protected CommandHandler Next;

        public CommandHandler SetNext(CommandHandler next)
        {
            Next = next;
            return this;
        }

        public abstract bool Handle(string command, ref Room currentRoom, Dictionary<string, Room> rooms);
    }

    class MoveCommandHandler : CommandHandler
{
    public override bool Handle(string command, ref Room currentRoom, Dictionary<string, Room> rooms)
    {
        // Odstranění mezer okolo vstupu a kontrola, zda je příkaz číslo
        command = command.Trim();
        if (int.TryParse(command, out int choice))
        {
            Console.WriteLine($"Zadaná volba: {choice}");
            if (choice > 0 && choice <= currentRoom.Neighbors.Count)
            {
                currentRoom = currentRoom.Neighbors[choice - 1];
                Console.WriteLine($"Přesun do místnosti: {currentRoom.Name}");
                return true;
            }
            else
            {
                Console.WriteLine("Zadané číslo není platné. Zkus to znovu.");
                return false;
            }
        }

        Console.WriteLine("Příkaz není platný nebo neobsahuje číslo.");
        return Next?.Handle(command, ref currentRoom, rooms) ?? false;
    }
}


    
        class HelpCommandHandler : CommandHandler
    {
        public override bool Handle(string command, ref Room currentRoom, Dictionary<string, Room> rooms)
        {
            if (command == "help")
            {
                Console.WriteLine("Dostupné příkazy:");
                Console.WriteLine("  move <number> - přesune tě do vedlejší místnosti.");
                Console.WriteLine("  quit - opuštění hry.");
                Console.WriteLine("  help - zobrazí se tato zpráva.");
                return true;
            }
            return Next?.Handle(command, ref currentRoom, rooms) ?? false;
        }
    }

    class QuitCommandHandler : CommandHandler
    {
        public override bool Handle(string command, ref Room currentRoom, Dictionary<string, Room> rooms)
        {
            if (command == "quit")
            {
                Console.WriteLine("Děkuji za hraní.");
                Environment.Exit(0);
                return true;
            }
            return Next?.Handle(command, ref currentRoom, rooms) ?? false;
        }
    }

    static class RoomFactory
    {
        public static Room CreateRoom(string name, string question, int correctAnswer, string enemyName = null, bool deadly = false, string deathMessage = null, IEnemyStrategy enemyStrategy = null)
        {
            return new Room(name, question, correctAnswer, enemyName, deadly, deathMessage, enemyStrategy);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Vítej ve hře!");
            Console.Write("Zadej své jméno, hrdino: ");
            string heroName = Console.ReadLine();

            Console.WriteLine($"\nVítej, statečný hrdino {heroName}!");
            Console.WriteLine("Tvým úkolem je najít klíč a osvobodit tento hrad od nestvůr.");
            Console.WriteLine("Použij svůj důvtip k porážení nepřátel a najdi cestu zpět do sklepa. Hodně štěstí!");

            Dictionary<string, Room> rooms = CreateMap();
            Room currentRoom = rooms["Sklep"];

            CommandHandler commandHandler = new MoveCommandHandler();
            commandHandler.SetNext(new HelpCommandHandler()).SetNext(new QuitCommandHandler());

            while (true)
            {
                Console.WriteLine($"\nNacházíš se v {currentRoom.Name}.");

                if (currentRoom.Deadly)
                {
                    Console.WriteLine(currentRoom.DeathMessage ?? "Tato místnost je smrtící! Konec hry!");
                    break;
                }

                if (!string.IsNullOrEmpty(currentRoom.EnemyName) && !currentRoom.Visited)
                {
                    Console.WriteLine($"Proti tobě stojí {currentRoom.EnemyName}!");
                    currentRoom.EnemyStrategy.Attack();
                }

                if (!currentRoom.Visited && !string.IsNullOrEmpty(currentRoom.Question))
                {
                    Console.WriteLine($"Otázka: {currentRoom.Question}");
                    if (int.TryParse(Console.ReadLine(), out int answer) && answer == currentRoom.CorrectAnswer)
                    {
                        Console.WriteLine("Správně! Porazil jsi nepřítele.");
                        currentRoom.Visited = true;
                    }
                    else
                    {
                        Console.WriteLine("Špatná odpověď. Respawn ve Sklepě!");
                        currentRoom = rooms["Sklep"];
                        continue;
                    }
                }

                Console.WriteLine("Dostupné východy:");
                for (int i = 0; i < currentRoom.Neighbors.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {currentRoom.Neighbors[i].Name}");
                }

                Console.Write("Enter command: ");
                string input = Console.ReadLine();
                if (!commandHandler.Handle(input, ref currentRoom, rooms))
                {
                    Console.WriteLine("Nesprávný příkaz. Zkus to znovu.");
                }
            }
        }

        static Dictionary<string, Room> CreateMap()
        {
            Dictionary<string, Room> rooms = new Dictionary<string, Room>
            {
                { "Sklep", RoomFactory.CreateRoom("Sklep", "1 - 1", 0, "Obrovský pavouk") },
                { "Kuchyň", RoomFactory.CreateRoom("Kuchyň", "2 + 3", 5, "Rychlesešířící Hrnečku vař") },
                { "Zahrada", RoomFactory.CreateRoom("Zahrada", "5 * 2", 10, "Krvelačná Mandragora", false, null, new DefensiveEnemy()) },
                { "Zvěřinec", RoomFactory.CreateRoom("Zvěřinec", "6 / 3", 2, "Vlčí strážce") },
                { "Lednice", RoomFactory.CreateRoom("Lednice", null, 0, null, true, "Umrzl jsi v lednici!") },
                { "Vinárna", RoomFactory.CreateRoom("Vinárna", "7 - 2", 5, "Opilý zloděj") },
                { "Pekárna", RoomFactory.CreateRoom("Pekárna", "3 * 3", 9, "Koláčový fantom") },
                { "Prádelna", RoomFactory.CreateRoom("Prádelna", "9 / 3", 3, "Spodničkový bandita") },
                { "Katovna", RoomFactory.CreateRoom("Katovna", null, 0, null, true, "Kat se špatně probudil a popravil tě!") },
                { "Lázně", RoomFactory.CreateRoom("Lázně", "4 + 2", 6, "Vodní přízrak") },
                { "Bastion", RoomFactory.CreateRoom("Bastion", "10 - 5", 5, "Kámen duchů") },
                { "Stáje", RoomFactory.CreateRoom("Stáje", "2 * 4", 8, "Koňský démon") },
                { "Strážnice", RoomFactory.CreateRoom("Strážnice", "8 / 4", 2, "Hrdinský strážce") },
                { "Velká síň", RoomFactory.CreateRoom("Velká síň", "6 + 1", 7, "Velký hlídač") },
                { "Královská komnata", RoomFactory.CreateRoom("Královská komnata", "7 - 3", 4, "Král zlodějů") },
                { "Skrytý tunel", RoomFactory.CreateRoom("Skrytý tunel", null, 0, null, true, "Ztratil ses v temnotě tunelu a tvé volání nikdo neslyšel.") },
                { "Trůní sál", RoomFactory.CreateRoom("Trůní sál", "7 * 3", 21, "Králičí král") },
                { "Observatoř", RoomFactory.CreateRoom("Observatoř", "2^3", 8, "Hvězdný věštec") },
                { "Laboratoř", RoomFactory.CreateRoom("Laboratoř", "11^2", 121, "Alchymistický mutant") },
                { "Knihovna", RoomFactory.CreateRoom("Knihovna", "150*0", 0, "Strážce zapomenutých svitků") }
            };

            rooms["Sklep"].Neighbors.AddRange(new[] { rooms["Kuchyň"], rooms["Zahrada"], rooms["Zvěřinec"] });
            rooms["Kuchyň"].Neighbors.AddRange(new[] { rooms["Sklep"], rooms["Lednice"], rooms["Vinárna"], rooms["Pekárna"] });
            rooms["Zahrada"].Neighbors.AddRange(new[] { rooms["Sklep"], rooms["Strážnice"], rooms["Katovna"], rooms["Lázně"], rooms["Bastion"] });
            rooms["Zvěřinec"].Neighbors.AddRange(new[] { rooms["Sklep"], rooms["Stáje"] });
            rooms["Stáje"].Neighbors.AddRange(new[] { rooms["Zvěřinec"], rooms["Strážnice"] });
            rooms["Strážnice"].Neighbors.AddRange(new[] { rooms["Stáje"], rooms["Knihovna"], rooms["Zahrada"] });
            rooms["Knihovna"].Neighbors.AddRange(new[] { rooms["Strážnice"], rooms["Skrytý tunel"] });
            rooms["Vinárna"].Neighbors.AddRange(new[] { rooms["Kuchyň"], rooms["Prádelna"] });
            rooms["Pekárna"].Neighbors.AddRange(new[] { rooms["Kuchyň"], rooms["Prádelna"] });
            rooms["Prádelna"].Neighbors.AddRange(new[] { rooms["Vinárna"], rooms["Pekárna"], rooms["Velká síň"], rooms["Observatoř"] });
            rooms["Velká síň"].Neighbors.AddRange(new[] { rooms["Královská komnata"], rooms["Prádelna"] });
            rooms["Observatoř"].Neighbors.AddRange(new[] { rooms["Prádelna"], rooms["Katovna"], rooms["Laboratoř"] });
            rooms["Královská komnata"].Neighbors.AddRange(new[] { rooms["Velká síň"], rooms["Laboratoř"] });
            rooms["Laboratoř"].Neighbors.AddRange(new[] { rooms["Trůní sál"], rooms["Královská komnata"], rooms["Observatoř"] });
            rooms["Trůní sál"].Neighbors.AddRange(new[] { rooms["Laboratoř"], rooms["Lázně"], rooms["Bastion"] });
            rooms["Lázně"].Neighbors.AddRange(new[] { rooms["Trůní sál"], rooms["Zahrada"] });
            rooms["Bastion"].Neighbors.AddRange(new[] { rooms["Zahrada"], rooms["Trůní sál"], rooms["Strážnice"] });

            Random random = new Random();
            List<string> eligibleRooms = new List<string> 
            { 
                "Kuchyň", "Zahrada", "Zvěřinec", "Vinárna", "Pekárna", "Prádelna", 
                "Lázně", "Bastion", "Stáje", "Velká síň", "Observatoř", "Laboratoř", "Knihovna" 
            };
            string keyRoomName = eligibleRooms[random.Next(eligibleRooms.Count)];
            rooms[keyRoomName].HasKey = true;

            return rooms;
        }
    }
}


