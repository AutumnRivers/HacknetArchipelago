using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Hacknet;

using BepInEx;
using BepInEx.Hacknet;

using HarmonyLib;

using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;

using Pathfinder.Replacements;
using Pathfinder.Meta.Load;
using Pathfinder.Event;
using Pathfinder.Event.Gameplay;
using Pathfinder.Event.Saving;
using Pathfinder.Event.Loading;
using Pathfinder.Command;
using Pathfinder.Util;
using Pathfinder.Util.XML;

using HacknetArchipelago.Patches; // Would this cause Harmony to not patch these if I removed this?
using HacknetArchipelago.Static;
using HacknetArchipelago.Commands;

using xvec2 = Microsoft.Xna.Framework.Vector2;

namespace HacknetArchipelago
{
    [BepInPlugin(ModGUID, ModName, ModVer)]
    [BepInDependency("com.Pathfinder.API", BepInDependency.DependencyFlags.HardDependency)]
    public class HacknetAPMod : HacknetPlugin
    {
        public const string ModGUID = "autumnrivers.hacknetapclient";
        public const string ModName = "Hacknet_Archipelago";
        public const string ModVer = "0.2.1";

        public static ArchipelagoSession archiSession;

        public static Dictionary<string, string> archiLocationNames = ArchipelagoLocations.MNameToArchiLocation;

        public static readonly List<string> completionistEvents = new List<string>()
        {
            "brokeIntoGibson", "finishedDLC", "brokePortHackHeart"
        };

        public static List<string> receivedItems = new List<string>();

        public static List<string> completedEvents = new List<string>();
        public static List<string> checkedNodes = new List<string>();

        public static bool hasCompletedSetup = false;

        private List<string> checkedFlags = new List<string>();

        public override bool Load()
        {
            HarmonyInstance.PatchAll(typeof(HacknetAPMod).Assembly);

            CommandManager.RegisterCommand("checkitems", DebugCommands.CheckReceivedItems, true, false);
            CommandManager.RegisterCommand("architest", DebugCommands.SendTestPacket, false, true);
            CommandManager.RegisterCommand("fakeconnect", DebugCommands.TestFakeConnect, false, true);
            CommandManager.RegisterCommand("archistatus", ArchipelagoStatusCommand.Command, true, false);

            Action<OSLoadedEvent> fixCampaign = StartCampaignFix;
            Action<SaveEvent> archipelagoSave = InjectArchipelagoSaveData;

            EventManager<OSLoadedEvent>.AddHandler(fixCampaign);
            EventManager<SaveEvent>.AddHandler(archipelagoSave);

            Settings.AllowExtensionMode = false;
            
            return true;
        }

        public void StartCampaignFix(OSLoadedEvent os_load_event)
        {
            OS os = os_load_event.Os;

            os.Flags.AddFlag("CSEC_Member");

            CreateArchipelagoBackupNode();

            archiSession.Items.ItemReceived += CheckReceivedItems;

            if(hasCompletedSetup) { CheckAlreadyReceivedItems(); }
        }

        public static void CreateArchipelagoBackupNode()
        {
            Computer existingBackupNode = ComputerLookup.FindById("archipelagoBackup");

            if(existingBackupNode != null) { return; }

            Computer backupNode = new Computer("Archipelago Backup Node", "archipelago.gg", new xvec2(0.5f, 0.5f), 0, 4, OS.currentInstance);
            backupNode.idName = "archipelagoBackup";

            FileEntry archipelagoNote = new FileEntry("Archipelago Backups Server: connect archipelago.gg", "ArchipelagoBackups.txt");
            OS.currentInstance.thisComputer.files.root.files.Add(archipelagoNote);

            OS.currentInstance.netMap.nodes.Add(backupNode);
            OS.currentInstance.netMap.visibleNodes.Add(OS.currentInstance.netMap.nodes.Count - 1);
        }

        public static void CheckItem(NetworkItem item)
        {
            long itemID = item.Item;
            string itemName = archiSession.Items.GetItemName(itemID);

            Console.WriteLine($"[Hacknet_Archipelago] Received {itemName} with ID: {itemID}");

            if(receivedItems.Contains(itemName.ToLower())) { return; }

            if (itemName == "ETASTrap")
            {
                ETASTrap();
            }
            else if (itemName == "Fake Connect")
            {
                int playerSlot = item.Player;
                string playerName = archiSession.Players.GetPlayerName(playerSlot);

                FakeConnectTrap(playerName);
            }
            else
            {
                var receivedItemValue = ArchipelagoItems.ItemNamesAndPortIDs.TryGetValue(itemName, out _);

                if (receivedItemValue)
                {
                    int receivedItemPort = ArchipelagoItems.ItemNamesAndPortIDs[itemName];
                    var exeContent = PortExploits.crackExeData[receivedItemPort];
                    receivedItems.Add(itemName.ToLower());

                    SAAddAsset addFileAction = new SAAddAsset
                    {
                        FileName = itemName + ".exe",
                        FileContents = exeContent,
                        TargetFolderpath = "bin",
                        TargetComp = "playerComp"
                    };

                    SAAddAsset addFileToBackupServerAction = new SAAddAsset
                    {
                        FileName = itemName + ".exe",
                        FileContents = exeContent,
                        TargetFolderpath = "bin",
                        TargetComp = "archipelagoBackup"
                    };

                    addFileAction.Trigger(OS.currentInstance);
                    addFileToBackupServerAction.Trigger(OS.currentInstance);
                }

                OS.currentInstance.warningFlash();
                OS.currentInstance.terminal.writeLine("You received " + itemName + "!");
            }
        }

