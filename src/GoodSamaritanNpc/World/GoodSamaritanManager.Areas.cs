namespace GoodSamaritanNpc;

public sealed partial class GoodSamaritanManager
{
    private static Vector3 GetWitnessEyePosition(GoodSamaritanWitness witness)
    {
        return ((Component)witness).transform.position + Vector3.up * 1.55f;
    }

    private static Vector3 GetPlayerEyePosition(PlayerModeManager player)
    {
        return ((Component)player).transform.position + Vector3.up * 1.55f;
    }

    [HideFromIl2Cpp]
    private NpcManager GetNpcManager()
    {
        var manager = NpcManager.ServerInstance;
        if (IsUnityNull(manager))
        {
            manager = Object.FindObjectOfType<NpcManager>();
        }

        return manager;
    }

    [HideFromIl2Cpp]
    private string ResolveAreaName(Vector3 position)
    {
        areaRefreshTimer -= Time.deltaTime;
        if (namedAreas.Count == 0 || areaRefreshTimer <= 0f)
        {
            RefreshNamedAreas();
        }

        string best = GoodSamaritanText.Get(Msg.AreaCurrentPosition);
        float bestDistSqr = float.MaxValue;
        for (int i = 0; i < namedAreas.Count; i++)
        {
            var area = namedAreas[i];
            if (IsUnityNull(area.Transform))
            {
                continue;
            }

            float distSqr = (area.Transform!.position - position).sqrMagnitude;
            if (distSqr < bestDistSqr)
            {
                bestDistSqr = distSqr;
                best = area.DisplayName;
            }
        }

        return best;
    }

    [HideFromIl2Cpp]
    private void RefreshNamedAreas()
    {
        namedAreas.Clear();
        areaRefreshTimer = 10f;

        var transforms = Object.FindObjectsOfType<Transform>(true);
        if (transforms == null)
        {
            return;
        }

        for (int i = 0; i < transforms.Length; i++)
        {
            var transform = transforms[i];
            if (IsUnityNull(transform))
            {
                continue;
            }

            string display = TryMapAreaName(((Object)(object)transform).name);
            if (display.Length > 0)
            {
                namedAreas.Add(new NamedArea(transform, display));
            }
        }
    }

    private static string TryMapAreaName(string objectName)
    {
        string name = objectName.ToLowerInvariant();
        if (name.Contains("scan line") || name.Contains("xray") || name.Contains("scanner"))
        {
            return GoodSamaritanText.Get(Msg.AreaSecurityLine);
        }

        if (name.Contains("terminal"))
        {
            return GoodSamaritanText.Get(Msg.AreaTerminal);
        }

        if (name.Contains("jail") || name.Contains("prison"))
        {
            return GoodSamaritanText.Get(Msg.AreaJail);
        }

        if (name.Contains("smuggler room"))
        {
            return GoodSamaritanText.Get(Msg.AreaSmugglerRoom);
        }

        if (name.Contains("copzone") || name.Contains("tsa"))
        {
            return GoodSamaritanText.Get(Msg.AreaTsa);
        }

        if (name.Contains("lobby"))
        {
            return GoodSamaritanText.Get(Msg.AreaLobby);
        }

        if (name.Contains("plane") || name.Contains("passenger aircraft"))
        {
            return GoodSamaritanText.Get(Msg.AreaPlane);
        }

        if (name.Contains("belt") || name.Contains("conveyor"))
        {
            return GoodSamaritanText.Get(Msg.AreaConveyor);
        }

        if (name.Contains("break room"))
        {
            return GoodSamaritanText.Get(Msg.AreaBreakRoom);
        }

        return string.Empty;
    }

    internal static bool IsUnityNull(Object obj)
    {
        return obj == null;
    }

    private readonly struct NamedArea
    {
        internal readonly Transform Transform;
        internal readonly string DisplayName;

        internal NamedArea(Transform transform, string displayName)
        {
            Transform = transform;
            DisplayName = displayName;
        }
    }

    private readonly struct WitnessReport
    {
        internal readonly PlayerModeManager Target;
        internal readonly Vector3 AreaPosition;
        internal readonly bool HasArea;
        internal readonly string Reason;

        private WitnessReport(PlayerModeManager target, Vector3 areaPosition, bool hasArea, string reason)
        {
            Target = target;
            AreaPosition = areaPosition;
            HasArea = hasArea;
            Reason = reason;
        }

        internal static WitnessReport Direct(PlayerModeManager target, string reason)
        {
            return new WitnessReport(target, default, false, reason);
        }

        internal static WitnessReport Area(Vector3 position, string reason)
        {
            return new WitnessReport(null, position, true, reason);
        }
    }
}
