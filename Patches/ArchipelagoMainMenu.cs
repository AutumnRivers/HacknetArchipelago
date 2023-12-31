﻿using Hacknet;
using Hacknet.Gui;

using HarmonyLib;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;

using System;
using System.IO;

using TextBox = HacknetArchipelago.Replacements.ArchipelagoTextBox;

namespace HacknetArchipelago.Patches
{
    [HarmonyPatch]
    public class ArchipelagoMainMenu
    {
        static string archiHost = "archipelago.gg";
        static string archiPort = "38281";
        static string archiSlot = "";
        static string archiPassword = "";

        static bool isConnected = false;
        static bool hasError = false;
        static bool hasReadLoginFile = false;

        static Color archiLogoColor = Color.White;

        public static Texture2D archiLogo;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenu),nameof(MainMenu.DrawBackgroundAndTitle))]
        static void Prefix(MainMenu __instance)
        {
            if (File.Exists("./archipelago.txt") && !hasReadLoginFile)
            {
                hasReadLoginFile = true;

                string[] archiDetails = File.ReadAllLines("./archipelago.txt");

                for(int x = 0; x < archiDetails.Length; x++)
                {
                    switch(x)
                    {
                        case 0:
                            archiHost = archiDetails[x];
                            break;
                        case 1:
                            archiPort = archiDetails[x];
                            break;
                        case 2:
                            archiSlot = archiDetails[x];
                            break;
                        case 3:
                            archiPassword = archiDetails[x];
                            break;
                        default:
                            continue;
                    }
                }
            }

            var screenManager = __instance.screenManager;
            int rightOffset = 600;

            TextItem.doFontLabel(new Vector2(520, 147), "Archipelago Edition", GuiData.font, Color.Orange);

            if(!DLC1SessionUpgrader.HasDLC1Installed)
            {
                TextItem.doFontLabel(new Vector2(50, screenManager.GraphicsDevice.Viewport.Height - 100), "You don't have Labyrinths installed. Things might break.", GuiData.smallfont, Color.OrangeRed);
            }
            TextItem.doFontLabel(new Vector2(50, screenManager.GraphicsDevice.Viewport.Height - 75), "Extensions are disabled while the Archipelago mod is installed.", GuiData.smallfont, Color.Red);
            TextItem.doFontLabel(new Vector2(50, screenManager.GraphicsDevice.Viewport.Height - 50), "For support, please contact @ohanamatsumae in the Archipelago Discord.", GuiData.smallfont, Color.White);

            Rectangle logoRect = new Rectangle()
            {
                Width = 225,
                Height = 225,
                X = (screenManager.GraphicsDevice.Viewport.Width - rightOffset) + 40,
                Y = 185
            };

            if (archiLogo != null) { 
                GuiData.spriteBatch.Draw(archiLogo, logoRect, archiLogoColor * 0.3f);
            } else
            {
                GraphicsDevice userGraphics = GuiData.spriteBatch.GraphicsDevice;

                FileStream logoStream = File.OpenRead("./BepInEx/plugins/assets/aplogo.png");
                archiLogo = Texture2D.FromStream(userGraphics, logoStream);
                logoStream.Dispose();
            }

            TextItem.doFontLabel(new Vector2(screenManager.GraphicsDevice.Viewport.Width - rightOffset, 200), "Archipelago Host:", GuiData.smallfont, Color.White);
            archiHost = TextBox.doTextBox(11111, screenManager.GraphicsDevice.Viewport.Width - rightOffset, 220, 300, 1, archiHost, GuiData.smallfont);

            TextItem.doFontLabel(new Vector2(screenManager.GraphicsDevice.Viewport.Width - rightOffset, 250), "Archipelago Port:", GuiData.smallfont, Color.White);
            archiPort = TextBox.doTextBox(11112, screenManager.GraphicsDevice.Viewport.Width - rightOffset, 270, 150, 1, archiPort, GuiData.smallfont);

            TextItem.doFontLabel(new Vector2(screenManager.GraphicsDevice.Viewport.Width - rightOffset, 300), "Archipelago Player Name:", GuiData.smallfont, Color.White);
            archiSlot = TextBox.doTextBox(11113, screenManager.GraphicsDevice.Viewport.Width - rightOffset, 320, 300, 1, archiSlot, GuiData.smallfont);

            TextItem.doFontLabel(new Vector2(screenManager.GraphicsDevice.Viewport.Width - rightOffset, 350), "Archipelago Room Pass:", GuiData.smallfont, Color.White);
            archiPassword = TextBox.doTextBox(11114, screenManager.GraphicsDevice.Viewport.Width - rightOffset, 370, 300, 1, archiPassword, GuiData.smallfont);

            bool connectButton = Button.doButton(11115, screenManager.GraphicsDevice.Viewport.Width - rightOffset, 425, 250, 40, "Connect to Archipelago", Color.Orange);

            if(isConnected)
            {
                TextItem.doFontLabel(new Vector2(screenManager.GraphicsDevice.Viewport.Width - rightOffset, 475), "Successfully connected to Archipelago.", GuiData.smallfont, Color.Green);
            } else if (hasError)
            {
                TextItem.doFontLabel(new Vector2(screenManager.GraphicsDevice.Viewport.Width - rightOffset, 475), "Failed to connect to Archipelago.", GuiData.smallfont, Color.Red);
            } else
            {
                TextItem.doFontLabel(new Vector2(screenManager.GraphicsDevice.Viewport.Width - rightOffset, 475), "Waiting to connect...", GuiData.smallfont, Color.Orange);
            }

            if(connectButton)
            {
                if(archiHost == "" || archiPort == "" || archiSlot == "")
                {
                    Console.WriteLine("[Hacknet_Archipelago] You left some fields empty - don't do that.");
                } else
                {
                    HacknetAPMod.archiSession = ArchipelagoSessionFactory.CreateSession(archiHost, int.Parse(archiPort));
                    LoginResult archiLogin = HacknetAPMod.archiSession.TryConnectAndLogin("Hacknet", archiSlot, ItemsHandlingFlags.AllItems, password:archiPassword);

                    if(archiLogin.Successful)
                    {
                        Console.WriteLine("[Hacknet_Archipelago] Connected to the Archipelago session.");
                        archiLogoColor = Color.Green;
                        isConnected = true;
                        hasError = false;

                        hasReadLoginFile = true;

                        try
                        {
                            StreamWriter archiLoginFile = new StreamWriter("./archipelago.txt", false);
                            archiLoginFile.WriteLineAsync(archiHost).Wait();
                            archiLoginFile.WriteLineAsync(archiPort).Wait();
                            archiLoginFile.WriteLineAsync(archiSlot).Wait();
                            archiLoginFile.WriteLineAsync(archiPassword).Wait();
                            archiLoginFile.FlushAsync().Wait();
                        } catch(Exception err)
                        {
                            Console.WriteLine("[Hacknet_Archipelago] Failed to write to Archipelago login file:");
                            Console.WriteLine(err.ToString());
                        }
                    } else
                    {
                        LoginFailure failure = (LoginFailure)archiLogin;

                        string errorMessage = $"Failed to connect to {archiHost}:{archiPort} as {archiSlot}:";

                        foreach (string error in failure.Errors)
                        {
                            errorMessage += $"\n    {error}";
                        }
                        foreach (ConnectionRefusedError error in failure.ErrorCodes)
                        {
                            errorMessage += $"\n    {error}";
                        }

                        Console.WriteLine(errorMessage);
                        hasError = true;
                        isConnected = false;
                    }
                }
            }
        }
    }
}
