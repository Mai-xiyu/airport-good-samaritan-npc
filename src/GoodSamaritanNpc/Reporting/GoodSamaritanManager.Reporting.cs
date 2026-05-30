namespace GoodSamaritanNpc;

public sealed partial class GoodSamaritanManager
{
    [HideFromIl2Cpp]
    private bool CanWitnessReport(GoodSamaritanWitness witness)
    {
        if (IsUnityNull(witness) || IsUnityNull(witness.Npc))
        {
            return false;
        }

        var ragdoll = ((Component)witness).GetComponent<NpcRagdollManager>();
        if (!IsUnityNull(ragdoll) && ragdoll!.IsRagdolled)
        {
            return false;
        }

        double now = Time.timeAsDouble;
        return now >= witness.NextReportTime && now >= nextGlobalReportTime;
    }

    [HideFromIl2Cpp]
    private bool CanPlayerWitnessReport(PlayerWitnessState witness)
    {
        if (witness == null || IsUnityNull(witness.Player))
        {
            return false;
        }

        uint netId = witness.Player.netId;
        if (netId == 0u || !playableWitnessNetIds.Contains(netId) || !moddedPlayerNetIds.Contains(netId))
        {
            return false;
        }

        if (!witness.Player.NetworkisAgent)
        {
            witness.Player.ServerSetIsAgent(true);
        }

        var ragdoll = ((Component)witness.Player).GetComponent<PlayerRagdollManager>();
        if (!IsUnityNull(ragdoll) && ragdoll!.IsRagdollActive)
        {
            return false;
        }

        double now = Time.timeAsDouble;
        return now >= witness.NextReportTime && now >= nextGlobalReportTime;
    }

    [HideFromIl2Cpp]
    private bool CanReportTarget(PlayerModeManager target)
    {
        int id = ((Object)(object)target).GetInstanceID();
        double now = Time.timeAsDouble;
        return !targetCooldownUntil.TryGetValue(id, out double until) || now >= until;
    }

    [HideFromIl2Cpp]
    private void ReportDirectTarget(GoodSamaritanWitness witness, PlayerModeManager target, string reason)
    {
        if (!CanWitnessReport(witness) || !CanReportTarget(target))
        {
            return;
        }

        double now = Time.timeAsDouble;
        float highlightSeconds = Mathf.Max(0.5f, GoodSamaritanPlugin.Settings.HighlightSeconds.Value);

        var arrest = ((Component)target).GetComponent<ArrestInteractable>();
        if (!IsUnityNull(arrest))
        {
            arrest!.RpcShowSpottedIcon(highlightSeconds);
        }

        AppendLog(GoodSamaritanText.Get(Msg.DirectReport));
        ShowWitnessIndicator(witness, highlightSeconds);
        ShowTargetIndicator(target, highlightSeconds);
        GoodSamaritanClientHighlighter.ShowPlayer(target, GoodSamaritanHighlightKind.Suspicious, highlightSeconds);

        witness.NextReportTime = now + Mathf.Max(1f, GoodSamaritanPlugin.Settings.ReportCooldownSeconds.Value);
        nextGlobalReportTime = now + Mathf.Max(0.25f, GoodSamaritanPlugin.Settings.ReportCooldownSeconds.Value);
        targetCooldownUntil[((Object)(object)target).GetInstanceID()] = now + Mathf.Max(1f, GoodSamaritanPlugin.Settings.TargetCooldownSeconds.Value);
        GoodSamaritanPlugin.LogSource.LogDebug($"Direct witness report fired for {reason}.");
    }

