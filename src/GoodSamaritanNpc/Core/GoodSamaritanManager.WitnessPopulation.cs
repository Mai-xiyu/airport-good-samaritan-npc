namespace GoodSamaritanNpc;

public sealed partial class GoodSamaritanManager
{
    [HideFromIl2Cpp]
    private void EnsureWitnessPopulation()
    {
        SpawnConfiguredExtraWitnesses();
        MarkExistingNpcs();
        CleanupWitnessList();
    }

    [HideFromIl2Cpp]
    private void SpawnConfiguredExtraWitnesses()
    {
        int desired = Mathf.Max(0, GoodSamaritanPlugin.Settings.ExtraSpawnCount.Value);
        if (desired <= extraSpawnedThisManager)
        {
            return;
        }

        var manager = GetNpcManager();
        if (IsUnityNull(manager))
        {
            return;
        }

        while (extraSpawnedThisManager < desired)
        {
            var spawn = manager!.GetRandomSpawnLocation();
            if (IsUnityNull(spawn))
            {
                break;
            }

            manager.ServerSpawnNpc(spawn, true);
            extraSpawnedThisManager++;
            pendingForcedWitnessMarks++;
        }
    }

    [HideFromIl2Cpp]
    private void MarkExistingNpcs()
    {
        var npcs = Object.FindObjectsOfType<NpcAiController>();
        if (npcs == null)
        {
            return;
        }

        float chance = Mathf.Clamp01(GoodSamaritanPlugin.Settings.ExistingNpcChance.Value);
        for (int i = 0; i < npcs.Length; i++)
        {
            var npc = npcs[i];
            if (IsUnityNull(npc))
            {
                continue;
            }

            int id = ((Object)(object)npc).GetInstanceID();
            if (HasWitness(npc))
            {
                evaluatedNpcIds.Add(id);
                continue;
            }

            if (pendingForcedWitnessMarks > 0)
            {
                evaluatedNpcIds.Add(id);
                pendingForcedWitnessMarks--;
                AddWitness(npc);
                continue;
            }

            if (!GoodSamaritanPlugin.Settings.ConvertExistingNpcs.Value || evaluatedNpcIds.Contains(id))
            {
                continue;
            }

            evaluatedNpcIds.Add(id);
            if (UnityEngine.Random.value <= chance)
            {
                AddWitness(npc);
            }
        }
    }

    [HideFromIl2Cpp]
    private bool HasWitness(NpcAiController npc)
    {
        return !IsUnityNull(((Component)npc).GetComponent<GoodSamaritanWitness>());
    }

    [HideFromIl2Cpp]
    private void AddWitness(NpcAiController npc)
    {
        var go = ((Component)npc).gameObject;
        if (IsUnityNull(go))
        {
            return;
        }

        var witness = go.GetComponent<GoodSamaritanWitness>();
        if (IsUnityNull(witness))
        {
            witness = go.AddComponent<GoodSamaritanWitness>();
        }

        witness!.Npc = npc;
        witness.NextReportTime = Time.timeAsDouble + UnityEngine.Random.Range(1f, 4f);
        witnesses.Add(witness);
    }

    [HideFromIl2Cpp]
    private void CleanupWitnessList()
    {
        for (int i = witnesses.Count - 1; i >= 0; i--)
        {
            var witness = witnesses[i];
            if (IsUnityNull(witness) || IsUnityNull(witness.Npc))
            {
                witnesses.RemoveAt(i);
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
