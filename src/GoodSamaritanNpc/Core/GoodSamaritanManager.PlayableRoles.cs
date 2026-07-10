namespace GoodSamaritanNpc;

public sealed partial class GoodSamaritanManager
{
    [HideFromIl2Cpp]
    private void UpdatePlayableWitnessAssignments()
    {
        if (!GoodSamaritanPlugin.Settings.RequiresModdedClientCapability)
        {
            CleanupPlayableRoles(true);
            return;
        }

        CleanupPlayableRoles(false);

        var gameManager = GameManager.Instance;
        bool gameStarted = !IsUnityNull(gameManager) && gameManager!.GameStartedSyncVar;
        if (!gameStarted)
        {
            if (lastGameStarted || playableWitnessNetIds.Count > 0 || playableUndercoverNetIds.Count > 0)
            {
                BroadcastAllRolesCleared();
            }

            lastGameStarted = false;
            playableWitnessesAssignedThisRound = false;
            playableUndercoverAssignedThisRound = false;
            playableWitnessNetIds.Clear();
            playableUndercoverNetIds.Clear();
            playerWitnesses.Clear();
            return;
        }

        if (!lastGameStarted)
        {
            playableWitnessesAssignedThisRound = false;
            playableUndercoverAssignedThisRound = false;
            nextPlayerAssignmentTime = Time.timeAsDouble + 1.5d;
        }

        lastGameStarted = true;
        if ((playableWitnessesAssignedThisRound && playableUndercoverAssignedThisRound) || Time.timeAsDouble < nextPlayerAssignmentTime)
        {
            return;
        }

        if (!AreNativeRolesReadyForAssignment())
        {
            nextPlayerAssignmentTime = Time.timeAsDouble + 1d;
            return;
        }

        if (!playableUndercoverAssignedThisRound)
        {
            playableUndercoverAssignedThisRound = AssignPlayableUndercoverPlayers();
        }

        if (!playableWitnessesAssignedThisRound)
        {
            playableWitnessesAssignedThisRound = AssignPlayableWitnessPlayers();
        }

        if (!playableWitnessesAssignedThisRound || !playableUndercoverAssignedThisRound)
        {
            nextPlayerAssignmentTime = Time.timeAsDouble + 2d;
        }
    }

    [HideFromIl2Cpp]
    private bool AssignPlayableWitnessPlayers()
    {
        if (!GoodSamaritanPlugin.Settings.EnablePlayableWitnessPlayers.Value)
        {
            return true;
        }

        int maxPlayers = Mathf.Max(0, GoodSamaritanPlugin.Settings.MaxPlayableWitnessPlayers.Value);
        if (maxPlayers <= 0 || moddedPlayerNetIds.Count == 0)
        {
            return true;
        }

        float chance = Mathf.Clamp01(GoodSamaritanPlugin.Settings.PlayableWitnessChance.Value);
        if (chance <= 0f)
        {
            return true;
        }

        var players = Object.FindObjectsOfType<PlayerModeManager>();
        if (players == null || players.Length == 0)
        {
            return false;
        }

        var candidates = new List<PlayerModeManager>();
        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            if (IsUnityNull(player))
            {
                continue;
            }

            uint netId = player!.netId;
            if (netId == 0u ||
                playableWitnessNetIds.Contains(netId) ||
                playableUndercoverNetIds.Contains(netId) ||
                !IsRoleSyncCapable(netId) ||
                GoodSamaritanClientRoleState.IsGameHijacker(player))
            {
                continue;
            }

            if (!player.NetworkisAgent && CountSmugglerPlayers(players) <= 1)
            {
                continue;
            }

            candidates.Add(player);
        }

        Shuffle(candidates);
        if (candidates.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < candidates.Count && playerWitnesses.Count < maxPlayers; i++)
        {
            if (UnityEngine.Random.value > chance)
            {
                continue;
            }

            AddPlayableWitness(candidates[i]);
        }

