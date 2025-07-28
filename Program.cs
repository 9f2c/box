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
                    else if (game.IsInTeleportMode)
                    {
                        game.ExecuteTeleport();
                    }
                    else if (game.IsInDeleteByAddressMode)
                    {
                        game.ExecuteDeleteByAddress();
                    }
                    else if (game.IsInCreationMode)
                    {
                        game.HandleCreationEnter();
                    }
                    break;
                case ConsoleKey.Escape:
                    if (game.IsEditingSign)
                    {
                        game.StopEditingSign(false);
                    }
                    else if (game.IsInTeleportMode)
                    {
                        game.CancelTeleport();
                    }
                    else if (game.IsInDeleteByAddressMode)
                    {
                        game.CancelDeleteByAddress();
                    }
                    else if (game.IsInCreationMode)
                    {
                        game.CancelCreation();
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
                    else if (game.IsInTeleportMode)
                    {
                        game.RemoveCharFromTeleportBuffer();
                    }
                    else if (game.IsInDeleteByAddressMode)
                    {
                        game.RemoveCharFromDeleteAddressBuffer();
                    }
                    else if (game.IsInCreationMode)
                    {
                        game.HandleCreationBackspace();
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
                    else if (game.IsInTeleportMode)
                    {
                        // When in teleport mode, only allow a-y characters
                        if (ki.KeyChar >= 'a' && ki.KeyChar <= 'y')
                        {
                            game.AddCharToTeleportBuffer(ki.KeyChar);
                        }
                    }
                    else if (game.IsInDeleteByAddressMode)
                    {
                        // When in delete by address mode, only allow a-y characters
                        if (ki.KeyChar >= 'a' && ki.KeyChar <= 'y')
                        {
                            game.AddCharToDeleteAddressBuffer(ki.KeyChar);
                        }
                    }
                    else if (game.IsInCreationMode)
                    {
                        // Handle creation mode input
                        if (ki.KeyChar >= '1' && ki.KeyChar <= '2')
                        {
                            if (game.IsInCreationMode && ki.KeyChar == '1' || ki.KeyChar == '2')
                            {
                                game.HandleCreationInput(ki.KeyChar);
                            }
                            else
                            {
                                game.HandleCreationDirection(ki.KeyChar);
                            }
                        }
                        else if (ki.KeyChar >= 'a' && ki.KeyChar <= 'y')
                        {
                            game.HandleCreationInput(ki.KeyChar);
                        }
                    }
                    else
                    {
                        // When not editing, teleporting, or creating, handle normal game controls
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
            case ConsoleKey.X:
                // Note: We can't check modifiers in HandleGameControls since we don't have ConsoleKeyInfo
                // For now, just handle the basic delete functionality
                game.DeleteThingAtPlayer();
                break;
            case ConsoleKey.F: game.StartTeleportMode(); break;
            case ConsoleKey.N: game.StartCreationMode(); break;
            case ConsoleKey.V: game.CreateQuickVortex(); break;
            case ConsoleKey.Q: game.End(); return;
        }
    }
}
