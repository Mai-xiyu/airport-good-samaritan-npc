using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Mirror;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GoodSamaritanNpc;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class GoodSamaritanPlugin : BasePlugin
{
    public const string PluginGuid = "com.airport.good_samaritan";
    public const string PluginName = "GoodSamaritanNpc";
    public const string PluginVersion = "1.0.0";

    internal static ManualLogSource LogSource = null!;
    internal static GoodSamaritanConfig Settings = null!;

    public override void Load()
    {
        LogSource = Log;
        Settings = new GoodSamaritanConfig(Config);
        Config.Save();

        ClassInjector.RegisterTypeInIl2Cpp<GoodSamaritanManager>();
        ClassInjector.RegisterTypeInIl2Cpp<GoodSamaritanWitness>();
        ClassInjector.RegisterTypeInIl2Cpp<GoodSamaritanMarker>();

        var managerGo = new GameObject("GoodSamaritanNpcManager");
        Object.DontDestroyOnLoad(managerGo);
        managerGo.AddComponent<GoodSamaritanManager>();

        new Harmony(PluginGuid).PatchAll(typeof(GoodSamaritanPlugin).Assembly);
        Log.LogInfo("Good Samaritan NPC plugin loaded.");
    }
}

internal sealed class GoodSamaritanConfig
{
    internal readonly ConfigEntry<bool> Enabled;
    internal readonly ConfigEntry<bool> ConvertExistingNpcs;
    internal readonly ConfigEntry<float> ExistingNpcChance;
    internal readonly ConfigEntry<int> ExtraSpawnCount;
    internal readonly ConfigEntry<float> ScanIntervalSeconds;
    internal readonly ConfigEntry<float> WitnessRadius;
    internal readonly ConfigEntry<float> WitnessFovDegrees;
    internal readonly ConfigEntry<float> ReportCooldownSeconds;
    internal readonly ConfigEntry<float> TargetCooldownSeconds;
    internal readonly ConfigEntry<float> HighlightSeconds;
    internal readonly ConfigEntry<bool> EnableCustomClientMarker;
    internal readonly ConfigEntry<bool> EnableVoiceLine;
    internal readonly ConfigEntry<string> Language;

    internal GoodSamaritanConfig(ConfigFile config)
    {
        Enabled = config.Bind("General", "Enabled", true, "Enable Good Samaritan NPC behaviour.");
        ConvertExistingNpcs = config.Bind("Spawn", "ConvertExistingNpcs", true, "Convert a sampled subset of existing NPCs into witnesses.");
        ExistingNpcChance = config.Bind("Spawn", "ExistingNpcChance", 0.18f, "Chance for each ordinary NPC to become a witness when first seen by the server.");
        ExtraSpawnCount = config.Bind("Spawn", "ExtraSpawnCount", 0, "Additional witness NPCs to spawn through NpcManager per level.");
        ScanIntervalSeconds = config.Bind("Detection", "ScanIntervalSeconds", 0.75f, "Seconds between witness scans.");
        WitnessRadius = config.Bind("Detection", "WitnessRadius", 18f, "Suspicious-player search radius around each witness.");
        WitnessFovDegrees = config.Bind("Detection", "WitnessFovDegrees", 90f, "Witness field of view for direct target callouts.");
        ReportCooldownSeconds = config.Bind("Detection", "ReportCooldownSeconds", 12f, "Cooldown per witness and global report throttle.");
        TargetCooldownSeconds = config.Bind("Detection", "TargetCooldownSeconds", 18f, "Cooldown before the same suspicious target can be called out again.");
        HighlightSeconds = config.Bind("Feedback", "HighlightSeconds", 4f, "Seconds to show the original spotted icon on a directly witnessed target.");
        EnableCustomClientMarker = config.Bind("Feedback", "EnableCustomClientMarker", true, "Show an additional local exclamation mark on modded clients.");
        EnableVoiceLine = config.Bind("Feedback", "EnableVoiceLine", true, "Play a short local witness alert sound on modded clients.");
        Language = config.Bind("Localization", "Language", "Auto", "Message language. Supported: Auto, zh-Hans, en, ja, ko, fr, de, es, ru, pt, tr, uk.");
    }
}

internal enum Msg
{
    SuspiciousBehavior,
    RevealingAction,
    CarryingSuspiciousItem,
    HiddenContraband,
    AttackingCivilian,
    TacklingCivilian,
    DirectReport,
    AreaReport,
    AreaSecurityLine,
    AreaTerminal,
    AreaJail,
    AreaSmugglerRoom,
    AreaTsa,
    AreaLobby,
    AreaPlane,
    AreaConveyor,
    AreaBreakRoom,
    AreaCurrentPosition,
    WitnessPrefix
}