    [HideFromIl2Cpp]
    private void ReportDirectTarget(PlayerWitnessState witness, PlayerModeManager target, string reason)
    {
        if (!CanPlayerWitnessReport(witness) || !CanReportTarget(target))
        {
            return;
        }

        double now = Time.timeAsDouble;
        float highlightSeconds = Mathf.Max(0.5f, GoodSamaritanPlugin.Settings.HighlightSeconds.Value);

        var arrest = ((Component)target).GetComponent<ArrestInteractable>();
        if (!IsUnityNull(arrest))
        {
            arrest!.RpcShowSpottedIcon(highlightSeconds);
        }

        AppendLog(GoodSamaritanText.Get(Msg.DirectReport));
        ShowPlayerWitnessIndicator(witness.Player, highlightSeconds);
        ShowTargetIndicator(target, highlightSeconds);
        GoodSamaritanClientHighlighter.ShowPlayer(target, GoodSamaritanHighlightKind.Suspicious, highlightSeconds);

        witness.NextReportTime = now + Mathf.Max(1f, GoodSamaritanPlugin.Settings.ReportCooldownSeconds.Value);
        nextGlobalReportTime = now + Mathf.Max(0.25f, GoodSamaritanPlugin.Settings.ReportCooldownSeconds.Value);
        targetCooldownUntil[((Object)(object)target).GetInstanceID()] = now + Mathf.Max(1f, GoodSamaritanPlugin.Settings.TargetCooldownSeconds.Value);
        GoodSamaritanPlugin.LogSource.LogDebug($"Playable witness report fired for {reason}.");
    }

    [HideFromIl2Cpp]
    private void ReportArea(GoodSamaritanWitness witness, Vector3 position, string reason)
    {
        if (!CanWitnessReport(witness))
        {
            return;
        }

        double now = Time.timeAsDouble;
        string area = ResolveAreaName(position);
        AppendLog(GoodSamaritanText.Format(Msg.AreaReport, area));
        ShowWitnessIndicator(witness, Mathf.Max(1f, GoodSamaritanPlugin.Settings.HighlightSeconds.Value));
        GoodSamaritanClientHighlighter.ShowArea(position, Mathf.Max(1f, GoodSamaritanPlugin.Settings.AreaHighlightSeconds.Value));

        witness.NextReportTime = now + Mathf.Max(1f, GoodSamaritanPlugin.Settings.ReportCooldownSeconds.Value);
        nextGlobalReportTime = now + Mathf.Max(0.25f, GoodSamaritanPlugin.Settings.ReportCooldownSeconds.Value);
        GoodSamaritanPlugin.LogSource.LogDebug($"Area witness report fired for {reason} at {area}.");
    }

    [HideFromIl2Cpp]
    private void ReportArea(PlayerWitnessState witness, Vector3 position, string reason)
    {
        if (!CanPlayerWitnessReport(witness))
        {
            return;
        }

        double now = Time.timeAsDouble;
        string area = ResolveAreaName(position);
        AppendLog(GoodSamaritanText.Format(Msg.AreaReport, area));
        ShowPlayerWitnessIndicator(witness.Player, Mathf.Max(1f, GoodSamaritanPlugin.Settings.HighlightSeconds.Value));
        GoodSamaritanClientHighlighter.ShowArea(position, Mathf.Max(1f, GoodSamaritanPlugin.Settings.AreaHighlightSeconds.Value));

        witness.NextReportTime = now + Mathf.Max(1f, GoodSamaritanPlugin.Settings.ReportCooldownSeconds.Value);
        nextGlobalReportTime = now + Mathf.Max(0.25f, GoodSamaritanPlugin.Settings.ReportCooldownSeconds.Value);
        GoodSamaritanPlugin.LogSource.LogDebug($"Playable witness area report fired for {reason} at {area}.");
    }

    [HideFromIl2Cpp]
    private void ShowWitnessIndicator(GoodSamaritanWitness witness, float seconds)
    {
        if (!IsUnityNull(witness.Npc))
        {
            var pvcm = FindRpcCarrier();
            if (!IsUnityNull(pvcm))
            {
                pvcm!.RpcNpcShowIndicatorQuestion(witness.Npc);
            }

            if (GoodSamaritanClientHighlighter.CanShowAllyFeedbackToLocal())
            {
                GoodSamaritanMarker.ShowOn(witness.Npc, seconds, true);
                GoodSamaritanClientHighlighter.ShowNpc(witness.Npc, GoodSamaritanHighlightKind.Ally, seconds);
            }
        }
    }

    [HideFromIl2Cpp]
    private void ShowPlayerWitnessIndicator(PlayerModeManager player, float seconds)
    {
        if (IsUnityNull(player))
        {
            return;
        }

        var targetPvcm = ((Component)player).GetComponent<PlayerVoiceControlManager>();
        if (!IsUnityNull(targetPvcm))
        {
            var carrier = FindRpcCarrier();
            if (!IsUnityNull(carrier))
            {
                carrier!.RpcPlayerShowIndicatorQuestion(targetPvcm);
            }
        }

        if (GoodSamaritanClientHighlighter.CanShowAllyFeedbackToLocal())
        {
            GoodSamaritanMarker.ShowOn((Component)player, seconds, true);
            GoodSamaritanClientHighlighter.ShowPlayer(player, GoodSamaritanHighlightKind.Ally, seconds);
        }
    }

