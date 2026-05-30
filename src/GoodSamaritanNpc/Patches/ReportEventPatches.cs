namespace GoodSamaritanNpc;

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
            GoodSamaritanManager.Instance?.NotifySuspiciousAction(
                pmm,
                ((Component)__instance).transform.position,
                GoodSamaritanText.Get(Msg.RevealingAction),
                SuspicionEventType.RevealingAction);
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
            if (!GoodSamaritanPlugin.Settings.ShouldDetectCivilianAttacks)
            {
                return;
            }

            var component = (Component)weapon;
            var pmm = component.GetComponent<PlayerModeManager>() ?? component.GetComponentInParent<PlayerModeManager>();
            GoodSamaritanManager.Instance?.NotifySuspiciousAction(pmm, component.transform.position, reason, SuspicionEventType.AttackingCivilian);
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
    private static bool Prefix(RaycastWeapon __instance, ref bool __state)
    {
        __state = false;
        try
        {
            var component = (Component)__instance;
            var pmm = component.GetComponent<PlayerModeManager>() ?? component.GetComponentInParent<PlayerModeManager>();
            if (GoodSamaritanManager.Instance == null || !GoodSamaritanManager.Instance.IsUndercover(pmm))
            {
                return true;
            }

            __state = true;
            if (GoodSamaritanPlugin.Settings.ShouldDetectCivilianAttacks)
            {
                GoodSamaritanManager.Instance.NotifySuspiciousAction(
                    pmm,
                    component.transform.position,
                    GoodSamaritanText.Get(Msg.AttackingCivilian),
                    SuspicionEventType.AttackingCivilian);
            }

            return false;
        }
        catch (Exception ex)
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"RaycastWeapon undercover jail bypass failed: {ex.Message}");
            return true;
        }
    }

    private static void Postfix(RaycastWeapon __instance, bool __state)
    {
        try
        {
            if (__state)
            {
                return;
            }

            if (!GoodSamaritanPlugin.Settings.ShouldDetectCivilianAttacks)
            {
                return;
            }

            var component = (Component)__instance;
            var pmm = component.GetComponent<PlayerModeManager>() ?? component.GetComponentInParent<PlayerModeManager>();
            GoodSamaritanManager.Instance?.NotifySuspiciousAction(
                pmm,
                component.transform.position,
                GoodSamaritanText.Get(Msg.AttackingCivilian),
                SuspicionEventType.AttackingCivilian);
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
            if (!GoodSamaritanPlugin.Settings.ShouldDetectCivilianAttacks)
            {
                return;
            }

            var component = (Component)__instance;
            var pmm = component.GetComponent<PlayerModeManager>() ?? component.GetComponentInParent<PlayerModeManager>();
            Vector3 position = GoodSamaritanManager.IsUnityNull(targetNpc)
                ? component.transform.position
                : ((Component)targetNpc).transform.position;
            GoodSamaritanManager.Instance?.NotifySuspiciousAction(
                pmm,
                position,
                GoodSamaritanText.Get(Msg.TacklingCivilian),
                SuspicionEventType.TacklingCivilian);
        }
        catch (Exception ex)
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"PlayerTackle NPC patch failed: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(Metater.MetaPlayer), nameof(Metater.MetaPlayer.UserCode_CmdJump__Quaternion))]
internal static class MetaPlayerJumpPatch
{
    private static void Postfix(Metater.MetaPlayer __instance)
    {
        try
        {
            if (!GoodSamaritanPlugin.Settings.ShouldDetectJumping)
            {
                return;
            }

            var component = (Component)__instance;
            var pmm = component.GetComponent<PlayerModeManager>() ?? component.GetComponentInParent<PlayerModeManager>();
            GoodSamaritanManager.Instance?.NotifySuspiciousAction(
                pmm,
                component.transform.position,
                GoodSamaritanText.Get(Msg.Jumping),
                SuspicionEventType.Jumping);
        }
        catch (Exception ex)
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"MetaPlayer jump patch failed: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(HeldItemInteractable), nameof(HeldItemInteractable.ServerOnPickedUp))]
internal static class HeldItemInteractablePickedUpPatch
{
    private static void Postfix(HeldItemInteractable __instance, Interactor interactor)
    {
        try
        {
            if (!GoodSamaritanPlugin.Settings.ShouldDetectContrabandPickup || !GoodSamaritanManager.IsSuspiciousItem(__instance) || GoodSamaritanManager.IsUnityNull(interactor))
            {
                return;
            }

            var component = (Component)interactor;
            var pmm = component.GetComponent<PlayerModeManager>() ?? component.GetComponentInParent<PlayerModeManager>();
            GoodSamaritanManager.Instance?.NotifySuspiciousAction(
                pmm,
                component.transform.position,
                GoodSamaritanText.Get(Msg.PickingContraband),
                SuspicionEventType.PickingContraband);
        }
        catch (Exception ex)
        {
            GoodSamaritanPlugin.LogSource.LogDebug($"Held item pickup patch failed: {ex.Message}");
        }
    }
}
