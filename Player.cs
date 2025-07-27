namespace box;
public class Player : Thing
{
    public Player()
    {
        Symbol = 'o';
        Color = (255, 100, 255); // Bright magenta
    }
    
    public string BoxAddress
    {
        get => Address.Length > 0 ? Address.Substring(0, Address.Length - 1) : "";
        set
        {
            if (Address.Length > 0)
            {
                char lastChar = Address[Address.Length - 1];
                Address = value + lastChar;
            }
            else
            {
                Address = value + 'a';
            }
        }
    }
    
    public void SetPosition(int x, int y, string boxAddress)
    {
        X = x;
        Y = y;
        char positionChar = (char)('a' + y * 5 + x);
        Address = boxAddress + positionChar;
    }
    
    public void SetFromAddress(string address)
    {
        if (address.Length > 0)
        {
            char lastChar = address[address.Length - 1];
            Address = address;
            
            int position = lastChar - 'a';
            X = position % 5;
            Y = position / 5;
        }
    }
}
