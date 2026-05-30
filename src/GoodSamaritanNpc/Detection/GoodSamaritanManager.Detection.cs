namespace GoodSamaritanNpc;

public sealed partial class GoodSamaritanManager
{
    [HideFromIl2Cpp]
    private WitnessReport FindReportForWitness(GoodSamaritanWitness witness)
    {
        var players = Object.FindObjectsOfType<PlayerModeManager>();
        if (players == null)
        {
            return default;
        }

        Vector3 witnessPos = GetWitnessEyePosition(witness);
        float radius = Mathf.Max(1f, GoodSamaritanPlugin.Settings.WitnessRadius.Value);
        PlayerModeManager areaCandidate = null;
        string areaReason = GoodSamaritanText.Get(Msg.SuspiciousBehavior);
        float bestAreaDistSqr = float.MaxValue;

        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            if (IsUnityNull(player))
            {
                continue;
            }

            if (!TryGetSuspicionReason(player, out string reason))
            {
                continue;
            }

            var playerTransform = ((Component)player).transform;
            float distSqr = (playerTransform.position - witnessPos).sqrMagnitude;
            if (distSqr > radius * radius)
            {
                continue;
            }

            if (GoodSamaritanPlugin.Settings.DirectTargetReportsEnabled && CanReportTarget(player) && IsDirectlyVisible(witness, playerTransform))
            {
                return WitnessReport.Direct(player, reason);
            }

            if (distSqr < bestAreaDistSqr)
            {
                areaCandidate = player;
                areaReason = reason;
                bestAreaDistSqr = distSqr;
            }
        }

