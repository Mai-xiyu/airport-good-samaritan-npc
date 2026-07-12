namespace GoodSamaritanNpc;

internal static class GoodSamaritanClientRoleState
{
    private const int AssignWitnessCommand = -7391201;
    private const int AssignUndercoverCommand = -7391202;
    private const int ClearRoleCommand = -7391203;
    private const int ClearAllRolesCommand = -7391204;
    private const int AssignWitnessNpcCommand = -7391205;
    private const int ClearWitnessNpcsCommand = -7391206;

    private static readonly Dictionary<uint, GoodSamaritanPlayerRole> Roles = new();
    private static readonly HashSet<uint> PendingVisuals = new();
    private static readonly HashSet<uint> WitnessNpcNetIds = new();

    internal static VoskCommandType GetSyncCommand(GoodSamaritanPlayerRole role)
    {
        return role switch
        {
            GoodSamaritanPlayerRole.PlayableWitness => (VoskCommandType)AssignWitnessCommand,
            GoodSamaritanPlayerRole.Undercover => (VoskCommandType)AssignUndercoverCommand,
            _ => (VoskCommandType)ClearRoleCommand
        };
    }

    internal static VoskCommandType GetClearAllCommand()
    {
        return (VoskCommandType)ClearAllRolesCommand;
    }

    internal static VoskCommandType GetWitnessNpcSyncCommand()
    {
        return (VoskCommandType)AssignWitnessNpcCommand;
    }

    internal static VoskCommandType GetClearWitnessNpcsCommand()
    {
        return (VoskCommandType)ClearWitnessNpcsCommand;
    }

    internal static bool TryHandleRoleSync(VoskCommandType commandType, int encodedNetId)
    {
        int command = (int)commandType;
        uint netId = unchecked((uint)encodedNetId);
        switch (command)
        {
            case AssignWitnessCommand:
                SetRole(netId, GoodSamaritanPlayerRole.PlayableWitness);
                return true;
            case AssignUndercoverCommand:
                SetRole(netId, GoodSamaritanPlayerRole.Undercover);
                return true;
            case ClearRoleCommand:
                ClearRole(netId);
                return true;
            case ClearAllRolesCommand:
                ClearAll(true);
                return true;
            case AssignWitnessNpcCommand:
                if (netId != 0u)
                {
                    WitnessNpcNetIds.Add(netId);
                }

                return true;
            case ClearWitnessNpcsCommand:
                WitnessNpcNetIds.Clear();
                return true;
            default:
                return false;
        }
    }

    internal static void SetRole(uint netId, GoodSamaritanPlayerRole role)
    {
        if (netId == 0u || role == GoodSamaritanPlayerRole.None)
        {
            ClearRole(netId);
            return;
        }

        Roles[netId] = role;
        PendingVisuals.Add(netId);
        RefreshPendingVisuals();
    }

    internal static GoodSamaritanPlayerRole GetRole(PlayerModeManager player)
    {
        if (GoodSamaritanManager.IsUnityNull(player) || player!.netId == 0u)
        {
            return GoodSamaritanPlayerRole.None;
        }

        return Roles.TryGetValue(player.netId, out var role)
            ? role
            : GoodSamaritanPlayerRole.None;
    }

    internal static bool IsGameHijacker(PlayerModeManager player)
    {
        if (GoodSamaritanManager.IsUnityNull(player))
        {
            return false;
        }

        var metaPlayer = ((Component)player!).GetComponent<Metater.MetaPlayer>() ??
                         ((Component)player).GetComponentInParent<Metater.MetaPlayer>();
        return !GoodSamaritanManager.IsUnityNull(metaPlayer) && metaPlayer!.NetworkisHijacker;
    }

    internal static bool IsWitnessNpc(uint netId)
    {
        return netId != 0u && WitnessNpcNetIds.Contains(netId);
    }

    internal static void CopyWitnessNpcNetIds(List<uint> destination)
    {
        destination.Clear();
        foreach (uint netId in WitnessNpcNetIds)
        {
            destination.Add(netId);
        }
    }

