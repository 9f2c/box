using System;
using System.Collections.Generic;
using System.Linq;
namespace box;

class Program
{
    public static void Main(string[] args)
    {
        var game = new Game();
        game.Start();
        game.Draw();
        
        while (true)
        {
            var ki = Console.ReadKey(true);
            var key = ki.Key;
            switch (key)
            {
                case ConsoleKey.A:
                case ConsoleKey.H:
                    if (!game.IsEditingSign) game.MoveLeft();
                    break;
                case ConsoleKey.D:
                case ConsoleKey.L:
                    if (!game.IsEditingSign) game.MoveRight();
                    break;
                case ConsoleKey.W:
                case ConsoleKey.K:
                    if (!game.IsEditingSign) game.MoveUp();
                    break;
                case ConsoleKey.S:
                    if (game.IsEditingSign)
                    {
                        game.AddCharToEditBuffer('s');
                    }
                    else if (ki.Modifiers == ConsoleModifiers.None) // Only if no modifiers
                    {
                        game.CreateSign();
                    }
                    else
                    {
                        game.MoveDown();
                    }
                    break;
                case ConsoleKey.J:
                    if (!game.IsEditingSign) game.MoveDown();
                    break;
                case ConsoleKey.T:
                    if (!game.IsEditingSign) game.ToggleAddresses();
                    break;
                case ConsoleKey.E:
                    if (!game.IsEditingSign) game.EditNearbySign();
                    break;
                case ConsoleKey.Delete:
                    if (!game.IsEditingSign) game.DeleteNearbySign();
                    break;
                case ConsoleKey.Enter:
                    if (game.IsEditingSign) game.StopEditingSign(true);
                    break;
                case ConsoleKey.Backspace:
                    if (game.IsEditingSign) game.RemoveCharFromEditBuffer();
                    break;
                case ConsoleKey.Escape:
                    if (game.IsEditingSign)
                    {
                        game.StopEditingSign(false);
                    }
                    else
                    {
                        game.End();
                        return;
                    }
                    break;
                case ConsoleKey.Q:
                    if (!game.IsEditingSign)
                    {
                        game.End();
                        return;
                    }
                    break;
                default:
                    // Handle regular character input for editing
                    if (game.IsEditingSign && ki.KeyChar != '\0' && !char.IsControl(ki.KeyChar))
                    {
                        game.AddCharToEditBuffer(ki.KeyChar);
                    }
                    break;
            }
            game.Draw();
        }
    }
}
