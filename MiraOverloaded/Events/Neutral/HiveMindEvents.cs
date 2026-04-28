using System.Collections;
using System.Linq;
using MiraAPI.Networking;
using MiraOverloaded.Modifiers;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraOverloaded.Options.Roles.Neutral;
using MiraOverloaded.Patches;
using MiraOverloaded.Roles.Neutral;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Game.Alliance;
using MiraOverloaded.Assets;
using UnityEngine;
using MiraOverloaded.Modifiers.Universal;
using ChaosTokensUtils = MiraOverloaded.Utilities.ChaosTokensUtils;
using ChaosTokensAssets = MiraOverloaded.Utilities.ChaosTokensAssets;
using MiraOverloaded.Buttons.Neutral;
using Object = UnityEngine.Object;

namespace MiraOverloaded.Events;

public static class HiveMindEvents
{
    private static SpriteRenderer? _killFilterRenderer;
    private static SpriteRenderer? _killDarknessRenderer;
    private static float? _cachedSpeedBeforeEffect;
    private static int _effectVersion;

    private static void DestroyKillOverlay()
    {
        if (_killFilterRenderer != null)
        {
            if (_killFilterRenderer.material != null)
                Object.Destroy(_killFilterRenderer.material);
            Object.Destroy(_killFilterRenderer.gameObject);
            _killFilterRenderer = null;
        }
        if (_killDarknessRenderer != null)
        {
            Object.Destroy(_killDarknessRenderer.gameObject);
            _killDarknessRenderer = null;
        }
    }

    public static void CleanupKillOverlay()
    {
        _effectVersion++;
        DestroyKillOverlay();
    }

    public static void ResetTimedDebuffs(PlayerControl? player = null)
    {
        _effectVersion++;

        var effectPlayer = player ?? PlayerControl.LocalPlayer;
        if (_cachedSpeedBeforeEffect.HasValue && effectPlayer != null && !effectPlayer.HasDied())
            effectPlayer.MyPhysics.Speed = _cachedSpeedBeforeEffect.Value;

        _cachedSpeedBeforeEffect = null;
        DestroyKillOverlay();
    }

    public static void SetSharedCooldown(PlayerControl player, float cooldown)
    {
        player.killTimer = cooldown;
        if (player.AmOwner)
            CustomButtonSingleton<HiveMindKillButton>.Instance?.SetTimer(cooldown);
    }

    public static void AddSharedCooldown(PlayerControl player, float amount)
    {
        player.killTimer = Mathf.Max(player.killTimer, 0f) + amount;
        if (player.AmOwner && CustomButtonSingleton<HiveMindKillButton>.Instance is { } button)
            button.SetTimer(Mathf.Max(button.Timer, 0f) + amount);
    }

    public static void ApplyTimedDebuffs(PlayerControl player, HiveMindOptions opts)
    {
        if (player.HasDied()) return;

        if (opts.SlowdownAfterKill > 0f)
        {
            _cachedSpeedBeforeEffect ??= player.MyPhysics.Speed;
            player.MyPhysics.Speed = _cachedSpeedBeforeEffect.Value * (1f - opts.SlowdownAfterKill);
        }

        if (opts.ScreenDarkenAfterKill)
        {
            DestroyKillOverlay();
            _killFilterRenderer = ChaosTokensUtils.CreatePostprocessFilter(
                Object.Instantiate(ChaosTokensAssets.ColorblindMaterial.LoadAsset()));
            _killDarknessRenderer = ChaosTokensUtils.CreateScreenOverlay(
                "HiveMindKillDarken", new Color(0f, 0f, 0f, 0.8f));
        }

        if (opts.EffectDuration > 0f && (opts.SlowdownAfterKill > 0f || opts.ScreenDarkenAfterKill))
        {
            _effectVersion++;
            Coroutines.Start(CoEffectTimer(opts.EffectDuration, _effectVersion));
        }
    }

    private static IEnumerator CoEffectTimer(float duration, int version)
    {
        yield return new WaitForSeconds(duration);
        if (_effectVersion != version) yield break;
        ResetTimedDebuffs();
    }

    public static void ApplyAwakenDebuffs(PlayerControl player, HiveMindOptions opts)
    {
        if (player.HasDied()) return;

        float slowdown = opts.SlowdownAfterKill > 0f ? opts.SlowdownAfterKill : 0.15f;
        _cachedSpeedBeforeEffect ??= player.MyPhysics.Speed;
        player.MyPhysics.Speed = _cachedSpeedBeforeEffect.Value * (1f - slowdown);

        DestroyKillOverlay();
        _killFilterRenderer = ChaosTokensUtils.CreatePostprocessFilter(
            Object.Instantiate(ChaosTokensAssets.ColorblindMaterial.LoadAsset()));
        _killDarknessRenderer = ChaosTokensUtils.CreateScreenOverlay(
            "HiveMindAwakenDarken", new Color(0f, 0f, 0f, 0.8f));

        float duration = opts.EffectDuration > 0f ? opts.EffectDuration : 5f;
        _effectVersion++;
        Coroutines.Start(CoEffectTimer(duration, _effectVersion));
    }

    [RegisterEvent]
    public static void OnRoundStartHost(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro || !AmongUsClient.Instance.AmHost) return;

        var startingHiveMinds = Helpers.GetAlivePlayers()
            .Where(player => player.Data.Role is HiveMindRole)
            .ToList();
        if (startingHiveMinds.Count == 0) return;

        var otherMinds = Mathf.RoundToInt(OptionGroupSingleton<HiveMindOptions>.Instance.OtherConnectedMinds);
        var eligible = Helpers.GetAlivePlayers()
            .Where(p =>
            {
                if (p.Data.Role.IsImpostor) return false;
                if (p.HasModifier<EgotistModifier>() || p.HasModifier<CrewpostorModifier>()) return false;
                if (p.Data.Role is ICustomRole customRole)
                {
                    if (customRole.Team == ModdedRoleTeams.Impostor) return false;
                    if (customRole is HiveMindRole) return false;
                }
                return true;
            })
            .ToList();

        for (var i = 0; i < otherMinds && eligible.Count > 0; i++)
        {
            var person = eligible.Random();
            if (person == null) break;
            eligible.Remove(person);
            person.RpcAddModifier<LinkedMindModifier>();
        }
    }

    [RegisterEvent]
    public static void OnIntroEnd(IntroEndEvent @event)
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || localPlayer.Data.Role is not HiveMindRole) return;

        localPlayer.RpcAddModifier<HiveMindAwakenModifier>();
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro) return;
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || localPlayer.Data.Role is not HiveMindRole) return;

        var opts = OptionGroupSingleton<HiveMindOptions>.Instance;
        SetSharedCooldown(localPlayer, opts.KillCooldown);
        ResetTimedDebuffs(localPlayer);
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent @event)
    {
        if (@event.Source.Data.Role is not HiveMindRole) return;
        if (!@event.Source.AmOwner) return;

        var opts = OptionGroupSingleton<HiveMindOptions>.Instance;

        HiveMindDeathPatch.RpcHiveMindKill(@event.Source);

        SetSharedCooldown(@event.Source, opts.KillCooldown);

        if (opts.CooldownsLinked)
            HiveMindDeathPatch.RpcHiveMindLinkedCooldown(@event.Source, opts.KillCooldown);
    }
}
