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
                    if (game.IsEditingSign)
                    {
                        game.AddCharToEditBuffer(ki.KeyChar);
                    }
                    else
                    {
                        game.MoveLeft();
                    }
                    break;
                case ConsoleKey.D:
                case ConsoleKey.L:
                    if (game.IsEditingSign)
                    {
                        game.AddCharToEditBuffer(ki.KeyChar);
                    }
                    else
                    {
                        game.MoveRight();
                    }
                    break;
                case ConsoleKey.W:
                case ConsoleKey.K:
                    if (game.IsEditingSign)
                    {
                        game.AddCharToEditBuffer(ki.KeyChar);
                    }
                    else
                    {
                        game.MoveUp();
                    }
                    break;
                case ConsoleKey.S:
                    if (game.IsEditingSign)
                    {
                        game.AddCharToEditBuffer('s');
                    }
                    else
                    {
                        game.MoveDown();
                    }
                    break;
                case ConsoleKey.J:
                    if (game.IsEditingSign)
                    {
                        game.AddCharToEditBuffer(ki.KeyChar);
                    }
                    else
                    {
                        game.MoveDown();
                    }
                    break;
                case ConsoleKey.T:
                    if (!game.IsEditingSign) game.CreateSign();
                    break;
                case ConsoleKey.G:
                    if (!game.IsEditingSign) game.ToggleAddresses();
                    break;
                case ConsoleKey.C:
                    if (!game.IsEditingSign) game.ToggleControlsTooltip();
                    break;
                case ConsoleKey.E:
                    if (game.IsEditingSign)
                    {
                        game.AddCharToEditBuffer('e');
                    }
                    else
                    {
                        game.EditNearbySign();
                    }
                    break;
                case ConsoleKey.Delete:
                    if (game.IsEditingSign)
                    {
                        // Delete key should still work as backspace during editing
                        game.RemoveCharFromEditBuffer();
                    }
                    else
                    {
                        game.DeleteNearbySign();
                    }
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
                    if (game.IsEditingSign)
                    {
                        game.AddCharToEditBuffer('q');
                    }
                    else
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
