using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hacknet;

using HarmonyLib;

using Pathfinder.Util;

namespace HacknetArchipelago.Patches.Missions
{
    [HarmonyPatch]
    public class KaguyaTrialsAccessCSEC
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MissionHubServer),nameof(MissionHubServer.addMission))]
        static bool Prefix(ActiveMission mission)
        {
            if(mission.email.subject == "The Kaguya Trials")
            {
                mission.postingAcceptFlagRequirements = new string[]
                {
                    "CanAccessTrials"
                };
            }

            return true;
        }
    }

    [HarmonyPatch]
    public class KaguyaTrialsLoadCheck
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ActiveMission),nameof(ActiveMission.sendEmail))]
        static bool Prefix(ActiveMission __instance, OS os)
        {
            string filename = __instance.reloadGoalsSourceFile;

            if (filename.EndsWith("DLCConnectorIntro.xml"))
            {
                var receivedItems = HacknetAPMod.receivedItems;

                bool denyMission = false;
                bool isEntropy = filename.Contains("Entropy");

                if (!os.Flags.HasFlag("CanAccessTrials")) { denyMission = true; }

                if (!receivedItems.Contains("torrentstreaminjector")
                    || (!receivedItems.Contains("ftpbounce") && !receivedItems.Contains("ftpsprint"))) { denyMission = true; }

                if (denyMission)
                {
                    ActiveMission denyMissionFile = (ActiveMission)ComputerLoader.readMission("./BepInEx/plugins/assets/DenyLabsAccess.xml");

                    os.currentMission = denyMissionFile;
                    __instance.email = denyMissionFile.email;

                    if (isEntropy)
                    {
                        Computer entropyComp = ComputerLookup.FindById("entropy00");

                        MissionListingServer entropyListing = entropyComp.getDaemon(typeof(MissionListingServer)) as MissionListingServer;

                        List<ActiveMission> branchMissions = os.branchMissions;
                        ActiveMission m = (ActiveMission)ComputerLoader.readMission("./Content/DLC/Missions/BaseGameConnectors/Missions/EntropyDLCConnectorIntro.xml");
                        os.branchMissions = branchMissions;
                        entropyListing.addMisison(m, true);
                    }
                    else
                    {
                        Computer csecComp = ComputerLookup.FindById("mainHub");

                        MissionHubServer csecListing = csecComp.getDaemon(typeof(MissionHubServer)) as MissionHubServer;

                        csecListing.AddMissionToListings("./Content/DLC/Missions/BaseGameConnectors/Missions/CSEC_DLCConnectorIntro.xml");
                    }
                }
            }

            return true;
        }
    }
}
