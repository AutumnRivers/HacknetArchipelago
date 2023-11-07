using System;
using System.Collections.Generic;
using System.Linq;

using Hacknet;

using BepInEx;
using BepInEx.Hacknet;

using HarmonyLib;

using Pathfinder.Event.Loading;
using Pathfinder.Event;

using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;

using Pathfinder.Replacements;
using Pathfinder.Meta.Load;
using Pathfinder.Event.Gameplay;
using Pathfinder.Event.Saving;
using Pathfinder.Command;

using HacknetArchipelago.Patches;
using HacknetArchipelago.Static;
using HacknetArchipelago.Commands;

using Archipelago.MultiClient.Net.Helpers;

using System.Xml.Linq;
using Pathfinder.Util.XML;

namespace HacknetArchipelago
{
    [BepInPlugin(ModGUID, ModName, ModVer)]
    [BepInDependency("com.Pathfinder.API", BepInDependency.DependencyFlags.HardDependency)]
    public class HacknetAPMod : HacknetPlugin
    {
        public const string ModGUID = "autumnrivers.hacknetapclient";
        public const string ModName = "Hacknet_Archipelago";
        public const string ModVer = "0.0.1";

        public static List<string> completedEvents = new List<string>();

        private readonly string defaultAPURI = "localhost";
        private readonly int defaultAPPort = 38281;

        public static ArchipelagoSession archiSession;

        public static Dictionary<string, string> archiLocationNames = ArchipelagoLocations.MNameToArchiLocation;
        public static List<string> receivedItems = new List<string>();

        public override bool Load()
        {
            HarmonyInstance.PatchAll(typeof(HacknetAPMod).Assembly);

            CommandManager.RegisterCommand("checkitems", DebugCommands.CheckReceivedItems, true, false);

            Action<OSLoadedEvent> fixCampaign = StartCampaignFix;
            Action<SaveEvent> archipelagoSave = InjectArchipelagoSaveData;

            EventManager<OSLoadedEvent>.AddHandler(fixCampaign);
            EventManager<SaveEvent>.AddHandler(archipelagoSave);
            
            return true;
        }

        public void StartCampaignFix(OSLoadedEvent os_load_event)
        {
            OS os = os_load_event.Os;

            os.Flags.AddFlag("CSEC_Member");

            archiSession = ArchipelagoSessionFactory.CreateSession(defaultAPURI, defaultAPPort);
            LoginResult archiLogin = archiSession.TryConnectAndLogin("Hacknet", "AutumnHN", ItemsHandlingFlags.AllItems);

            if(archiLogin.Successful)
            {
                os.terminal.writeLine("Successfully connected to Archipelago.");
                Console.WriteLine("[Hacknet_Archipelago] Connected to the Archipelago session.");
                Console.WriteLine(archiLogin.ToString());

                archiSession.Items.ItemReceived += CheckReceivedItems;
            } else
            {
                Console.WriteLine("[Hacknet_Archipelago] Error attempting to connect to Archipelago:");

                LoginFailure failure = (LoginFailure)archiLogin;

                string errorMessage = $"Failed to Connect to {defaultAPURI} as AutumnHN:";

                foreach (string error in failure.Errors)
                {
                    errorMessage += $"\n    {error}";
                }
                foreach (ConnectionRefusedError error in failure.ErrorCodes)
                {
                    errorMessage += $"\n    {error}";
                }

                Console.WriteLine(errorMessage);
            }
        }

        public void CheckReceivedItems(ReceivedItemsHelper receivedItemsHelper)
        {
            var itemReceivedName = receivedItemsHelper.PeekItemName();

            if(itemReceivedName == "ETASTrap")
            {
                ETASTrap();
            } else if (!receivedItems.Contains(itemReceivedName.ToLower()))
            {
                int receivedItemPort = ArchipelagoItems.ItemNamesAndPortIDs.First(e => e.Key == itemReceivedName.ToLower()).Value;
                receivedItems.Add(itemReceivedName.ToLower());

                var exeContent = PortExploits.crackExeData[receivedItemPort];

                SAAddAsset addFileAction = new SAAddAsset
                {
                    FileName = itemReceivedName + ".exe",
                    FileContents = exeContent,
                    TargetFolderpath = "bin",
                    TargetComp = "playerComp"
                };

                addFileAction.Trigger(OS.currentInstance);

                OS.currentInstance.warningFlash();
                OS.currentInstance.terminal.writeLine("You received " + itemReceivedName + "!");
            }

            receivedItemsHelper.DequeueItem();
        }

