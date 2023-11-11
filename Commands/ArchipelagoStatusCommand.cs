using Hacknet;

namespace HacknetArchipelago.Commands
{
    public class ArchipelagoStatusCommand
    {
        public static void Command(OS os, string[] args)
        {
            var session = HacknetAPMod.archiSession;

            os.terminal.writeLine("--- ARCHIPELAGO ---");

            if (session.ConnectionInfo.Slot == -1)
            {
                os.terminal.writeLine("You are not currently connected to Archipelago!");
                os.terminal.writeLine("Reconnect via the main menu.");
                return;
            }

            string playerName = session.Players.GetPlayerName(session.ConnectionInfo.Slot);

            os.terminal.writeLine($"Connected to Archipelago as {playerName}.");
        }
    }
}
