using System.Collections.Generic;

using Hacknet;
using HarmonyLib;

namespace HacknetArchipelago.Patches.Missions
{
    public class MissionChecks
    {
        [HarmonyPatch]
        public class CheckMissionForArchiLoc
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(ActiveMission), nameof(ActiveMission.finish))]
            static void Postfix(ActiveMission __instance)
            {
                OS os = OS.currentInstance;

                string missionName = __instance.email.subject;

                Dictionary<string, string> locNames = HacknetAPMod.archiLocationNames;

                string archiLocation = locNames[missionName];

                long missionLocationID = HacknetAPMod.archiSession.Locations.GetLocationIdFromName("Hacknet", archiLocation);

                HacknetAPMod.archiSession.Locations.CompleteLocationChecks(missionLocationID);

                os.terminal.writeLine("HACKNET_ARCHIPELAGO: You found a check!");
            }
        }
    }
}
