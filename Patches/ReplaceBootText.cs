using HarmonyLib;

using Hacknet;

namespace HacknetArchipelago.Patches
{
    [HarmonyPatch]
    public class ReplaceIntroText
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(IntroTextModule), nameof(IntroTextModule.Update))]
        static bool Prefix(IntroTextModule __instance)
        {
            __instance.text = new string[]
            {
                "Hacknet: Archipelago " + HacknetAPMod.ModVer,
                "~~~~~~~~~~~~~~~~~~~~~~~~~~~",
                " ",
                "To check your Archipelago connection status, run 'archistatus' at any time.",
                " ",
                "If you need support, open an issue on the GitHub repository, or ask in the Archipelago Discord.",
                " ",
                "As long as the Archipelago mod is installed, you are free to save and load as you like.",
                " ",
                "Work smart, work hard, and work in unison."
            };

            return true;
        }
    }
}
