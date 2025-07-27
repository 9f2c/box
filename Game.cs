namespace box;
public class Game
{
    public bool ShowAddressesInCurrentBox { get; set; } = false;
    public string Seed { get; set; } = "";
    public Player Player { get; private set; }
    public Dictionary<string, Vortex> Vortexes { get; private set; }
    private bool _justTeleported = false;
    
    public Game()
    {
        Player = new Player();
        InitializeVortexes();
    }
    
    private void InitializeVortexes()
    {
        var vortexPairs = new Dictionary<string, string>
        {
            {"e", "ea"},
            {"ey", "eye"}
        };
        
        Vortexes = new Dictionary<string, Vortex>();
        
        foreach (var pair in vortexPairs)
        {
            Vortexes[pair.Key] = new Vortex(pair.Key, pair.Value, true);
            Vortexes[pair.Value] = new Vortex(pair.Value, pair.Key, false);
        }
    }
    
    public static void SetRgbColor(int r, int g, int b)
    {
        Console.Write($"\x1b[38;2;{r};{g};{b}m");
    }
    
    public static void SetRgbBackgroundColor(int r, int g, int b)
    {
        Console.Write($"\x1b[48;2;{r};{g};{b}m");
    }
    
    public static void ResetColor()
    {
        Console.Write("\x1b[0m");
    }
    
    public void Start()
    {
        Console.CursorVisible = false;
        SetRgbBackgroundColor(20, 20, 40); // Dark blue background
        Console.Clear();
    }
    
    public void End()
    {
        Console.CursorVisible = true;
        ResetColor();
    }
    
    public void Draw()
    {
        Console.Clear();
        Console.SetCursorPosition(0, 0);
        
        // Draw border with gradient effect
        for (int y = 0; y < 7; y++)
        {
            for (int x = 0; x < 7; x++)
            {
                if (y == 0 || y == 6 || x == 0 || x == 6)
                {
                    // Rainbow border
                    int hue = (x + y) * 30 % 360;
                    var (r, g, b) = HsvToRgb(hue, 1.0, 1.0);
                    SetRgbColor(r, g, b);
                    Console.Write("█");
                }
                else if (x == Player.X + 1 && y == Player.Y + 1)
                {
                    Player.Draw();
                }
                else
                {
                    int cellIndex = (y - 1) * 5 + (x - 1);
                    char letter = (char)('a' + cellIndex);
                    string cellAddress = Player.BoxAddress + letter;
                    
                    if (Vortexes.ContainsKey(cellAddress))
                    {
                        Vortexes[cellAddress].Draw();
                    }
                    else if (ShowAddressesInCurrentBox)
                    {
                        SetRgbColor(100, 100, 100);
                        Console.Write(letter);
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                }
            }
            Console.WriteLine();
        }
        
        ResetColor();
        Console.WriteLine();
        SetRgbColor(100, 255, 100); // Bright green
        Console.WriteLine($"Address: {Player.Address}");
        SetRgbColor(150, 150, 255); // Light blue
        Console.WriteLine($"Box: {Player.BoxAddress}");
        SetRgbColor(255, 200, 100); // Orange
        Console.WriteLine($"Position: ({Player.X}, {Player.Y})");
        ResetColor();
    }
    
    private static (int r, int g, int b) HsvToRgb(double h, double s, double v)
    {
        int i = (int)(h / 60) % 6;
        double f = h / 60 - i;
        double p = v * (1 - s);
        double q = v * (1 - f * s);
        double t = v * (1 - (1 - f) * s);
        
        return i switch
        {
            0 => ((int)(v * 255), (int)(t * 255), (int)(p * 255)),
            1 => ((int)(q * 255), (int)(v * 255), (int)(p * 255)),
            2 => ((int)(p * 255), (int)(v * 255), (int)(t * 255)),
            3 => ((int)(p * 255), (int)(q * 255), (int)(v * 255)),
            4 => ((int)(t * 255), (int)(p * 255), (int)(v * 255)),
            _ => ((int)(v * 255), (int)(p * 255), (int)(q * 255)),
        };
    }
    
    public void Move(int offsetX, int offsetY)
    {
        int newX = Player.X + offsetX;
        int newY = Player.Y + offsetY;
        
        if (_justTeleported)
        {
            _justTeleported = false;
            Player.X = Math.Clamp(newX, 0, 4);
            Player.Y = Math.Clamp(newY, 0, 4);
            UpdatePlayerAddress();
            return;
        }
        
        Player.X = Math.Clamp(newX, 0, 4);
        Player.Y = Math.Clamp(newY, 0, 4);
        UpdatePlayerAddress();
        
        if (Vortexes.ContainsKey(Player.Address))
        {
            var vortex = Vortexes[Player.Address];
            Player.SetFromAddress(vortex.TargetAddress);
            _justTeleported = true;
        }
    }
    
    private void UpdatePlayerAddress()
    {
        char positionChar = (char)('a' + Player.Y * 5 + Player.X);
        Player.Address = Player.BoxAddress + positionChar;
    }
    
    public void MoveLeft() => Move(-1, 0);
    public void MoveRight() => Move(1, 0);
    public void MoveUp() => Move(0, -1);
    public void MoveDown() => Move(0, 1);
    
    public void ToggleAddresses()
    {
        ShowAddressesInCurrentBox = !ShowAddressesInCurrentBox;
    }
}
