using System;
using System.Linq;
using System.Text.RegularExpressions;

using HarmonyLib;

using Hacknet;
using Hacknet.Mission;

using Pathfinder.Util;

namespace HacknetArchipelago.Patches.Missions
{
    // Since SSHCrack is removed from Viper's comp, this breaks one of the opening missions. That's no good!
    // Instead, we replace the requirement of downloading SSHCrack to a requirement of gaining admin access.
    // Is simple, no?
    [HarmonyPatch]
    public class PatchMissions
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ActiveMission),nameof(ActiveMission.Update))]
        static bool Prefix(ActiveMission __instance)
        {
            if(__instance.goals.OfType<FileDownloadMission>().Any())
            {
                __instance.activeCheck = true; // huehuehue. this is the only reliable way of checking for admin on fast admin servers
            }

            return true;
        }
    }

    [HarmonyPatch]
    public class PatchDownloadMissions
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FileDownloadMission),nameof(FileDownloadMission.isComplete))]
        static bool Prefix(FileDownloadMission __instance, ref bool __result)
        {
            string filename = __instance.target;

            if(filename.EndsWith(".exe"))
            {
                Computer targetComp;

                Regex ipRegex = new Regex(@"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$");

                Match isIP = ipRegex.Match(__instance.targetComp);

                targetComp = isIP.Success ? ComputerLookup.FindByIp(__instance.targetComp) : ComputerLookup.FindById(__instance.targetComp);

                if (targetComp.PlayerHasAdminPermissions())
                {
                    __result = true;
                    return false;
                }

                __result = false;
                return false;
            }

            return true;
        }
    }
}
