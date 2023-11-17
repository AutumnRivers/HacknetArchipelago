using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hacknet;

using HarmonyLib;

namespace HacknetArchipelago.Patches
{
    [HarmonyPatch]
    public class AddNewBashLogs
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Utils),nameof(Utils.readEntireFile))]
        static bool Prefix(ref string filename)
        {
            const string targetFile = "Content/BashLogs.txt";
            if(filename != targetFile) { return true; }

            filename = "BepInEx/plugins/assets/NewBashLogs.txt";

            return true;
        }
    }
}
