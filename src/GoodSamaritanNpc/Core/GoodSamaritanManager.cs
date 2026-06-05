namespace GoodSamaritanNpc;

public sealed partial class GoodSamaritanManager : MonoBehaviour
{
    private const float ClientHelloRadius = -7391.25f;
    private const string ClientHelloToken = "GSNPC_HELLO_1";

    internal static GoodSamaritanManager Instance;

    private readonly List<GoodSamaritanWitness> witnesses = new();
    private readonly List<PlayerWitnessState> playerWitnesses = new();
    private readonly HashSet<int> evaluatedNpcIds = new();
    private readonly HashSet<uint> moddedPlayerNetIds = new();
    private readonly HashSet<uint> playableWitnessNetIds = new();
    private readonly HashSet<uint> playableUndercoverNetIds = new();
    private readonly Dictionary<int, double> targetCooldownUntil = new();
    private readonly List<NamedArea> namedAreas = new();
    private double nextGlobalReportTime;
    private double nextPlayerAssignmentTime;
    private float scanTimer;
    private float clientHelloTimer;
    private float areaRefreshTimer;
    private int lastNpcManagerInstanceId;
    private int extraSpawnedThisManager;
    private int pendingForcedWitnessMarks;
    private bool serverWasActive;
    private bool lastGameStarted;
    private bool playableWitnessesAssignedThisRound;
    private bool playableUndercoverAssignedThisRound;

    public GoodSamaritanManager(IntPtr ptr) : base(ptr)
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
        if (!GoodSamaritanPlugin.Settings.Enabled.Value)
        {
            return;
        }

        TrySendClientCapabilityHello();

        if (!NetworkServer.active)
        {
            if (serverWasActive)
            {
                ResetServerState();
            }

            return;
        }

        serverWasActive = true;
        UpdatePlayableWitnessAssignments();
        EnsureServerNpcManagerState();
        EnsureWitnessPopulation();

        scanTimer -= Time.deltaTime;
        if (scanTimer > 0f)
        {
            return;
        }

        scanTimer = Mathf.Max(0.1f, GoodSamaritanPlugin.Settings.ScanIntervalSeconds.Value);
        ScanWitnesses();
    }

    [HideFromIl2Cpp]
    internal void NotifySuspiciousAction(PlayerModeManager actor, Vector3 eventPosition, string reason, SuspicionEventType eventType)
    {
        if (!GoodSamaritanPlugin.Settings.Enabled.Value || !NetworkServer.active)
        {
            return;
        }

        if (!ShouldReportActorForEvent(actor, eventType))
        {
            return;
        }

        EnsureWitnessPopulation();
        var witness = FindNearestReadyWitness(eventPosition);
        if (IsUnityNull(witness))
        {
            return;
        }

        if (GoodSamaritanPlugin.Settings.DirectTargetReportsEnabled && !IsUnityNull(actor) && IsDirectlyVisible(witness!, ((Component)actor!).transform))
        {
            ReportDirectTarget(witness!, actor!, reason);
            return;
        }

        ReportArea(witness!, eventPosition, reason);
    }

    [HideFromIl2Cpp]
    internal void RegisterModdedPlayer(PlayerModeManager player)
    {
        if (!GoodSamaritanPlugin.Settings.Enabled.Value ||
            !GoodSamaritanPlugin.Settings.RequiresModdedClientCapability ||
            !NetworkServer.active ||
            IsUnityNull(player))
        {
            return;
        }

        uint netId = player!.netId;
        if (netId == 0u)
        {
            return;
        }

        if (moddedPlayerNetIds.Add(netId))
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"Registered modded player capability for netId {netId}.");
        }
    }

    internal static bool IsClientCapabilityHello(float radius, Il2CppStringArray translations)
    {
        if (radius > ClientHelloRadius + 0.01f || radius < ClientHelloRadius - 0.01f || translations == null || translations.Length == 0)
        {
            return false;
        }

        return string.Equals(translations[0], ClientHelloToken, StringComparison.Ordinal);
    }

    [HideFromIl2Cpp]
    private void TrySendClientCapabilityHello()
    {
        if (!GoodSamaritanPlugin.Settings.RequiresModdedClientCapability || !NetworkClient.active)
        {
            return;
        }

        clientHelloTimer -= Time.deltaTime;
        if (clientHelloTimer > 0f)
        {
            return;
        }

        clientHelloTimer = 5f;

        var pvcm = FindLocalVoiceControlManager();
        if (IsUnityNull(pvcm))
        {
            return;
        }

        try
        {
            var translations = new Il2CppStringArray(2);
            translations[0] = ClientHelloToken;
            translations[1] = GoodSamaritanPlugin.PluginVersion;
            pvcm!.CmdVoiceCommand(((Component)pvcm).transform.position, (VoskCommandType)0, translations, ClientHelloRadius);

            if (NetworkServer.active)
            {
                var pmm = ((Component)pvcm).GetComponent<PlayerModeManager>() ?? ((Component)pvcm).GetComponentInParent<PlayerModeManager>();
                RegisterModdedPlayer(pmm);
            }
        }
        catch (Exception ex)
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"Playable witness handshake failed: {ex.Message}");
        }
    }

    [HideFromIl2Cpp]
    private PlayerVoiceControlManager FindLocalVoiceControlManager()
    {
        var managers = Object.FindObjectsOfType<PlayerVoiceControlManager>();
        if (managers == null)
        {
            return null;
        }

        for (int i = 0; i < managers.Length; i++)
        {
            var manager = managers[i];
            if (!IsUnityNull(manager) && manager!.isLocalPlayer)
            {
                return manager;
            }
        }

        return null;
    }

    [HideFromIl2Cpp]
    private void ResetServerState()
    {
        witnesses.Clear();
        playerWitnesses.Clear();
        evaluatedNpcIds.Clear();
        moddedPlayerNetIds.Clear();
        playableWitnessNetIds.Clear();
        playableUndercoverNetIds.Clear();
        targetCooldownUntil.Clear();
        namedAreas.Clear();
        nextGlobalReportTime = 0d;
        nextPlayerAssignmentTime = 0d;
        scanTimer = 0f;
        clientHelloTimer = 0f;
        areaRefreshTimer = 0f;
        lastNpcManagerInstanceId = 0;
        extraSpawnedThisManager = 0;
        pendingForcedWitnessMarks = 0;
        serverWasActive = false;
        lastGameStarted = false;
        playableWitnessesAssignedThisRound = false;
        playableUndercoverAssignedThisRound = false;
    }

    [HideFromIl2Cpp]
    private void EnsureServerNpcManagerState()
    {
        int sourceId = GetNpcPopulationSourceId();
        if (sourceId == 0)
        {
            return;
        }

        if (sourceId == lastNpcManagerInstanceId)
        {
            return;
        }

        witnesses.Clear();
        evaluatedNpcIds.Clear();
        targetCooldownUntil.Clear();
        extraSpawnedThisManager = 0;
        pendingForcedWitnessMarks = 0;
        lastNpcManagerInstanceId = sourceId;
        RefreshNamedAreas();
    }
}
