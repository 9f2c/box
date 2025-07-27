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
                    game.MoveLeft();
                    break;
                case ConsoleKey.D:
                case ConsoleKey.L:
                    game.MoveRight();
                    break;
                case ConsoleKey.W:
                case ConsoleKey.K:
                    game.MoveUp();
                    break;
                case ConsoleKey.S:
                case ConsoleKey.J:
                    game.MoveDown();
                    break;
                case ConsoleKey.T:
                    game.ToggleAddresses();
                    break;
                case ConsoleKey.Escape:
                case ConsoleKey.Q:
                    game.End();
                    return;
            }
            game.Draw();
        }
    }
}
