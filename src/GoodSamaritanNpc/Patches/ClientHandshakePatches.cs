namespace GoodSamaritanNpc;

[HarmonyPatch(typeof(PlayerVoiceControlManager), "Method_Protected_Void_Vector3_VoskCommandType_Il2CppStringArray_Single_PDM_0")]
internal static class PlayerVoiceControlManagerVoiceCommandHandshakePatch0
{
    private static bool Prefix(PlayerVoiceControlManager __instance, Il2CppStringArray translations, float radius)
    {
        return PlayerVoiceControlManagerVoiceCommandHandshake.Handle(__instance, translations, radius);
    }
}

[HarmonyPatch(typeof(PlayerVoiceControlManager), "Method_Protected_Void_Vector3_VoskCommandType_Il2CppStringArray_Single_PDM_1")]
internal static class PlayerVoiceControlManagerVoiceCommandHandshakePatch1
{
    private static bool Prefix(PlayerVoiceControlManager __instance, Il2CppStringArray translations, float radius)
    {
        return PlayerVoiceControlManagerVoiceCommandHandshake.Handle(__instance, translations, radius);
    }
}

internal static class PlayerVoiceControlManagerVoiceCommandHandshake
{
    internal static bool Handle(PlayerVoiceControlManager instance, Il2CppStringArray translations, float radius)
    {
        try
        {
            if (!GoodSamaritanManager.IsClientCapabilityHello(radius, translations))
            {
                return true;
            }

            var component = (Component)instance;
            var pmm = component.GetComponent<PlayerModeManager>() ?? component.GetComponentInParent<PlayerModeManager>();
            GoodSamaritanManager.Instance?.RegisterModdedPlayer(pmm);
            return false;
        }
        catch (Exception ex)
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"Playable witness handshake patch failed: {ex.Message}");
            return false;
        }
    }
}
