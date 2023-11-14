using Hacknet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HacknetArchipelago.Static
{
    public class ArchipelagoMissionRules
    {
        public static readonly Dictionary<string, string[]> RequiredMissionExecutables = new Dictionary<string, string[]>()
        {
            { "The Kaguya Trials", new string[1] { "TorrentStreamInjector" } },
            { "Through the Spyglass", new string[2] { "Decypher", "DECHead" } },
            { "Ghosting the Vault", new string[1] { "DECHead" } },
            { "Project Junebug", new string[1] { "KBTPortTest" } },
            { "Bit -- Foundation", new string[1] { "KBTPortTest" } }
        };
    }
}
