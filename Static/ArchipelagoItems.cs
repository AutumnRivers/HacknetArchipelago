using System.Collections.Generic;

namespace HacknetArchipelago.Static
{
    public class ArchipelagoItems
    {
        public static readonly Dictionary<string, int> ItemNamesAndPortIDs = new Dictionary<string, int>()
        {
            { "sshcrack", 22 },
            { "ftpbounce", 21 },
            { "smtpoverflow", 25 },
            { "sql_memcorrupt", 1433 },
            { "webserverworm", 80 },
            { "kbtporttest", 104 },
            { "decypher", 9 },
            { "dechead", 10 },
            { "eosdevicescan", 13 },
            { "opshell", 41 },
            { "tracekill", 12 },
            { "themechanger", 14 },
            { "clock", 11 },
            { "hexclock", 16 },
            { "securitytracer", 4 },
            { "hacknetexe", 15 },
            { "torrentstreaminjector", 6881 },
            { "ssltrojan", 443 },
            { "ftpsprint", 211 },
            { "memdumpgenerator", 34 },
            { "memforensics", 33 },
            { "signalscrambler", 32 },
            { "pacificportcrusher", 192 },
            { "comshell", 36 },
            { "netmaporganizer", 35 },
            { "dnotes", 37 },
            { "tuneswap", 39 },
            { "clockv2", 38 }
        };

        public static readonly List<string> ProgressionItems = new List<string>()
        {
            "sshcrack", "ftpbounce", "smtpoverflow", "sql_memcorrupt", "webserverworm",
            "kbtporttest", "decyper", "dechead", "eosdevicescan", "torrentstreaminjector",
            "ssltrojan", "ftpsprint", "memdumpgenerator", "memforensics", "signalscrambler",
            "pacificportcrusher"
        };

        public static readonly List<string> UsefulItems = new List<string>()
        {
            "opshell",
            "tracekill",
            "comshell",
            "netmaporganizer"
        };

        public static readonly Dictionary<string, string> ItemToFlags = new Dictionary<string, string>()
        {
            { "Kaguya Trials Access", "CanAccessTrials" }
        };
    }
}
