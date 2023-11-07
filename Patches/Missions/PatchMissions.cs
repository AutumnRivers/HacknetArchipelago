using System;
using System.Collections.Generic;

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
        [HarmonyPatch(typeof(ActiveMission))]
        [HarmonyPatch(new Type[] { typeof(List<MisisonGoal>), typeof(string), typeof(MailServer.EMailData) })]
        [HarmonyPatch(MethodType.Constructor)]
        static bool Prefix(ref List<MisisonGoal> _goals, ref string next, ref MailServer.EMailData _email, ActiveMission __instance)
        {
            string missionSubject = __instance.email.subject;

            if(missionSubject == "Getting some tools together")
            {
                Computer sshComp = ComputerLookup.FindById("portcrack01");

                GetAdminMission getAdmin = new GetAdminMission(sshComp.ip, OS.currentInstance);

                __instance.goals = new List<MisisonGoal> { getAdmin };
            }

            return true;
        }
    }
}
