using System.Collections.Generic;

namespace HacknetArchipelago.Static
{
    public class ArchipelagoLocations
    {
        public static readonly Dictionary<string, string> MNameToArchiLocation = new Dictionary<string, string>()
        {
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
            { "Bit -- Termination", "VBIT Termination / Sequencer" }

            // TODO: Add Labyrinths
        };
    }
}
