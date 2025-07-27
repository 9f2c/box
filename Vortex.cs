namespace box;

public class Vortex : Thing
{
    public string TargetAddress { get; set; } = "";
    public Vortex? PairedVortex { get; set; } = null;
    public bool IsEntry { get; set; } = true;

    public Vortex(string address, string targetAddress, bool isEntry = true)
    {
        Address = address;
        TargetAddress = targetAddress;
        IsEntry = isEntry;
        Symbol = '@';
        Color = isEntry ? (30, 144, 255) : (255, 165, 0); // Blue for entry, orange for exit
    }

    public static (Vortex entry, Vortex exit) CreatePair(string entryAddress, string exitAddress, bool isOneWay = false)
    {
        var entry = new Vortex(entryAddress, exitAddress, true);
        var exit = new Vortex(exitAddress, entryAddress, false);
        
        entry.PairedVortex = exit;
        exit.PairedVortex = entry; // Always pair both directions for the objects, even if one-way
            
        return (entry, exit);
    }
}
