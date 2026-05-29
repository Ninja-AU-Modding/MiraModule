using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraOverloaded.Assets;
using MiraOverloaded.Events;
using MiraOverloaded.Modifiers;
using MiraOverloaded.Modifiers.Universal;
using MiraOverloaded.Options.Roles.Neutral;
using MiraOverloaded.Roles.Neutral;
using MiraOverloaded.Utilities;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace MiraOverloaded.Patches;

[HarmonyPatch]
public static class HiveMindDeathPatch
{
    public static bool SuppressNextDeathBroadcast;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
    public static void Postfix(PlayerControl __instance, DeathReason reason)
    {
        if (__instance.Data.Role is not HiveMindRole) return;
        if (!__instance.AmOwner) return;
        HiveMindEvents.CleanupKillOverlay();
        if (SuppressNextDeathBroadcast)
        {
            SuppressNextDeathBroadcast = false;
            return;
        }
        RpcHiveMindDeath(__instance);
    }

    [MethodRpc((uint)MiraOverloadedRpc.HiveMindDeath)]
    public static void RpcHiveMindDeath(PlayerControl sender)
    {
        if (sender.AmOwner) return;

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return;
        if (localPlayer.Data.Role is not HiveMindRole) return;

        var opts = OptionGroupSingleton<HiveMindOptions>.Instance;

        HiveMindEvents.AddSharedCooldown(localPlayer, opts.DeathCooldownAddition);
        HiveMindEvents.ApplyTimedDebuffs(localPlayer, opts);

        if (!opts.KnowsDeath) return;

        SoundManager.Instance.PlaySound(MiraOverloadedAudio.HiveMind.LoadAsset(), false, 1f);
        var deathLine = opts.HiveMindKnows
            ? MiraOverloadedLocale.GetString("MiraHiveMindNotificationDeath", "[player] has died!").Replace("<player>",sender.name)
            : MiraOverloadedLocale.GetString("MiraHiveMindNotificationDeathAnon", "A member of the hive has died!");
        ChaosTokensUtils.Notification($"<b><#FF0000>{deathLine}</color></b>");
    }

    [MethodRpc((uint)MiraOverloadedRpc.HiveMindKill)]
    public static void RpcHiveMindKill(PlayerControl sender)
    {
        if (sender.AmOwner) return;
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return;
        if (localPlayer.Data.Role is not HiveMindRole) return;

        var opts = OptionGroupSingleton<HiveMindOptions>.Instance;
        HiveMindEvents.ApplyTimedDebuffs(localPlayer, opts);

        if (!opts.KnowsDeath) return;

        SoundManager.Instance.PlaySound(MiraOverloadedAudio.HiveMind.LoadAsset(), false, 1f);
        var killLine = opts.HiveMindKnows
            ? MiraOverloadedLocale.GetString("MiraHiveMindNotificationKill", "[player] has made a kill!").Replace("<player>",sender.name)
            : MiraOverloadedLocale.GetString("MiraHiveMindNotificationKillAnon", "A member of the hive has made a kill!");
        ChaosTokensUtils.Notification($"<b><#DDB244>{killLine}</color></b>");
    }

    [MethodRpc((uint)MiraOverloadedRpc.HiveMindLinkedCooldown)]
    public static void RpcHiveMindLinkedCooldown(PlayerControl sender, float cooldown)
    {
        if (sender.AmOwner) return;
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return;
        if (localPlayer.Data?.Role is not HiveMindRole &&
            !localPlayer.HasModifier<LinkedMindModifier>()) return;
        HiveMindEvents.SetSharedCooldown(localPlayer, cooldown);
    }

    [MethodRpc((uint)MiraOverloadedRpc.HiveMindAwaken)]
    public static void RpcHiveMindAwaken(PlayerControl hiveMind)
    {
        var localPlayer = PlayerControl.LocalPlayer;
        var localWasAlreadyHiveMind = localPlayer != null && localPlayer.Data?.Role is HiveMindRole;

        if (!hiveMind.HasModifier<HiveMindAwakenModifier>()) return;

        hiveMind.RemoveModifier<HiveMindAwakenModifier>();

        foreach (var player in PlayerControl.AllPlayerControls.ToArray()
                     .Where(p => p != null && p.HasModifier<LinkedMindModifier>()))
        {
            player.ChangeRole(RoleId.Get<HiveMindRole>());
            player.RemoveModifier<LinkedMindModifier>();
        }

        if (localPlayer == null || hiveMind.AmOwner) return;
        if (!localWasAlreadyHiveMind) return;

        var opts = OptionGroupSingleton<HiveMindOptions>.Instance;

        var nameDisplay = opts.HiveMindKnows ? hiveMind.name : "The main mind";

        if (HasLos(localPlayer, hiveMind))
        {
            SoundManager.Instance.PlaySound(MiraOverloadedAudio.HiveMind.LoadAsset(), false, 1f);
            ChaosTokensUtils.Notification(
                $"<b><#FF0000>{MiraOverloadedLocale.GetString("MiraHiveMindNotificationAwakened", "⚠ [player] has awakened the Hive Mind!").Replace("<player>",nameDisplay)}</color></b>");
        }
    }

    [MethodRpc((uint)MiraOverloadedRpc.HiveMindTimeout)]
    public static void RpcHiveMindTimeout(PlayerControl hiveMind)
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return;

        if (hiveMind.AmOwner)
        {
            SoundManager.Instance.PlaySound(MiraOverloadedAudio.HiveMind.LoadAsset(), false, 1f);
            ChaosTokensUtils.Notification($"<b><#FF0000>{MiraOverloadedLocale.GetString("MiraHiveMindNotificationTimeoutSelf", "⚠ You failed to awaken the Hive Mind in time!")}</color></b>");
            return;
        }

        bool wasLinked = localPlayer.HasModifier<LinkedMindModifier>();
        if (wasLinked)
        {
            localPlayer.RemoveModifier<LinkedMindModifier>();
        }

        if (!HasLos(localPlayer, hiveMind)) return;

        var opts = OptionGroupSingleton<HiveMindOptions>.Instance;
        string nameDisplay;

        if (localPlayer.Data.Role is HiveMindRole || wasLinked)
        {
            nameDisplay = opts.HiveMindKnows ? hiveMind.name : "The main mind";
        }
        else
        {
            nameDisplay = "Someone";
        }

        SoundManager.Instance.PlaySound(MiraOverloadedAudio.HiveMind.LoadAsset(), false, 1f);
        ChaosTokensUtils.Notification($"<b><#FF0000>{MiraOverloadedLocale.GetString("MiraHiveMindNotificationTimeout", "⚠ [player] failed to awaken the Hive Mind in time!").Replace("<player>",nameDisplay)}</color></b>");
    }

    private static bool HasLos(PlayerControl viewer, PlayerControl target)
    {
        var hit = Physics2D.Linecast(
            viewer.GetTruePosition(),
            target.GetTruePosition(),
            LayerMask.GetMask("Ship"));
        return hit.collider == null;
    }
}
