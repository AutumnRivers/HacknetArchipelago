using HarmonyLib;
using Hacknet;

namespace HacknetArchipelago.Patches
{
    public class TutorialSetup
    {
        [HarmonyPatch]
        public class CheckForTutorialKill
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(AdvancedTutorial), nameof(AdvancedTutorial.Killed))]
            static void Postfix()
            {
                HacknetAPMod.hasCompletedSetup = true;
                HacknetAPMod.CreateArchipelagoBackupNode();
                HacknetAPMod.CheckAlreadyReceivedItems();
            }
        }
    }
}
