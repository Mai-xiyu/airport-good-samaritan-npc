namespace GoodSamaritanNpc;

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

            float seconds = Mathf.Max(1f, GoodSamaritanPlugin.Settings.HighlightSeconds.Value);
            GoodSamaritanMarker.ShowOn(npcAiController, seconds, true);
            GoodSamaritanClientHighlighter.ShowNpc(npcAiController, GoodSamaritanHighlightKind.Ally, seconds);
            if (GoodSamaritanClientAlertGate.ShouldEnhanceAreaIndicator() && !GoodSamaritanManager.IsUnityNull(npcAiController))
            {
                GoodSamaritanClientHighlighter.ShowArea(((Component)npcAiController!).transform.position, Mathf.Max(1f, GoodSamaritanPlugin.Settings.AreaHighlightSeconds.Value));
            }
        }
        catch (Exception ex)
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"Question indicator patch failed: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(PlayerVoiceControlManager), nameof(PlayerVoiceControlManager.UserCode_RpcPlayerShowIndicatorQuestion__PlayerVoiceControlManager))]
internal static class PlayerVoiceControlManagerPlayerQuestionIndicatorPatch
{
    private static void Postfix(PlayerVoiceControlManager pvcm)
    {
        try
        {
            if (!GoodSamaritanClientAlertGate.ShouldEnhanceQuestionIndicator() || GoodSamaritanManager.IsUnityNull(pvcm))
            {
                return;
            }

            float seconds = Mathf.Max(1f, GoodSamaritanPlugin.Settings.HighlightSeconds.Value);
            GoodSamaritanMarker.ShowOn((Component)pvcm!, seconds, true);
            var player = ((Component)pvcm!).GetComponent<PlayerModeManager>() ?? ((Component)pvcm).GetComponentInParent<PlayerModeManager>();
            if (!GoodSamaritanManager.IsUnityNull(player))
            {
                GoodSamaritanClientHighlighter.ShowPlayer(player!, GoodSamaritanHighlightKind.Suspicious, seconds);
            }
        }
        catch (Exception ex)
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"Player question indicator patch failed: {ex.Message}");
        }
    }
}
