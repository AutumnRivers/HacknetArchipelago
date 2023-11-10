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
    public class NodeChecks
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Computer),nameof(Computer.log))]
        static void Postfix(Computer __instance, ref string message)
        {
            if (HacknetAPMod.checkedNodes.Contains(__instance.idName)) { return; }

            if(message.EndsWith("Became_Admin"))
            {
                string playerIP = OS.currentInstance.thisComputer.ip;

                if(!message.Contains(playerIP)) { return; }

                HacknetAPMod.checkedNodes.Add(__instance.idName);

                if(HacknetAPMod.archiSession.ConnectionInfo.Slot == -1) { return; }

                var nodeLocations = ArchipelagoLocations.NodeIDToLocations;

                if(!nodeLocations.ContainsKey(__instance.idName)) { return; }

                string locationName = nodeLocations[__instance.idName];

                long locationID = HacknetAPMod.archiSession.Locations.GetLocationIdFromName("Hacknet", locationName);

                if(locationID == -1) { return; }

                HacknetAPMod.archiSession.Locations.CompleteLocationChecks(locationID);
            }
        }
    }
}
