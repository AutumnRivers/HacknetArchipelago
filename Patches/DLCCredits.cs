using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hacknet;

using HarmonyLib;

using HacknetArchipelago.Static;
using Archipelago.MultiClient.Net.Packets;

namespace HacknetArchipelago.Patches
{
    [HarmonyPatch]
    public class DLCCredits
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(DLCCreditsDaemon),nameof(DLCCreditsDaemon.EndDLC))]
        static void Postfix()
        {
            var session = HacknetAPMod.archiSession;

            if(session.ConnectionInfo.Slot >= 0)
            {
                HacknetAPMod.completedEvents.Add("finishedDLC");

                int playerGoal = int.Parse(session.DataStorage.GetSlotData()["victory_condition"].ToString());
                if(playerGoal != (int)ArchipelagoEnums.PlayerGoals.AltitudeLoss) { return; }

                var statusUpdate = new StatusUpdatePacket
                {
                    Status = Archipelago.MultiClient.Net.Enums.ArchipelagoClientState.ClientGoal
                };

                session.Socket.SendPacket(statusUpdate);

                Console.WriteLine("[Hacknet_Archipelago] User reached client goal - victory packet sent to server.");
            }
        }
    }
}
