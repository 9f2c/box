namespace box;

class Program
{
    public static string PlayerAddress
    {
        get => CurrentBoxAddress + (char)('a' + PlayerY * 5 + PlayerX);
        set
        {
            if (value.Length > 0)
            {
                char lastChar = value[value.Length - 1];
                CurrentBoxAddress = value.Substring(0, value.Length - 1);
                
                int position = lastChar - 'a';
                PlayerX = position % 5;
                PlayerY = position / 5;
            }
        }
    }
    public static bool ShowAddressesInCurrentBox = false;
    public static string CurrentBoxAddress = "";
    public static string Seed = "";
    public static int PlayerX = 0; // 0-4
    public static int PlayerY = 0; // 0-4
    public static void Main(string[] args)
    {
        Start();
        Draw();
        while (true)
        {
            var ki = Console.ReadKey(true);
            var key = ki.Key;
            switch (key)
            {
                case ConsoleKey.A:
                case ConsoleKey.H:
                    MoveLeft();
                    break;
                case ConsoleKey.D:
                case ConsoleKey.L:
                    MoveRight();
                    break;
                case ConsoleKey.W:
                case ConsoleKey.K:
                    MoveUp();
                    break;
                case ConsoleKey.S:
                case ConsoleKey.J:
                    MoveDown();
                    break;
            }
            Draw();
        }
        End();
    }

    public static void Start()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.CursorVisible = false;
    }

    public static void End()
    {
        Console.CursorVisible = true;
    }

    public static void Draw()
    {
        Console.Clear();
        Console.SetCursorPosition(0, 0);
        
        for (int y = 0; y < 7; y++)
        {
            for (int x = 0; x < 7; x++)
            {
                if (y == 0 || y == 6 || x == 0 || x == 6)
                {
                    Console.Write("█");
                }
                else if (x == PlayerX + 1 && y == PlayerY + 1)
                {
                    Console.Write("o");
                }
                else if (ShowAddressesInCurrentBox)
                {
                    int cellIndex = (y - 1) * 5 + (x - 1);
                    char letter = (char)('a' + cellIndex);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(letter);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.Write(" ");
                }
            }
            Console.WriteLine();
        }
        Console.WriteLine();
        Console.WriteLine($"Address: {PlayerAddress}");
    }

    public static void Move(int offsetX, int offsetY)
    {
        PlayerX += offsetX;
        PlayerY += offsetY;
        PlayerY = Math.Clamp(PlayerY, 0, 4);
        PlayerX = Math.Clamp(PlayerX, 0, 4);
    }
    
    public static void MoveLeft() => Move(-1, 0);
    public static void MoveRight() => Move(1, 0);
    public static void MoveUp() => Move(0, -1);
    public static void MoveDown() => Move(0, 1);
}
