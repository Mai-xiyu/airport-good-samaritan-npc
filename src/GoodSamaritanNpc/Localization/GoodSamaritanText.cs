namespace GoodSamaritanNpc;

internal static class GoodSamaritanText
{
    private static readonly Dictionary<Msg, string> EnglishTable = new()
    {
        [Msg.SuspiciousBehavior] = "suspicious behavior",
        [Msg.RevealingAction] = "revealing action",
        [Msg.CarryingSuspiciousItem] = "carrying suspicious item",
        [Msg.HiddenContraband] = "hidden contraband",
        [Msg.PickingContraband] = "picking up contraband",
        [Msg.AttackingCivilian] = "attacking a civilian",
        [Msg.TacklingCivilian] = "tackling a civilian",
        [Msg.Jumping] = "suspicious jumping",
        [Msg.CuttingLine] = "possible line cutting",
        [Msg.DirectReport] = "A witness points out a suspicious person.",
        [Msg.AreaReport] = "A witness reports suspicious behavior near {0}.",
        [Msg.AreaSecurityLine] = "security line",
        [Msg.AreaTerminal] = "terminal",
        [Msg.AreaJail] = "jail",
        [Msg.AreaSmugglerRoom] = "smuggler room",
        [Msg.AreaTsa] = "TSA area",
        [Msg.AreaLobby] = "lobby",
        [Msg.AreaPlane] = "the plane",
        [Msg.AreaCabin] = "aircraft cabin",
        [Msg.AreaCockpit] = "cockpit",
        [Msg.AreaGalley] = "aircraft galley",
        [Msg.AreaCargo] = "cargo hold",
        [Msg.AreaPlaneExterior] = "outside the plane",
        [Msg.AreaGate] = "boarding gate",
        [Msg.AreaRunway] = "runway",
        [Msg.AreaVending] = "vending area",
        [Msg.AreaTower] = "tower area",
        [Msg.AreaDeathmatch] = "deathmatch area",
        [Msg.AreaSandbox] = "sandbox area",
        [Msg.AreaConveyor] = "conveyor area",
        [Msg.AreaBreakRoom] = "break room",
        [Msg.AreaCurrentPosition] = "this area",
        [Msg.WitnessPrefix] = "A witness"
    };

