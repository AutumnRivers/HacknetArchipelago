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

using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace HacknetArchipelago.Replacements
{
    [HarmonyPatch]
    public class LockRequiredExecutableMissions
    {
        [HarmonyILManipulator]
        [HarmonyPatch(typeof(MissionHubServer))]
        [HarmonyPatch(nameof(MissionHubServer.drawMissionEntry))]
        static void FindAndLockMissions(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.After,
                x => x.MatchLdcI4(0),
                x => x.MatchStloc(0)
            );

            c.Emit(OpCodes.Ldarg_3);

            c.EmitDelegate<Func<ActiveMission, bool>>((mission) =>
            {
                if (MissionRules.RequiredMissionExecutables.TryGetValue(mission.email.subject, out string[] requiredExecutables))
                {
                    for (int i = 0; i < requiredExecutables.Length; i++)
                    {
                        if (!HacknetAPMod.receivedItems.Contains(requiredExecutables[i].ToLower()))
                        {
                            return true;
                        }
                    }
                }

                return false;
            });

            ILLabel skipLabel = il.DefineLabel();

            c.Emit(OpCodes.Brfalse_S, skipLabel);

            c.Emit(OpCodes.Ldc_I4_1);
            c.Emit(OpCodes.Stloc_0);

            skipLabel.Target = c.Next;
        }
    }
}
