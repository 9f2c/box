using Newtonsoft.Json;
using System.IO;

namespace box;
public class Game
{
    public bool ShowAddressesInCurrentBox { get; set; } = false;
    public string Seed { get; set; } = "";
    public bool ShowControlsTooltip { get; set; } = false;
    private bool _showSignTooltip = false;
    private string _signTooltipText = "";
    public Player Player { get; private set; }
    private bool _justTeleported = false;
    
    private List<Thing> allThings = new List<Thing>();
    public List<Player> Players { get; private set; } = new List<Player>();
    public List<Sign> Signs { get; private set; } = new List<Sign>();
    public List<Vortex> Vortexes { get; private set; } = new List<Vortex>();

    private string GetBoxAddressFromAddress(string address)
    {
        return address.Length > 0 ? address.Substring(0, address.Length - 1) : "";
    }
    public Sign? CurrentlyEditingSign { get; private set; } = null;
    public bool IsEditingSign => CurrentlyEditingSign != null;
    public string EditBuffer { get; private set; } = "";
    
    public Game()
    {
        Player = new Player();
        Players.Add(Player);
        allThings.Add(Player);
        InitializeVortexes();
        InitializeSigns();
    }
    
    private void InitializeVortexes()
    {
        var vortexPairs = new Dictionary<string, string>
        {
            {"e", "ea"},
            {"ey", "eye"}
        };
        
        foreach (var pair in vortexPairs)
        {
            // Create entry vortex
            var entryVortex = new Vortex(pair.Key, pair.Value, true);
            Vortexes.Add(entryVortex);
            allThings.Add(entryVortex);
            
            // Create exit vortex (since IsOneWay defaults to false)
            var exitVortex = new Vortex(pair.Value, pair.Key, false);
            Vortexes.Add(exitVortex);
            allThings.Add(exitVortex);
        }
    }
    
    private void InitializeSigns()
    {
        // Example: Add a sign at position (2,2) in the root box
        var sign = new Sign(2, 2, "Welcome!", "");
        Signs.Add(sign);
        allThings.Add(sign);
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
        // SetRgbBackgroundColor(20, 20, 40); // Dark blue background
        Console.Clear();
        LoadGame(); // Auto-load on start
    }
    