        public void CheckForFlags(OSUpdateEvent os_update)
        {
            /*OS os = os_update.OS;
            ProgressionFlags currentFlags = os.Flags;
            Folder userFolder = os.thisComputer.files.root;

            if(!completedEvents.Exists(e => e == "finishedIntro"))
            {
                for (var index = 0; index < userFolder.folders.Count; ++index)
                {
                    if (userFolder.folders[index].searchForFile("Entropy_Induction_Test") != null)
                    {
                        long introLocationID = archiSession.Locations.GetLocationIdFromName("Hacknet", "INTRO Complete Introduction");

                        archiSession.Locations.CompleteLocationChecks(new long[] {introLocationID});

                        os.terminal.writeLine("HACKNET_ARCHIPELAGO: You found a check!");

                        completedEvents.Add("finishedIntro");
                    }
                }
            }*/
        }

        public void InjectArchipelagoSaveData(SaveEvent save_event)
        {
            XElement archipelagoElement = new XElement("HacknetArchipelagoData");
            XAttribute archiItems = new XAttribute("ReceivedItems", receivedItems.Join(delimiter: ","));
            XAttribute archiEvents = new XAttribute("CompletedEvents", completedEvents.Join(delimiter: ","));

            archipelagoElement.Add(archiItems);
            archipelagoElement.Add(archiEvents);

            save_event.Save.Add(archipelagoElement);
        }

        private void ETASTrap()
        {
            OS os = OS.currentInstance;

            HackerScriptExecuter.executeThreadedScript(new string[]
            {
                "config playerComp jmail 0.1 $#%#$",
                " ",
                "instanttrace $#%#$"
            }, os);
        }
    }

    // Patch DHS mission completion to check if it's an Archipelago location
    [HarmonyPatch]
    public class CheckDHSMissionForArchiLoc
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(DLCHubServer), nameof(DLCHubServer.PlayerAttemptCompleteMission))]
        static void Postfix(DLCHubServer __instance)
        {
            OS os = OS.currentInstance;
            var currentMission = __instance.SelectedMission;

            string missionName = currentMission.Mission.email.subject;

            if(!HacknetAPMod.completedEvents.Exists(e => e == missionName))
            {
                long missionLocationID = HacknetAPMod.archiSession.Locations.GetLocationIdFromName("Hacknet", "LABS " + missionName);

                HacknetAPMod.archiSession.Locations.CompleteLocationChecks(new long[] { missionLocationID });

                os.terminal.writeLine("HACKNET_ARCHIPELAGO: You found a check!");

                HacknetAPMod.completedEvents.Add(missionName);
            }
        }
    }

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

            if (!HacknetAPMod.completedEvents.Exists(e => e == missionName) && locNames.ContainsKey(missionName))
            {
                string archiLocation = locNames[missionName];

                long missionLocationID = HacknetAPMod.archiSession.Locations.GetLocationIdFromName("Hacknet", archiLocation);

                Console.WriteLine("Current Archi Location: " + archiLocation);
                Console.WriteLine("Current Location ID: " + missionLocationID);

                HacknetAPMod.archiSession.Locations.CompleteLocationChecks(missionLocationID);

                os.terminal.writeLine("HACKNET_ARCHIPELAGO: You found a check!");

                HacknetAPMod.completedEvents.Add(missionName);
            }
        }
    }

    [SaveExecutor("HacknetSave.HacknetArchipelagoData")]
    public class ReadArchipelagoSaveData : SaveLoader.SaveExecutor
    {
        public void LoadSaveData(ElementInfo info)
        {
            char[] delimiterChar = ",".ToCharArray();

            string[] itemsArray = info.Attributes["ReceivedItems"].Split(delimiterChar);
            string[] eventsArray = info.Attributes["CompletedEvents"].Split(delimiterChar);

            List<string> receivedItems = itemsArray.ToList();
            List<string> completedEvents = eventsArray.ToList();

            HacknetAPMod.receivedItems = receivedItems;
            HacknetAPMod.completedEvents = completedEvents;
        }

        public override void Execute(EventExecutor exec, ElementInfo info) { LoadSaveData(info); }
    }
}
