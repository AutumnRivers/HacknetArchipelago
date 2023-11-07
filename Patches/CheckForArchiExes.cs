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
    /*[HarmonyPatch]
    public class CheckForArchipelagoExecutables
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ComputerLoader),nameof(ComputerLoader.loadComputer))]
        static void Postfix(ref object __result)
        {
            Computer currentComp = (Computer)__result;
            ArchipelagoSession archiSession = HacknetAPMod.archiSession;

            List<string> receivedItems = new List<string>();

            foreach(NetworkItem archiItem in archiSession.Items.AllItemsReceived)
            {
                long itemID = archiItem.Item;
                string itemName = archiSession.Items.GetItemName(itemID);
                receivedItems.Add(itemName);
            }

            foreach(FileEntry file in currentComp.files.root.files)
            {
                if(file.name.Length >= 5) {
                    string fileName = file.name.Substring(0, file.name.Length - 4); // Removes file extensions

                    if(!receivedItems.Exists(f => f == fileName))
                    {
                        currentComp.files.root.files.RemoveAll(f => f.data == file.data);
                    }
                }
            }
        }
    }*/

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

            string fileName = nameEntry.Substring(0, nameEntry.Length - 4);

            if(items.ContainsKey(fileName.ToLower()) && !receivedItems.Contains(fileName.ToLower()))
            {
                dataEntry = "There's usually a " + nameEntry + " here, but you haven't unlocked it yet! Venture forth in this Archipelago!";
                nameEntry = "BLOCKED_" + nameEntry;
            }

            return true;
        }
    }
}
