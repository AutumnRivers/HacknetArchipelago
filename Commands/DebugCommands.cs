using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hacknet;

using HarmonyLib;

namespace HacknetArchipelago.Commands
{
    public class DebugCommands
    {
        public static void CheckReceivedItems(OS os, string[] args)
        {
            List<string> items = HacknetAPMod.receivedItems;

            os.terminal.writeLine("Your received items: " + items.Join(delimiter: ","));
        }
    }
}
