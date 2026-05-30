namespace GoodSamaritanNpc;

public sealed class GoodSamaritanClientHighlighter : MonoBehaviour
{
    private static readonly Color AllyBlue = new(0.1f, 0.55f, 1f, 1f);
    private static readonly Color SuspiciousYellow = new(1f, 0.82f, 0.08f, 1f);
    private static readonly Color AreaYellow = new(1f, 0.78f, 0.05f, 0.28f);
    private static readonly HashSet<uint> LocalUndercoverNetIds = new();

    internal static GoodSamaritanClientHighlighter Instance;

    private readonly List<AreaHighlight> areaHighlights = new();
    private float teamScanTimer;
    private Material areaMaterial;

    public GoodSamaritanClientHighlighter(IntPtr ptr) : base(ptr)
    {
    }

    public void Awake()
    {
        Instance = this;
    }

    public void OnDestroy()
    {
        if (ReferenceEquals(Instance, this))
        {
            Instance = null;
        }
    }

    public void Update()
    {
        CleanupAreas();
        if (!GoodSamaritanPlugin.Settings.Enabled.Value || !NetworkClient.active)
        {
            return;
        }

        teamScanTimer -= Time.deltaTime;
        if (teamScanTimer > 0f)
        {
            return;
        }

        teamScanTimer = 1f;
        UpdateTeamHighlights();
    }

    internal static void NoteUndercoverAssignment(PlayerModeManager player)
    {
        if (!GoodSamaritanManager.IsUnityNull(player) && player!.netId != 0u)
        {
            LocalUndercoverNetIds.Add(player.netId);
        }
    }

    internal static void ShowPlayer(PlayerModeManager player, GoodSamaritanHighlightKind kind, float seconds)
    {
        if (GoodSamaritanManager.IsUnityNull(player))
        {
            return;
        }

        ShowComponent((Component)player!, kind, seconds);
    }

    internal static void ShowNpc(NpcAiController npc, GoodSamaritanHighlightKind kind, float seconds)
    {
        if (GoodSamaritanManager.IsUnityNull(npc))
        {
            return;
        }

        ShowComponent((Component)npc!, kind, seconds);
    }

    internal static void ShowComponent(Component component, GoodSamaritanHighlightKind kind, float seconds)
    {
        if (GoodSamaritanManager.IsUnityNull(component))
        {
            return;
        }

        if (kind == GoodSamaritanHighlightKind.Ally && !GoodSamaritanPlugin.Settings.ShowTeamHighlights.Value)
        {
            return;
        }

        if (kind == GoodSamaritanHighlightKind.Suspicious && !GoodSamaritanPlugin.Settings.ShowReportHighlightsToAllModdedClients.Value)
        {
            return;
        }

        var target = component!.gameObject.GetComponent<GoodSamaritanHighlightTarget>();
        if (GoodSamaritanManager.IsUnityNull(target))
        {
            target = component.gameObject.AddComponent<GoodSamaritanHighlightTarget>();
        }

        Color color = kind == GoodSamaritanHighlightKind.Ally ? AllyBlue : SuspiciousYellow;
        float width = kind == GoodSamaritanHighlightKind.Ally ? 4f : 5.5f;
        target!.Show(color, seconds, width);
    }

    internal static void ShowArea(Vector3 position, float seconds)
    {
        if (!GoodSamaritanPlugin.Settings.ShowReportHighlightsToAllModdedClients.Value || Instance == null)
        {
            return;
        }

        Instance.ShowAreaInstance(position, seconds);
    }

    [HideFromIl2Cpp]
    private void UpdateTeamHighlights()
    {
        if (!GoodSamaritanPlugin.Settings.ShowTeamHighlights.Value)
        {
            return;
        }

        var local = FindLocalPlayer();
        if (GoodSamaritanManager.IsUnityNull(local))
        {
            return;
        }

        bool localIsUndercover = IsLocalUndercover(local);
        bool localCanSeeTeam = local!.NetworkisAgent || localIsUndercover;
        if (!localCanSeeTeam)
        {
            return;
        }

        var players = Object.FindObjectsOfType<PlayerModeManager>();
        if (players == null)
        {
            return;
        }

        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            if (GoodSamaritanManager.IsUnityNull(player))
            {
                continue;
            }

            if (player!.NetworkisAgent)
            {
                ShowPlayer(player, GoodSamaritanHighlightKind.Ally, 1.35f);
            }
            else if (localIsUndercover)
            {
                ShowPlayer(player, GoodSamaritanHighlightKind.Suspicious, 1.35f);
            }
        }

        var witnesses = Object.FindObjectsOfType<GoodSamaritanWitness>();
        if (witnesses == null)
        {
            return;
        }

        for (int i = 0; i < witnesses.Length; i++)
        {
            var witness = witnesses[i];
            if (!GoodSamaritanManager.IsUnityNull(witness) && !GoodSamaritanManager.IsUnityNull(witness!.Npc))
            {
                ShowNpc(witness.Npc, GoodSamaritanHighlightKind.Ally, 1.35f);
            }
        }
    }

    private static bool IsLocalUndercover(PlayerModeManager local)
    {
        if (GoodSamaritanManager.IsUnityNull(local) || local!.netId == 0u)
        {
            return false;
        }

        if (LocalUndercoverNetIds.Contains(local.netId))
        {
            return true;
        }

        return GoodSamaritanManager.Instance != null && GoodSamaritanManager.Instance.IsUndercover(local);
    }

    private static PlayerModeManager FindLocalPlayer()
    {
        var players = Object.FindObjectsOfType<PlayerModeManager>();
        if (players == null)
        {
            return null;
        }

        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            if (!GoodSamaritanManager.IsUnityNull(player) && player!.isLocalPlayer)
            {
                return player;
            }
        }

        return null;
    }

    [HideFromIl2Cpp]
    private void ShowAreaInstance(Vector3 position, float seconds)
    {
        var area = GameObject.CreatePrimitive(PrimitiveType.Plane);
        area.name = "GoodSamaritanAreaHighlight";
        area.transform.position = new Vector3(position.x, position.y + 0.03f, position.z);
        area.transform.localScale = new Vector3(0.85f, 1f, 0.85f);

        var collider = area.GetComponent<Collider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }

        var renderer = area.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = GetAreaMaterial();
        }

        areaHighlights.Add(new AreaHighlight(area, Time.time + Mathf.Max(0.5f, seconds)));
    }

    [HideFromIl2Cpp]
    private Material GetAreaMaterial()
    {
        if (areaMaterial != null)
        {
            return areaMaterial;
        }

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        areaMaterial = new Material(shader);
        areaMaterial.color = AreaYellow;
        return areaMaterial;
    }

    [HideFromIl2Cpp]
    private void CleanupAreas()
    {
        for (int i = areaHighlights.Count - 1; i >= 0; i--)
        {
            var area = areaHighlights[i];
            if (area.GameObject == null || Time.time > area.HideAt)
            {
                if (area.GameObject != null)
                {
                    Object.Destroy(area.GameObject);
                }

                areaHighlights.RemoveAt(i);
            }
        }
    }

    private readonly struct AreaHighlight
    {
        internal readonly GameObject GameObject;
        internal readonly float HideAt;

        internal AreaHighlight(GameObject gameObject, float hideAt)
        {
            GameObject = gameObject;
            HideAt = hideAt;
        }
    }
}
