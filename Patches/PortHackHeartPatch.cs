/*
 * PortHackHeartDaemon - BreakHeart
 * 
 * Send completion status to Archipelago server when PortHack.Heart is PortHack'd
 */

using System;

using HarmonyLib;
using Hacknet;

using Archipelago.MultiClient.Net.Packets;

namespace HacknetArchipelago.Patches
{
    [HarmonyPatch]
    internal class PortHackHeartPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PorthackHeartDaemon),nameof(PorthackHeartDaemon.BreakHeart))]
        static void Postfix(PorthackHeartDaemon __instance)
        {
            if(HacknetAPMod.archiSession.ConnectionInfo.Slot >= 0) // Only send the packet when the user is connected, Slot would be -1 if not connected
            {
                var statusUpdate = new StatusUpdatePacket();
                statusUpdate.Status = Archipelago.MultiClient.Net.Enums.ArchipelagoClientState.ClientGoal;
                HacknetAPMod.archiSession.Socket.SendPacket(statusUpdate);

                Console.WriteLine("[Hacknet_Archipelago] User reached client goal - victory packet sent to server.");
            } else
            {
                Console.WriteLine("[Hacknet_Archipelago] User reached client goal, but was not connected to server.\n[*] If this wasn't intenional, a manual release is recommended.");
            }
        }
    }
}
