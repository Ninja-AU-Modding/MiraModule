using System;
using System.Collections;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraOverloaded.Modifiers.ChaosTokens;
using MiraOverloaded.Modifiers.ChaosTokens.Effects;
using MiraOverloaded.Options.Modifiers;
using MiraOverloaded.Utilities;
using ModuleHelpers = MiraOverloaded.Utilities.Helpers;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Modules.Anims;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MiraOverloaded.Networking;

public static class ChaosTokensRpc
{
    private static int _revealsLeft = (int)OptionGroupSingleton<ChaosTokensOptions>.Instance.MaxRoleReveals;
    
    [MethodRpc((uint)MiraOverloadedRpc.ChaosTokenRoll)]
    public static void RpcChaosTokenRoll(this PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost) return; // Host runs the checks

        if (!player.TryGetModifier<ChaosTokenModifier>(out var chaosToken)) return;
        if (chaosToken.Tokens <= 0)
        {
            return;
        }
        
        while (true)
        {
            int random = Random.RandomRangeInt(0, Enum.GetValues<ChaosEffects>().Length);
            if (!ApplyEffect(player, (ChaosEffects)random))
            {
                break;
            }
        }

        RpcChaosTokenDecrease(player, 1);
    }
    
    [MethodRpc((uint)MiraOverloadedRpc.ChaosTokenGain)]
    public static void RpcChaosTokenIncrease(this PlayerControl player, int amount, bool showNotification = false)
    {
        if (!player.TryGetModifier<ChaosTokenModifier>(out var chaosTokenModifier))
        {
            player.AddModifier<ChaosTokenModifier>(amount, showNotification);
        }
        else
        {
            chaosTokenModifier.IncreaseTokens(amount, showNotification);
        }
    }

    public static void RpcChaosTokenDecrease(this PlayerControl player, int amount)
    {
        if (!player.TryGetModifier<ChaosTokenModifier>(out var chaosTokenModifier))
        {
            Error($"Cannot decrease tokens: player {player.Data.PlayerName} has no modifier");
            return;
        }
        
        chaosTokenModifier.DecreaseTokens(amount);
    }
    
    [MethodRpc((uint)MiraOverloadedRpc.SendAgentChat)]
    public static void RpcTokenPositionSwap(PlayerControl player, PlayerControl victim)
    {
        Vector3 pos1 = player.GetTruePosition();
        Vector3 pos2 = victim.GetTruePosition();
               
        TransporterRole.Transport(player, pos2);
        TransporterRole.Transport(victim, pos1);

        if (player.AmOwner)
        {
            ChaosTokensUtils.Notification("<b>You swapped positions with someone!</b>", showSpr: true);
        }
        
        if (victim.AmOwner)
        {
            ChaosTokensUtils.Notification("<b>Someone swapped positions with you!</b>", showSpr: true);
        }
    }
    
    public static void RpcTokenRoleSwap(PlayerControl player, PlayerControl victim)
    {
        if (player.AmOwner)
        {
            ChaosTokensUtils.Notification("<b>You swapped roles with someone!</b>", showSpr: true);
        }
        if (victim.AmOwner)
        {
            ChaosTokensUtils.Notification("<b>Someone swapped roles with you!</b>", showSpr: true);
        }
        
        RoleTypes role1 = player.Data.Role.Role;
        RoleTypes role2 = victim.Data.Role.Role;

        player.ChangeRole((ushort)role2);
        victim.ChangeRole((ushort)role1);
    }

    public static void RpcTokenRevive(PlayerControl source, PlayerControl target)
    {
        Coroutines.Start(CoRevivePlayer(target));

        if (target.AmOwner)
        {
            ChaosTokensUtils.Notification("<b>Someone revived you, you are back from the dead!</b>", showSpr: true);
            Coroutines.Start(MiscUtils.CoFlash(MiraOverloadedColors.ChaosTokens));
        }

        if (source.AmOwner)
        {
            ChaosTokensUtils.Notification("<b>You revived a random person!</b>", showSpr: true);
        }
    }

    internal static bool ApplyEffect(PlayerControl player, ChaosEffects effect)
    {
        bool reroll = false;
        void Reroll()
        {
            reroll = true;
        }
        
        var playerRole = player.Data.Role;
        switch (effect)
        {
            case ChaosEffects.Defense:
                if (player.HasModifier<TokenDefense>())
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenDefense>((float)Random.RandomRange(15, 60));
                break;
            case ChaosEffects.Speed:
                if (player.HasModifier<TokenSpeed>())
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenSpeed>(Random.RandomRange(1.25f, 3f));
                break;
            case ChaosEffects.Votes:
                if (player.HasModifier<TokenVotes>())
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenVotes>();
                break;
            case ChaosEffects.MoreTokens:
                player.RpcChaosTokenIncrease(Random.RandomRangeInt(1, 3), true);
                break;
            case ChaosEffects.Transparent:
                if (player.HasModifier<TokenTransparent>())
                {
                    Reroll();
                    break;
                }
                if (player.HasModifier<TokenInvisible>())
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenTransparent>(Random.RandomRange(0f, 0.8f));
                break;
            case ChaosEffects.LowerCooldown:
                if (player.HasModifier<TokenCooldown>())
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenCooldown>(Random.RandomRange(0.5f, 1f));
                break;
            case ChaosEffects.MediumIsReal:
                if (player.HasModifier<TokenMedium>())
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenMedium>();
                break;
            case ChaosEffects.KillButton:
                if (player.HasModifier<TokenOneTimeKill>())
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenOneTimeKill>();
                break;
            case ChaosEffects.Tasks:
                if (player.HasModifier<TokenTasks>())
                {
                    Reroll();
                    break;
                }
                
                if (ChaosTokensUtils.GetUncompletedTasks(player).Count == 0)
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenTasks>();
                break;
            case ChaosEffects.Vision:
                if (player.HasModifier<TokenVision>())
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenVision>(Random.RandomRange(1f, 2f));
                break;
            case ChaosEffects.Invisible:
                if (player.HasModifier<TokenInvisible>())
                {
                    Reroll();
                    break;
                }
                if (player.HasModifier<TokenTransparent>())
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenInvisible>();
                break;
            case ChaosEffects.Assassin:
                if (player.HasModifier<TokenAssassin>())
                {
                    Reroll();
                    break;
                }
                if (player.HasModifier<AssassinModifier>())
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenAssassin>();
                break;


            case ChaosEffects.RevealSelf:
                if (player.HasModifier<TokenReveal>())
                {
                    Reroll();
                    break;
                }
                if (_revealsLeft <= 0)
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenReveal>(player.Data.Role.Role, player.Data.PlayerId);
                _revealsLeft--;
                break;
            case ChaosEffects.Death:
                if (player.HasModifier<TokenDeath>())
                {
                    Reroll();
                    break;
                }

                if (OptionGroupSingleton<ChaosTokensOptions>.Instance.DeathDisabled)
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenDeath>();
                break;
            case ChaosEffects.Blackmailed:
                if (player.HasModifier<TokenBlackmail>() || player.HasModifier<BlackmailedModifier>())
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenBlackmail>(player.PlayerId);
                break;
            case ChaosEffects.Drunk:
                if (player.HasModifier<TokenDrunk>())
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenDrunk>();
                break;
            case ChaosEffects.FakeRevealSelf:
                if (player.HasModifier<TokenReveal>())
                {
                    Reroll();
                    break;
                }
                
                if (_revealsLeft <= 0)
                {
                    Reroll();
                    break;
                }

                var validRoles = CustomRoleManager.CustomMiraRoles
                    .Where(r => r.Team != player.GetTownOfUsRole()?.Team)
                    .Select(r => r as RoleBehaviour)
                    .OfType<RoleBehaviour>()
                    .Select(r => r.Role)
                    .ToList();
                if (validRoles.Count == 0)
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenReveal>(validRoles.Random(), player.Data.PlayerId);
                _revealsLeft--;
                break;
            case ChaosEffects.Hyperactive:
                if (player.HasModifier<TokenHyperactive>())
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenHyperactive>();
                break;
            case ChaosEffects.Colorblind:
                if (player.HasModifier<TokenColorblind>() ||
                    player.HasModifier<MiraOverloaded.Modifiers.Universal.ColorblindModifier>() ||
                    player.HasModifier<TokenNausea>())
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenColorblind>();
                break;

            case ChaosEffects.RevealRandom:
                if (_revealsLeft <= 0)
                {
                    Reroll();
                    break;
                }

                var revealVictims = ModuleHelpers.GetAlivePlayers()
                    .Where(x => x.PlayerId != player.PlayerId)
                    .Where(x => !x.HasModifier<MiraOverloaded.Modifiers.ChaosTokens.RevealModifier>())
                    .ToList();

                if (revealVictims.Count <= 0)
                {
                    Reroll();
                    break;
                }

                var revealVictim = revealVictims.Random();
                if (revealVictim == null)
                {
                    Reroll();
                    break;
                }

                revealVictim.RpcAddModifier<TokenReveal>(revealVictim.Data.Role.Role, player.PlayerId);
                _revealsLeft--;
                break;
            case ChaosEffects.PositionSwap:
                var swapVictim = ModuleHelpers.GetAlivePlayers()
                    .Where(x => !x.Data.IsDead)
                    .Where(x => x.PlayerId != player.PlayerId)
                    .Random();
                if (swapVictim == null)
                {
                    Reroll();
                    break;
                }

                RpcTokenPositionSwap(player, swapVictim);
                break;
            case ChaosEffects.RoleSwap:
                // This will be annoying as hell but the players have to be on the same team so it's kind of balanced
                ModdedRoleTeams playerTeam;
                if (playerRole is ICustomRole customRole)
                {
                    playerTeam = customRole.Team;
                }
                else
                {
                    playerTeam = (ModdedRoleTeams)playerRole.TeamType;
                }

                var roleSwapVictims = ModuleHelpers.GetAlivePlayers().Where(potentialVictim =>
                {
                    if (potentialVictim.PlayerId == player.PlayerId) return false;

                    var potentialVictimRole = potentialVictim.Data.Role;
                    if (potentialVictimRole is ICustomRole customVictimRole)
                    {
                        return customVictimRole.Team == playerTeam;
                    }

                    if (playerTeam == ModdedRoleTeams.Custom)
                        return false;

                    return potentialVictimRole.TeamType == (RoleTeamTypes)playerTeam;
                }).ToList();

                if (roleSwapVictims.Count <= 0)
                {
                    Reroll();
                    break;
                }

                var roleSwapVictim = roleSwapVictims.Random();
                if (roleSwapVictim == null)
                {
                    Reroll();
                    break;
                }

                RpcTokenRoleSwap(player, roleSwapVictim);
                break;
            case ChaosEffects.Revive:
                if (OptionGroupSingleton<ChaosTokensOptions>.Instance.ReviveDisabled)
                {
                    Reroll();
                    break;
                }
                
                var canBeRevived = PlayerControl.AllPlayerControls
                    .ToArray()
                    .Where(x => x.Data.IsDead && !x.Data.Disconnected)
                    .Where(x => x.GetModifier<DeathHandlerModifier>()?.RoundOfDeath == DeathEventHandlers.CurrentRound)
                    .ToList();

                if (canBeRevived.Count <= 0)
                {
                    Reroll();
                    break;
                }

                var reviveTarget = canBeRevived.Random();
                if (reviveTarget == null)
                {
                    Reroll();
                    break;
                }

                RpcTokenRevive(player, reviveTarget);
                break;
            case ChaosEffects.RandomModifier:
                if (player.HasModifier<TokenRandomModifier>())
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenRandomModifier>();
                break;
            case ChaosEffects.NoSkip:
                if (player.HasModifier<TokenNoSkip>())
                {
                    Reroll();
                    break;
                }

                player.RpcAddModifier<TokenNoSkip>();
                break;
            
            default:
                Reroll();
                break;
        }

        return reroll;
    }
    
    public static IEnumerator CoRevivePlayer(PlayerControl dead)
    {
        var roleWhenAlive = dead.GetRoleWhenAlive();

        var body = GameObject.FindObjectsOfType<DeadBody>()
            .FirstOrDefault(b => b.ParentId == dead.PlayerId);
        var reviveVent = ShipStatus.Instance.AllVents.Random();
        var position = reviveVent != null ? reviveVent.transform.position : dead.transform.position;

        if (body != null)
        {
            GameObject.Destroy(body.gameObject);
        }

        // Notify the killer about revival to make it a bit more fair
        var killerId = GameHistory.KilledPlayers.FirstOrDefault(x => x.VictimId == dead.PlayerId)?.KillerId;
        if (killerId.HasValue)
        {
            var killer = MiscUtils.PlayerById(killerId.Value);
            if (killer != null && killer.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Altruist));
            }
        }

        yield return new WaitForSeconds(1);

        if (!MeetingHud.Instance)
        {
            GameHistory.ClearMurder(dead);

            dead.Revive();

            dead.transform.position = new Vector2(position.x, position.y);
            if (dead.AmOwner)
            {
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(position.x, position.y));
            }

            if (ModCompatibility.IsSubmerged() && PlayerControl.LocalPlayer.PlayerId == dead.PlayerId)
            {
                ModCompatibility.ChangeFloor(dead.transform.position.y > -7);
            }

            if (dead.AmOwner && !dead.HasModifier<LoverModifier>())
            {
                HudManager.Instance.Chat.gameObject.SetActive(false);
            }

            // return player from ghost role back to what they were when alive
            dead.ChangeRole((ushort)roleWhenAlive!.Role, false);

            if (dead.Data.Role is IAnimated animated)
            {
                animated.IsVisible = true;
                animated.SetVisible();
            }

            foreach (var button in CustomButtonManager.Buttons.Where(x => x.Enabled(dead.Data.Role))
                         .OfType<IAnimated>())
            {
                button.IsVisible = true;
                button.SetVisible();
            }

            foreach (var modifier in dead.GetModifiers<GameModifier>().Where(x => x is IAnimated))
            {
                if (modifier is IAnimated animatedMod)
                {
                    animatedMod.IsVisible = true;
                    animatedMod.SetVisible();
                }
            }

            dead.RemainingEmergencies = 0;
        }

        yield return null;
    }
}
