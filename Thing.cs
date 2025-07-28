namespace box;

public class Thing
{
    public string Address { get; set; } = "";
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public char Symbol { get; set; } = ' ';
    public (int r, int g, int b) Color { get; set; } = (255, 255, 255);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsInvisible { get; set; } = false;
    
    public virtual void Update() { }
    public virtual void Draw()
    {
        if (IsInvisible) return;
        Console.Write($"\x1b[38;2;{Color.r};{Color.g};{Color.b}m{Symbol}\x1b[0m");
    }
}
