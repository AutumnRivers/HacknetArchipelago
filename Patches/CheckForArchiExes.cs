using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using Hacknet;

using HacknetArchipelago.Static;

using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Models;

using System.Collections;
using static System.Collections.Specialized.BitVector32;

namespace HacknetArchipelago.Patches
{
    [HarmonyPatch]
    public class ReplaceFileEntry
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FileEntry))]
        [HarmonyPatch(new Type[] { typeof(string), typeof(string) })]
        [HarmonyPatch(MethodType.Constructor)]
        static bool Prefix(ref string dataEntry, ref string nameEntry)
        {
            var items = ArchipelagoItems.ItemNamesAndPortIDs;
            var receivedItems = HacknetAPMod.receivedItems;

            if (!nameEntry.EndsWith(".exe")) { return true; }

            List<string> possibleExeData = new List<string>();
            List<string> receivedData = new List<string>();

            foreach(var port in items.Values)
            {
                string fileData = PortExploits.crackExeData[port];
                possibleExeData.Add(fileData);
            }

            foreach(var rItem in receivedItems)
            {
                if (!items.ContainsKey(rItem)) { continue; }

                int itemPort = items[rItem];

                string itemData = PortExploits.crackExeData[itemPort];
                receivedData.Add(itemData);
            }

            if (possibleExeData.Contains(dataEntry) && !receivedData.Contains(dataEntry))
            {

                dataEntry = "There's usually a " + nameEntry + " here, but you haven't unlocked it yet! Venture forth in this Archipelago!";
                nameEntry = "BLOCKED_" + nameEntry;
            }

            return true;
        }
    }
}
