namespace GoodSamaritanNpc;

internal sealed class GoodSamaritanConfig
{
    internal readonly ConfigEntry<bool> Enabled;
    internal readonly ConfigEntry<bool> ConvertExistingNpcs;
    internal readonly ConfigEntry<float> ExistingNpcChance;
    internal readonly ConfigEntry<int> ExtraSpawnCount;
    internal readonly ConfigEntry<float> ScanIntervalSeconds;
    internal readonly ConfigEntry<float> WitnessRadius;
    internal readonly ConfigEntry<float> WitnessFovDegrees;
    internal readonly ConfigEntry<float> ReportCooldownSeconds;
    internal readonly ConfigEntry<float> TargetCooldownSeconds;
    internal readonly ConfigEntry<float> HighlightSeconds;
    internal readonly ConfigEntry<string> NpcSuspicionPreset;
    internal readonly ConfigEntry<bool> EnableDirectTargetReports;
    internal readonly ConfigEntry<bool> DetectRevealingActions;
    internal readonly ConfigEntry<bool> DetectCarriedContraband;
    internal readonly ConfigEntry<bool> DetectHiddenContraband;
    internal readonly ConfigEntry<bool> DetectCivilianAttacks;
    internal readonly ConfigEntry<bool> DetectJumping;
    internal readonly ConfigEntry<bool> DetectLineCutting;
    internal readonly ConfigEntry<bool> DetectContrabandPickup;
    internal readonly ConfigEntry<bool> IgnoreTsaSuspicion;
    internal readonly ConfigEntry<bool> ReportTsaCivilianAttacks;
    internal readonly ConfigEntry<bool> ShowTeamHighlights;
    internal readonly ConfigEntry<bool> ShowReportHighlightsToAllModdedClients;
    internal readonly ConfigEntry<float> AreaHighlightSeconds;
    internal readonly ConfigEntry<bool> EnablePlayableWitnessPlayers;
    internal readonly ConfigEntry<int> MaxPlayableWitnessPlayers;
    internal readonly ConfigEntry<float> PlayableWitnessChance;
    internal readonly ConfigEntry<bool> EnablePlayableUndercoverPlayers;
    internal readonly ConfigEntry<int> MaxPlayableUndercoverPlayers;
    internal readonly ConfigEntry<float> PlayableUndercoverChance;
    internal readonly ConfigEntry<bool> EnableCustomClientMarker;
    internal readonly ConfigEntry<bool> EnableVoiceLine;
    internal readonly ConfigEntry<string> Language;

    internal GoodSamaritanConfig(ConfigFile config)
    {
        Enabled = config.Bind("General", "Enabled", true, "Enable Good Samaritan NPC behaviour.");
        ConvertExistingNpcs = config.Bind("Spawn", "ConvertExistingNpcs", true, "Convert a sampled subset of existing NPCs into witnesses.");
        ExistingNpcChance = config.Bind("Spawn", "ExistingNpcChance", 0.18f, "Chance for each ordinary NPC to become a witness when first seen by the server.");
        ExtraSpawnCount = config.Bind("Spawn", "ExtraSpawnCount", 0, "Additional witness NPCs to spawn through NpcManager per level.");
        ScanIntervalSeconds = config.Bind("Detection", "ScanIntervalSeconds", 0.75f, "Seconds between witness scans.");
        WitnessRadius = config.Bind("Detection", "WitnessRadius", 18f, "Suspicious-player search radius around each witness.");
        WitnessFovDegrees = config.Bind("Detection", "WitnessFovDegrees", 90f, "Witness field of view for direct target callouts.");
        ReportCooldownSeconds = config.Bind("Detection", "ReportCooldownSeconds", 12f, "Cooldown per witness and global report throttle.");
        TargetCooldownSeconds = config.Bind("Detection", "TargetCooldownSeconds", 18f, "Cooldown before the same suspicious target can be called out again.");
        NpcSuspicionPreset = config.Bind("Detection", "NpcSuspicionPreset", "Normal", "NPC suspicion preset. Supported: Easy, Normal, Hard, Custom.");
        EnableDirectTargetReports = config.Bind("Detection", "EnableDirectTargetReports", true, "Allow witnesses to point out a directly visible suspicious player. Easy preset disables this.");
        DetectRevealingActions = config.Bind("Detection", "DetectRevealingActions", true, "Report reveal actions such as CCTV or Cupcake reveals.");
        DetectCarriedContraband = config.Bind("Detection", "DetectCarriedContraband", true, "Report players carrying contraband in hand or hip slot.");
        DetectHiddenContraband = config.Bind("Detection", "DetectHiddenContraband", true, "Report players hiding contraband in bags or butt storage.");
        DetectCivilianAttacks = config.Bind("Detection", "DetectCivilianAttacks", true, "Report attacks, shots, or tackles against civilians.");
        DetectJumping = config.Bind("Detection", "DetectJumping", false, "Report nearby jumping players. Hard preset enables this.");
        DetectLineCutting = config.Bind("Detection", "DetectLineCutting", false, "Report likely queue cutting near NPC lines. Hard preset enables this.");
        DetectContrabandPickup = config.Bind("Detection", "DetectContrabandPickup", true, "Immediately report players who pick up contraband.");
        IgnoreTsaSuspicion = config.Bind("Detection", "IgnoreTsaSuspicion", true, "Ignore ordinary TSA/agent actions when witnesses look for suspicious behavior.");
        ReportTsaCivilianAttacks = config.Bind("Detection", "ReportTsaCivilianAttacks", false, "Allow witnesses to report TSA/agent civilian attacks. Original game punishment still applies.");
        HighlightSeconds = config.Bind("Feedback", "HighlightSeconds", 4f, "Seconds to show the original spotted icon on a directly witnessed target.");
        ShowTeamHighlights = config.Bind("Feedback", "ShowTeamHighlights", true, "Show local blue outlines for TSA/agent and playable witness allies on modded clients.");
        ShowReportHighlightsToAllModdedClients = config.Bind("Feedback", "ShowReportHighlightsToAllModdedClients", true, "Show local yellow outlines/areas for witness reports on modded clients.");
        AreaHighlightSeconds = config.Bind("Feedback", "AreaHighlightSeconds", 5f, "Seconds to show the yellow local floor rectangle for area reports.");
        EnablePlayableWitnessPlayers = config.Bind("Playable", "EnablePlayableWitnessPlayers", true, "Randomly assign modded players as playable witnesses when the host also has the mod.");
        MaxPlayableWitnessPlayers = config.Bind("Playable", "MaxPlayableWitnessPlayers", 1, "Maximum playable witness players per round.");
        PlayableWitnessChance = config.Bind("Playable", "PlayableWitnessChance", 0.25f, "Chance for each eligible modded player to become a playable witness.");
        EnablePlayableUndercoverPlayers = config.Bind("Playable", "EnablePlayableUndercoverPlayers", false, "Randomly assign modded players as undercover smuggler-side witnesses.");
        MaxPlayableUndercoverPlayers = config.Bind("Playable", "MaxPlayableUndercoverPlayers", 1, "Maximum undercover players per round.");
        PlayableUndercoverChance = config.Bind("Playable", "PlayableUndercoverChance", 0.15f, "Chance for each eligible modded player to become undercover.");
        EnableCustomClientMarker = config.Bind("Feedback", "EnableCustomClientMarker", true, "Show an additional local exclamation mark on modded clients.");
        EnableVoiceLine = config.Bind("Feedback", "EnableVoiceLine", true, "Play a short local witness alert sound on modded clients.");
        Language = config.Bind("Localization", "Language", "Auto", "Message language. Supported: Auto, zh-Hans, en, ja, ko, fr, de, es, ru, pt, tr, uk.");
    }

