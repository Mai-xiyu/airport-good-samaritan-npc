namespace GoodSamaritanNpc;

public sealed partial class GoodSamaritanManager
{
    [HideFromIl2Cpp]
    private void EnsureWitnessPopulation()
    {
        SpawnConfiguredExtraWitnesses();
        MarkExistingNpcs();
        CleanupWitnessList();
        SyncWitnessNpcIdentities();
    }

    [HideFromIl2Cpp]
    private void SpawnConfiguredExtraWitnesses()
    {
        int desired = Mathf.Max(0, GoodSamaritanPlugin.Settings.ExtraSpawnCount.Value);
        if (desired <= extraSpawnedThisManager)
        {
            return;
        }

        var hijackingNpcs = GetHijackingNpcs();
        if (IsActiveComponent(hijackingNpcs))
        {
            SpawnHijackingWitnessNpcs(hijackingNpcs!, desired);
            return;
        }

        var manager = GetNpcManager();
        if (!IsUnityNull(manager))
        {
            SpawnAirportWitnessNpcs(manager!, desired);
        }
    }

    [HideFromIl2Cpp]
    private void SpawnAirportWitnessNpcs(NpcManager manager, int desired)
    {
        while (extraSpawnedThisManager < desired)
        {
            var spawn = manager.GetRandomSpawnLocation();
            if (IsUnityNull(spawn))
            {
                break;
            }

            int previousCount = manager.serverNpcs?.Count ?? 0;
            manager.ServerSpawnNpc(spawn, true);
            extraSpawnedThisManager++;
            if (!TryMarkNewAirportNpcs(manager, previousCount))
            {
                pendingForcedWitnessMarks++;
            }
        }
    }

    [HideFromIl2Cpp]
    private void SpawnHijackingWitnessNpcs(HijackingNpcs hijackingNpcs, int desired)
    {
        while (extraSpawnedThisManager < desired)
        {
            Vector3 spawn = hijackingNpcs.GetRandomPoint();
            if (!float.IsFinite(spawn.x) || !float.IsFinite(spawn.y) || !float.IsFinite(spawn.z))
            {
                break;
            }

            int previousCount = hijackingNpcs.serverNpcs?.Count ?? 0;
            hijackingNpcs.ServerSpawnNpc(spawn);
            extraSpawnedThisManager++;
            if (!TryMarkNewHijackingNpcs(hijackingNpcs, previousCount))
            {
                pendingForcedWitnessMarks++;
            }
        }
    }

    [HideFromIl2Cpp]
    private bool TryMarkNewAirportNpcs(NpcManager manager, int previousCount)
    {
        var serverNpcs = manager.serverNpcs;
        if (serverNpcs == null || serverNpcs.Count <= previousCount)
        {
            return false;
        }

        bool marked = false;
        for (int i = Mathf.Max(0, previousCount); i < serverNpcs.Count; i++)
        {
            marked |= TryAddSpawnedWitness(serverNpcs[i]);
        }

        return marked;
    }

    [HideFromIl2Cpp]
    private bool TryMarkNewHijackingNpcs(HijackingNpcs manager, int previousCount)
    {
        var serverNpcs = manager.serverNpcs;
        if (serverNpcs == null || serverNpcs.Count <= previousCount)
        {
            return false;
        }

        bool marked = false;
        for (int i = Mathf.Max(0, previousCount); i < serverNpcs.Count; i++)
        {
            marked |= TryAddSpawnedWitness(serverNpcs[i]);
        }

        return marked;
    }

    [HideFromIl2Cpp]
    private bool TryAddSpawnedWitness(GameObject gameObject)
    {
        if (IsUnityNull(gameObject))
        {
            return false;
        }

        Component source = gameObject!.GetComponent<NpcAiController>();
        source ??= gameObject.GetComponent<PlaneWanderNpcAi>();
        source ??= gameObject.GetComponent<NpcRagdollManager>();
        if (IsUnityNull(source))
        {
            return false;
        }

        evaluatedNpcIds.Add(gameObject.GetInstanceID());
        if (!HasWitness(source))
        {
            AddWitness(source);
        }

        return true;
    }