        public static void CheckAlreadyReceivedItems()
        {
            if(archiSession.ConnectionInfo.Slot == -1) { return; }

            var afkReceivedItems = archiSession.Items.AllItemsReceived;

            foreach(var item in afkReceivedItems)
            {
                CheckItem(item);
            }
        }

        public static void CheckReceivedItems(ReceivedItemsHelper receivedItemsHelper)
        {
            string item = receivedItemsHelper.PeekItemName();
            if(OS.currentInstance == null && (item == "ETASTrap" || item == "Fake Connect")) { return; }

            CheckItem(receivedItemsHelper.DequeueItem());
        }

        public void CheckForFlags(OSUpdateEvent os_update)
        {
            OS os = os_update.OS;
            List<string> currentFlags = os.Flags.Flags;

            foreach(string flag in currentFlags)
            {
                if(checkedFlags.Contains(flag)) { continue; }

                checkedFlags.Add(flag);

                var flagLocations = ArchipelagoLocations.FlagsToLocations;

                if(!flagLocations.ContainsKey(flag)) { continue; }

                string locationName = flagLocations[flag];

                long flagLocationID = archiSession.Locations.GetLocationIdFromName("Hacknet", locationName);

                if(flagLocationID == -1) { continue; }

                archiSession.Locations.CompleteLocationChecks(flagLocationID);

                os.terminal.writeLine("HACKNET_ARCHIPELAGO: You found a check!");
            }
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

        public static void ETASTrap()
        {
            OS os = OS.currentInstance;

            TrackerCompleteSequence.TriggerETAS(os);
        }

        public static void FakeConnectTrap(string offender)
        {
            OS os = OS.currentInstance;
            string currentPlayerName = "";

            if(archiSession.ConnectionInfo.Slot > -1) {
                currentPlayerName = archiSession.Players.GetPlayerName(archiSession.ConnectionInfo.Slot);
            }

            os.IncConnectionOverlay.Activate();
            os.warningFlash();
            os.beepSound.Play();

            if(offender == currentPlayerName) { os.thisComputer.log("You got yourself with a fake connection!"); }
            else { os.thisComputer.log(offender + " got you with a fake connection!"); }
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

            long missionLocationID = HacknetAPMod.archiSession.Locations.GetLocationIdFromName("Hacknet", "LABS " + missionName);

            HacknetAPMod.archiSession.Locations.CompleteLocationChecks(new long[] { missionLocationID });

            os.terminal.writeLine("HACKNET_ARCHIPELAGO: You found a check!");
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

            string archiLocation = locNames[missionName];

            long missionLocationID = HacknetAPMod.archiSession.Locations.GetLocationIdFromName("Hacknet", archiLocation);

            HacknetAPMod.archiSession.Locations.CompleteLocationChecks(missionLocationID);

            os.terminal.writeLine("HACKNET_ARCHIPELAGO: You found a check!");
        }
    }

    [HarmonyPatch]
    public class CheckForTutorialKill
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AdvancedTutorial),nameof(AdvancedTutorial.Killed))]
        static void Postfix()
        {
            HacknetAPMod.hasCompletedSetup = true;
            HacknetAPMod.CreateArchipelagoBackupNode();
            HacknetAPMod.CheckAlreadyReceivedItems();
        }
    }

    [SaveExecutor("HacknetSave.HacknetArchipelagoData")]
    public class ReadArchipelagoSaveData : SaveLoader.SaveExecutor
    {
        public void LoadSaveData(ElementInfo info)
        {
            char delimiterChar = ',';

            string[] itemsArray = info.Attributes["ReceivedItems"].Split(delimiterChar);
            string[] eventsArray = info.Attributes["CompletedEvents"].Split(delimiterChar);

            List<string> receivedItems = itemsArray.ToList();
            List<string> completedEvents = eventsArray.ToList();

            HacknetAPMod.receivedItems = receivedItems;
            HacknetAPMod.completedEvents = completedEvents;

            HacknetAPMod.hasCompletedSetup = true;
        }

        public override void Execute(EventExecutor exec, ElementInfo info) { LoadSaveData(info); }
    }
}
