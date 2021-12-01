using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper
{
    class Program
    {
        static void Main(string[] args)
        {
            string gameParameters = GetGameParameters();

            if (string.IsNullOrEmpty(gameParameters))
            {
                return;
            }

            Game game = new Game()
            {
                GridSize = Convert.ToInt32(gameParameters.Split('|')[0]),
                NumberOfMines = Convert.ToInt32(gameParameters.Split('|')[1]),
                Grid = new List<Square>()
            };

            Initialize(ref game);
            PlayGame(ref game);

            Console.Write("Press ENTER key to close . . .");
            Console.ReadLine();
        }

        static void PlayGame(ref Game game)
        {
            DisplayGrid(game);
            string command = "";
            while (command != "done" && command != "quit" && command != "loser")
            {
                Console.Write("COMMAND: ");
                string userInput = Console.ReadLine();
                command = HandleCommand(userInput, ref game);
                if (command == "loser" || command == "done")
                {
                    DisplayGrid(game, true);
                }
                else
                {
                    DisplayGrid(game);
                }
                switch (command)
                {
                    case "quit":
                        Console.WriteLine("\n   You quitter!");
                        break;
                    case "error":
                        Console.WriteLine("\n   BAD COMMAND, TRY AGAIN");
                        break;
                    case "done":
                        Console.WriteLine("\n   YOU'RE AMAZING!  YOU WON!!!");
                        break;
                    case "loser":
                        Console.WriteLine("\n   BOOM!  G A M E   O V E R !");
                        break;
                }
            }
        }

        static string HandleCommand(string userInput, ref Game game)
        {
            if (userInput.Contains("quit"))
            {
                userInput = "quit 0,0";
            }
            var commandString = userInput.Split(' ');
            if (commandString.Count() <= 1)
            {
                return "error";
            }
            string command = commandString[0];
            string coordinates = commandString[1];
            if (coordinates.Split(',').Count() != 2)
            {
                return "error";
            }
            int x;
            int y;
            if (!int.TryParse(coordinates.Split(',')[0], out x) ||
                !int.TryParse(coordinates.Split(',')[1], out y))
            {
                return "error";
            }
            var square = GetSquare(x, y, game.Grid);
            if (square == null)
            {
                command = "error";
            }

            switch (command)
            {
                case "flag":
                    square.Flagged = true;
                    square.Display = "!";
                    break;

                case "reveal":
                    if (square.Mined)
                    {
                        command = "loser";
                    }
                    else if (square.MineCount > 0)
                    {
                        square.Display = square.MineCount.ToString();
                    }
                    else
                    {
                        CascadeReveal(x, y, ref game);
                    }
                    break;

                case "quit":
                    break;

                default:
                    command = "error";
                    break;
            }

            if (GameIsDone(game))
            {
                command = "done";
            }

            return command;
        }

        static bool GameIsDone(Game game)
        {
            bool result = true;
            foreach (var square in game.Grid)
            {
                if (square.Display == "H" || (square.Display == "!" && !square.Mined))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        static void CascadeReveal(int squareX, int squareY, ref Game game)
        {
            List<string> displaysToIgnore = new List<string>() {" ", "!"};
            var square = GetSquare(squareX, squareY, game.Grid);
            square.Display = " ";
            for (int x = square.X - 1; x <= square.X + 1; x++)
            {
                for (int y = square.Y - 1; y <= square.Y + 1; y++)
                {
                    if (x != square.X || y != square.Y)
                    {
                        var nearSquare = GetSquare(x, y, game.Grid);

                        if (nearSquare != null && nearSquare.MineCount == 0 &&
                            !displaysToIgnore.Contains(nearSquare.Display))
                        {
                            CascadeReveal(nearSquare.X, nearSquare.Y, ref game);
                        }
                        else if (nearSquare?.MineCount > 0)
                        {
                            nearSquare.Display = nearSquare.MineCount.ToString();
                        }
                    }
                }
            }
        }

        static void Initialize(ref Game game)
        {
            List<int> randomNumbers = GetRandomNumbers(game.NumberOfMines, game.GridSize);
            //List<int> randomNumbers = new List<int>() {5, 9, 29, 42, 45, 50, 54};  // TEST TEST
            int i = 0;
            for (int x = 0; x < game.GridSize; x++)
            {
                for (int y = 0; y < game.GridSize; y++)
                {
                    game.Grid.Add(new Square()
                    {
                        Flagged = false,
                        Mined = randomNumbers.Contains(i),
                        Revealed = false,
                        MineCount = 0,
                        Display = "H",
                        X = x,
                        Y = y,
                        Gridsize = game.GridSize
                    });

                    i++;
                }
            }

            foreach (var square in game.Grid)
            {
                square.MineCount = GetMineCount(square, game.Grid);
            }
        }

        static int GetMineCount(Square square, List<Square> grid)
        {
            int output = 0;
            for (int x = square.X - 1; x <= square.X + 1; x++)
            {
                for (int y = square.Y - 1; y <= square.Y + 1; y++)
                {
                    if (x != square.X || y != square.Y)
                    {
                        var nearSquare = GetSquare(x, y, grid);

                        if (nearSquare != null)
                        {
                            output += nearSquare.Mined ? 1 : 0;
                        }
                    }
                }
            }

            return output;
        }

        static List<int> GetRandomNumbers(int count, int max)
        {
            List<int> output = new List<int>();
            int totalSquares = max * max;
            Random random = new Random();
            for (int i = 0; i < count; i++)
            {
                int number = random.Next(0, totalSquares);
                while (output.Contains(number))
                {
                    number = random.Next(0, totalSquares);
                }
                output.Add(number);
            }
            return output;
        }

        static Square GetSquare(int x, int y, List<Square> grid)
        {
            return grid.FirstOrDefault(a => a.X == x && a.Y == y);
        }

        static string GetGameParameters()
        {
            string output = "";
            bool quit = false;
            Console.WriteLine("WELCOME TO MINESWEEPER!\n");
            Console.Write("Enter grid size (min 8, max 50): ");
            string userinput = Console.ReadLine();
            int gridsize = 0;
            int minecount = 0;
            if (int.TryParse(userinput, out gridsize))
            {
                while (gridsize < 8 || gridsize > 50 && !quit)
                {
                    Console.Clear();
                    Console.WriteLine("WELCOME TO MINESWEEPER!\n");
                    Console.WriteLine("Invalid gridsize!");
                    Console.Write("Enter grid size (min 8, max 50): ");
                    userinput = Console.ReadLine();
                    if (!int.TryParse(userinput, out gridsize))
                    {
                        // they entered non-numeric
                        quit = true;
                    }
                }
            }
            else
            {
                quit = true;
            }

            if (!quit)
            {
                Console.Clear();
                Console.WriteLine("WELCOME TO MINESWEEPER!\n");
                Console.WriteLine("Enter grid size (min 8, max 50): " + gridsize);
                Console.Write("Enter number of mines (min 5, max 20): ");
                userinput = Console.ReadLine();
                if (int.TryParse(userinput, out minecount))
                {
                    while (minecount < 5 || minecount > 20 && !quit)
                    {
                        Console.Clear();
                        Console.WriteLine("WELCOME TO MINESWEEPER!\n");
                        Console.WriteLine("Enter grid size (min 8, max 50): " + gridsize);
                        Console.WriteLine("Invalid number of mines!");
                        Console.Write("Enter number of mines (min 5, max 20): ");
                        userinput = Console.ReadLine();
                        if (!int.TryParse(userinput, out minecount))
                        {
                            // they entered non-numeric
                            quit = true;
                        }
                    }
                }
            }

            if (!quit)
            {
                output = gridsize + "|" + minecount;
            }

            return output;
        }

        static void DisplayGrid(Game game, bool showAllMines = false)
        {
            string rows = "";

            Console.Clear();
            Console.WriteLine("WELCOME TO MINESWEEPER!\n");
            Console.WriteLine("COMMANDS: reveal x,y  OR  flag x,y  OR  quit\n");
            Console.WriteLine("Gridsize: " + game.GridSize);
            Console.WriteLine("Mines: " + game.NumberOfMines + "\n");

            // display the top label row
            rows = "     ";
            for (int y = 0; y < game.GridSize; y++)
            {
                rows += "  " + y + "  ";
            }
            rows += "\n";
            rows += "    ";
            for (int y = 0; y < game.GridSize; y++)
            {
                rows += "-----";
            }
            for (int y = 0; y < game.GridSize; y++)
            {
                rows += "\n";
                for (int x = 0; x < game.GridSize; x++)
                {
                    if (x == 0)
                    {
                        rows += "  " + y + " |";
                    }
                    var thisSquare = GetSquare(x, y, game.Grid);
                    if (showAllMines && thisSquare.Mined)
                    {
                        rows += "  B  ";
                    }
                    else
                    {
                        rows += "  " + thisSquare.Display + "  ";
                    }
                }
            }

            Console.WriteLine(rows);
        }
    }

    public class Square
    {
        public bool Flagged { get; set; }
        public bool Mined { get; set; }
        public bool Revealed { get; set; }
        public string Display { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Gridsize { get; set; }
        public int MineCount { get; set; }
    }

    public class Game
    {
        public int GridSize { get; set; }
        public int NumberOfMines { get; set; }
        public List<Square> Grid { get; set; }
    }
}
