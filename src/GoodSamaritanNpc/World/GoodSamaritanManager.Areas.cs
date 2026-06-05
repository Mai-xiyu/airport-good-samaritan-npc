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
    private HijackingNpcs GetHijackingNpcs()
    {
        var manager = HijackingNpcs.Instance;
        if (IsUnityNull(manager))
        {
            manager = Object.FindObjectOfType<HijackingNpcs>();
        }

        return manager;
    }

    [HideFromIl2Cpp]
    private int GetNpcPopulationSourceId()
    {
        var hijackingNpcs = GetHijackingNpcs();
        if (IsActiveComponent(hijackingNpcs))
        {
            return ((Object)(object)hijackingNpcs!).GetInstanceID();
        }

        var manager = GetNpcManager();
        if (!IsUnityNull(manager))
        {
            return ((Object)(object)manager!).GetInstanceID();
        }

        return 0;
    }

    private static bool IsActiveComponent(Component component)
    {
        return !IsUnityNull(component) && !IsUnityNull(component!.gameObject) && component.gameObject.activeInHierarchy;
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
        if (name.Contains("scan line") || name.Contains("xray") || name.Contains("scanner") || name.Contains("checkpoint") || name.Contains("security"))
        {
            return GoodSamaritanText.Get(Msg.AreaSecurityLine);
        }

        if (name.Contains("terminal") || name.Contains("concourse"))
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

        if (name.Contains("gate") || name.Contains("boarding"))
        {
            return GoodSamaritanText.Get(Msg.AreaGate);
        }

        if (name.Contains("lobby"))
        {
            return GoodSamaritanText.Get(Msg.AreaLobby);
        }

        if (name.Contains("cockpit") || name.Contains("flight deck"))
        {
            return GoodSamaritanText.Get(Msg.AreaCockpit);
        }

        if (name.Contains("cabin") || name.Contains("aisle") || name.Contains("seat") || name.Contains("passenger"))
        {
            return GoodSamaritanText.Get(Msg.AreaCabin);
        }

        if (name.Contains("galley") || name.Contains("kitchen"))
        {
            return GoodSamaritanText.Get(Msg.AreaGalley);
        }

        if (name.Contains("cargo") || name.Contains("baggage hold"))
        {
            return GoodSamaritanText.Get(Msg.AreaCargo);
        }

        if (name.Contains("engine") || name.Contains("landing gear") || name.Contains("wing"))
        {
            return GoodSamaritanText.Get(Msg.AreaPlaneExterior);
        }

        if (name.Contains("plane") || name.Contains("passenger aircraft") || name.Contains("hijack"))
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

        if (name.Contains("runway") || name.Contains("tarmac"))
        {
            return GoodSamaritanText.Get(Msg.AreaRunway);
        }

        if (name.Contains("vending"))
        {
            return GoodSamaritanText.Get(Msg.AreaVending);
        }

        if (name.Contains("tower"))
        {
            return GoodSamaritanText.Get(Msg.AreaTower);
        }

        if (name.Contains("deathmatch"))
        {
            return GoodSamaritanText.Get(Msg.AreaDeathmatch);
        }

        if (name.Contains("sandbox"))
        {
            return GoodSamaritanText.Get(Msg.AreaSandbox);
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
