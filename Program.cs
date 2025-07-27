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
    public static Dictionary<string, string> Vortexes = new()
    {
        {"e", "ea"},
        {"ey", "eye"}
    };
    
    private static readonly Dictionary<string, string> ReverseVortexes = 
        Vortexes.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    
    private static bool _justTeleported = false;
    
    public static string GetVortexTarget(string vortexAddress)
    {
        if (Vortexes.TryGetValue(vortexAddress, out string target))
        {
            return target;
        }
        
        ReverseVortexes.TryGetValue(vortexAddress, out string reverseTarget);
        return reverseTarget;
    }
    
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
                else
                {
                    int cellIndex = (y - 1) * 5 + (x - 1);
                    char letter = (char)('a' + cellIndex);
                    string cellAddress = CurrentBoxAddress + letter;
                    
                    bool isVortex = false;
                    bool isEntry = false;
                    
                    if (Vortexes.ContainsKey(cellAddress))
                    {
                        isVortex = true;
                        isEntry = true;
                    }
                    else if (ReverseVortexes.ContainsKey(cellAddress))
                    {
                        isVortex = true;
                        isEntry = false;
                    }
                    
                    if (isVortex)
                    {
                        Console.ForegroundColor = isEntry ? ConsoleColor.Cyan : ConsoleColor.Yellow;
                        Console.Write("@");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (ShowAddressesInCurrentBox)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(letter);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                }
            }
            Console.WriteLine();
        }
        Console.WriteLine();
        Console.WriteLine($"Address: {PlayerAddress}");
    }

    public static void Move(int offsetX, int offsetY)
    {
        int newX = PlayerX + offsetX;
        int newY = PlayerY + offsetY;
        
        if (_justTeleported)
        {
            _justTeleported = false;
            PlayerX = Math.Clamp(newX, 0, 4);
            PlayerY = Math.Clamp(newY, 0, 4);
            return;
        }
        
        PlayerX = Math.Clamp(newX, 0, 4);
        PlayerY = Math.Clamp(newY, 0, 4);
        
        string vortexTarget = GetVortexTarget(PlayerAddress);
        if (!string.IsNullOrEmpty(vortexTarget))
        {
            PlayerAddress = vortexTarget;
            _justTeleported = true;
        }
    }
    
    public static void MoveLeft() => Move(-1, 0);
    public static void MoveRight() => Move(1, 0);
    public static void MoveUp() => Move(0, -1);
    public static void MoveDown() => Move(0, 1);
}
