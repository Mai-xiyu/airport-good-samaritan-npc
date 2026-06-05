namespace GoodSamaritanNpc;

[HarmonyPatch(typeof(HijackingManager), nameof(HijackingManager.UserCode_TargetStart__NetworkConnectionToClient__Boolean__String))]
internal static class HijackingManagerTargetStartPatch
{
    private static void Postfix(bool isHijacker)
    {
        GoodSamaritanClientHighlighter.NoteLocalHijackerRole(isHijacker);
    }
}

[HarmonyPatch(typeof(HijackingManager), nameof(HijackingManager.UserCode_RpcClearHijackingRole))]
internal static class HijackingManagerClearRolePatch
{
    private static void Postfix()
    {
        GoodSamaritanClientHighlighter.NoteLocalHijackerRole(false);
    }
}