        return true;
    }

    [HideFromIl2Cpp]
    private bool AssignPlayableUndercoverPlayers()
    {
        if (!GoodSamaritanPlugin.Settings.EnablePlayableUndercoverPlayers.Value)
        {
            return true;
        }

        // Native hijacking allegiance is controlled by NetworkisHijacker, so the
        // smuggler-side undercover role is only valid in the standard game modes.
        if (IsHijackingModeActive())
        {
            return true;
        }

        int maxPlayers = Mathf.Max(0, GoodSamaritanPlugin.Settings.MaxPlayableUndercoverPlayers.Value);
        if (maxPlayers <= 0 || moddedPlayerNetIds.Count == 0)
        {
            return true;
        }

        float chance = Mathf.Clamp01(GoodSamaritanPlugin.Settings.PlayableUndercoverChance.Value);
        if (chance <= 0f)
        {
            return true;
        }

        var players = Object.FindObjectsOfType<PlayerModeManager>();
        if (players == null || players.Length == 0)
        {
            return false;
        }

        int agentCount = CountAgentPlayers(players);
        var candidates = new List<PlayerModeManager>();
        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            if (IsUnityNull(player))
            {
                continue;
            }

            uint netId = player!.netId;
            if (netId == 0u ||
                playableWitnessNetIds.Contains(netId) ||
                playableUndercoverNetIds.Contains(netId) ||
                !IsRoleSyncCapable(netId) ||
                GoodSamaritanClientRoleState.IsGameHijacker(player))
            {
                continue;
            }

            if (player.NetworkisAgent && agentCount <= 1)
            {
                continue;
            }

            candidates.Add(player);
        }

        Shuffle(candidates);
        if (candidates.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < candidates.Count && playableUndercoverNetIds.Count < maxPlayers; i++)
        {
            if (UnityEngine.Random.value > chance)
            {
                continue;
            }

            AddPlayableUndercover(candidates[i]);
        }

        return true;
    }

    [HideFromIl2Cpp]
    private void AddPlayableWitness(PlayerModeManager player)
    {
        if (IsUnityNull(player))
        {
            return;
        }

        uint netId = player!.netId;
        if (netId == 0u || playableWitnessNetIds.Contains(netId) || playableUndercoverNetIds.Contains(netId))
        {
            return;
        }

        if (!player.NetworkisAgent)
        {
            player.ServerSetIsAgent(true);
        }

        playableWitnessNetIds.Add(netId);
        BroadcastRoleAssignment(netId, GoodSamaritanPlayerRole.PlayableWitness);
        playerWitnesses.Add(new PlayerWitnessState(player)
        {
            NextReportTime = Time.timeAsDouble + UnityEngine.Random.Range(1f, 4f)
        });

        ShowPlayerWitnessIndicator(player, Mathf.Max(2f, GoodSamaritanPlugin.Settings.HighlightSeconds.Value));
        GoodSamaritanPlugin.LogSource.LogInfo($"Assigned playable witness player netId {netId}.");
    }

    [HideFromIl2Cpp]
    private void AddPlayableUndercover(PlayerModeManager player)
    {
        if (IsUnityNull(player))
        {
            return;
        }

        uint netId = player!.netId;
        if (netId == 0u || playableUndercoverNetIds.Contains(netId) || playableWitnessNetIds.Contains(netId))
        {
            return;
        }

        if (player.NetworkisAgent)
        {
            player.ServerSetIsAgent(false);
        }

        playableUndercoverNetIds.Add(netId);
        BroadcastRoleAssignment(netId, GoodSamaritanPlayerRole.Undercover);
        ShowTargetIndicator(player, Mathf.Max(2f, GoodSamaritanPlugin.Settings.HighlightSeconds.Value));
        GoodSamaritanPlugin.LogSource.LogInfo($"Assigned undercover player netId {netId}.");
    }

    [HideFromIl2Cpp]
    private void CleanupPlayableRoles(bool clearAll)
    {
        if (clearAll)
        {
            if (playableWitnessNetIds.Count > 0 || playableUndercoverNetIds.Count > 0)
            {
                BroadcastAllRolesCleared();
            }

            playerWitnesses.Clear();
            playableWitnessNetIds.Clear();
            playableUndercoverNetIds.Clear();
            return;
        }

        var alive = new HashSet<uint>();
        var players = Object.FindObjectsOfType<PlayerModeManager>();
        if (players != null)
        {
            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                if (!IsUnityNull(player) && player!.netId != 0u)
                {
                    alive.Add(player.netId);

                    if (GoodSamaritanClientRoleState.IsGameHijacker(player) &&
                        (playableWitnessNetIds.Remove(player.netId) || playableUndercoverNetIds.Remove(player.netId)))
                    {
                        BroadcastRoleClear(player.netId);
                        if (player.NetworkisAgent)
                        {
                            player.ServerSetIsAgent(false);
                        }

                        GoodSamaritanPlugin.LogSource.LogWarning($"Removed conflicting playable role from native hijacker netId {player.netId}.");
                    }
                }
            }
        }

        var disconnectedRoles = new HashSet<uint>();
        foreach (uint id in playableWitnessNetIds)
        {
            if (!alive.Contains(id))
            {
                disconnectedRoles.Add(id);
            }
        }

        foreach (uint id in playableUndercoverNetIds)
        {
            if (!alive.Contains(id))
            {
                disconnectedRoles.Add(id);
            }
        }

        foreach (uint id in disconnectedRoles)
        {
            BroadcastRoleClear(id);
        }

        moddedPlayerNetIds.RemoveWhere(id => !alive.Contains(id));
        foreach (uint id in new List<uint>(moddedPlayerVersions.Keys))
        {
            if (!alive.Contains(id))
            {
                moddedPlayerVersions.Remove(id);
            }
        }

        playableWitnessNetIds.RemoveWhere(id => !alive.Contains(id));
        playableUndercoverNetIds.RemoveWhere(id => !alive.Contains(id));

        for (int i = playerWitnesses.Count - 1; i >= 0; i--)
        {
            var witness = playerWitnesses[i];
            if (witness == null || IsUnityNull(witness.Player) || !playableWitnessNetIds.Contains(witness.Player.netId))
            {
                playerWitnesses.RemoveAt(i);
            }
        }
    }

    private static int CountSmugglerPlayers(Il2CppArrayBase<PlayerModeManager> players)
    {
        int count = 0;
        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            if (!IsUnityNull(player) && !player!.NetworkisAgent)
            {
                count++;
            }
        }

        return count;
    }

    private static int CountAgentPlayers(Il2CppArrayBase<PlayerModeManager> players)
    {
        int count = 0;
        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            if (!IsUnityNull(player) && player!.NetworkisAgent)
            {
                count++;
            }
        }

        return count;
    }

    [HideFromIl2Cpp]
    internal GoodSamaritanPlayerRole GetPlayerRole(PlayerModeManager player)
    {
        if (IsUnityNull(player) || player!.netId == 0u)
        {
            return GoodSamaritanPlayerRole.None;
        }

        if (playableWitnessNetIds.Contains(player.netId))
        {
            return GoodSamaritanPlayerRole.PlayableWitness;
        }

        return playableUndercoverNetIds.Contains(player.netId)
            ? GoodSamaritanPlayerRole.Undercover
            : GoodSamaritanPlayerRole.None;
    }

    [HideFromIl2Cpp]
    internal bool IsUndercover(PlayerModeManager player)
    {
        return GetPlayerRole(player) == GoodSamaritanPlayerRole.Undercover;
    }

    [HideFromIl2Cpp]
    private bool ShouldReportActorForEvent(PlayerModeManager actor, SuspicionEventType eventType)
    {
        if (IsUnityNull(actor))
        {
            return true;
        }

        if (IsUndercover(actor))
        {
            return true;
        }

        if (GoodSamaritanClientRoleState.IsGameHijacker(actor))
        {
            return true;
        }

        bool isTsa = actor!.NetworkisAgent;
        if (!isTsa)
        {
            return true;
        }

        if (!GoodSamaritanPlugin.Settings.IgnoreTsaSuspicion.Value)
        {
            return true;
        }

        return IsCivilianAttackEvent(eventType) && GoodSamaritanPlugin.Settings.ReportTsaCivilianAttacks.Value;
    }

    private static bool IsCivilianAttackEvent(SuspicionEventType eventType)
    {
        return eventType == SuspicionEventType.AttackingCivilian || eventType == SuspicionEventType.TacklingCivilian;
    }

    [HideFromIl2Cpp]
    private static bool IsHijackingModeActive()
    {
        var gameManager = GameManager.Instance;
        return !IsUnityNull(gameManager) && gameManager!.ActiveGameMode is HijackingGameMode;
    }

    [HideFromIl2Cpp]
    private static bool AreNativeRolesReadyForAssignment()
    {
        if (!IsHijackingModeActive())
        {
            return true;
        }

        var hijackingManager = HijackingManager.Instance;
        return !IsUnityNull(hijackingManager) &&
               hijackingManager!.serverParticipants != null &&
               hijackingManager.serverParticipants.Count > 0;
    }

    private static void Shuffle(List<PlayerModeManager> players)
    {
        for (int i = players.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (players[i], players[j]) = (players[j], players[i]);
        }
    }
}
