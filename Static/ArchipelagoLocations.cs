using System.Collections.Generic;

namespace HacknetArchipelago.Static
{
    public class ArchipelagoLocations
    {
        public static readonly Dictionary<string, string> MNameToArchiLocation = new Dictionary<string, string>()
        {
            { "First Contact", "INTRO First Contact" },
            { "Getting some tools together", "INTRO Getting Some Tools Together" },
            { "Maiden Flight", "INTRO Maiden Flight" },
            { "Something in return", "INTRO Something In Return" },
            { "Where to from here", "INTRO Complete Introduction" },

            { "Confirmation Mission", "ENT Intro / Confirmation" },
            { "Welcome", "ENT Intro / Welcome" },
            { "Point Clicker", "ENT PointClicker" },
            { "The famous counter-hack", "ENT The Famous Counter-Hack" },
            { "Back to School", "ENT Back to School" },
            { "X-C Project", "ENT X-C Project" },
            { "eOS Device Scanning", "ENT eOS Intro" },
            { "Smash N' Grab", "ENT Smash N' Grab" },
            { "Aggression must be Punished", "ENT Naix" },

            { "CSEC Invitation", "CSEC Invitation OR /el Sec Completion" },
            { "A Victory - Perhaps a turning point", "CSEC Invitation OR /el Sec Completion" }, // For /el Sec Completion
            { "CSEC Invitation - Affirmation", "CSEC CFC Herbs and Spices" },
            { "CSEC Invitation - Attenuation", "CSEC CFC Herbs and Spices" }, // For /el Sec Path

            { "Rod of Asclepius", "CSEC Rod of Asclepius" },
            { "Binary Universe(ity)", "CSEC Binary University" },
            { "Ghosting the Vault", "CSEC Ghosting The Vault" },
            { "Imposters on Death Row", "CSEC Imposters on Death Row" },
            { "RE: Through the Spyglass", "CSEC Through the Spyglass" },
            { "Red Line", "CSEC Red Line" }, // This one is basically a freebie
            { "Wipe the record clean", "CSEC Wipe Clean An Academic Record" },
            { "A Convincing Application", "CSEC A Convincing Application" },
            { "Unjust Absence", "CSEC Unjust Absence" },
            { "Two ships in the night", "CSEC Decrypt A Secure Transmission" },
            { "Jailbreak", "CSEC Compromise an eOS Device" },
            { "Project Junebug", "CSEC Project Junebug" },
            { "Bit's disappearance Investigation", "CSEC Start Bit Path" },

            { "Bit -- Foundation", "VBIT Foundation" },
            { "Bit -- Substantiation", "VBIT Substantiation" },
            { "Bit -- Investigation", "VBIT Investigation" },
            { "Bit -- Propagation", "VBIT Propagation" },
            { "Bit -- Vindication", "VBIT Vindication" },
            { "Bit -- Termination", "VBIT Termination / Sequencer" },

            { "The Ricer", "LABS The Ricer" },
            { "DDOSer on some critical servers", "LABS DDOSer On Critical Servers" },
            { "It Follows", "LABS It Follows" },
            { "Bean Stalk", "LABS Bean Stalk" },
            { "Expo Grave", "LABS Expo Grave" },
            { "The Keyboard Life", "LABS The Keyboard Life" },
            { "The Hermetic Alchemists", "LABS Hermetic Alchemists" },
            { "Memory Forensics", "LABS Memory Forensics" },
            { "Neopals", "LABS Neopals" },
            { "Striker's Stash", "LABS Striker's Stash" },
            { "Take Flight", "LABS Take Flight" },
            { "Take_Flight Cont.", "LABS Take Flight Cont." }
        };

        public static readonly Dictionary<string, string> FlagsToLocations = new Dictionary<string, string>()
        {
            { "DLC_Player_IRC_Authenticated", "LABS Finish Kaguya Trials" },
            { "dlc_complete", "LABS Altitude Loss" },
            { "startupBreakinTrapPassed", "STRK CoelTrain Recovery" },
            { "BootFailureThemeSongChange", "NAIX Recover" },

            { "pointclicker_basic_Unlocked", "ACHV PointClicker" },
            { "pointclicker_expert_Unlocked", "ACHV You better not have clicked for those..." },
            { "themeswitch_run_Unlocked", "ACHV Makeover" },
            { "trace_close_Unlocked", "ACHV To the Wire" },
            { "kill_tutorial_Unlocked", "ACHV Quickdraw" }
        };

        public static readonly Dictionary<string, string> NodeIDToLocations = new Dictionary<string, string>()
        {
            { "viperScanEarlyGame", "NODE Entropy Asset Cache" },
            { "clockServer", "NODE Timekeeper's Vault" },
            { "psTrial01", "NODE Polar Star / Trial of Patience" },
            { "psTrial02", "NODE Polar Star / Trial of Haste" },
            { "psTrial03b", "NODE Polar Star / Trial of Dilligence" },
            { "psTrial04", "NODE Polar Star / Trial of Focus" },
            { "honeypot01", "NODE CCC Hacksquad Dump" },
            { "dCoelgateway", "NODE Coel_Gateway" },
            { "dNaixSecretLink", "NODE Pellium Box" },
            { "dGibson", "LABS Break Into Gibson" }
        };
    }
}