        return areaCandidate == null
            ? default
            : WitnessReport.Area(((Component)areaCandidate).transform.position, areaReason);
    }

    [HideFromIl2Cpp]
    private void ScanPlayerWitnesses()
    {
        CleanupPlayableRoles(false);
        for (int i = 0; i < playerWitnesses.Count; i++)
        {
            var witness = playerWitnesses[i];
            if (!CanPlayerWitnessReport(witness))
            {
                continue;
            }

            var report = FindReportForPlayerWitness(witness);
            if (report.Target != null)
            {
                ReportDirectTarget(witness, report.Target, report.Reason);
            }
            else if (report.HasArea)
            {
                ReportArea(witness, report.AreaPosition, report.Reason);
            }
        }
    }

    [HideFromIl2Cpp]
    private WitnessReport FindReportForPlayerWitness(PlayerWitnessState witness)
    {
        var players = Object.FindObjectsOfType<PlayerModeManager>();
        if (players == null || IsUnityNull(witness.Player))
        {
            return default;
        }

        Transform witnessTransform = ((Component)witness.Player).transform;
        Vector3 witnessPos = GetPlayerEyePosition(witness.Player);
        float radius = Mathf.Max(1f, GoodSamaritanPlugin.Settings.WitnessRadius.Value);
        PlayerModeManager areaCandidate = null;
        string areaReason = GoodSamaritanText.Get(Msg.SuspiciousBehavior);
        float bestAreaDistSqr = float.MaxValue;

        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            if (IsUnityNull(player) || player == witness.Player)
            {
                continue;
            }

            Transform playerTransform = ((Component)player!).transform;
            float distSqr = (playerTransform.position - witnessPos).sqrMagnitude;
            if (distSqr > radius * radius)
            {
                continue;
            }

            if (!TryGetSuspicionReason(player, out string reason))
            {
                continue;
            }

            if (GoodSamaritanPlugin.Settings.DirectTargetReportsEnabled && IsDirectlyVisible(witnessPos, witnessTransform.forward, playerTransform) && CanReportTarget(player))
            {
                return WitnessReport.Direct(player, reason);
            }

            if (distSqr < bestAreaDistSqr)
            {
                areaCandidate = player;
                areaReason = reason;
                bestAreaDistSqr = distSqr;
            }
        }

        return areaCandidate == null
            ? default
            : WitnessReport.Area(((Component)areaCandidate).transform.position, areaReason);
    }

    [HideFromIl2Cpp]
    private bool TryGetSuspicionReason(PlayerModeManager player, out string reason)
    {
        reason = GoodSamaritanText.Get(Msg.SuspiciousBehavior);
        if (!ShouldReportActorForEvent(player, SuspicionEventType.PassiveScan))
        {
            return false;
        }

        var revealingActions = ((Component)player).GetComponent<PlayerRevealingActions>();
        if (GoodSamaritanPlugin.Settings.ShouldDetectReveals && !IsUnityNull(revealingActions))
        {
            try
            {
                if (revealingActions!.IsRevealedToCupcake || revealingActions.IsRevealedToCctv ||
                    revealingActions.NetworkisRevealedToCupcake || revealingActions.NetworkisRevealedToCctv)
                {
                    reason = GoodSamaritanText.Get(Msg.RevealingAction);
                    return true;
                }
            }
            catch (Exception ex)
            {
                GoodSamaritanPlugin.LogSource.LogDebug($"Reveal check failed: {ex.Message}");
            }
        }

        var interactor = ((Component)player).GetComponent<PlayerInteractor>();
        if (GoodSamaritanPlugin.Settings.ShouldDetectCarriedContraband && !IsUnityNull(interactor))
        {
            if (IsSuspiciousItem(interactor!.CurrentHeldItem) || IsSuspiciousItem(interactor.CurrentHipItem))
            {
                reason = GoodSamaritanText.Get(Msg.CarryingSuspiciousItem);
                return true;
            }
        }

        var buttStorage = ((Component)player).GetComponent<ButtStorage>();
        if (GoodSamaritanPlugin.Settings.ShouldDetectHiddenContraband && !IsUnityNull(buttStorage))
        {
            try
            {
                if (buttStorage!.GetStoredStars() > 0)
                {
                    reason = GoodSamaritanText.Get(Msg.HiddenContraband);
                    return true;
                }
            }
            catch (Exception ex)
            {
                GoodSamaritanPlugin.LogSource.LogDebug($"ButtStorage check failed: {ex.Message}");
            }
        }

        if (GoodSamaritanPlugin.Settings.ShouldDetectLineCutting && IsLikelyCuttingLine(player))
        {
            reason = GoodSamaritanText.Get(Msg.CuttingLine);
            return true;
        }

        return false;
    }

    [HideFromIl2Cpp]
    private bool IsLikelyCuttingLine(PlayerModeManager player)
    {
        if (IsUnityNull(player))
        {
            return false;
        }

        var lines = Object.FindObjectsOfType<NpcLine>();
        if (lines == null)
        {
            return false;
        }

        Vector3 pos = ((Component)player).transform.position;
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (IsUnityNull(line) || line!.points == null || line.points.Count < 2 || line.occupants == null || line.occupants.Count < 2)
            {
                continue;
            }

            if (LineContainsPlayer(line, player))
            {
                continue;
            }

            float along = ClosestDistanceAlongLine(line, pos, out float lineDistSqr, out float totalLength);
            if (totalLength <= 0.1f || lineDistSqr > 3.5f * 3.5f)
            {
                continue;
            }

            float frontThreshold = totalLength * 0.45f;
            if (along > frontThreshold)
            {
                continue;
            }

            int occupantsBehind = 0;
            for (int j = 0; j < line.occupants.Count; j++)
            {
                var occupant = line.occupants[j];
                if (occupant == null || !occupant.IsValid)
                {
                    continue;
                }

                float occupantAlong = line.GetAccumulatedDistanceAlongLine(occupant);
                if (occupantAlong > along + 1.25f)
                {
                    occupantsBehind++;
                }
            }

            if (occupantsBehind >= 2)
            {
                return true;
            }
        }

        return false;
    }

    private static bool LineContainsPlayer(NpcLine line, PlayerModeManager player)
    {
        var meta = ((Component)player).GetComponent<Metater.MetaPlayer>();
        if (IsUnityNull(meta))
        {
            return false;
        }

        for (int i = 0; i < line.occupants.Count; i++)
        {
            var occupant = line.occupants[i];
            if (occupant != null && occupant.IsPlayer && occupant.player == meta)
            {
                return true;
            }
        }

        return false;
    }

    private static float ClosestDistanceAlongLine(NpcLine line, Vector3 position, out float distSqr, out float totalLength)
    {
        distSqr = float.MaxValue;
        totalLength = 0f;
        float bestAlong = 0f;
        float accumulated = 0f;

        for (int i = 0; i < line.points.Count - 1; i++)
        {
            var aTransform = line.points[i];
            var bTransform = line.points[i + 1];
            if (IsUnityNull(aTransform) || IsUnityNull(bTransform))
            {
                continue;
            }

            Vector3 a = aTransform.position;
            Vector3 b = bTransform.position;
            Vector3 ab = b - a;
            float segmentLength = ab.magnitude;
            if (segmentLength <= 0.01f)
            {
                continue;
            }

            float t = Mathf.Clamp01(Vector3.Dot(position - a, ab) / (segmentLength * segmentLength));
            Vector3 closest = a + ab * t;
            float currentDistSqr = (position - closest).sqrMagnitude;
            if (currentDistSqr < distSqr)
            {
                distSqr = currentDistSqr;
                bestAlong = accumulated + segmentLength * t;
            }

            accumulated += segmentLength;
        }

        totalLength = accumulated;
        return bestAlong;
    }

    internal static bool IsSuspiciousItem(HeldItemInteractable item)
    {
        if (IsUnityNull(item))
        {
            return false;
        }

        try
        {
            if (item!.IsActuallyContraband || item.itemContrabandStars > 0)
            {
                return true;
            }

            var bag = ((Component)item).GetComponent<BagInteractable>();
            if (!IsUnityNull(bag) && bag!.GetBagHeldContrabandStars() > 0)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"Item suspicion check failed: {ex.Message}");
        }

        return false;
    }
}
