﻿using System;
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

        public static int etasCount = 0;
        public static int fakeConnectCount = 0;

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

            if(hasCompletedSetup)
            {
                if(receivedItems.Count > 0) { RestockBackupsServer(); }
            }
        }

        public void RestockBackupsServer()
        {
            Computer backupNode = ComputerLookup.FindById("archipelagoBackup");

            Folder binFolder = Programs.getFolderAtPath("bin", OS.currentInstance, backupNode.files.root, false);
            binFolder.files.RemoveAll(x => true);

            foreach(var item in receivedItems)
            {
                if (ArchipelagoItems.ItemNamesAndPortIDs.TryGetValue(item.ToLower(), out int receivedItemPort))
                {
                    string exeContent = PortExploits.crackExeData[receivedItemPort];

                    SAAddAsset addFileToBackupServerAction = new SAAddAsset
                    {
                        FileName = item + ".exe",
                        FileContents = exeContent,
                        TargetFolderpath = "bin",
                        TargetComp = "archipelagoBackup"
                    };

                    OS.currentInstance.delayer.Post(ActionDelayer.NextTick(), () =>
                    {
                        addFileToBackupServerAction.Trigger(OS.currentInstance);
                    });
                }
            }
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

            int backupNodeIndex = OS.currentInstance.netMap.nodes.Count - 1;

            OS.currentInstance.thisComputer.links.Add(backupNodeIndex);
        }

        public static void CheckItem(NetworkItem item, bool isReplay = false)
        {
            long itemID = item.Item;
            string itemName = archiSession.Items.GetItemName(itemID);

            // TODO: Clean this up later, make it future-proof
            if(itemName == "l33t hax0r skills" || itemName == "the sudden urge to play PointClicker" || itemName == "matt") { return; }

            Console.WriteLine($"[Hacknet_Archipelago] Received {itemName} with ID: {itemID}");

            if(receivedItems.Contains(itemName.ToLower())) { return; }

            Console.WriteLine("[*] User does not have item");

            if (itemName == "ETASTrap")
            {
                etasCount++;
                ETASTrap();
            }
            else if (itemName == "Fake Connect")
            {
                fakeConnectCount++;
                int playerSlot = item.Player;
                string playerName = archiSession.Players.GetPlayerName(playerSlot);

                FakeConnectTrap(playerName);
            }
            else
            {
                if (ArchipelagoItems.ItemNamesAndPortIDs.TryGetValue(itemName.ToLower(), out int receivedItemPort))
                {
                    Console.WriteLine("[*] Item is executable");

                    var exeContent = PortExploits.crackExeData[receivedItemPort];

                    receivedItems.Add(itemName.ToLower());

                    try
                    {
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

                        OS.currentInstance.delayer.Post(ActionDelayer.NextTick(), () =>
                        {
                            addFileAction.Trigger(OS.currentInstance);
                            addFileToBackupServerAction.Trigger(OS.currentInstance);
                        });
                    } catch(Exception err)
                    {
                        Console.WriteLine(err);
                        throw;
                    }
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
                long itemID = item.Item;
                string itemName = archiSession.Items.GetItemName(itemID);

                if(itemName == "ETASTrap")
                {
                    int receivedETAS = afkReceivedItems.Count(i => i.Item == itemID);
                    if(receivedETAS >= etasCount) { continue; }
                }

                if(itemName == "Fake Connect")
                {
                    int receivedFakeConnect = afkReceivedItems.Count(i => i.Item == itemID);
                    if (receivedFakeConnect >= fakeConnectCount) { continue; }
                }

                CheckItem(item);
            }
        }

        public static void CheckReceivedItems(ReceivedItemsHelper receivedItemsHelper)
        {
            if(OS.currentInstance == null) { return; }

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
            XAttribute archiFakeConnects = new XAttribute("ReceivedFakeConnects", fakeConnectCount);
            XAttribute archiETAS = new XAttribute("ReceivedETAS", etasCount);

            archipelagoElement.Add(archiItems);
            archipelagoElement.Add(archiEvents);
            archipelagoElement.Add(archiFakeConnects);
            archipelagoElement.Add(archiETAS);

            save_event.Save.FirstNode.AddBeforeSelf(archipelagoElement);
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

            int savedFakeConnectCount = int.Parse(info.Attributes["ReceivedFakeConnects"]);
            int savedETASCount = int.Parse(info.Attributes["ReceivedETAS"]);

            HacknetAPMod.fakeConnectCount = savedFakeConnectCount;
            HacknetAPMod.etasCount = savedETASCount;

            HacknetAPMod.hasCompletedSetup = true;
        }

        public override void Execute(EventExecutor exec, ElementInfo info) { LoadSaveData(info); }
    }
}
