namespace box;

public class Sign : Thing
{
    public string Text { get; set; } = "";
    public bool IsBeingEdited { get; set; } = false;
    
    public Sign(int x, int y, string text = "")
    {
        X = x;
        Y = y;
        Text = text;
        Symbol = '■';
        Color = (200, 200, 100); // Yellow-ish color
    }
    
    public override void Draw()
    {
        if (IsBeingEdited)
        {
            // Blinking effect when being edited
            Color = (255, 255, 255); // White when editing
        }
        else
        {
            Color = (200, 200, 100); // Normal yellow
        }
        
        base.Draw();
    }
}
