namespace box;

public class Vortex : Thing
{
    public string EntryAddress { get; set; } = "";
    public string ExitAddress { get; set; } = "";
    public bool IsOneWay { get; set; } = false;
    
    public Vortex(string entryAddress, string exitAddress, bool isOneWay = false)
    {
        EntryAddress = entryAddress;
        ExitAddress = exitAddress;
        IsOneWay = isOneWay;
        Symbol = '@';
        Color = (30, 144, 255); // Blue color for all vortexes
        Address = entryAddress; // Set Address to entry point for compatibility
    }
    
    public string GetTargetAddress(string fromAddress)
    {
        if (fromAddress == EntryAddress)
            return ExitAddress;
        else if (!IsOneWay && fromAddress == ExitAddress)
            return EntryAddress;
        return "";
    }
    
    public bool HasEndpointAt(string address)
    {
        return address == EntryAddress || (!IsOneWay && address == ExitAddress);
    }
}