    [HideFromIl2Cpp]
    private void ShowTargetIndicator(PlayerModeManager target, float seconds)
    {
        if (IsUnityNull(target))
        {
            return;
        }

        var targetPvcm = ((Component)target).GetComponent<PlayerVoiceControlManager>();
        if (!IsUnityNull(targetPvcm))
        {
            var carrier = FindRpcCarrier();
            if (!IsUnityNull(carrier))
            {
                carrier!.RpcPlayerShowIndicatorQuestion(targetPvcm);
            }
        }

        GoodSamaritanMarker.ShowOn((Component)target, seconds, false);
        GoodSamaritanClientHighlighter.ShowPlayer(target, GoodSamaritanHighlightKind.Suspicious, seconds);
    }

    private static void AppendLog(string message)
    {
        var logManager = LogManager.Instance;
        if (IsUnityNull(logManager))
        {
            logManager = Object.FindObjectOfType<LogManager>();
        }

        if (!IsUnityNull(logManager))
        {
            logManager!.RpcAppendSimple(message);
        }
    }

    [HideFromIl2Cpp]
    private PlayerVoiceControlManager FindRpcCarrier()
    {
        var managers = Object.FindObjectsOfType<PlayerVoiceControlManager>();
        if (managers == null)
        {
            return null;
        }

        for (int i = 0; i < managers.Length; i++)
        {
            var manager = managers[i];
            if (!IsUnityNull(manager) && manager.isServer)
            {
                return manager;
            }
        }

        return managers.Length > 0 ? managers[0] : null;
    }

    [HideFromIl2Cpp]
    private GoodSamaritanWitness FindNearestReadyWitness(Vector3 position)
    {
        CleanupWitnessList();
        GoodSamaritanWitness best = null;
        float bestDistSqr = float.MaxValue;
        float maxDist = Mathf.Max(1f, GoodSamaritanPlugin.Settings.WitnessRadius.Value) * 1.25f;

        foreach (var witness in witnesses)
        {
            if (!CanWitnessReport(witness))
            {
                continue;
            }

            float distSqr = (((Component)witness).transform.position - position).sqrMagnitude;
            if (distSqr <= maxDist * maxDist && distSqr < bestDistSqr)
            {
                best = witness;
                bestDistSqr = distSqr;
            }
        }

        return best;
    }

    [HideFromIl2Cpp]
    private bool IsDirectlyVisible(GoodSamaritanWitness witness, Transform target)
    {
        if (IsUnityNull(witness) || IsUnityNull(target))
        {
            return false;
        }

        Transform witnessTransform = ((Component)witness).transform;
        return IsDirectlyVisible(GetWitnessEyePosition(witness), witnessTransform.forward, target);
    }

    [HideFromIl2Cpp]
    private bool IsDirectlyVisible(Vector3 origin, Vector3 forward, Transform target)
    {
        if (IsUnityNull(target))
        {
            return false;
        }

        Vector3 targetPos = target.position + Vector3.up * 1.2f;
        Vector3 toTarget = targetPos - origin;
        float dist = toTarget.magnitude;
        if (dist <= 0.1f || dist > Mathf.Max(1f, GoodSamaritanPlugin.Settings.WitnessRadius.Value))
        {
            return false;
        }

        float halfFov = Mathf.Clamp(GoodSamaritanPlugin.Settings.WitnessFovDegrees.Value, 1f, 360f) * 0.5f;
        float angle = Vector3.Angle(forward.sqrMagnitude <= 0.001f ? Vector3.forward : forward, toTarget);
        if (angle > halfFov)
        {
            return false;
        }

        if (Physics.Raycast(origin, toTarget.normalized, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
        {
            Transform hitTransform = hit.transform;
            if (!IsUnityNull(hitTransform) && (hitTransform == target || hitTransform.IsChildOf(target) || target.IsChildOf(hitTransform)))
            {
                return true;
            }

            return false;
        }

        return true;
    }
}