    internal bool DirectTargetReportsEnabled => !IsPreset("Easy") && EnableDirectTargetReports.Value;
    internal bool ShouldDetectReveals => DetectRevealingActions.Value;
    internal bool ShouldDetectCarriedContraband => DetectCarriedContraband.Value || IsPreset("Hard");
    internal bool ShouldDetectHiddenContraband => DetectHiddenContraband.Value || IsPreset("Hard");
    internal bool ShouldDetectCivilianAttacks => DetectCivilianAttacks.Value;
    internal bool ShouldDetectJumping => DetectJumping.Value || IsPreset("Hard");
    internal bool ShouldDetectLineCutting => DetectLineCutting.Value || IsPreset("Hard");
    internal bool ShouldDetectContrabandPickup => DetectContrabandPickup.Value || IsPreset("Hard");
    internal bool RequiresModdedClientCapability => EnablePlayableWitnessPlayers.Value || EnablePlayableUndercoverPlayers.Value;

    internal bool IsPreset(string preset)
    {
        return string.Equals(NpcSuspicionPreset.Value, preset, StringComparison.OrdinalIgnoreCase);
    }

    internal void ApplyPreset(string preset)
    {
        if (string.Equals(preset, "Easy", StringComparison.OrdinalIgnoreCase))
        {
            NpcSuspicionPreset.Value = "Easy";
            EnableDirectTargetReports.Value = false;
            DetectRevealingActions.Value = true;
            DetectCarriedContraband.Value = true;
            DetectHiddenContraband.Value = false;
            DetectCivilianAttacks.Value = true;
            DetectJumping.Value = false;
            DetectLineCutting.Value = false;
            DetectContrabandPickup.Value = false;
            ReportCooldownSeconds.Value = Mathf.Max(18f, ReportCooldownSeconds.Value);
            TargetCooldownSeconds.Value = Mathf.Max(24f, TargetCooldownSeconds.Value);
            return;
        }

        if (string.Equals(preset, "Hard", StringComparison.OrdinalIgnoreCase))
        {
            NpcSuspicionPreset.Value = "Hard";
            EnableDirectTargetReports.Value = true;
            DetectRevealingActions.Value = true;
            DetectCarriedContraband.Value = true;
            DetectHiddenContraband.Value = true;
            DetectCivilianAttacks.Value = true;
            DetectJumping.Value = true;
            DetectLineCutting.Value = true;
            DetectContrabandPickup.Value = true;
            ReportCooldownSeconds.Value = Mathf.Min(8f, ReportCooldownSeconds.Value);
            TargetCooldownSeconds.Value = Mathf.Min(12f, TargetCooldownSeconds.Value);
            return;
        }

        NpcSuspicionPreset.Value = "Normal";
        EnableDirectTargetReports.Value = true;
        DetectRevealingActions.Value = true;
        DetectCarriedContraband.Value = true;
        DetectHiddenContraband.Value = true;
        DetectCivilianAttacks.Value = true;
        DetectJumping.Value = false;
        DetectLineCutting.Value = false;
        DetectContrabandPickup.Value = true;
    }

    internal void Save()
    {
        Enabled.ConfigFile.Save();
    }
}
