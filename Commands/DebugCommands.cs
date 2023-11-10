using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hacknet;

using HarmonyLib;

using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Packets;

namespace HacknetArchipelago.Commands
{
    public class DebugCommands
    {

        public static void CheckReceivedItems(OS os, string[] args)
        {
            if(!OS.DEBUG_COMMANDS) { return; }

            List<string> items = HacknetAPMod.receivedItems;

            os.terminal.writeLine("Your received items: " + items.Join(delimiter: ","));
        }

        public static void TestFakeConnect(OS os, string[] args)
        {
            if (!OS.DEBUG_COMMANDS) { return; }

            HacknetAPMod.FakeConnectTrap("Test");
        }

        public static void SendTestPacket(OS os, string[] args)
        {
            if (!OS.DEBUG_COMMANDS) { return; }

            var session = HacknetAPMod.archiSession;

            session.Socket.SendPacket(new BouncePacket());
            session.Socket.SendPacket(new SayPacket() { Text = "Hello, Archipelago!" });
        }
    }
}
