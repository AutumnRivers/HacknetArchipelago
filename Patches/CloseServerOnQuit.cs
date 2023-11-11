using HarmonyLib;
using Microsoft.Xna.Framework;

namespace HacknetArchipelago.Patches
{
    [HarmonyPatch]
    public class CloseServerOnQuit
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Game), nameof(Game.Exit))]
        static bool Prefix()
        {
            if(HacknetAPMod.archiSession == null) { return true; }
            if(HacknetAPMod.archiSession.ConnectionInfo.Slot == -1) { return true; }
            HacknetAPMod.archiSession.Socket.DisconnectAsync().Wait();
            return true;
        }
    }
}
