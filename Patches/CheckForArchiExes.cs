using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using Hacknet;

using HacknetArchipelago.Static;

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
            var session = HacknetAPMod.archiSession;

            if (!nameEntry.EndsWith(".exe") || session.ConnectionInfo.Slot == -1) { return true; }

            List<string> possibleExeData = new List<string>();
            List<string> receivedData = new List<string>();

            foreach(var item in items)
            {
                int playerExecs = int.Parse(session.DataStorage.GetSlotData()["victory_condition"].ToString());
                
                if(
                    playerExecs == (int)ArchipelagoEnums.ShuffleExecutableTypes.ProgressiveAndUseful &&
                    (!ArchipelagoItems.UsefulItems.Contains(item.Key) && !ArchipelagoItems.ProgressionItems.Contains(item.Key))
                    ) { continue; } else if(
                    playerExecs == (int)ArchipelagoEnums.ShuffleExecutableTypes.ProgressionOnly &&
                    !ArchipelagoItems.ProgressionItems.Contains(item.Key)
                    ) { continue; }

                int port = item.Value;
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
