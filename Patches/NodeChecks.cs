using System;

using HarmonyLib;

using Hacknet;

using HacknetArchipelago.Static;

using Goals = HacknetArchipelago.Static.ArchipelagoEnums.PlayerGoals;

using Archipelago.MultiClient.Net.Packets;

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

                CheckForGibson(__instance);

                string locationName = nodeLocations[__instance.idName];

                long locationID = HacknetAPMod.archiSession.Locations.GetLocationIdFromName("Hacknet", locationName);

                if(locationID == -1) { return; }

                HacknetAPMod.archiSession.Locations.CompleteLocationChecks(locationID);
            }
        }

        public static void CheckForGibson(Computer targetComp)
        {
            var session = HacknetAPMod.archiSession;

            if(session.ConnectionInfo.Slot == -1 || targetComp.idName != "dGibson") { return; }

            int playerGoal = int.Parse(session.DataStorage.GetSlotData()["victory_condition"].ToString());

            HacknetAPMod.completedEvents.Add("brokeIntoGibson");

            if(playerGoal != (int)Goals.Veteran && playerGoal != (int)Goals.Completionist) { return; }

            if(playerGoal == (int)Goals.Completionist && HacknetAPMod.completedEvents != HacknetAPMod.completionistEvents) { return; }

            var statusUpdate = new StatusUpdatePacket();
            statusUpdate.Status = Archipelago.MultiClient.Net.Enums.ArchipelagoClientState.ClientGoal;
            session.Socket.SendPacket(statusUpdate);

            Console.WriteLine("[Hacknet_Archipelago] User reached client goal - victory packet sent to server.");
        }
    }
}
