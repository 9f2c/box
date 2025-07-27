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
                case ConsoleKey.Enter:
                    if (game.IsEditingSign) 
                    {
                        game.StopEditingSign(true);
                    }
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
                case ConsoleKey.Backspace:
                    if (game.IsEditingSign) 
                    {
                        game.RemoveCharFromEditBuffer();
                    }
                    break;
                default:
                    if (game.IsEditingSign)
                    {
                        // When editing, all other keys go to the sign text
                        if (ki.KeyChar != '\0' && !char.IsControl(ki.KeyChar))
                        {
                            game.AddCharToEditBuffer(ki.KeyChar);
                        }
                    }
                    else
                    {
                        // When not editing, handle normal game controls
                        HandleGameControls(game, key, ki.KeyChar);
                    }
                    break;
            }
            game.Draw();
        }
    }

    private static void HandleGameControls(Game game, ConsoleKey key, char keyChar)
    {
        switch (key)
        {
            case ConsoleKey.A: case ConsoleKey.H: game.MoveLeft(); break;
            case ConsoleKey.D: case ConsoleKey.L: game.MoveRight(); break;
            case ConsoleKey.W: case ConsoleKey.K: game.MoveUp(); break;
            case ConsoleKey.S: case ConsoleKey.J: game.MoveDown(); break;
            case ConsoleKey.T: game.CreateOrEditSignAtPlayer(); break;
            case ConsoleKey.G: game.ToggleAddresses(); break;
            case ConsoleKey.C: game.ToggleControlsTooltip(); break;
            case ConsoleKey.Delete: game.DeleteNearbySign(); break;
            case ConsoleKey.Q: game.End(); return;
        }
    }
}
