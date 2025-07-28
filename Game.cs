using Newtonsoft.Json;
using System.IO;

namespace box;
public class Game
{
    public bool ShowAddressesInCurrentBox { get; set; } = false;
    public string Seed { get; set; } = "";
    public bool ShowAdvancedInfo { get; set; } = false;
    [JsonIgnore]
    private bool _showSignTooltip = false;
    [JsonIgnore]
    private string _signTooltipText = "";
    [JsonIgnore]
    private bool _showThingTooltip = false;
    [JsonIgnore]
    private string _thingTooltipText = "";
    [JsonIgnore]
    private (int r, int g, int b) _signTooltipColor = (255, 255, 150);

    [JsonIgnore]
    public bool IsInTeleportMode { get; private set; } = false;
    [JsonIgnore]
    private string _teleportBuffer = "";

    [JsonIgnore]
    public bool IsInDeleteByAddressMode { get; private set; } = false;
    [JsonIgnore]
    private string _deleteAddressBuffer = "";

    [JsonIgnore]
    public bool IsInCreationMode { get; private set; } = false;
    [JsonIgnore]
    private bool _isSelectingCreationType = false;
    [JsonIgnore]
    private bool _isSpecifyingVortexTarget = false;
    [JsonIgnore]
    private bool _isSpecifyingVortexDirection = false;
    [JsonIgnore]
    private string _creationBuffer = "";
    [JsonIgnore]
    private string _pendingVortexTarget = "";

    public bool IsHoldingSign { get; set; } = false;
    public Sign? HeldSign { get; set; } = null;

    public string PlayerAddress { get; set; } = "";
    
    [JsonIgnore]
    public Player Player { get; private set; }
    [JsonIgnore]
    private bool _justTeleported = false;
    
    [JsonIgnore]
    private List<Thing> allThings = new List<Thing>();
    public List<Sign> Signs { get; private set; } = new List<Sign>();
    public List<Vortex> Vortexes { get; private set; } = new List<Vortex>();

    private string GetBoxAddressFromAddress(string address)
    {
        return address.Length > 0 ? address.Substring(0, address.Length - 1) : "";
    }

    private bool IsBarrierAtPosition(int x, int y)
    {
        var signAtPosition = Signs.FirstOrDefault(s => s.X == x && s.Y == y && 
            GetBoxAddressFromAddress(s.Address) == Player.BoxAddress && 
            s.Text == "/barrier");
        return signAtPosition != null;
    }

    private bool ShouldIgnoreBarriers()
    {
        var ignoreBarrierSign = Signs.FirstOrDefault(s => 
            GetBoxAddressFromAddress(s.Address) == Player.BoxAddress && 
            s.Text == "/ignorebarriers");
        return ignoreBarrierSign != null;
    }

    [JsonIgnore]
    public Sign? CurrentlyEditingSign { get; private set; } = null;
    [JsonIgnore]
    public bool IsEditingSign => CurrentlyEditingSign != null;
    [JsonIgnore]
    public string EditBuffer { get; private set; } = "";
    
    public Game()
    {
        Player = new Player();
        allThings.Add(Player);
        ResolveDuplicateAddresses();
    }

    private void ResolveDuplicateAddresses()
    {
        var addressGroups = allThings.Where(t => t != Player).GroupBy(t => t.Address).Where(g => g.Count() > 1);
        
        foreach (var group in addressGroups)
        {
            // Keep the thing with the latest creation time, remove others
            var thingsToRemove = group.OrderBy(t => t.CreatedAt).Take(group.Count() - 1).ToList();
            
            foreach (var thingToRemove in thingsToRemove)
            {
                allThings.Remove(thingToRemove);
                
                // Remove from specific collections
                if (thingToRemove is Sign sign)
                    Signs.Remove(sign);
                else if (thingToRemove is Vortex vortex)
                    Vortexes.Remove(vortex);
            }
        }
    }
    
    private void InitializeVortexes()
    {
        // Remove all the vortex creation code - leave method empty
    }
    
