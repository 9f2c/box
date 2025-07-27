namespace box;

public class Vortex : Thing
{
    public string TargetAddress { get; set; } = "";
    public bool IsEntry { get; set; } = true;
    
    public Vortex(string address, string targetAddress, bool isEntry = true)
    {
        Address = address;
        TargetAddress = targetAddress;
        IsEntry = isEntry;
        Symbol = '@';
        Color = isEntry ? (30, 144, 255) : (255, 165, 0); // Dodger blue for entry, Orange for exit
        
        if (address.Length > 0)
        {
            char lastChar = address[address.Length - 1];
            int position = lastChar - 'a';
            X = position % 5;
            Y = position / 5;
        }
    }
}