    public void End()
    {
        SaveGame(); // Auto-save on exit
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
                    
                    var vortex = Vortexes.FirstOrDefault(v => v.Address == cellAddress);
                    if (vortex != null)
                    {
                        vortex.Draw();
                    }
                    else
                    {
                        // Check for signs at this position that are in the current box
                        var sign = Signs.FirstOrDefault(s => s.X == x - 1 && s.Y == y - 1 && 
                            GetBoxAddressFromAddress(s.Address) == Player.BoxAddress);
                        if (sign != null)
                        {
                            sign.Draw();
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
        
        if (IsEditingSign)
        {
            SetRgbColor(255, 255, 100); // Bright yellow
            Console.WriteLine($"Editing sign: {EditBuffer}_");
            SetRgbColor(200, 200, 200); // Light gray
            Console.WriteLine("Press Enter to save, Escape to cancel");
        }
        else if (ShowControlsTooltip)
        {
            SetRgbColor(200, 200, 200); // Light gray
            Console.WriteLine("Press T to create sign, E to edit nearby sign, Del to delete sign, G to toggle coordinates, C to toggle controls, F5 to save, F9 to load");
        }

        if (_showSignTooltip && !IsEditingSign)
        {
            SetRgbColor(255, 255, 150); // Light yellow
            Console.WriteLine($"Sign: {_signTooltipText}");
        }
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
        
        var vortex = Vortexes.FirstOrDefault(v => v.Address == Player.Address);
        if (vortex != null)
        {
            Player.SetFromAddress(vortex.TargetAddress);
            _justTeleported = true;
        }

        // Check for sign tooltip
        var signAtPosition = Signs.FirstOrDefault(s => s.X == Player.X && s.Y == Player.Y && 
            GetBoxAddressFromAddress(s.Address) == Player.BoxAddress);
        if (signAtPosition != null && !string.IsNullOrEmpty(signAtPosition.Text))
        {
            _showSignTooltip = true;
            _signTooltipText = signAtPosition.Text;
        }
        else
        {
            _showSignTooltip = false;
            _signTooltipText = "";
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
    
    public void ToggleControlsTooltip()
    {
        ShowControlsTooltip = !ShowControlsTooltip;
    }
    
    public void CreateOrEditSignAtPlayer()
    {
        // Check if there's already a sign here
        var existingSign = Signs.FirstOrDefault(s => s.X == Player.X && s.Y == Player.Y && 
            GetBoxAddressFromAddress(s.Address) == Player.BoxAddress);
        
        if (existingSign != null)
        {
            // Edit existing sign
            StartEditingSign(existingSign);
        }
        else
        {
            // Don't create if there's a vortex here
            if (Vortexes.Any(v => v.Address == Player.Address))
                return;
                
            // Create new sign
            var newSign = new Sign(Player.X, Player.Y, "", Player.BoxAddress);
            Signs.Add(newSign);
            StartEditingSign(newSign);
        }
    }

    public void StartEditingSign(Sign sign)
    {
        CurrentlyEditingSign = sign;
        sign.IsBeingEdited = true;
        EditBuffer = sign.Text;
    }

    public void StopEditingSign(bool save = false)
    {
        if (CurrentlyEditingSign != null)
        {
            if (save)
            {
                CurrentlyEditingSign.Text = EditBuffer;
            }
            CurrentlyEditingSign.IsBeingEdited = false;
            CurrentlyEditingSign = null;
            EditBuffer = "";
        }
    }

    public void DeleteNearbySign()
    {
        var nearbySign = Signs.FirstOrDefault(s => s.X == Player.X && s.Y == Player.Y && 
            GetBoxAddressFromAddress(s.Address) == Player.BoxAddress);
        if (nearbySign != null)
        {
            Signs.Remove(nearbySign);
        }
    }

    public void AddCharToEditBuffer(char c)
    {
        if (IsEditingSign && EditBuffer.Length < 50) // Limit text length
        {
            EditBuffer += c;
        }
    }

    public void RemoveCharFromEditBuffer()
    {
        if (IsEditingSign && EditBuffer.Length > 0)
        {
            EditBuffer = EditBuffer.Substring(0, EditBuffer.Length - 1);
        }
    }


    public void SaveGame(string filePath = "savegame.json")
    {
        var gameState = new GameState
        {
            Player = this.Player,
            Signs = this.Signs,
            Vortexes = this.Vortexes,
            ShowAddressesInCurrentBox = this.ShowAddressesInCurrentBox,
            ShowControlsTooltip = this.ShowControlsTooltip,
            Seed = this.Seed
        };
        
        string json = JsonConvert.SerializeObject(gameState, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    public void LoadGame(string filePath = "savegame.json")
    {
        if (!File.Exists(filePath)) return;
        
        try
        {
            string json = File.ReadAllText(filePath);
            var gameState = JsonConvert.DeserializeObject<GameState>(json);
            
            if (gameState != null)
            {
                // Remove old player from collections
                allThings.Remove(this.Player);
                Players.Clear();
                
                // Set new player
                this.Player = gameState.Player;
                Players.Add(this.Player);
                allThings.Add(this.Player);
                
                // Clear and reload signs
                foreach (var sign in Signs)
                    allThings.Remove(sign);
                Signs.Clear();
                Signs.AddRange(gameState.Signs);
                foreach (var sign in Signs)
                    allThings.Add(sign);
                
                // Clear and reload vortexes
                foreach (var vortex in Vortexes)
                    allThings.Remove(vortex);
                Vortexes.Clear();
                Vortexes.AddRange(gameState.Vortexes);
                foreach (var vortex in Vortexes)
                    allThings.Add(vortex);
                
                // Restore other properties
                this.ShowAddressesInCurrentBox = gameState.ShowAddressesInCurrentBox;
                this.ShowControlsTooltip = gameState.ShowControlsTooltip;
                this.Seed = gameState.Seed;
            }
        }
        catch (Exception)
        {
            // Silently fail if save file is corrupted
        }
    }
}