    internal static void RefreshPendingVisuals()
    {
        if (PendingVisuals.Count == 0)
        {
            return;
        }

        var players = Object.FindObjectsOfType<PlayerModeManager>();
        if (players == null)
        {
            return;
        }

        var resolved = new List<uint>();
        foreach (uint netId in PendingVisuals)
        {
            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                if (GoodSamaritanManager.IsUnityNull(player) || player!.netId != netId)
                {
                    continue;
                }

                ApplyVisualOverride(player);
                resolved.Add(netId);
                break;
            }
        }

        for (int i = 0; i < resolved.Count; i++)
        {
            PendingVisuals.Remove(resolved[i]);
        }
    }

    internal static void ApplyVisualOverride(PlayerModeManager player)
    {
        if (GoodSamaritanManager.IsUnityNull(player) ||
            GetRole(player) != GoodSamaritanPlayerRole.PlayableWitness ||
            !GoodSamaritanPlugin.Settings.UseCivilianModelForPlayableWitnessPlayers.Value)
        {
            return;
        }

        var smugglerVisuals = player!.smugglerVisualsGameObject;
        var agentVisuals = player.agentVisualsGameObject;
        if (!GoodSamaritanManager.IsUnityNull(smugglerVisuals) && !smugglerVisuals!.activeSelf)
        {
            smugglerVisuals.SetActive(true);
        }

        if (!GoodSamaritanManager.IsUnityNull(agentVisuals) && agentVisuals!.activeSelf)
        {
            agentVisuals.SetActive(false);
        }

        EnsureCivilianSkinGenerated(player);
    }

    internal static void ClearAll(bool restoreVisuals)
    {
        WitnessNpcNetIds.Clear();
        if (Roles.Count == 0)
        {
            PendingVisuals.Clear();
            return;
        }

        var previousNetIds = new HashSet<uint>(Roles.Keys);
        Roles.Clear();
        PendingVisuals.Clear();
        if (restoreVisuals)
        {
            RestoreOriginalVisuals(previousNetIds);
        }
    }

    private static void ClearRole(uint netId)
    {
        if (netId == 0u || !Roles.Remove(netId))
        {
            return;
        }

        PendingVisuals.Remove(netId);
        var player = FindPlayer(netId);
        RestoreOriginalVisual(player);
    }

    private static PlayerModeManager FindPlayer(uint netId)
    {
        var players = Object.FindObjectsOfType<PlayerModeManager>();
        if (players == null)
        {
            return null;
        }

        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            if (!GoodSamaritanManager.IsUnityNull(player) && player!.netId == netId)
            {
                return player;
            }
        }

        return null;
    }

    private static void RestoreOriginalVisuals(HashSet<uint> netIds)
    {
        var players = Object.FindObjectsOfType<PlayerModeManager>();
        if (players == null)
        {
            return;
        }

        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            if (!GoodSamaritanManager.IsUnityNull(player) && netIds.Contains(player!.netId))
            {
                RestoreOriginalVisual(player);
            }
        }
    }

    private static void RestoreOriginalVisual(PlayerModeManager player)
    {
        if (GoodSamaritanManager.IsUnityNull(player))
        {
            return;
        }

        try
        {
            player!.ApplyModeObjects(player.NetworkisAgent);
        }
        catch (Exception ex)
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"Playable witness visual restore failed: {ex.Message}");
        }
    }

    private static void EnsureCivilianSkinGenerated(PlayerModeManager player)
    {
        try
        {
            var metaPlayer = ((Component)player).GetComponent<Metater.MetaPlayer>() ??
                             ((Component)player).GetComponentInParent<Metater.MetaPlayer>();
            if (!GoodSamaritanManager.IsUnityNull(metaPlayer) &&
                !GoodSamaritanManager.IsUnityNull(metaPlayer!.npcGenerator))
            {
                metaPlayer.npcGenerator.LoadSkinPreset(metaPlayer.NetworkskinSeed);
            }
        }
        catch (Exception ex)
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"Playable witness civilian skin refresh failed: {ex.Message}");
        }
    }
}
