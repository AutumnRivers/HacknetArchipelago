using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hacknet;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using HarmonyLib;
using Hacknet.Gui;

using static Hacknet.MissionHubServer;

using MissionRules = HacknetArchipelago.Static.ArchipelagoMissionRules;

namespace HacknetArchipelago.Replacements
{
    [HarmonyPatch]
    public class MissionHubServer_DrawMissionEntry
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MissionHubServer))]
        [HarmonyPatch(nameof(MissionHubServer.drawMissionEntry))]
        static bool Prefix(MissionHubServer __instance, Rectangle bounds, SpriteBatch sb, ActiveMission mission, int index)
        {
            OS os = OS.currentInstance;

            Color themeColor = __instance.themeColor;
            Color themeColorBackground = __instance.themeColorBackground;
            Color themeColorLine = __instance.themeColorLine;

            Texture2D lockIcon = __instance.lockIcon;

            int selectedElementIndex = __instance.selectedElementIndex;
            HubState state = __instance.state;
            float screenTransition = __instance.screenTransition;

            bool flag = false;

            if (mission.postingAcceptFlagRequirements != null)
            {
                for (int i = 0; i < mission.postingAcceptFlagRequirements.Length; i++)
                {
                    if (!os.Flags.HasFlag(mission.postingAcceptFlagRequirements[i]))
                    {
                        flag = true;
                    }
                }
            }

            if(MissionRules.RequiredMissionExecutables.TryGetValue(mission.email.subject, out string[] requiredExecutables))
            {
                for(int i = 0; i < requiredExecutables.Length; i++)
                {
                    if (!HacknetAPMod.receivedItems.Contains(requiredExecutables[i].ToLower()))
                    {
                        flag = true;
                    }
                }
            }

            if (os.currentFaction != null && os.currentFaction.playerValue < mission.requiredRank)
            {
                flag = true;
            }

            int num = index * 139284 + 984275 + index;
            bool outlineOnly = Button.outlineOnly;
            bool drawingOutline = Button.drawingOutline;
            Button.outlineOnly = true;
            Button.drawingOutline = false;
            if (GuiData.active == num)
            {
                sb.Draw(Utils.white, bounds, Color.Black);
            }
            else if (GuiData.hot == num)
            {
                sb.Draw(Utils.white, bounds, themeColor * 0.12f);
            }
            else
            {
                Color color = ((index % 2 == 0) ? themeColorLine : themeColorBackground);
                if (flag)
                {
                    color = Color.Lerp(color, Color.Gray, 0.25f);
                }

                sb.Draw(Utils.white, bounds, color);
            }

            if (mission.postingTitle.StartsWith("#"))
            {
                PatternDrawer.draw(bounds, 1f, Color.Transparent, Color.Black * 0.6f, sb);
            }

            // Lock mission
            if (flag)
            {
                Rectangle destinationRectangle = bounds;
                destinationRectangle.Height -= 6;
                destinationRectangle.Y += 3;
                destinationRectangle.X += bounds.Width - bounds.Height - 6;
                destinationRectangle.Width = destinationRectangle.Height;
                sb.Draw(lockIcon, destinationRectangle, Color.White * 0.2f);
            }

            if (!flag && Button.doButton(num, bounds.X, bounds.Y, bounds.Width, bounds.Height, "", Color.Transparent))
            {
                selectedElementIndex = index;
                state = HubState.ContractPreview;
                screenTransition = 1f;
            }

            string text = mission.postingTitle.Replace("#", "") ?? "";
            TextItem.doFontLabel(new Vector2(bounds.X + 1 + getTransitionOffset(index, screenTransition), bounds.Y + 3), text, GuiData.smallfont, Color.White, bounds.Width);
            string text2 = "Target: " + mission.target + " -- Client: " + mission.client + " -- Key: " + mission.generationKeys;
            TextItem.doFontLabel(new Vector2(bounds.X + 1, bounds.Y + bounds.Height - 16), text2, GuiData.detailfont, Color.White * 0.3f, bounds.Width, 13f);
            bounds.Y += bounds.Height - 1;
            bounds.Height = 1;
            sb.Draw(Utils.white, bounds, themeColor * 0.2f);
            Button.outlineOnly = outlineOnly;
            Button.drawingOutline = drawingOutline;

            return false;
        }

        public static int getTransitionOffset(int position, float screenTransition)
        {
            return (int)(Math.Pow(Math.Min((double)screenTransition + (double)position * 0.1, 1.0), 1.0) * 40.0 * (double)screenTransition);
        }
    }
}
