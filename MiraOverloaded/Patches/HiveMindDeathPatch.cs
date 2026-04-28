using HarmonyLib;
using JetBrains.Annotations;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
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
using System.Linq;
using UnityEngine;

namespace MiraOverloaded.Patches;

[HarmonyPatch]
public static class HiveMindDeathPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
    public static void Postfix(PlayerControl __instance, DeathReason reason)
    {
        if (__instance.Data.Role is not HiveMindRole) return;
        if (__instance.AmOwner)
            HiveMindEvents.CleanupKillOverlay();
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
        var extraMessage = opts.HiveMindKnows ? "" : ", who was a part of the hive,";
        ChaosTokensUtils.Notification($"<b><#FF0000>{sender.name}{extraMessage} has died!</color></b>");
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
        var killerName = opts.HiveMindKnows ? sender.name : "A member of the hive";
        ChaosTokensUtils.Notification($"<b><#DDB244>{killerName} has made a kill!</color></b>");
    }

    [MethodRpc((uint)MiraOverloadedRpc.HiveMindLinkedCooldown)]
    public static void RpcHiveMindLinkedCooldown(PlayerControl sender, float cooldown)
    {
        if (sender.AmOwner) return;
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return;
        if (localPlayer.Data.Role is not HiveMindRole) return;
        HiveMindEvents.SetSharedCooldown(localPlayer, cooldown);
    }

    [MethodRpc((uint)MiraOverloadedRpc.HiveMindAwaken)]
    public static void RpcHiveMindAwaken(PlayerControl hiveMind)
    {
        hiveMind.RemoveModifier<HiveMindAwakenModifier>();

        foreach (var player in PlayerControl.AllPlayerControls.ToArray()
                     .Where(p => p != null && p.HasModifier<LinkedMindModifier>()))
        {
            player.ChangeRole(RoleId.Get<HiveMindRole>());
            player.RemoveModifier<LinkedMindModifier>();
        }

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || hiveMind.AmOwner) return;

        if (localPlayer.Data.Role is not HiveMindRole) return;

        var opts = OptionGroupSingleton<HiveMindOptions>.Instance;

        var nameDisplay = opts.HiveMindKnows ? hiveMind.name : "The main mind";

        if (HasLos(localPlayer, hiveMind))
        {
            SoundManager.Instance.PlaySound(MiraOverloadedAudio.HiveMind.LoadAsset(), false, 1f);
            ChaosTokensUtils.Notification(
                $"<b><#FF0000>⚠ {nameDisplay} has awakened the Hive Mind!</color></b>");
        }
    }

    [MethodRpc((uint)MiraOverloadedRpc.HiveMindTimeout)]
    public static void RpcHiveMindTimeout(PlayerControl hiveMind)
    {
        if (hiveMind.AmOwner) return;

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return;

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
        ChaosTokensUtils.Notification($"<b><#FF0000>⚠ {nameDisplay} failed to awaken the Hive Mind in time!</color></b>");
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