internal static class GoodSamaritanText
{
    private static readonly Dictionary<string, Dictionary<Msg, string>> Tables = new()
    {
        ["zh-Hans"] = new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "可疑行为",
            [Msg.RevealingAction] = "暴露动作",
            [Msg.CarryingSuspiciousItem] = "携带可疑物品",
            [Msg.HiddenContraband] = "藏匿违禁品",
            [Msg.AttackingCivilian] = "攻击平民",
            [Msg.TacklingCivilian] = "冲撞平民",
            [Msg.DirectReport] = "有目击者指出可疑人员。",
            [Msg.AreaReport] = "有目击者举报 {0} 附近有可疑行为。",
            [Msg.AreaSecurityLine] = "安检线",
            [Msg.AreaTerminal] = "候机区",
            [Msg.AreaJail] = "监狱",
            [Msg.AreaSmugglerRoom] = "走私者房间",
            [Msg.AreaTsa] = "TSA 区域",
            [Msg.AreaLobby] = "大厅",
            [Msg.AreaPlane] = "飞机附近",
            [Msg.AreaConveyor] = "传送带区域",
            [Msg.AreaBreakRoom] = "休息室",
            [Msg.AreaCurrentPosition] = "当前位置",
            [Msg.WitnessPrefix] = "有目击者"
        },
        ["en"] = new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "suspicious behavior",
            [Msg.RevealingAction] = "revealing action",
            [Msg.CarryingSuspiciousItem] = "carrying suspicious item",
            [Msg.HiddenContraband] = "hidden contraband",
            [Msg.AttackingCivilian] = "attacking a civilian",
            [Msg.TacklingCivilian] = "tackling a civilian",
            [Msg.DirectReport] = "A witness points out a suspicious person.",
            [Msg.AreaReport] = "A witness reports suspicious behavior near {0}.",
            [Msg.AreaSecurityLine] = "security line",
            [Msg.AreaTerminal] = "terminal",
            [Msg.AreaJail] = "jail",
            [Msg.AreaSmugglerRoom] = "smuggler room",
            [Msg.AreaTsa] = "TSA area",
            [Msg.AreaLobby] = "lobby",
            [Msg.AreaPlane] = "the plane",
            [Msg.AreaConveyor] = "conveyor area",
            [Msg.AreaBreakRoom] = "break room",
            [Msg.AreaCurrentPosition] = "this area",
            [Msg.WitnessPrefix] = "A witness"
        },
        ["ja"] = new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "不審な行動",
            [Msg.RevealingAction] = "露見した行動",
            [Msg.CarryingSuspiciousItem] = "不審物を所持",
            [Msg.HiddenContraband] = "密輸品を隠匿",
            [Msg.AttackingCivilian] = "民間人への攻撃",
            [Msg.TacklingCivilian] = "民間人へのタックル",
            [Msg.DirectReport] = "目撃者が不審人物を指摘しています。",
            [Msg.AreaReport] = "目撃者が {0} 付近の不審な行動を通報しました。",
            [Msg.AreaSecurityLine] = "保安検査場",
            [Msg.AreaTerminal] = "ターミナル",
            [Msg.AreaJail] = "拘置所",
            [Msg.AreaSmugglerRoom] = "密輸者の部屋",
            [Msg.AreaTsa] = "TSA エリア",
            [Msg.AreaLobby] = "ロビー",
            [Msg.AreaPlane] = "飛行機付近",
            [Msg.AreaConveyor] = "コンベア付近",
            [Msg.AreaBreakRoom] = "休憩室",
            [Msg.AreaCurrentPosition] = "このエリア",
            [Msg.WitnessPrefix] = "目撃者"
        },
        ["ko"] = new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "수상한 행동",
            [Msg.RevealingAction] = "노출 행동",
            [Msg.CarryingSuspiciousItem] = "수상한 물품 소지",
            [Msg.HiddenContraband] = "밀수품 은닉",
            [Msg.AttackingCivilian] = "민간인 공격",
            [Msg.TacklingCivilian] = "민간인 태클",
            [Msg.DirectReport] = "목격자가 수상한 사람을 지목했습니다.",
            [Msg.AreaReport] = "목격자가 {0} 근처의 수상한 행동을 신고했습니다.",
            [Msg.AreaSecurityLine] = "보안 검색대",
            [Msg.AreaTerminal] = "터미널",
            [Msg.AreaJail] = "감옥",
            [Msg.AreaSmugglerRoom] = "밀수범 방",
            [Msg.AreaTsa] = "TSA 구역",
            [Msg.AreaLobby] = "로비",
            [Msg.AreaPlane] = "비행기 근처",
            [Msg.AreaConveyor] = "컨베이어 구역",
            [Msg.AreaBreakRoom] = "휴게실",
            [Msg.AreaCurrentPosition] = "현재 구역",
            [Msg.WitnessPrefix] = "목격자"
        },
        ["fr"] = new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "comportement suspect",
            [Msg.RevealingAction] = "action révélatrice",
            [Msg.CarryingSuspiciousItem] = "objet suspect porté",
            [Msg.HiddenContraband] = "contrebande cachée",
            [Msg.AttackingCivilian] = "attaque d'un civil",
            [Msg.TacklingCivilian] = "plaquage d'un civil",
            [Msg.DirectReport] = "Un témoin signale une personne suspecte.",
            [Msg.AreaReport] = "Un témoin signale un comportement suspect près de {0}.",
            [Msg.AreaSecurityLine] = "contrôle de sécurité",
            [Msg.AreaTerminal] = "terminal",
            [Msg.AreaJail] = "prison",
            [Msg.AreaSmugglerRoom] = "salle des contrebandiers",
            [Msg.AreaTsa] = "zone TSA",
            [Msg.AreaLobby] = "hall",
            [Msg.AreaPlane] = "avion",
            [Msg.AreaConveyor] = "zone du convoyeur",
            [Msg.AreaBreakRoom] = "salle de pause",
            [Msg.AreaCurrentPosition] = "cette zone",
            [Msg.WitnessPrefix] = "Un témoin"
        },
        ["de"] = new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "verdächtiges Verhalten",
            [Msg.RevealingAction] = "auffällige Handlung",
            [Msg.CarryingSuspiciousItem] = "verdächtiger Gegenstand",
            [Msg.HiddenContraband] = "versteckte Schmuggelware",
            [Msg.AttackingCivilian] = "Angriff auf Zivilisten",
            [Msg.TacklingCivilian] = "Zivilisten gerammt",
            [Msg.DirectReport] = "Ein Zeuge weist auf eine verdächtige Person hin.",
            [Msg.AreaReport] = "Ein Zeuge meldet verdächtiges Verhalten nahe {0}.",
            [Msg.AreaSecurityLine] = "Sicherheitskontrolle",
            [Msg.AreaTerminal] = "Terminal",
            [Msg.AreaJail] = "Gefängnis",
            [Msg.AreaSmugglerRoom] = "Schmugglerraum",
            [Msg.AreaTsa] = "TSA-Bereich",
            [Msg.AreaLobby] = "Lobby",
            [Msg.AreaPlane] = "Flugzeug",
            [Msg.AreaConveyor] = "Förderbandbereich",
            [Msg.AreaBreakRoom] = "Pausenraum",
            [Msg.AreaCurrentPosition] = "dieser Bereich",
            [Msg.WitnessPrefix] = "Ein Zeuge"
        },
        ["es"] = new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "conducta sospechosa",
            [Msg.RevealingAction] = "acción reveladora",
            [Msg.CarryingSuspiciousItem] = "objeto sospechoso",
            [Msg.HiddenContraband] = "contrabando oculto",
            [Msg.AttackingCivilian] = "ataque a civil",
            [Msg.TacklingCivilian] = "placaje a civil",
            [Msg.DirectReport] = "Un testigo señala a una persona sospechosa.",
            [Msg.AreaReport] = "Un testigo informa conducta sospechosa cerca de {0}.",
            [Msg.AreaSecurityLine] = "control de seguridad",
            [Msg.AreaTerminal] = "terminal",
            [Msg.AreaJail] = "cárcel",
            [Msg.AreaSmugglerRoom] = "sala de contrabandistas",
            [Msg.AreaTsa] = "zona TSA",
            [Msg.AreaLobby] = "vestíbulo",
            [Msg.AreaPlane] = "avión",
            [Msg.AreaConveyor] = "zona de cinta",
            [Msg.AreaBreakRoom] = "sala de descanso",
            [Msg.AreaCurrentPosition] = "esta zona",
            [Msg.WitnessPrefix] = "Un testigo"
        },
        ["ru"] = new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "подозрительное поведение",
            [Msg.RevealingAction] = "заметное действие",
            [Msg.CarryingSuspiciousItem] = "подозрительный предмет",
            [Msg.HiddenContraband] = "спрятанная контрабанда",
            [Msg.AttackingCivilian] = "нападение на гражданского",
            [Msg.TacklingCivilian] = "сбит гражданский",
            [Msg.DirectReport] = "Свидетель указывает на подозрительного человека.",
            [Msg.AreaReport] = "Свидетель сообщает о подозрительном поведении рядом с {0}.",
            [Msg.AreaSecurityLine] = "зона досмотра",
            [Msg.AreaTerminal] = "терминал",
            [Msg.AreaJail] = "тюрьма",
            [Msg.AreaSmugglerRoom] = "комната контрабандистов",
            [Msg.AreaTsa] = "зона TSA",
            [Msg.AreaLobby] = "вестибюль",
            [Msg.AreaPlane] = "самолет",
            [Msg.AreaConveyor] = "зона конвейера",
            [Msg.AreaBreakRoom] = "комната отдыха",
            [Msg.AreaCurrentPosition] = "эта зона",
            [Msg.WitnessPrefix] = "Свидетель"
        },
        ["pt"] = new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "comportamento suspeito",
            [Msg.RevealingAction] = "ação reveladora",
            [Msg.CarryingSuspiciousItem] = "item suspeito",
            [Msg.HiddenContraband] = "contrabando escondido",
            [Msg.AttackingCivilian] = "ataque a civil",
            [Msg.TacklingCivilian] = "derrubada de civil",
            [Msg.DirectReport] = "Uma testemunha aponta uma pessoa suspeita.",
            [Msg.AreaReport] = "Uma testemunha relata comportamento suspeito perto de {0}.",
            [Msg.AreaSecurityLine] = "segurança",
            [Msg.AreaTerminal] = "terminal",
            [Msg.AreaJail] = "prisão",
            [Msg.AreaSmugglerRoom] = "sala dos contrabandistas",
            [Msg.AreaTsa] = "área TSA",
            [Msg.AreaLobby] = "saguão",
            [Msg.AreaPlane] = "avião",
            [Msg.AreaConveyor] = "área da esteira",
            [Msg.AreaBreakRoom] = "sala de descanso",
            [Msg.AreaCurrentPosition] = "esta área",
            [Msg.WitnessPrefix] = "Uma testemunha"
        },
        ["tr"] = new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "şüpheli davranış",
            [Msg.RevealingAction] = "açığa çıkaran hareket",
            [Msg.CarryingSuspiciousItem] = "şüpheli eşya taşıma",
            [Msg.HiddenContraband] = "gizli kaçak eşya",
            [Msg.AttackingCivilian] = "sivile saldırı",
            [Msg.TacklingCivilian] = "sivile çarpma",
            [Msg.DirectReport] = "Bir tanık şüpheli bir kişiyi işaret ediyor.",
            [Msg.AreaReport] = "Bir tanık {0} yakınında şüpheli davranış bildirdi.",
            [Msg.AreaSecurityLine] = "güvenlik hattı",
            [Msg.AreaTerminal] = "terminal",
            [Msg.AreaJail] = "hapishane",
            [Msg.AreaSmugglerRoom] = "kaçakçı odası",
            [Msg.AreaTsa] = "TSA bölgesi",
            [Msg.AreaLobby] = "lobi",
            [Msg.AreaPlane] = "uçak yakını",
            [Msg.AreaConveyor] = "konveyör alanı",
            [Msg.AreaBreakRoom] = "mola odası",
            [Msg.AreaCurrentPosition] = "bu alan",
            [Msg.WitnessPrefix] = "Bir tanık"
        },
        ["uk"] = new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "підозріла поведінка",
            [Msg.RevealingAction] = "викривальна дія",
            [Msg.CarryingSuspiciousItem] = "підозрілий предмет",
            [Msg.HiddenContraband] = "схована контрабанда",
            [Msg.AttackingCivilian] = "напад на цивільного",
            [Msg.TacklingCivilian] = "збито цивільного",
            [Msg.DirectReport] = "Свідок вказує на підозрілу особу.",
            [Msg.AreaReport] = "Свідок повідомляє про підозрілу поведінку біля {0}.",
            [Msg.AreaSecurityLine] = "контроль безпеки",
            [Msg.AreaTerminal] = "термінал",
            [Msg.AreaJail] = "в'язниця",
            [Msg.AreaSmugglerRoom] = "кімната контрабандистів",
            [Msg.AreaTsa] = "зона TSA",
            [Msg.AreaLobby] = "вестибюль",
            [Msg.AreaPlane] = "літак",
            [Msg.AreaConveyor] = "зона конвеєра",
            [Msg.AreaBreakRoom] = "кімната відпочинку",
            [Msg.AreaCurrentPosition] = "ця зона",
            [Msg.WitnessPrefix] = "Свідок"
        }
    };

    internal static string Get(Msg id)
    {
        var table = GetTable();
        if (table.TryGetValue(id, out string value))
        {
            return value;
        }

        return Tables["en"][id];
    }

    internal static string Format(Msg id, params object[] args)
    {
        return string.Format(Get(id), args);
    }

    internal static bool IsWitnessLog(string message)
    {
        foreach (var table in Tables.Values)
        {
            if (table.TryGetValue(Msg.WitnessPrefix, out string prefix) &&
                message.StartsWith(prefix, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static Dictionary<Msg, string> GetTable()
    {
        string language = NormalizeLanguage(GoodSamaritanPlugin.Settings.Language.Value);
        if (Tables.TryGetValue(language, out var table))
        {
            return table;
        }

        return Tables["en"];
    }

    private static string NormalizeLanguage(string configured)
    {
        if (string.IsNullOrWhiteSpace(configured) || configured.Equals("Auto", StringComparison.OrdinalIgnoreCase))
        {
            return Application.systemLanguage == SystemLanguage.ChineseSimplified ? "zh-Hans" : "en";
        }

        configured = configured.Trim();
        if (configured.Equals("zh", StringComparison.OrdinalIgnoreCase) ||
            configured.Equals("zh-cn", StringComparison.OrdinalIgnoreCase) ||
            configured.Equals("zh-hans", StringComparison.OrdinalIgnoreCase) ||
            configured.Equals("chinese", StringComparison.OrdinalIgnoreCase))
        {
            return "zh-Hans";
        }

        if (configured.StartsWith("en", StringComparison.OrdinalIgnoreCase)) return "en";
        if (configured.StartsWith("ja", StringComparison.OrdinalIgnoreCase)) return "ja";
        if (configured.StartsWith("ko", StringComparison.OrdinalIgnoreCase)) return "ko";
        if (configured.StartsWith("fr", StringComparison.OrdinalIgnoreCase)) return "fr";
        if (configured.StartsWith("de", StringComparison.OrdinalIgnoreCase)) return "de";
        if (configured.StartsWith("es", StringComparison.OrdinalIgnoreCase)) return "es";
        if (configured.StartsWith("ru", StringComparison.OrdinalIgnoreCase)) return "ru";
        if (configured.StartsWith("pt", StringComparison.OrdinalIgnoreCase)) return "pt";
        if (configured.StartsWith("tr", StringComparison.OrdinalIgnoreCase)) return "tr";
        if (configured.StartsWith("uk", StringComparison.OrdinalIgnoreCase)) return "uk";

        return configured;
    }
}

public sealed class GoodSamaritanManager : MonoBehaviour
{
    internal static GoodSamaritanManager Instance;

    private readonly List<GoodSamaritanWitness> witnesses = new();
    private readonly HashSet<int> evaluatedNpcIds = new();
    private readonly Dictionary<int, double> targetCooldownUntil = new();
    private readonly List<NamedArea> namedAreas = new();
    private double nextGlobalReportTime;
    private float scanTimer;
    private float areaRefreshTimer;
    private int lastNpcManagerInstanceId;
    private int extraSpawnedThisManager;
    private int pendingForcedWitnessMarks;
    private bool serverWasActive;

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

        if (!NetworkServer.active)
        {
            if (serverWasActive)
            {
                ResetServerState();
            }

            return;
        }

        serverWasActive = true;
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

    internal void NotifySuspiciousAction(PlayerModeManager actor, Vector3 eventPosition, string reason)
    {
        if (!GoodSamaritanPlugin.Settings.Enabled.Value || !NetworkServer.active)
        {
            return;
        }

        EnsureWitnessPopulation();
        var witness = FindNearestReadyWitness(eventPosition);
        if (IsUnityNull(witness))
        {
            return;
        }

        if (!IsUnityNull(actor) && IsDirectlyVisible(witness!, ((Component)actor!).transform))
        {
            ReportDirectTarget(witness!, actor!, reason);
            return;
        }

        ReportArea(witness!, eventPosition, reason);
    }

    private void ResetServerState()
    {
        witnesses.Clear();
        evaluatedNpcIds.Clear();
        targetCooldownUntil.Clear();
        namedAreas.Clear();
        nextGlobalReportTime = 0d;
        scanTimer = 0f;
        areaRefreshTimer = 0f;
        lastNpcManagerInstanceId = 0;
        extraSpawnedThisManager = 0;
        pendingForcedWitnessMarks = 0;
        serverWasActive = false;
    }

    private void EnsureServerNpcManagerState()
    {
        var manager = GetNpcManager();
        if (IsUnityNull(manager))
        {
            return;
        }

        int managerId = ((Object)(object)manager!).GetInstanceID();
        if (managerId == lastNpcManagerInstanceId)
        {
            return;
        }

        witnesses.Clear();
        evaluatedNpcIds.Clear();
        targetCooldownUntil.Clear();
        extraSpawnedThisManager = 0;
        pendingForcedWitnessMarks = 0;
        lastNpcManagerInstanceId = managerId;
        RefreshNamedAreas();
    }

    private void EnsureWitnessPopulation()
    {
        SpawnConfiguredExtraWitnesses();
        MarkExistingNpcs();
        CleanupWitnessList();
    }

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

    private bool HasWitness(NpcAiController npc)
    {
        return !IsUnityNull(((Component)npc).GetComponent<GoodSamaritanWitness>());
    }

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
    }

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

            if (CanReportTarget(player) && IsDirectlyVisible(witness, playerTransform))
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

    private bool TryGetSuspicionReason(PlayerModeManager player, out string reason)
    {
        reason = GoodSamaritanText.Get(Msg.SuspiciousBehavior);

        var revealingActions = ((Component)player).GetComponent<PlayerRevealingActions>();
        if (!IsUnityNull(revealingActions))
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
        if (!IsUnityNull(interactor))
        {
            if (IsSuspiciousItem(interactor!.CurrentHeldItem) || IsSuspiciousItem(interactor.CurrentHipItem))
            {
                reason = GoodSamaritanText.Get(Msg.CarryingSuspiciousItem);
                return true;
            }
        }

        var buttStorage = ((Component)player).GetComponent<ButtStorage>();
        if (!IsUnityNull(buttStorage))
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

        return false;
    }

    private static bool IsSuspiciousItem(HeldItemInteractable item)
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

    private bool CanReportTarget(PlayerModeManager target)
    {
        int id = ((Object)(object)target).GetInstanceID();
        double now = Time.timeAsDouble;
        return !targetCooldownUntil.TryGetValue(id, out double until) || now >= until;
    }

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

        witness.NextReportTime = now + Mathf.Max(1f, GoodSamaritanPlugin.Settings.ReportCooldownSeconds.Value);
        nextGlobalReportTime = now + Mathf.Max(0.25f, GoodSamaritanPlugin.Settings.ReportCooldownSeconds.Value);
        targetCooldownUntil[((Object)(object)target).GetInstanceID()] = now + Mathf.Max(1f, GoodSamaritanPlugin.Settings.TargetCooldownSeconds.Value);
        GoodSamaritanPlugin.LogSource.LogDebug($"Direct witness report fired for {reason}.");
    }

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

        witness.NextReportTime = now + Mathf.Max(1f, GoodSamaritanPlugin.Settings.ReportCooldownSeconds.Value);
        nextGlobalReportTime = now + Mathf.Max(0.25f, GoodSamaritanPlugin.Settings.ReportCooldownSeconds.Value);
        GoodSamaritanPlugin.LogSource.LogDebug($"Area witness report fired for {reason} at {area}.");
    }

    private void ShowWitnessIndicator(GoodSamaritanWitness witness, float seconds)
    {
        if (!IsUnityNull(witness.Npc))
        {
            var pvcm = FindRpcCarrier();
            if (!IsUnityNull(pvcm))
            {
                pvcm!.RpcNpcShowIndicatorQuestion(witness.Npc);
            }

            GoodSamaritanMarker.ShowOn(witness.Npc, seconds, true);
        }
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

    private bool IsDirectlyVisible(GoodSamaritanWitness witness, Transform target)
    {
        if (IsUnityNull(witness) || IsUnityNull(target))
        {
            return false;
        }

        Transform witnessTransform = ((Component)witness).transform;
        Vector3 origin = GetWitnessEyePosition(witness);
        Vector3 targetPos = target.position + Vector3.up * 1.2f;
        Vector3 toTarget = targetPos - origin;
        float dist = toTarget.magnitude;
        if (dist <= 0.1f || dist > Mathf.Max(1f, GoodSamaritanPlugin.Settings.WitnessRadius.Value))
        {
            return false;
        }

        float halfFov = Mathf.Clamp(GoodSamaritanPlugin.Settings.WitnessFovDegrees.Value, 1f, 360f) * 0.5f;
        float angle = Vector3.Angle(witnessTransform.forward, toTarget);
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

    private static Vector3 GetWitnessEyePosition(GoodSamaritanWitness witness)
    {
        return ((Component)witness).transform.position + Vector3.up * 1.55f;
    }

    private NpcManager GetNpcManager()
    {
        var manager = NpcManager.ServerInstance;
        if (IsUnityNull(manager))
        {
            manager = Object.FindObjectOfType<NpcManager>();
        }

        return manager;
    }

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

public sealed class GoodSamaritanWitness : MonoBehaviour
{
    internal NpcAiController Npc;
    internal double NextReportTime;

    public GoodSamaritanWitness(IntPtr ptr) : base(ptr)
    {
    }
}

public sealed class GoodSamaritanMarker : MonoBehaviour
{
    private static AudioClip alertClip;

    private TextMesh textMesh;
    private GameObject markerObject;
    private AudioSource audioSource;
    private float hideAt;
    private float nextAudioTime;

    public GoodSamaritanMarker(IntPtr ptr) : base(ptr)
    {
    }

    public void Awake()
    {
        EnsureVisual();
    }

    public void Update()
    {
        if (markerObject == null)
        {
            return;
        }

        bool visible = Time.time < hideAt;
        if (markerObject.activeSelf != visible)
        {
            markerObject.SetActive(visible);
        }

        if (!visible)
        {
            return;
        }

        var cam = Camera.main;
        if (cam != null)
        {
            markerObject.transform.rotation = Quaternion.LookRotation(markerObject.transform.position - cam.transform.position);
        }

        float pulse = 1f + Mathf.Sin(Time.time * 9f) * 0.12f;
        markerObject.transform.localScale = new Vector3(pulse, pulse, pulse);
    }

    internal void Show(float seconds, bool playVoice)
    {
        if (!GoodSamaritanPlugin.Settings.EnableCustomClientMarker.Value)
        {
            return;
        }

        EnsureVisual();
        hideAt = Mathf.Max(hideAt, Time.time + Mathf.Max(0.5f, seconds));
        markerObject?.SetActive(true);

        if (playVoice && GoodSamaritanPlugin.Settings.EnableVoiceLine.Value && Time.time >= nextAudioTime)
        {
            EnsureAudio();
            audioSource?.PlayOneShot(GetAlertClip(), 0.65f);
            nextAudioTime = Time.time + 3f;
        }
    }

    internal static void ShowOn(NpcAiController npc, float seconds, bool playVoice)
    {
        if (!GoodSamaritanPlugin.Settings.EnableCustomClientMarker.Value || GoodSamaritanManager.IsUnityNull(npc))
        {
            return;
        }

        var go = ((Component)npc!).gameObject;
        if (GoodSamaritanManager.IsUnityNull(go))
        {
            return;
        }

        var marker = go.GetComponent<GoodSamaritanMarker>();
        if (GoodSamaritanManager.IsUnityNull(marker))
        {
            marker = go.AddComponent<GoodSamaritanMarker>();
        }

        marker!.Show(seconds, playVoice);
    }

    private void EnsureVisual()
    {
        if (markerObject != null)
        {
            return;
        }

        markerObject = new GameObject("GoodSamaritanExclamation");
        markerObject.transform.SetParent(((Component)this).transform, false);
        markerObject.transform.localPosition = new Vector3(0f, 2.55f, 0f);
        markerObject.transform.localScale = Vector3.one;

        textMesh = markerObject.AddComponent<TextMesh>();
        textMesh.text = "!";
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontSize = 96;
        textMesh.characterSize = 0.08f;
        textMesh.color = new Color(1f, 0.18f, 0.03f, 1f);
        markerObject.SetActive(false);
    }

    private void EnsureAudio()
    {
        if (audioSource != null)
        {
            return;
        }

        audioSource = ((Component)this).gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = ((Component)this).gameObject.AddComponent<AudioSource>();
        }

        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 18f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    private static AudioClip GetAlertClip()
    {
        if (alertClip != null)
        {
            return alertClip;
        }

        const int sampleRate = 22050;
        const float duration = 0.45f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float env = Mathf.Clamp01(t / 0.04f) * Mathf.Clamp01((duration - t) / 0.08f);
            float freq = 420f + Mathf.Sin(t * 31f) * 65f;
            float tone = Mathf.Sin(2f * Mathf.PI * freq * t) + 0.35f * Mathf.Sin(2f * Mathf.PI * freq * 2.02f * t);
            samples[i] = tone * env * 0.18f;
        }

        alertClip = AudioClip.Create("GoodSamaritanWitnessAlert", sampleCount, 1, sampleRate, false);
        alertClip.SetData(samples, 0);
        return alertClip;
    }
}

internal static class GoodSamaritanClientAlertGate
{
    private static float allowQuestionMarkerUntil;

    internal static void NoteLogMessage(string message)
    {
        if (!string.IsNullOrEmpty(message) && GoodSamaritanText.IsWitnessLog(message))
        {
            allowQuestionMarkerUntil = Time.time + 3f;
        }
    }

    internal static bool ShouldEnhanceQuestionIndicator()
    {
        return Time.time <= allowQuestionMarkerUntil;
    }
}

[HarmonyPatch(typeof(LogManager), nameof(LogManager.UserCode_RpcAppendSimple__String))]
internal static class LogManagerAppendSimplePatch
{
    private static void Postfix(string message)
    {
        GoodSamaritanClientAlertGate.NoteLogMessage(message);
    }
}

[HarmonyPatch(typeof(PlayerRevealingActions), nameof(PlayerRevealingActions.ServerOnRevealed))]
internal static class PlayerRevealingActionsServerOnRevealedPatch
{
    private static void Postfix(PlayerRevealingActions __instance)
    {
        try
        {
            var pmm = ((Component)__instance).GetComponent<PlayerModeManager>();
            GoodSamaritanManager.Instance?.NotifySuspiciousAction(pmm, ((Component)__instance).transform.position, GoodSamaritanText.Get(Msg.RevealingAction));
        }
        catch (Exception ex)
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"ServerOnRevealed patch failed: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(RaycastWeapon), nameof(RaycastWeapon.UserCode_CmdSmugglerAttackedCivilian))]
internal static class RaycastWeaponSmugglerAttackedCivilianPatch
{
    private static void Postfix(RaycastWeapon __instance)
    {
        NotifyWeaponNpcAttack(__instance, GoodSamaritanText.Get(Msg.AttackingCivilian));
    }

    private static void NotifyWeaponNpcAttack(RaycastWeapon weapon, string reason)
    {
        try
        {
            var component = (Component)weapon;
            var pmm = component.GetComponent<PlayerModeManager>() ?? component.GetComponentInParent<PlayerModeManager>();
            GoodSamaritanManager.Instance?.NotifySuspiciousAction(pmm, component.transform.position, reason);
        }
        catch (Exception ex)
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"RaycastWeapon NPC attack patch failed: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(RaycastWeapon), nameof(RaycastWeapon.UserCode_CmdJailSelfForNpcHit))]
internal static class RaycastWeaponJailSelfForNpcHitPatch
{
    private static void Postfix(RaycastWeapon __instance)
    {
        try
        {
            var component = (Component)__instance;
            var pmm = component.GetComponent<PlayerModeManager>() ?? component.GetComponentInParent<PlayerModeManager>();
            GoodSamaritanManager.Instance?.NotifySuspiciousAction(pmm, component.transform.position, GoodSamaritanText.Get(Msg.AttackingCivilian));
        }
        catch (Exception ex)
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"RaycastWeapon jail patch failed: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(PlayerTackle), nameof(PlayerTackle.UserCode_CmdTackleNpc__NpcRagdollManager__Vector3))]
internal static class PlayerTackleNpcPatch
{
    private static void Postfix(PlayerTackle __instance, NpcRagdollManager targetNpc)
    {
        try
        {
            var component = (Component)__instance;
            var pmm = component.GetComponent<PlayerModeManager>() ?? component.GetComponentInParent<PlayerModeManager>();
            Vector3 position = GoodSamaritanManager.IsUnityNull(targetNpc)
                ? component.transform.position
                : ((Component)targetNpc).transform.position;
            GoodSamaritanManager.Instance?.NotifySuspiciousAction(pmm, position, GoodSamaritanText.Get(Msg.TacklingCivilian));
        }
        catch (Exception ex)
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"PlayerTackle NPC patch failed: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(PlayerVoiceControlManager), nameof(PlayerVoiceControlManager.UserCode_RpcNpcShowIndicatorQuestion__NpcAiController))]
internal static class PlayerVoiceControlManagerQuestionIndicatorPatch
{
    private static void Postfix(NpcAiController npcAiController)
    {
        try
        {
            if (!GoodSamaritanClientAlertGate.ShouldEnhanceQuestionIndicator())
            {
                return;
            }

            GoodSamaritanMarker.ShowOn(npcAiController, Mathf.Max(1f, GoodSamaritanPlugin.Settings.HighlightSeconds.Value), true);
        }
        catch (Exception ex)
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"Question indicator patch failed: {ex.Message}");
        }
    }
}
