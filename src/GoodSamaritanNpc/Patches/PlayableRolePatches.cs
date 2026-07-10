namespace GoodSamaritanNpc;

[HarmonyPatch(typeof(PlayerVoiceControlManager), nameof(PlayerVoiceControlManager.UserCode_TargetNpcVoiceCommandSucceeded__NetworkConnection__VoskCommandType__Int32))]
internal static class PlayerVoiceControlManagerRoleSyncPatch
{
    private static bool Prefix(VoskCommandType commandType, int affectedCount)
    {
        return !GoodSamaritanClientRoleState.TryHandleRoleSync(commandType, affectedCount);
    }
}

[HarmonyPatch(typeof(PlayerModeManager), nameof(PlayerModeManager.ApplyModeObjects))]
internal static class PlayerModeManagerApplyModeObjectsPatch
{
    private static void Postfix(PlayerModeManager __instance)
    {
        GoodSamaritanClientRoleState.ApplyVisualOverride(__instance);
    }
}