    [HideFromIl2Cpp]
    private void MarkExistingNpcs()
    {
        var npcs = Object.FindObjectsOfType<NpcAiController>();
        if (npcs != null)
        {
            for (int i = 0; i < npcs.Length; i++)
            {
                TryEvaluateWitnessSource(npcs[i]);
            }
        }

        MarkPlaneNpcs();
    }

    [HideFromIl2Cpp]
    private void MarkPlaneNpcs()
    {
        var planeNpcs = Object.FindObjectsOfType<PlaneWanderNpcAi>();
        if (planeNpcs == null)
        {
            return;
        }

        for (int i = 0; i < planeNpcs.Length; i++)
        {
            TryEvaluateWitnessSource(planeNpcs[i]);
        }
    }

    [HideFromIl2Cpp]
    private void TryEvaluateWitnessSource(Component source)
    {
        if (IsUnityNull(source))
        {
            return;
        }

        int id = source!.gameObject.GetInstanceID();
        if (HasWitness(source))
        {
            evaluatedNpcIds.Add(id);
            return;
        }

        if (pendingForcedWitnessMarks > 0)
        {
            evaluatedNpcIds.Add(id);
            pendingForcedWitnessMarks--;
            AddWitness(source);
            return;
        }

        if (!GoodSamaritanPlugin.Settings.ConvertExistingNpcs.Value || evaluatedNpcIds.Contains(id))
        {
            return;
        }

        evaluatedNpcIds.Add(id);
        if (UnityEngine.Random.value <= Mathf.Clamp01(GoodSamaritanPlugin.Settings.ExistingNpcChance.Value))
        {
            AddWitness(source);
        }
    }

    [HideFromIl2Cpp]
    private bool HasWitness(Component source)
    {
        return !IsUnityNull(source) && !IsUnityNull(source!.GetComponent<GoodSamaritanWitness>());
    }

    [HideFromIl2Cpp]
    private void AddWitness(Component source)
    {
        var go = source.gameObject;
        if (IsUnityNull(go))
        {
            return;
        }

        var witness = go.GetComponent<GoodSamaritanWitness>();
        if (IsUnityNull(witness))
        {
            witness = go.AddComponent<GoodSamaritanWitness>();
        }

        witness!.Source = source;
        witness.Npc = source.GetComponent<NpcAiController>() ?? source.GetComponentInParent<NpcAiController>();
        witness.NextReportTime = Time.timeAsDouble + UnityEngine.Random.Range(1f, 4f);
        witnesses.Add(witness);
    }

    [HideFromIl2Cpp]
    private void CleanupWitnessList()
    {
        for (int i = witnesses.Count - 1; i >= 0; i--)
        {
            var witness = witnesses[i];
            if (IsUnityNull(witness) || IsUnityNull(witness!.SourceOrSelf))
            {
                witnesses.RemoveAt(i);
            }
        }
    }

    [HideFromIl2Cpp]
    private void SyncWitnessNpcIdentities()
    {
        for (int i = 0; i < witnesses.Count; i++)
        {
            uint netId = GetWitnessNetworkId(witnesses[i]);
            if (netId != 0u && syncedWitnessNpcNetIds.Add(netId))
            {
                BroadcastWitnessNpcAssignment(netId);
            }
        }
    }

    [HideFromIl2Cpp]
    private void ScanWitnesses()
    {
        CleanupWitnessList();
        foreach (var witness in witnesses)
        {
            if (!CanWitnessReport(witness))
            {
                continue;
            }

            var report = FindReportForWitness(witness);
            if (report.Target != null)
            {
                ReportDirectTarget(witness, report.Target, report.Reason);
            }
            else if (report.HasArea)
            {
                ReportArea(witness, report.AreaPosition, report.Reason);
            }
        }

        ScanPlayerWitnesses();
    }
}
