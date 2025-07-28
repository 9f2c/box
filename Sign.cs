namespace box;

public class Sign : Thing
{
    public string Text { get; set; } = "";
    public bool IsBeingEdited { get; set; } = false;
    
    public Sign(int x, int y, string text = "", string boxAddress = "")
    {
        X = x;
        Y = y;
        Text = text;
        Symbol = '■';
        Color = (200, 200, 100); // Yellow-ish color
        
        // Set the sign's address
        char positionChar = (char)('a' + y * 5 + x);
        Address = boxAddress + positionChar;
    }
    
    public override void Draw()
    {
        if (IsBeingEdited)
        {
            // Blinking effect when being edited
            Color = (255, 255, 255); // White when editing
        }
        else if (Text.StartsWith("@"))
        {
            Color = (100, 255, 100); // Green for room name signs
        }
        else
        {
            Color = (200, 200, 100); // Normal yellow
        }
        
        base.Draw();
    }
}
