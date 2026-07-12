namespace GoodSamaritanNpc;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class GoodSamaritanPlugin : BasePlugin
{
    public const string PluginGuid = "com.airport.good_samaritan";
    public const string PluginName = "GoodSamaritanNpc";
    public const string PluginVersion = "1.4.3";

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
        ClassInjector.RegisterTypeInIl2Cpp<GoodSamaritanHighlightTarget>();
        ClassInjector.RegisterTypeInIl2Cpp<GoodSamaritanClientHighlighter>();

        var managerGo = new GameObject("GoodSamaritanNpcManager");
        Object.DontDestroyOnLoad(managerGo);
        managerGo.AddComponent<GoodSamaritanManager>();
        managerGo.AddComponent<GoodSamaritanClientHighlighter>();

        new Harmony(PluginGuid).PatchAll(typeof(GoodSamaritanPlugin).Assembly);
        Log.LogInfo("Good Samaritan NPC plugin loaded.");
    }
}