    private void InitializeSigns()
    {
        // Remove the sign creation code - leave method empty
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
        
        ProcessInvisibilityCommands();

        // Draw border with gradient effect
        for (int y = 0; y < 7; y++)
        {
            for (int x = 0; x < 7; x++)
            {
                if (y == 0 || y == 6 || x == 0 || x == 6)
                {
                    // Seeded gradient border
                    var (r, g, b) = GetGradientBorderColor(x, y, Player.BoxAddress);
                    SetRgbColor(r, g, b);
                    Console.Write("█");
                }
                else if (x == Player.X + 1 && y == Player.Y + 1)
                {
                    Player.Draw();
                }
                else
                {
                    // Only process cells that are in the game area (not border)
                    if (x >= 1 && x <= 5 && y >= 1 && y <= 5)
                    {
                        int cellIndex = (y - 1) * 5 + (x - 1);
                        char letter = (char)('a' + cellIndex);
                        string cellAddress = Player.BoxAddress + letter;
                        
                        var vortex = Vortexes.FirstOrDefault(v => v.Address == cellAddress);
                        if (vortex != null && !vortex.IsInvisible)
                        {
                            vortex.Draw();
                        }
                        else
                        {
                            var sign = Signs.FirstOrDefault(s => s.X == x - 1 && s.Y == y - 1 && 
                                GetBoxAddressFromAddress(s.Address) == Player.BoxAddress);
                            if (sign != null)
                            {
                                sign.Draw();
                            }
                            else
                            {
                                // Check if there's an invisible thing at this position
                                // Check if there's an invisible thing at this position
                                var invisibleThing = allThings.FirstOrDefault(t => t.Address == cellAddress && t.IsInvisible && t != Player);
                                if (invisibleThing != null)
                                {
                                    if (ShowAddressesInCurrentBox)
                                    {
                                        SetRgbColor(100, 100, 100);
                                        Console.Write(letter);
                                    }
                                    else
                                    {
                                        Console.Write(" ");
                                    }
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
                    else
                    {
                        Console.Write(" ");  // Empty space for non-border, non-game areas
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
        var roomName = GetRoomNameForCurrentBox();
        if (roomName != null)
        {
            Console.WriteLine($"Box: {Player.BoxAddress} ({roomName})");
        }
        else
        {
            Console.WriteLine($"Box: {Player.BoxAddress}");
        }
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
        else if (ShowAdvancedInfo)
        {
            SetRgbColor(200, 200, 200); // Light gray
            Console.WriteLine(
                """
                T - place sign
                X - delete thing
                Shift+X - delete thing at address
                G - toggle coordinates
                C - toggle advanced info
                N - create thing
                F - teleport to accessible address
                V - quick vortex (two-way to another box)
                M - move sign (pick up/place)
                """
            );
        }

        if (_showSignTooltip && !IsEditingSign)
        {
            SetRgbColor(_signTooltipColor.r, _signTooltipColor.g, _signTooltipColor.b);
            Console.WriteLine($"Sign: {_signTooltipText}");
        }

        if (_showThingTooltip && !IsEditingSign && ShowAdvancedInfo)
        {
            SetRgbColor(150, 255, 150); // Light green
            Console.WriteLine($"Thing: {_thingTooltipText}");
        }

        if (IsHoldingSign && HeldSign != null)
        {
            SetRgbColor(255, 255, 100); // Bright yellow
            Console.WriteLine($"Holding sign: \"{HeldSign.Text}\"");
            SetRgbColor(200, 200, 200); // Light gray
            Console.WriteLine("Press M to place at current position");
        }

        if (IsInTeleportMode)
        {
            SetRgbColor(255, 255, 100); // Bright yellow
            Console.WriteLine($"Teleport to: {_teleportBuffer}_");
            SetRgbColor(200, 200, 200); // Light gray
            Console.WriteLine("Enter valid address (a-y letters), Enter to teleport, Escape to cancel");
        }
        else if (IsInDeleteByAddressMode)
        {
            SetRgbColor(255, 100, 100); // Bright red
            Console.WriteLine($"Delete at address: {_deleteAddressBuffer}_");
            SetRgbColor(200, 200, 200); // Light gray
            Console.WriteLine("Enter valid address (a-y letters), Enter to delete, Escape to cancel");
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
        
        if (IsValidAddressFormat(_teleportBuffer))
        {
            Player.SetFromAddress(_teleportBuffer);
            UpdatePlayerAddress(); // Add this line to ensure PlayerAddress property is updated
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

    public void StartDeleteByAddress()
    {
        IsInDeleteByAddressMode = true;
        _deleteAddressBuffer = "";
    }

    public void CancelDeleteByAddress()
    {
        IsInDeleteByAddressMode = false;
        _deleteAddressBuffer = "";
    }

    public void AddCharToDeleteAddressBuffer(char c)
    {
        if (IsInDeleteByAddressMode && c >= 'a' && c <= 'y')
        {
            _deleteAddressBuffer += c;
        }
    }

    public void RemoveCharFromDeleteAddressBuffer()
    {
        if (IsInDeleteByAddressMode && _deleteAddressBuffer.Length > 0)
        {
            _deleteAddressBuffer = _deleteAddressBuffer.Substring(0, _deleteAddressBuffer.Length - 1);
        }
    }

    public void ExecuteDeleteByAddress()
    {
        if (!IsInDeleteByAddressMode) return;

        if (IsValidAddressFormat(_deleteAddressBuffer))
        {
            var thingToDelete = allThings.FirstOrDefault(t => t.Address == _deleteAddressBuffer && t != Player);

            if (thingToDelete != null)
            {
                // Special handling for vortexes
                if (thingToDelete is Vortex vortex)
                {
                    // Only allow deletion of blue (entry) vortexes
                    if (!vortex.IsEntry)
                    {
                        // Cannot delete orange (exit) vortexes directly
                        // They are removed when their paired entry vortex is deleted
                        // For now, just return without deleting
                        IsInDeleteByAddressMode = false;
                        _deleteAddressBuffer = "";
                        return;
                    }

                    // Remove the blue vortex
                    allThings.Remove(vortex);
                    Vortexes.Remove(vortex);

                    // If it has a paired vortex, remove that too
                    if (!vortex.IsOneWay)
                    {
                        var pairedVortex = Vortexes.FirstOrDefault(v => v.Address == vortex.PairedVortexAddress);
                        if (pairedVortex != null)
                        {
                            Vortexes.Remove(pairedVortex);
                            allThings.Remove(pairedVortex);
                        }
                    }
                }
                else
                {
                    // Handle other thing types (signs, etc.)
                    allThings.Remove(thingToDelete);

                    if (thingToDelete is Sign sign)
                    {
                        Signs.Remove(sign);
                    }
                }
                SaveGame();
                UpdateTooltips(); // Refresh tooltips after deletion
            }
        }

        IsInDeleteByAddressMode = false;
        _deleteAddressBuffer = "";
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
        else if (_isSpecifyingVortexDirection)
        {
            HandleCreationDirection(c);
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
        // Check if there's already any thing here (except player)
        if (allThings.Any(t => t.Address == Player.Address && t != Player))
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
        // Check if there's already any thing here (except player)
        if (allThings.Any(t => t.Address == Player.Address && t != Player))
            return;
            
        if (isOneWay)
        {
            var entryVortex = new Vortex(Player.Address, targetAddress, true);
            entryVortex.X = Player.X;
            entryVortex.Y = Player.Y;
            Vortexes.Add(entryVortex);
            allThings.Add(entryVortex);
        }
        else
        {
            var (entry, exit) = Vortex.CreatePair(Player.Address, targetAddress);
            entry.X = Player.X;
            entry.Y = Player.Y;
            Vortexes.Add(entry);
            Vortexes.Add(exit);
            allThings.Add(entry);
            allThings.Add(exit);
        }
        
        SaveGame();
    }

    public void RemoveCharFromTeleportBuffer()
    {
        if (IsInTeleportMode && _teleportBuffer.Length > 0)
        {
            _teleportBuffer = _teleportBuffer.Substring(0, _teleportBuffer.Length - 1);
        }
    }

    private bool IsValidAddressFormat(string address)
    {
        // Valid if it contains only letters a-y and is not empty
        return !string.IsNullOrEmpty(address) && address.All(c => c >= 'a' && c <= 'y');
    }

    private (int r, int g, int b) GetSignColor(Sign sign)
    {
        if (sign.Text == "/barrier")
        {
            return (255, 100, 100); // Red for barriers
        }
        else if (sign.Text == "/ignorebarriers")
        {
            return (255, 165, 0); // Orange for ignore barriers
        }
        else if (sign.Text.StartsWith("/invisible "))
        {
            return (128, 128, 128); // Gray for invisible command signs
        }
        else if (sign.Text.StartsWith("@"))
        {
            return (100, 255, 100); // Green for room name signs
        }
        else
        {
            return (200, 200, 100); // Normal yellow
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
        }
        
        // Clamp to bounds first
        newX = Math.Clamp(newX, 0, 4);
        newY = Math.Clamp(newY, 0, 4);
        
        // Check for barrier collision (only if trying to move to a different position)
        if ((newX != Player.X || newY != Player.Y) && 
            IsBarrierAtPosition(newX, newY) && 
            !ShouldIgnoreBarriers())
        {
            // Cannot move into barrier, return early
            return;
        }
        
        Player.X = newX;
        Player.Y = newY;
        UpdatePlayerAddress();
        SaveGame();

        var vortex = Vortexes.FirstOrDefault(v => v.Address == Player.Address);
        if (vortex != null && !string.IsNullOrEmpty(vortex.TargetAddress))
        {
            Player.SetFromAddress(vortex.TargetAddress);
            _justTeleported = true;
            
            // Check for chained vortexes at the destination
            int chainCount = 0;
            const int maxChainLength = 10; // Prevent infinite loops
            
            while (chainCount < maxChainLength)
            {
                var nextVortex = Vortexes.FirstOrDefault(v => v.Address == Player.Address);
                if (nextVortex != null && !string.IsNullOrEmpty(nextVortex.TargetAddress))
                {
                    Player.SetFromAddress(nextVortex.TargetAddress);
                    chainCount++;
                }
                else
                {
                    break;
                }
            }
        }

        UpdateTooltips();
    }
    
    private void UpdateTooltips()
    {
        // Check for sign tooltip
        var signAtPosition = Signs.FirstOrDefault(s => s.X == Player.X && s.Y == Player.Y && 
            GetBoxAddressFromAddress(s.Address) == Player.BoxAddress);
        if (signAtPosition != null && !string.IsNullOrEmpty(signAtPosition.Text))
        {
            _showSignTooltip = true;
            _signTooltipText = signAtPosition.Text;
            _signTooltipColor = GetSignColor(signAtPosition);
        }
        else
        {
            _showSignTooltip = false;
            _signTooltipText = "";
        }

        var thingAtPosition = allThings.FirstOrDefault(t => t.Address == Player.Address && t != Player && !t.IsInvisible);
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
        PlayerAddress = Player.Address;
    }
    
    public void MoveLeft() => Move(-1, 0);
    public void MoveRight() => Move(1, 0);
    public void MoveUp() => Move(0, -1);
    public void MoveDown() => Move(0, 1);
    
    public void ToggleAddresses()
    {
        ShowAddressesInCurrentBox = !ShowAddressesInCurrentBox;
    }
    
    public void ToggleAdvancedInfo()
    {
        ShowAdvancedInfo = !ShowAdvancedInfo;
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
            // Check if there's any other thing here (except player)
            if (allThings.Any(t => t.Address == Player.Address && t != Player))
                return;
                
            // Create new sign
            var newSign = new Sign(Player.X, Player.Y, "", Player.BoxAddress);
            Signs.Add(newSign);
            allThings.Add(newSign); // Add this line
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
                UpdateTooltips(); // Add this line to refresh tooltips after editing
            }
            CurrentlyEditingSign.IsBeingEdited = false;
            CurrentlyEditingSign = null;
            EditBuffer = "";
        }
    }

    public void DeleteThingAtPlayer()
    {
        var thingAtPlayer = allThings.FirstOrDefault(t => t.X == Player.X && t.Y == Player.Y && 
            GetBoxAddressFromAddress(t.Address) == Player.BoxAddress && t != Player); // Exclude player itself
        
        if (thingAtPlayer != null)
        {
            // Special handling for vortexes
            if (thingAtPlayer is Vortex vortex)
            {
                // Only allow deletion of blue (entry) vortexes
                if (!vortex.IsEntry)
                {
                    return; // Can't delete orange (exit) vortexes
                }
                
                // Remove the blue vortex
                allThings.Remove(vortex);
                Vortexes.Remove(vortex);
                
                // If it has a paired vortex, remove that too
                if (!vortex.IsOneWay)
                {
                    var pairedVortex = Vortexes.FirstOrDefault(v => v.Address == vortex.PairedVortexAddress);
                    if (pairedVortex != null)
                    {
                        Vortexes.Remove(pairedVortex);
                        allThings.Remove(pairedVortex);
                    }
                }
            }
            else
            {
                // Handle other thing types (signs, etc.)
                allThings.Remove(thingAtPlayer);
                
                if (thingAtPlayer is Sign sign)
                {
                    Signs.Remove(sign);
                }
            }
            
            SaveGame();
            UpdateTooltips(); // Refresh tooltips after deletion
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

    public void CreateQuickVortex()
    {
        // Check if there's already any thing here (except player)
        if (allThings.Any(t => t.Address == Player.Address && t != Player))
            return;
            
        string targetAddress = Player.Address + "a";
        
        var (entry, exit) = Vortex.CreatePair(Player.Address, targetAddress);
        entry.X = Player.X;
        entry.Y = Player.Y;
        
        Vortexes.Add(entry);
        Vortexes.Add(exit);
        allThings.Add(entry);
        allThings.Add(exit);
        
        SaveGame();
    }

    public void ToggleMoveSign()
    {
        if (IsHoldingSign && HeldSign != null)
        {
            // Place the held sign at current position
            PlaceHeldSign();
        }
        else
        {
            // Pick up sign at current position
            PickUpSignAtPlayer();
        }
    }

    private void PickUpSignAtPlayer()
    {
        var signAtPlayer = Signs.FirstOrDefault(s => s.X == Player.X && s.Y == Player.Y && 
            GetBoxAddressFromAddress(s.Address) == Player.BoxAddress);
        
        if (signAtPlayer != null)
        {
            // Remove sign from its current position
            HeldSign = signAtPlayer;
            IsHoldingSign = true;
            
            // Remove from collections but don't delete it
            Signs.Remove(signAtPlayer);
            allThings.Remove(signAtPlayer);
            
            SaveGame();
        }
    }

    private void PlaceHeldSign()
    {
        if (HeldSign == null) return;
        
        // Check if there's already any thing here (except player)
        if (allThings.Any(t => t.Address == Player.Address && t != Player))
        {
            // Can't place here, position is occupied
            return;
        }
        
        // Update sign's position and address
        HeldSign.X = Player.X;
        HeldSign.Y = Player.Y;
        HeldSign.Address = Player.Address;
        
        // Add back to collections
        Signs.Add(HeldSign);
        allThings.Add(HeldSign);
        
        // Clear held sign
        HeldSign = null;
        IsHoldingSign = false;
        
        ResolveDuplicateAddresses();
        SaveGame();
    }

    public Vortex? GetVortexByAddress(string address)
    {
        return Vortexes.FirstOrDefault(v => v.Address == address);
    }

    public Vortex? GetPairedVortex(Vortex vortex)
    {
        if (string.IsNullOrEmpty(vortex.PairedVortexAddress))
            return null;
        return GetVortexByAddress(vortex.PairedVortexAddress);
    }

    private string? GetRoomNameForCurrentBox()
    {
        var roomNameSign = Signs.FirstOrDefault(s => 
            GetBoxAddressFromAddress(s.Address) == Player.BoxAddress && 
            s.Text.StartsWith("@"));
        
        if (roomNameSign != null && roomNameSign.Text.Length > 1)
        {
            return roomNameSign.Text.Substring(1).Trim();
        }
        
        return null;
    }

    private void ProcessInvisibilityCommands()
    {
        // Reset all things' invisibility state first (except player)
        foreach (var thing in allThings)
        {
            if (thing != Player)
            {
                thing.IsInvisible = false;
            }
        }

        // Process signs with /invisible command
        foreach (var sign in Signs)
        {
            if (sign.Text.StartsWith("/invisible "))
            {
                var parts = sign.Text.Split(' ', 2);
                if (parts.Length == 2)
                {
                    string targetAddress = parts[1].Trim();
                    var targetThing = allThings.FirstOrDefault(t => t.Address == targetAddress);
                    if (targetThing != null && targetThing != Player)
                    {
                        targetThing.IsInvisible = true;
                    }
                }
            }
        }
    }


    private int GetSeedFromBoxAddress(string boxAddress)
    {
        if (string.IsNullOrEmpty(boxAddress))
            return 0;
        
        int seed = 0;
        for (int i = 0; i < boxAddress.Length; i++)
        {
            seed = seed * 31 + (boxAddress[i] - 'a' + 1);
        }
        return Math.Abs(seed);
    }

    private (int r, int g, int b) GetGradientBorderColor(int x, int y, string boxAddress)
    {
        int seed = GetSeedFromBoxAddress(boxAddress);
        Random random = new Random(seed);
        
        // Generate 3-6 colors for smoother transitions
        int colorCount = random.Next(3, 7);
        var baseHue = random.Next(0, 360);
        var colors = new List<(int r, int g, int b)>();
        
        for (int i = 0; i < colorCount; i++)
        {
            // Spread colors more evenly around the hue circle
            int hue = (baseHue + (i * 360 / colorCount) + random.Next(-20, 21)) % 360;
            if (hue < 0) hue += 360;
            
            double saturation = 0.6 + random.NextDouble() * 0.4; // 0.6 to 1.0
            double value = 0.7 + random.NextDouble() * 0.3; // 0.7 to 1.0
            
            colors.Add(HsvToRgb(hue, saturation, value));
        }
        
        // Calculate position along the border perimeter (for 9x9 grid)
        double position;
        
        if (y == 0) // Top edge
        {
            position = x / 6.0;
        }
        else if (x == 6) // Right edge
        {
            position = 1.0 + (y / 6.0);
        }
        else if (y == 6) // Bottom edge
        {
            position = 2.0 + ((6 - x) / 6.0);
        }
        else // Left edge
        {
            position = 3.0 + ((6 - y) / 6.0);
        }
        
        // Make the gradient seamless by wrapping around
        double normalizedPosition = (position % 4.0) / 4.0; // 0.0 to 1.0
        double t = normalizedPosition * colorCount;
        
        int colorIndex = (int)t % colorCount;
        int nextColorIndex = (colorIndex + 1) % colorCount;
        double fraction = t - Math.Floor(t);
        
        var color1 = colors[colorIndex];
        var color2 = colors[nextColorIndex];
        
        int r = (int)(color1.r * (1 - fraction) + color2.r * fraction);
        int g = (int)(color1.g * (1 - fraction) + color2.g * fraction);
        int b = (int)(color1.b * (1 - fraction) + color2.b * fraction);
        
        return (r, g, b);
    }

    public void SaveGame(string filePath = "savegame.json")
    {
        string json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    public void LoadGame(string filePath = "savegame.json")
    {
        if (!File.Exists(filePath)) 
        {
            // No save file exists, start with empty world
            // Player is already initialized in constructor
            // All collections are already empty
            return;
        }
        
        try
        {
            string json = File.ReadAllText(filePath);
            var loadedGame = JsonConvert.DeserializeObject<Game>(json);
            
            if (loadedGame != null)
            {
                // Load player address and recreate player
                this.PlayerAddress = loadedGame.PlayerAddress ?? "";
                this.Player.SetFromAddress(this.PlayerAddress);
                
                this.Signs = loadedGame.Signs;
                this.Vortexes = loadedGame.Vortexes;
                this.ShowAddressesInCurrentBox = loadedGame.ShowAddressesInCurrentBox;
                this.ShowAdvancedInfo = loadedGame.ShowAdvancedInfo;
                this.Seed = loadedGame.Seed;
                
                // Rebuild allThings list
                allThings.Clear();
                allThings.Add(Player); // Add the single player
                allThings.AddRange(Signs);
                allThings.AddRange(Vortexes);

                // Restore held sign state
                if (loadedGame.IsHoldingSign && loadedGame.HeldSign != null)
                {
                    this.IsHoldingSign = loadedGame.IsHoldingSign;
                    this.HeldSign = loadedGame.HeldSign;
                    // Don't add held sign to collections since it's being held
                }
                
                ResolveDuplicateAddresses();
                SaveGame(); // Save after resolving duplicates
                UpdateTooltips(); // Initialize tooltips after loading
            }
        }
        catch (Exception)
        {
            // Silently fail if save file is corrupted
        }
    }
}
