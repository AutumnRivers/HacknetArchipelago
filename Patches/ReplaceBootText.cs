using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                "Hacknet: Archipelago 0.0.1",
                "~~~~~~~~~~~~~~~~~~~~~~~~~~~",
                " ",
                "To check your Archipelago connection status, run 'archistatus' at any time.",
                " ",
                "If you need support, open an issue on the GitHub repository.",
                "As this world is unofficial, please do not ask for support through any official Archipelago channels.",
                " ",
                "As long as the Archipelago mod is installed, you are free to save and load as you like.",
                " ",
                "Have fun!"
            };

            return true;
        }
    }
}
