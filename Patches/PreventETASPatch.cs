using Hacknet;

using HarmonyLib;

using SeqState = Hacknet.SequencerExe.SequencerExeState;

namespace HacknetArchipelago.Patches
{
    [HarmonyPatch]
    public class PreventETASDuringSequencer
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SequencerExe),nameof(SequencerExe.LoadContent))]
        static void Postfix_LoadContent(SequencerExe __instance)
        {
            bool isActive = __instance.state == SeqState.AwaitingActivation || __instance.state == SeqState.Active;

            if(isActive) { HacknetAPMod.doNotFireEtas = true; }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SequencerExe),nameof(SequencerExe.Killed))]
        static void Postfix_Killed(SequencerExe __instance)
        {
            bool wasActive = __instance.state == SeqState.Active || __instance.state == SeqState.AwaitingActivation;

            if(wasActive)
            {
                HacknetAPMod.doNotFireEtas = false;
                if(HacknetAPMod.etasIsQueued)
                {
                    HacknetAPMod.ETASTrap();
                }
            }
        }
    }

    [HarmonyPatch]
    public class PreventETASDuringAltitudeLoss
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Faction),nameof(Faction.addValue))]
        static void Postfix_AltitudeLossETAS(Faction __instance)
        {
            const string targetFaction = "Bibliotheque";
            string currentFaction = __instance.name;

            if(targetFaction != currentFaction) { return; }

            const int targetValue = 12;
            int currentValue = __instance.playerValue;

            if(currentValue < targetValue) { return; }

            HacknetAPMod.doNotFireEtas = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MissionFunctions),nameof(MissionFunctions.runCommand))]
        static void Postfix_EndAltitudeLossETAS(int value, string name)
        {
            const string targetCommand = "deActivateAircraftStatusOverlay";
            
            if(name != targetCommand) { return; }

            HacknetAPMod.doNotFireEtas = false;

            if(HacknetAPMod.etasIsQueued)
            {
                HacknetAPMod.ETASTrap();
            }
        }
    }

    [HarmonyPatch]
    public class PreventETASDuringCredits
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PorthackHeartDaemon),nameof(PorthackHeartDaemon.BreakHeart))]
        static void Postfix_PortHackHeartETAS()
        {
            HacknetAPMod.doNotFireEtas = true;
            // We don't need to check for a queued ETAS here, because it's the end of the game
            // The game closes after the credits
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DLCCreditsDaemon),nameof(DLCCreditsDaemon.draw))]
        static void Postfix_DLCCreditsETAS(DLCCreditsDaemon __instance)
        {
            HacknetAPMod.doNotFireEtas = __instance.showingCredits;

            if(!__instance.showingCredits && HacknetAPMod.etasIsQueued)
            {
                HacknetAPMod.ETASTrap();
            }
        }
    }
}
