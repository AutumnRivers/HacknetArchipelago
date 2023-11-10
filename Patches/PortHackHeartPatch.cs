/*
 * PortHackHeartDaemon - BreakHeart
 * 
 * Send completion status to Archipelago server when PortHack.Heart is PortHack'd
 */

using System;

using HarmonyLib;
using Hacknet;

using Archipelago.MultiClient.Net.Packets;
using Pathfinder.Util;

using HacknetArchipelago.Static;

namespace HacknetArchipelago.Patches
{
    [HarmonyPatch]
    internal class PortHackHeartPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PorthackHeartDaemon),nameof(PorthackHeartDaemon.BreakHeart))]
        static void Postfix(PorthackHeartDaemon __instance)
        {
            var session = HacknetAPMod.archiSession;

            if(session.ConnectionInfo.Slot >= 0) // Only send the packet when the user is connected, Slot would be -1 if not connected
            {
                int playerGoal = int.Parse(session.DataStorage.GetSlotData()["victory_condition"].ToString());
                if(playerGoal != (int)ArchipelagoEnums.PlayerGoals.Heartstopper) { return; }

                var statusUpdate = new StatusUpdatePacket();
                statusUpdate.Status = Archipelago.MultiClient.Net.Enums.ArchipelagoClientState.ClientGoal;
                HacknetAPMod.archiSession.Socket.SendPacket(statusUpdate);

                Console.WriteLine("[Hacknet_Archipelago] User reached client goal - victory packet sent to server.");
            }
        }
    }
}
