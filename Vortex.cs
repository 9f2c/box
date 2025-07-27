namespace box;

public class Vortex : Thing
{
    public string TargetAddress { get; set; } = "";
    public bool IsEntry { get; set; } = true;
    public bool IsOneWay { get; set; } = false;
    
    public Vortex(string address, string targetAddress, bool isEntry = true)
    {
        Address = address;
        TargetAddress = targetAddress;
        IsOneWay = false; // Default to bidirectional
        Symbol = '@';
        Color = isEntry ? (30, 144, 255) : (255, 165, 0); // Keep color logic for now
        
    }
}
