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
    private bool _showThingTooltip = false;
    private string _thingTooltipText = "";

    public bool IsInTeleportMode { get; private set; } = false;
    private string _teleportBuffer = "";

    public bool IsInCreationMode { get; private set; } = false;
    private bool _isSelectingCreationType = false;
    private bool _isCreatingVortex = false;
    private bool _isSpecifyingVortexTarget = false;
    private bool _isSpecifyingVortexDirection = false;
    private string _creationBuffer = "";
    private string _pendingVortexTarget = "";

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

    private void ResolveDuplicateAddresses()
    {
        var addressGroups = allThings.GroupBy(t => t.Address).Where(g => g.Count() > 1);
        
        foreach (var group in addressGroups)
        {
            // Keep the thing with the latest creation time, remove others
            var thingsToRemove = group.OrderBy(t => t.CreatedAt).Take(group.Count() - 1).ToList();
            
            foreach (var thingToRemove in thingsToRemove)
            {
                allThings.Remove(thingToRemove);
                
                // Remove from specific collections
                if (thingToRemove is Player player)
                    Players.Remove(player);
                else if (thingToRemove is Sign sign)
                    Signs.Remove(sign);
                else if (thingToRemove is Vortex vortex)
                    Vortexes.Remove(vortex);
            }
        }
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
            Console.WriteLine("Press T to create sign, E to edit nearby sign, Del to delete sign, G to toggle coordinates, C to toggle controls");
        }

        if (_showSignTooltip && !IsEditingSign)
        {
            SetRgbColor(255, 255, 150); // Light yellow
            Console.WriteLine($"Sign: {_signTooltipText}");
        }

        if (_showThingTooltip && !IsEditingSign)
        {
            SetRgbColor(150, 255, 150); // Light green
            Console.WriteLine($"Thing: {_thingTooltipText}");
        }

        if (IsInTeleportMode)
        {
            SetRgbColor(255, 255, 100); // Bright yellow
            Console.WriteLine($"Teleport to: {_teleportBuffer}_");
            SetRgbColor(200, 200, 200); // Light gray
            Console.WriteLine("Enter valid address (a-y letters), Enter to teleport, Escape to cancel");
        }

        if (IsInCreationMode)
        {
            SetRgbColor(255, 255, 100); // Bright yellow
            if (_isSelectingCreationType)
            {
                Console.WriteLine("Create: 1=Vortex, 2=Sign, Escape to cancel");
            }
            else if (_isSpecifyingVortexTarget)
            {
                Console.WriteLine($"Vortex target address: {_creationBuffer}_");
                SetRgbColor(200, 200, 200); // Light gray
                Console.WriteLine("Enter valid address (a-y letters), Enter to confirm, Escape to cancel");
            }
            else if (_isSpecifyingVortexDirection)
            {
                Console.WriteLine($"Target: {_pendingVortexTarget} | 1=One-way, 2=Two-way, Escape to cancel");
            }
        }

        SaveGame();
    }


    public void StartTeleportMode()
    {
        IsInTeleportMode = true;
        _teleportBuffer = "";
    }

    public void CancelTeleport()
    {
        IsInTeleportMode = false;
        _teleportBuffer = "";
    }

    public void ExecuteTeleport()
    {
        if (!IsInTeleportMode) return;
        
        if (IsValidTeleportTarget(_teleportBuffer))
        {
            Player.SetFromAddress(_teleportBuffer);
            _justTeleported = true; // Set this to true so the next move doesn't change the box
            SaveGame();
        }
        
        IsInTeleportMode = false;
        _teleportBuffer = "";
    }

    public void AddCharToTeleportBuffer(char c)
    {
        if (IsInTeleportMode && c >= 'a' && c <= 'y')
        {
            _teleportBuffer += c;
        }
    }

    public void StartCreationMode()
    {
        IsInCreationMode = true;
        _isSelectingCreationType = true;
        _creationBuffer = "";
    }

    public void CancelCreation()
    {
        IsInCreationMode = false;
        _isSelectingCreationType = false;
        _isCreatingVortex = false;
        _isSpecifyingVortexTarget = false;
        _isSpecifyingVortexDirection = false;
        _creationBuffer = "";
        _pendingVortexTarget = "";
    }

    public void HandleCreationInput(char c)
    {
        if (_isSelectingCreationType)
        {
            if (c == '1')
            {
                // Create vortex
                _isSelectingCreationType = false;
                _isCreatingVortex = true;
                _isSpecifyingVortexTarget = true;
                _creationBuffer = "";
            }
            else if (c == '2')
            {
                // Create sign
                CreateSignAtPlayerForCreation();
                CancelCreation();
            }
        }
        else if (_isSpecifyingVortexTarget && c >= 'a' && c <= 'y')
        {
            _creationBuffer += c;
        }
    }

    public void HandleCreationEnter()
    {
        if (_isSpecifyingVortexTarget && _creationBuffer.Length > 0)
        {
            _pendingVortexTarget = _creationBuffer;
            _isSpecifyingVortexTarget = false;
            _isSpecifyingVortexDirection = true;
            _creationBuffer = "";
        }
    }

    public void HandleCreationBackspace()
    {
        if (_isSpecifyingVortexTarget && _creationBuffer.Length > 0)
        {
            _creationBuffer = _creationBuffer.Substring(0, _creationBuffer.Length - 1);
        }
    }

    public void HandleCreationDirection(char c)
    {
        if (_isSpecifyingVortexDirection)
        {
            if (c == '1') // One-way
            {
                CreateVortexAtPlayer(_pendingVortexTarget, true);
                CancelCreation();
            }
            else if (c == '2') // Two-way
            {
                CreateVortexAtPlayer(_pendingVortexTarget, false);
                CancelCreation();
            }
        }
    }

    private void CreateSignAtPlayerForCreation()
    {
        // Don't create if there's already a vortex here
        if (Vortexes.Any(v => v.Address == Player.Address))
            return;
            
        var newSign = new Sign(Player.X, Player.Y, "", Player.BoxAddress);
        Signs.Add(newSign);
        allThings.Add(newSign);
        StartEditingSign(newSign);
        ResolveDuplicateAddresses();
        SaveGame();
    }

    private void CreateVortexAtPlayer(string targetAddress, bool isOneWay)
    {
        // Don't create if there's already a vortex or sign here
        if (Vortexes.Any(v => v.Address == Player.Address) || 
            Signs.Any(s => s.Address == Player.Address))
            return;
            
        var newVortex = new Vortex(Player.Address, targetAddress, true);
        newVortex.IsOneWay = isOneWay;
        Vortexes.Add(newVortex);
        allThings.Add(newVortex);
        
        // If two-way, create the return vortex
        if (!isOneWay)
        {
            var returnVortex = new Vortex(targetAddress, Player.Address, false);
            returnVortex.IsOneWay = false;
            Vortexes.Add(returnVortex);
            allThings.Add(returnVortex);
        }
        
        ResolveDuplicateAddresses();
        SaveGame();
    }

    public void RemoveCharFromTeleportBuffer()
    {
        if (IsInTeleportMode && _teleportBuffer.Length > 0)
        {
            _teleportBuffer = _teleportBuffer.Substring(0, _teleportBuffer.Length - 1);
        }
    }

    private bool IsValidTeleportTarget(string address)
    {
        // Valid if it's a single letter (origin box)
        if (address.Length == 1) return true;
        
        // Check if there's a vortex path leading to this address
        return HasVortexPathTo(address);
    }

    private bool HasVortexPathTo(string targetAddress)
    {
        // Use BFS to find if there's a path from any origin box position to the target
        var visited = new HashSet<string>();
        var queue = new Queue<string>();
        
        // Start from all positions in origin box
        for (char c = 'a'; c <= 'y'; c++)
        {
            queue.Enqueue(c.ToString());
            visited.Add(c.ToString());
        }
        
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            
            if (current == targetAddress)
                return true;
            
            // Find vortexes at this address
            var vortex = Vortexes.FirstOrDefault(v => v.Address == current);
            if (vortex != null && !visited.Contains(vortex.TargetAddress))
            {
                visited.Add(vortex.TargetAddress);
                queue.Enqueue(vortex.TargetAddress);
            }
        }
        
        return false;
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
        SaveGame();

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

        var thingAtPosition = allThings.FirstOrDefault(t => t.Address == Player.Address && t != Player);
        if (thingAtPosition != null)
        {
            _showThingTooltip = true;
            _thingTooltipText = $"Created: {thingAtPosition.CreatedAt:yyyy-MM-dd HH:mm:ss}";
        }
        else
        {
            _showThingTooltip = false;
            _thingTooltipText = "";
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
        ResolveDuplicateAddresses();
        SaveGame();
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
                SaveGame();
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
            SaveGame();
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
            // Remove Vortexes from saving - they're always the same
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
                
                // Recreate vortexes instead of loading them
                foreach (var vortex in Vortexes)
                    allThings.Remove(vortex);
                Vortexes.Clear();
                InitializeVortexes(); // This recreates the standard vortexes
                
                // Restore other properties
                this.ShowAddressesInCurrentBox = gameState.ShowAddressesInCurrentBox;
                this.ShowControlsTooltip = gameState.ShowControlsTooltip;
                this.Seed = gameState.Seed;

                ResolveDuplicateAddresses();
                SaveGame(); // Save after resolving duplicates
            }
        }
        catch (Exception)
        {
            // Silently fail if save file is corrupted
        }
    }
}

public class GameState
{
    public Player Player { get; set; }
    public List<Sign> Signs { get; set; }
    // Remove Vortexes - they will be recreated from InitializeVortexes()
    public bool ShowAddressesInCurrentBox { get; set; }
    public bool ShowControlsTooltip { get; set; }
    public string Seed { get; set; }
}