    private static readonly Dictionary<string, Dictionary<Msg, string>> Tables = new()
    {
        ["zh-Hans"] = CreateTable(new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "\u53ef\u7591\u884c\u4e3a",
            [Msg.RevealingAction] = "\u66b4\u9732\u52a8\u4f5c",
            [Msg.CarryingSuspiciousItem] = "\u643a\u5e26\u53ef\u7591\u7269\u54c1",
            [Msg.HiddenContraband] = "\u85cf\u533f\u8fdd\u7981\u54c1",
            [Msg.PickingContraband] = "\u62fe\u53d6\u8fdd\u7981\u54c1",
            [Msg.AttackingCivilian] = "\u653b\u51fb\u5e73\u6c11",
            [Msg.TacklingCivilian] = "\u51b2\u649e\u5e73\u6c11",
            [Msg.Jumping] = "\u5f02\u5e38\u8df3\u8dc3",
            [Msg.CuttingLine] = "\u7591\u4f3c\u63d2\u961f",
            [Msg.DirectReport] = "\u6709\u76ee\u51fb\u8005\u6307\u51fa\u53ef\u7591\u4eba\u5458\u3002",
            [Msg.AreaReport] = "\u6709\u76ee\u51fb\u8005\u4e3e\u62a5 {0} \u9644\u8fd1\u6709\u53ef\u7591\u884c\u4e3a\u3002",
            [Msg.AreaSecurityLine] = "\u5b89\u68c0\u7ebf",
            [Msg.AreaTerminal] = "\u5019\u673a\u533a",
            [Msg.AreaJail] = "\u76d1\u72f1",
            [Msg.AreaSmugglerRoom] = "\u8d70\u79c1\u8005\u623f\u95f4",
            [Msg.AreaTsa] = "TSA \u533a\u57df",
            [Msg.AreaLobby] = "\u5927\u5385",
            [Msg.AreaPlane] = "\u98de\u673a\u9644\u8fd1",
            [Msg.AreaCabin] = "\u5ba2\u8231",
            [Msg.AreaCockpit] = "\u9a7e\u9a76\u8231",
            [Msg.AreaGalley] = "\u98de\u673a\u914d\u9910\u533a",
            [Msg.AreaCargo] = "\u8d27\u8231",
            [Msg.AreaPlaneExterior] = "\u98de\u673a\u5916\u4fa7",
            [Msg.AreaGate] = "\u767b\u673a\u53e3",
            [Msg.AreaRunway] = "\u8dd1\u9053",
            [Msg.AreaVending] = "\u552e\u8d27\u673a\u533a\u57df",
            [Msg.AreaTower] = "\u5854\u53f0\u533a\u57df",
            [Msg.AreaDeathmatch] = "\u6b7b\u6597\u533a\u57df",
            [Msg.AreaSandbox] = "\u6c99\u76d2\u533a\u57df",
            [Msg.AreaConveyor] = "\u4f20\u9001\u5e26\u533a\u57df",
            [Msg.AreaBreakRoom] = "\u4f11\u606f\u5ba4",
            [Msg.AreaCurrentPosition] = "\u5f53\u524d\u4f4d\u7f6e",
            [Msg.WitnessPrefix] = "\u6709\u76ee\u51fb\u8005"
        }),
        ["en"] = CreateTable(new Dictionary<Msg, string>()),
        ["ja"] = CreateTable(new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "\u4e0d\u5be9\u306a\u884c\u52d5",
            [Msg.RevealingAction] = "\u9732\u898b\u884c\u52d5",
            [Msg.CarryingSuspiciousItem] = "\u4e0d\u5be9\u7269\u3092\u6240\u6301",
            [Msg.HiddenContraband] = "\u5bc6\u8f38\u54c1\u3092\u96a0\u3057\u3066\u3044\u308b",
            [Msg.PickingContraband] = "\u5bc6\u8f38\u54c1\u3092\u62fe\u3063\u305f",
            [Msg.AttackingCivilian] = "\u6c11\u9593\u4eba\u3092\u653b\u6483",
            [Msg.TacklingCivilian] = "\u6c11\u9593\u4eba\u306b\u30bf\u30c3\u30af\u30eb",
            [Msg.Jumping] = "\u4e0d\u5be9\u306a\u30b8\u30e3\u30f3\u30d7",
            [Msg.CuttingLine] = "\u5272\u308a\u8fbc\u307f\u306e\u7591\u3044",
            [Msg.DirectReport] = "\u76ee\u6483\u8005\u304c\u4e0d\u5be9\u4eba\u7269\u3092\u6307\u6458\u3057\u307e\u3057\u305f\u3002",
            [Msg.AreaReport] = "\u76ee\u6483\u8005\u304c {0} \u4ed8\u8fd1\u306e\u4e0d\u5be9\u884c\u52d5\u3092\u901a\u5831\u3057\u307e\u3057\u305f\u3002",
            [Msg.AreaSecurityLine] = "\u4fdd\u5b89\u691c\u67fb\u5217",
            [Msg.AreaTerminal] = "\u30bf\u30fc\u30df\u30ca\u30eb",
            [Msg.AreaJail] = "\u7559\u7f6e\u6240",
            [Msg.AreaSmugglerRoom] = "\u5bc6\u8f38\u8005\u306e\u90e8\u5c4b",
            [Msg.AreaTsa] = "TSA \u30a8\u30ea\u30a2",
            [Msg.AreaLobby] = "\u30ed\u30d3\u30fc",
            [Msg.AreaPlane] = "\u98db\u884c\u6a5f\u4ed8\u8fd1",
            [Msg.AreaConveyor] = "\u30b3\u30f3\u30d9\u30e4\u30fc\u5468\u8fba",
            [Msg.AreaBreakRoom] = "\u4f11\u61a9\u5ba4",
            [Msg.AreaCurrentPosition] = "\u3053\u306e\u30a8\u30ea\u30a2",
            [Msg.WitnessPrefix] = "\u76ee\u6483\u8005"
        }),
        ["ko"] = CreateTable(new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "\uc218\uc0c1\ud55c \ud589\ub3d9",
            [Msg.RevealingAction] = "\ub178\ucd9c \ud589\ub3d9",
            [Msg.CarryingSuspiciousItem] = "\uc218\uc0c1\ud55c \ubb3c\ud488 \uc18c\uc9c0",
            [Msg.HiddenContraband] = "\uae08\uc9c0\ud488 \uc740\ub2c9",
            [Msg.PickingContraband] = "\uae08\uc9c0\ud488 \ud68d\ub4dd",
            [Msg.AttackingCivilian] = "\ubbfc\uac04\uc778 \uacf5\uaca9",
            [Msg.TacklingCivilian] = "\ubbfc\uac04\uc778 \ud0dc\ud074",
            [Msg.Jumping] = "\uc218\uc0c1\ud55c \uc810\ud504",
            [Msg.CuttingLine] = "\uc0c8\uce58\uae30 \uc758\uc2ec",
            [Msg.DirectReport] = "\ubaa9\uaca9\uc790\uac00 \uc218\uc0c1\ud55c \uc0ac\ub78c\uc744 \uc9c0\ubaa9\ud588\uc2b5\ub2c8\ub2e4.",
            [Msg.AreaReport] = "\ubaa9\uaca9\uc790\uac00 {0} \uadfc\ucc98\uc758 \uc218\uc0c1\ud55c \ud589\ub3d9\uc744 \uc2e0\uace0\ud588\uc2b5\ub2c8\ub2e4.",
            [Msg.AreaSecurityLine] = "\ubcf4\uc548 \uac80\uc0c9\uc904",
            [Msg.AreaTerminal] = "\ud130\ubbf8\ub110",
            [Msg.AreaJail] = "\uac10\uc625",
            [Msg.AreaSmugglerRoom] = "\ubc00\uc218\ubc94 \ubc29",
            [Msg.AreaTsa] = "TSA \uad6c\uc5ed",
            [Msg.AreaLobby] = "\ub85c\ube44",
            [Msg.AreaPlane] = "\ube44\ud589\uae30 \uadfc\ucc98",
            [Msg.AreaConveyor] = "\ucee8\ubca0\uc774\uc5b4 \uad6c\uc5ed",
            [Msg.AreaBreakRoom] = "\ud734\uac8c\uc2e4",
            [Msg.AreaCurrentPosition] = "\uc774 \uad6c\uc5ed",
            [Msg.WitnessPrefix] = "\ubaa9\uaca9\uc790"
        }),
        ["fr"] = CreateTable(new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "comportement suspect",
            [Msg.RevealingAction] = "action r\u00e9v\u00e9latrice",
            [Msg.CarryingSuspiciousItem] = "objet suspect port\u00e9",
            [Msg.HiddenContraband] = "contrebande dissimul\u00e9e",
            [Msg.PickingContraband] = "ramassage de contrebande",
            [Msg.AttackingCivilian] = "attaque contre un civil",
            [Msg.TacklingCivilian] = "plaquage contre un civil",
            [Msg.Jumping] = "saut suspect",
            [Msg.CuttingLine] = "possible d\u00e9passement de file",
            [Msg.DirectReport] = "Un t\u00e9moin signale une personne suspecte.",
            [Msg.AreaReport] = "Un t\u00e9moin signale un comportement suspect pr\u00e8s de {0}.",
            [Msg.AreaSecurityLine] = "file de s\u00e9curit\u00e9",
            [Msg.AreaTerminal] = "terminal",
            [Msg.AreaJail] = "prison",
            [Msg.AreaSmugglerRoom] = "salle des contrebandiers",
            [Msg.AreaTsa] = "zone TSA",
            [Msg.AreaLobby] = "hall",
            [Msg.AreaPlane] = "pr\u00e8s de l'avion",
            [Msg.AreaConveyor] = "zone du convoyeur",
            [Msg.AreaBreakRoom] = "salle de pause",
            [Msg.AreaCurrentPosition] = "cette zone",
            [Msg.WitnessPrefix] = "Un t\u00e9moin"
        }),
        ["de"] = CreateTable(new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "verd\u00e4chtiges Verhalten",
            [Msg.RevealingAction] = "aufdeckende Aktion",
            [Msg.CarryingSuspiciousItem] = "verd\u00e4chtigen Gegenstand getragen",
            [Msg.HiddenContraband] = "versteckte Schmuggelware",
            [Msg.PickingContraband] = "Schmuggelware aufgehoben",
            [Msg.AttackingCivilian] = "Angriff auf Zivilperson",
            [Msg.TacklingCivilian] = "Tackle gegen Zivilperson",
            [Msg.Jumping] = "verd\u00e4chtiges Springen",
            [Msg.CuttingLine] = "m\u00f6gliches Vordr\u00e4ngeln",
            [Msg.DirectReport] = "Ein Zeuge weist auf eine verd\u00e4chtige Person hin.",
            [Msg.AreaReport] = "Ein Zeuge meldet verd\u00e4chtiges Verhalten nahe {0}.",
            [Msg.AreaSecurityLine] = "Sicherheitsreihe",
            [Msg.AreaTerminal] = "Terminal",
            [Msg.AreaJail] = "Gef\u00e4ngnis",
            [Msg.AreaSmugglerRoom] = "Schmugglerraum",
            [Msg.AreaTsa] = "TSA-Bereich",
            [Msg.AreaLobby] = "Lobby",
            [Msg.AreaPlane] = "beim Flugzeug",
            [Msg.AreaConveyor] = "F\u00f6rderbandbereich",
            [Msg.AreaBreakRoom] = "Pausenraum",
            [Msg.AreaCurrentPosition] = "dieser Bereich",
            [Msg.WitnessPrefix] = "Ein Zeuge"
        }),
        ["es"] = CreateTable(new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "comportamiento sospechoso",
            [Msg.RevealingAction] = "acci\u00f3n reveladora",
            [Msg.CarryingSuspiciousItem] = "objeto sospechoso transportado",
            [Msg.HiddenContraband] = "contrabando oculto",
            [Msg.PickingContraband] = "recogiendo contrabando",
            [Msg.AttackingCivilian] = "ataque a un civil",
            [Msg.TacklingCivilian] = "embestida a un civil",
            [Msg.Jumping] = "salto sospechoso",
            [Msg.CuttingLine] = "posible salto de fila",
            [Msg.DirectReport] = "Un testigo se\u00f1ala a una persona sospechosa.",
            [Msg.AreaReport] = "Un testigo informa comportamiento sospechoso cerca de {0}.",
            [Msg.AreaSecurityLine] = "fila de seguridad",
            [Msg.AreaTerminal] = "terminal",
            [Msg.AreaJail] = "c\u00e1rcel",
            [Msg.AreaSmugglerRoom] = "sala de contrabandistas",
            [Msg.AreaTsa] = "zona TSA",
            [Msg.AreaLobby] = "vest\u00edbulo",
            [Msg.AreaPlane] = "cerca del avi\u00f3n",
            [Msg.AreaConveyor] = "zona de cinta transportadora",
            [Msg.AreaBreakRoom] = "sala de descanso",
            [Msg.AreaCurrentPosition] = "esta zona",
            [Msg.WitnessPrefix] = "Un testigo"
        }),
        ["ru"] = CreateTable(new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "\u043f\u043e\u0434\u043e\u0437\u0440\u0438\u0442\u0435\u043b\u044c\u043d\u043e\u0435 \u043f\u043e\u0432\u0435\u0434\u0435\u043d\u0438\u0435",
            [Msg.RevealingAction] = "\u0434\u0435\u0439\u0441\u0442\u0432\u0438\u0435 \u0440\u0430\u0441\u043a\u0440\u044b\u0442\u0438\u044f",
            [Msg.CarryingSuspiciousItem] = "\u043f\u043e\u0434\u043e\u0437\u0440\u0438\u0442\u0435\u043b\u044c\u043d\u044b\u0439 \u043f\u0440\u0435\u0434\u043c\u0435\u0442 \u043f\u0440\u0438 \u0441\u0435\u0431\u0435",
            [Msg.HiddenContraband] = "\u0441\u043f\u0440\u044f\u0442\u0430\u043d\u043d\u0430\u044f \u043a\u043e\u043d\u0442\u0440\u0430\u0431\u0430\u043d\u0434\u0430",
            [Msg.PickingContraband] = "\u043f\u043e\u0434\u043d\u044f\u0442\u0438\u0435 \u043a\u043e\u043d\u0442\u0440\u0430\u0431\u0430\u043d\u0434\u044b",
            [Msg.AttackingCivilian] = "\u043d\u0430\u043f\u0430\u0434\u0435\u043d\u0438\u0435 \u043d\u0430 \u0433\u0440\u0430\u0436\u0434\u0430\u043d\u0441\u043a\u043e\u0433\u043e",
            [Msg.TacklingCivilian] = "\u0442\u043e\u043b\u0447\u043e\u043a \u0433\u0440\u0430\u0436\u0434\u0430\u043d\u0441\u043a\u043e\u0433\u043e",
            [Msg.Jumping] = "\u043f\u043e\u0434\u043e\u0437\u0440\u0438\u0442\u0435\u043b\u044c\u043d\u044b\u0439 \u043f\u0440\u044b\u0436\u043e\u043a",
            [Msg.CuttingLine] = "\u0432\u043e\u0437\u043c\u043e\u0436\u043d\u043e\u0435 \u043d\u0430\u0440\u0443\u0448\u0435\u043d\u0438\u0435 \u043e\u0447\u0435\u0440\u0435\u0434\u0438",
            [Msg.DirectReport] = "\u0421\u0432\u0438\u0434\u0435\u0442\u0435\u043b\u044c \u0443\u043a\u0430\u0437\u0430\u043b \u043d\u0430 \u043f\u043e\u0434\u043e\u0437\u0440\u0438\u0442\u0435\u043b\u044c\u043d\u043e\u0433\u043e \u0447\u0435\u043b\u043e\u0432\u0435\u043a\u0430.",
            [Msg.AreaReport] = "\u0421\u0432\u0438\u0434\u0435\u0442\u0435\u043b\u044c \u0441\u043e\u043e\u0431\u0449\u0430\u0435\u0442 \u043e \u043f\u043e\u0434\u043e\u0437\u0440\u0438\u0442\u0435\u043b\u044c\u043d\u043e\u043c \u043f\u043e\u0432\u0435\u0434\u0435\u043d\u0438\u0438 \u0440\u044f\u0434\u043e\u043c \u0441 {0}.",
            [Msg.AreaSecurityLine] = "\u043e\u0447\u0435\u0440\u0435\u0434\u044c \u0434\u043e\u0441\u043c\u043e\u0442\u0440\u0430",
            [Msg.AreaTerminal] = "\u0442\u0435\u0440\u043c\u0438\u043d\u0430\u043b",
            [Msg.AreaJail] = "\u0442\u044e\u0440\u044c\u043c\u0430",
            [Msg.AreaSmugglerRoom] = "\u043a\u043e\u043c\u043d\u0430\u0442\u0430 \u043a\u043e\u043d\u0442\u0440\u0430\u0431\u0430\u043d\u0434\u0438\u0441\u0442\u043e\u0432",
            [Msg.AreaTsa] = "\u0437\u043e\u043d\u0430 TSA",
            [Msg.AreaLobby] = "\u0432\u0435\u0441\u0442\u0438\u0431\u044e\u043b\u044c",
            [Msg.AreaPlane] = "\u0440\u044f\u0434\u043e\u043c \u0441 \u0441\u0430\u043c\u043e\u043b\u0435\u0442\u043e\u043c",
            [Msg.AreaConveyor] = "\u0437\u043e\u043d\u0430 \u043a\u043e\u043d\u0432\u0435\u0439\u0435\u0440\u0430",
            [Msg.AreaBreakRoom] = "\u043a\u043e\u043c\u043d\u0430\u0442\u0430 \u043e\u0442\u0434\u044b\u0445\u0430",
            [Msg.AreaCurrentPosition] = "\u044d\u0442\u0430 \u0437\u043e\u043d\u0430",
            [Msg.WitnessPrefix] = "\u0421\u0432\u0438\u0434\u0435\u0442\u0435\u043b\u044c"
        }),
        ["pt"] = CreateTable(new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "comportamento suspeito",
            [Msg.RevealingAction] = "a\u00e7\u00e3o reveladora",
            [Msg.CarryingSuspiciousItem] = "item suspeito carregado",
            [Msg.HiddenContraband] = "contrabando escondido",
            [Msg.PickingContraband] = "pegando contrabando",
            [Msg.AttackingCivilian] = "ataque contra civil",
            [Msg.TacklingCivilian] = "investida contra civil",
            [Msg.Jumping] = "pulo suspeito",
            [Msg.CuttingLine] = "poss\u00edvel furo de fila",
            [Msg.DirectReport] = "Uma testemunha aponta uma pessoa suspeita.",
            [Msg.AreaReport] = "Uma testemunha relata comportamento suspeito perto de {0}.",
            [Msg.AreaSecurityLine] = "fila de seguran\u00e7a",
            [Msg.AreaTerminal] = "terminal",
            [Msg.AreaJail] = "pris\u00e3o",
            [Msg.AreaSmugglerRoom] = "sala dos contrabandistas",
            [Msg.AreaTsa] = "\u00e1rea TSA",
            [Msg.AreaLobby] = "sagu\u00e3o",
            [Msg.AreaPlane] = "perto do avi\u00e3o",
            [Msg.AreaConveyor] = "\u00e1rea da esteira",
            [Msg.AreaBreakRoom] = "sala de descanso",
            [Msg.AreaCurrentPosition] = "esta \u00e1rea",
            [Msg.WitnessPrefix] = "Uma testemunha"
        }),
        ["tr"] = CreateTable(new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "\u015f\u00fcpheli davran\u0131\u015f",
            [Msg.RevealingAction] = "if\u015fa edici hareket",
            [Msg.CarryingSuspiciousItem] = "\u015f\u00fcpheli e\u015fya ta\u015f\u0131ma",
            [Msg.HiddenContraband] = "gizlenmi\u015f ka\u00e7ak e\u015fya",
            [Msg.PickingContraband] = "ka\u00e7ak e\u015fya alma",
            [Msg.AttackingCivilian] = "sivile sald\u0131r\u0131",
            [Msg.TacklingCivilian] = "sivile \u00e7arpma",
            [Msg.Jumping] = "\u015f\u00fcpheli z\u0131plama",
            [Msg.CuttingLine] = "olas\u0131 s\u0131ra ihlali",
            [Msg.DirectReport] = "Bir tan\u0131k \u015f\u00fcpheli bir ki\u015fiyi i\u015faret ediyor.",
            [Msg.AreaReport] = "Bir tan\u0131k {0} yak\u0131n\u0131nda \u015f\u00fcpheli davran\u0131\u015f bildiriyor.",
            [Msg.AreaSecurityLine] = "g\u00fcvenlik s\u0131ras\u0131",
            [Msg.AreaTerminal] = "terminal",
            [Msg.AreaJail] = "hapishane",
            [Msg.AreaSmugglerRoom] = "ka\u00e7ak\u00e7\u0131 odas\u0131",
            [Msg.AreaTsa] = "TSA b\u00f6lgesi",
            [Msg.AreaLobby] = "lobi",
            [Msg.AreaPlane] = "u\u00e7a\u011f\u0131n yak\u0131n\u0131",
            [Msg.AreaConveyor] = "konvey\u00f6r alan\u0131",
            [Msg.AreaBreakRoom] = "dinlenme odas\u0131",
            [Msg.AreaCurrentPosition] = "bu b\u00f6lge",
            [Msg.WitnessPrefix] = "Bir tan\u0131k"
        }),
        ["uk"] = CreateTable(new Dictionary<Msg, string>
        {
            [Msg.SuspiciousBehavior] = "\u043f\u0456\u0434\u043e\u0437\u0440\u0456\u043b\u0430 \u043f\u043e\u0432\u0435\u0434\u0456\u043d\u043a\u0430",
            [Msg.RevealingAction] = "\u0434\u0456\u044f \u0432\u0438\u043a\u0440\u0438\u0442\u0442\u044f",
            [Msg.CarryingSuspiciousItem] = "\u043f\u0456\u0434\u043e\u0437\u0440\u0456\u043b\u0438\u0439 \u043f\u0440\u0435\u0434\u043c\u0435\u0442 \u0443 \u0440\u0443\u043a\u0430\u0445",
            [Msg.HiddenContraband] = "\u0441\u0445\u043e\u0432\u0430\u043d\u0430 \u043a\u043e\u043d\u0442\u0440\u0430\u0431\u0430\u043d\u0434\u0430",
            [Msg.PickingContraband] = "\u043f\u0456\u0434\u0431\u0438\u0440\u0430\u043d\u043d\u044f \u043a\u043e\u043d\u0442\u0440\u0430\u0431\u0430\u043d\u0434\u0438",
            [Msg.AttackingCivilian] = "\u043d\u0430\u043f\u0430\u0434 \u043d\u0430 \u0446\u0438\u0432\u0456\u043b\u044c\u043d\u043e\u0433\u043e",
            [Msg.TacklingCivilian] = "\u0440\u0438\u0432\u043e\u043a \u0443 \u0446\u0438\u0432\u0456\u043b\u044c\u043d\u043e\u0433\u043e",
            [Msg.Jumping] = "\u043f\u0456\u0434\u043e\u0437\u0440\u0456\u043b\u0438\u0439 \u0441\u0442\u0440\u0438\u0431\u043e\u043a",
            [Msg.CuttingLine] = "\u043c\u043e\u0436\u043b\u0438\u0432\u0435 \u043f\u043e\u0440\u0443\u0448\u0435\u043d\u043d\u044f \u0447\u0435\u0440\u0433\u0438",
            [Msg.DirectReport] = "\u0421\u0432\u0456\u0434\u043e\u043a \u0432\u043a\u0430\u0437\u0443\u0454 \u043d\u0430 \u043f\u0456\u0434\u043e\u0437\u0440\u0456\u043b\u0443 \u043e\u0441\u043e\u0431\u0443.",
            [Msg.AreaReport] = "\u0421\u0432\u0456\u0434\u043e\u043a \u043f\u043e\u0432\u0456\u0434\u043e\u043c\u043b\u044f\u0454 \u043f\u0440\u043e \u043f\u0456\u0434\u043e\u0437\u0440\u0456\u043b\u0443 \u043f\u043e\u0432\u0435\u0434\u0456\u043d\u043a\u0443 \u0431\u0456\u043b\u044f {0}.",
            [Msg.AreaSecurityLine] = "\u0447\u0435\u0440\u0433\u0430 \u0431\u0435\u0437\u043f\u0435\u043a\u0438",
            [Msg.AreaTerminal] = "\u0442\u0435\u0440\u043c\u0456\u043d\u0430\u043b",
            [Msg.AreaJail] = "\u0432'\u044f\u0437\u043d\u0438\u0446\u044f",
            [Msg.AreaSmugglerRoom] = "\u043a\u0456\u043c\u043d\u0430\u0442\u0430 \u043a\u043e\u043d\u0442\u0440\u0430\u0431\u0430\u043d\u0434\u0438\u0441\u0442\u0456\u0432",
            [Msg.AreaTsa] = "\u0437\u043e\u043d\u0430 TSA",
            [Msg.AreaLobby] = "\u0432\u0435\u0441\u0442\u0438\u0431\u044e\u043b\u044c",
            [Msg.AreaPlane] = "\u0431\u0456\u043b\u044f \u043b\u0456\u0442\u0430\u043a\u0430",
            [Msg.AreaConveyor] = "\u0437\u043e\u043d\u0430 \u043a\u043e\u043d\u0432\u0435\u0454\u0440\u0430",
            [Msg.AreaBreakRoom] = "\u043a\u0456\u043c\u043d\u0430\u0442\u0430 \u0432\u0456\u0434\u043f\u043e\u0447\u0438\u043d\u043a\u0443",
            [Msg.AreaCurrentPosition] = "\u0446\u044f \u0437\u043e\u043d\u0430",
            [Msg.WitnessPrefix] = "\u0421\u0432\u0456\u0434\u043e\u043a"
        })
    };

    private static Dictionary<Msg, string> CreateTable(Dictionary<Msg, string> overrides)
    {
        var table = new Dictionary<Msg, string>(EnglishTable);
        foreach (var pair in overrides)
        {
            table[pair.Key] = pair.Value;
        }

        return table;
    }

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

    internal static bool IsDirectReportLog(string message)
    {
        foreach (var table in Tables.Values)
        {
            if (table.TryGetValue(Msg.DirectReport, out string direct) &&
                string.Equals(message, direct, StringComparison.Ordinal))
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
