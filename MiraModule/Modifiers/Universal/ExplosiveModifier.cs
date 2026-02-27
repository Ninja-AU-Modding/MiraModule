using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Options.Modifiers;
using MiraModule.Options.Modifiers.Universal;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modifiers.Game;
using UnityEngine;

namespace MiraModule.Modifiers.Universal;

/// <summary>
/// Explosive modifier: when this player dies, they explode and kill nearby living players.
/// </summary>
public sealed class ExplosiveModifier : UniversalGameModifier
{
    public override string LocaleKey => "Explosive";
    public override string ModifierName => "Explosive";
    public override ModifierFaction FactionType => ModifierFaction.UniversalPostmortem;
    public override Color FreeplayFileColor => new Color32(255, 120, 20, 255);
    public override LoadableAsset<Sprite>? ModifierIcon => ModifierIcons.Explosive;

    private bool _exploded;

    public override string GetDescription() =>
        "You will explode upon death, killing nearby players!";

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<ExplosiveModifierOptions>.Instance.ExplosiveChance;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<ExplosiveModifierOptions>.Instance.ExplosiveAmount;

    public override void OnActivate()
    {
        _exploded = false;
    }

    public override void OnMeetingStart()
    {
        // Reset between rounds so the modifier stays but doesn't double-trigger
        _exploded = false;
    }

    /// <summary>
    /// Fires on every client when the player dies (via MiraAPI's PlayerControlPatches.PlayerControlDiePostfix).
    /// We only want the HOST to execute the actual kills so we don't get duplicate murders.
    /// </summary>
    public override void OnDeath(DeathReason reason)
    {
        if (_exploded) return;
        if (!AmongUsClient.Instance.AmHost) return;
        if (MeetingHud.Instance) return;  // No explosions during meetings

        _exploded = true;
        Coroutines.Start(CoExplode());
    }

    private System.Collections.IEnumerator CoExplode()
    {
        var opts = OptionGroupSingleton<ExplosiveOptions>.Instance;
        float killDistance = opts.KillDistance;
        float duration = opts.ExplosiveDuration;
        int maxKills = (int)opts.MaxKills;

        // Announce the explosion to all clients
        RpcAnnounceExplosion(Player);

        // Brief delay so the death animation can play first
        yield return new UnityEngine.WaitForSeconds(0.3f);

        var bodyPos = (Vector2)Player.transform.position;
        var killed = 0;

        // Get all living players within the kill radius, sorted nearest-first
        var nearby = Helpers.GetClosestPlayers(bodyPos, killDistance)
            .Where(p => !p.Data.IsDead && !p.Data.Disconnected && p.PlayerId != Player.PlayerId)
            .ToList();

        foreach (var target in nearby)
        {
            if (killed >= maxKills) break;

            // Small stagger between each kill so Among Us doesn't drop packets
            yield return new UnityEngine.WaitForSeconds(0.05f);

            target.RpcMurderPlayer(target, true);
            killed++;
        }
    }

    // ── RPC: show a chat message + screen flash on every client ──────────
    [MethodRpc((uint)MiraModuleRpc.ExplosiveExplode)]
    public static void RpcAnnounceExplosion(PlayerControl player)
    {
        if (!player) return;

        var name = player.Data.PlayerName;
        TownOfUs.Utilities.MiscUtils.AddFakeChat(
            player.Data,
            $"<color=#FF7814>{name} (Explosive)</color>",
            $"{name} exploded, taking nearby players with them!",
            showHeadsup: true);

        // Brief orange flash on everyone's screen
        if (HudManager.InstanceExists)
        {
            Coroutines.Start(
                TownOfUs.Utilities.MiscUtils.CoFlash(new Color(1f, 0.47f, 0.08f), 0.4f, 0.35f));
        }
    }
}
