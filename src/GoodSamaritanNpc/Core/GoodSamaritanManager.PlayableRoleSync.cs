namespace GoodSamaritanNpc;

public sealed partial class GoodSamaritanManager
{
    [HideFromIl2Cpp]
    private void SendRoleSnapshot(PlayerModeManager recipient)
    {
        if (IsUnityNull(recipient) || !IsRoleSyncCapable(recipient!.netId))
        {
            return;
        }

        SendRoleCommand(recipient, GoodSamaritanClientRoleState.GetClearAllCommand(), 0u);
        foreach (uint netId in playableWitnessNetIds)
        {
            SendRoleCommand(recipient, GoodSamaritanClientRoleState.GetSyncCommand(GoodSamaritanPlayerRole.PlayableWitness), netId);
        }

        foreach (uint netId in playableUndercoverNetIds)
        {
            SendRoleCommand(recipient, GoodSamaritanClientRoleState.GetSyncCommand(GoodSamaritanPlayerRole.Undercover), netId);
        }

        foreach (var witness in witnesses)
        {
            uint netId = GetWitnessNetworkId(witness);
            if (netId != 0u)
            {
                SendRoleCommand(recipient, GoodSamaritanClientRoleState.GetWitnessNpcSyncCommand(), netId);
            }
        }
    }

    [HideFromIl2Cpp]
    private void BroadcastRoleAssignment(uint netId, GoodSamaritanPlayerRole role)
    {
        if (netId == 0u)
        {
            return;
        }

        GoodSamaritanClientRoleState.SetRole(netId, role);
        BroadcastRoleCommand(GoodSamaritanClientRoleState.GetSyncCommand(role), netId);
    }

    [HideFromIl2Cpp]
    private void BroadcastRoleClear(uint netId)
    {
        if (netId == 0u)
        {
            return;
        }

        GoodSamaritanClientRoleState.SetRole(netId, GoodSamaritanPlayerRole.None);
        BroadcastRoleCommand(GoodSamaritanClientRoleState.GetSyncCommand(GoodSamaritanPlayerRole.None), netId);
    }

    [HideFromIl2Cpp]
    private void BroadcastAllRolesCleared()
    {
        GoodSamaritanClientRoleState.ClearAll(true);
        BroadcastRoleCommand(GoodSamaritanClientRoleState.GetClearAllCommand(), 0u);
    }

    [HideFromIl2Cpp]
    private void BroadcastWitnessNpcAssignment(uint netId)
    {
        if (netId == 0u)
        {
            return;
        }

        GoodSamaritanClientRoleState.TryHandleRoleSync(GoodSamaritanClientRoleState.GetWitnessNpcSyncCommand(), unchecked((int)netId));
        BroadcastRoleCommand(GoodSamaritanClientRoleState.GetWitnessNpcSyncCommand(), netId);
    }

    [HideFromIl2Cpp]
    private void BroadcastWitnessNpcsCleared()
    {
        GoodSamaritanClientRoleState.TryHandleRoleSync(GoodSamaritanClientRoleState.GetClearWitnessNpcsCommand(), 0);
        BroadcastRoleCommand(GoodSamaritanClientRoleState.GetClearWitnessNpcsCommand(), 0u);
    }

    [HideFromIl2Cpp]
    private static uint GetWitnessNetworkId(GoodSamaritanWitness witness)
    {
        if (IsUnityNull(witness) || IsUnityNull(witness!.SourceOrSelf))
        {
            return 0u;
        }

        var identity = witness.SourceOrSelf.GetComponent<NetworkIdentity>() ??
                       witness.SourceOrSelf.GetComponentInParent<NetworkIdentity>();
        return IsUnityNull(identity) ? 0u : identity!.netId;
    }

    [HideFromIl2Cpp]
    private void BroadcastRoleCommand(VoskCommandType command, uint encodedNetId)
    {
        var players = Object.FindObjectsOfType<PlayerModeManager>();
        if (players == null)
        {
            return;
        }

        for (int i = 0; i < players.Length; i++)
        {
            var recipient = players[i];
            if (!IsUnityNull(recipient) && IsRoleSyncCapable(recipient!.netId))
            {
                SendRoleCommand(recipient, command, encodedNetId);
            }
        }
    }

    [HideFromIl2Cpp]
    private static void SendRoleCommand(PlayerModeManager recipient, VoskCommandType command, uint encodedNetId)
    {
        try
        {
            var connection = recipient.connectionToClient;
            var voice = ((Component)recipient).GetComponent<PlayerVoiceControlManager>() ??
                        ((Component)recipient).GetComponentInParent<PlayerVoiceControlManager>();
            if (IsUnityNull(voice) || connection == null)
            {
                return;
            }

            voice!.TargetNpcVoiceCommandSucceeded(connection, command, unchecked((int)encodedNetId));
        }
        catch (Exception ex)
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"Playable role sync failed for netId {recipient.netId}: {ex.Message}");
        }
    }
}
