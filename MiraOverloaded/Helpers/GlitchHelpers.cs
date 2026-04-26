using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace MiraOverloaded.Utilities;

public static class Helpers
{
    public static PlayerControl? GetPlayerById(byte id)
    {
        return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.PlayerId == id);
    }

    public static PlayerControl? GetPlayerByName(string name)
    {
        return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.Data.PlayerName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public static PlayerControl LocalPlayer()
    {
        return PlayerControl.LocalPlayer;
    }

    public static bool AmHost()
    {
        return AmongUsClient.Instance.AmHost;
    }

    public static PlayerControl? GetHost()
    {
        return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.AmOwner);
    }

    public static List<PlayerControl> GetAllPlayers(bool includeDead = true)
    {
        var players = PlayerControl.AllPlayerControls.ToArray().ToList();
        return includeDead ? players : players.Where(p => !p.Data.IsDead).ToList();
    }

    public static List<PlayerControl> GetAlivePlayers()
    {
        return PlayerControl.AllPlayerControls.ToArray().Where(p => !p.Data.IsDead).ToList();
    }

    public static List<PlayerControl> GetDeadPlayers()
    {
        return PlayerControl.AllPlayerControls.ToArray().Where(p => p.Data.IsDead).ToList();
    }

    public static void Kill(PlayerControl source, PlayerControl target)
    {
        if (AmHost())
        {
            source.RpcMurderPlayer(target, true);
        }
    }

    public static void Murder(PlayerControl target)
    {
        if (AmHost())
        {
            target.RpcMurderPlayer(target, true);
        }
    }

    public static void Suicide(PlayerControl player)
    {
        if (AmHost())
        {
            player.RpcMurderPlayer(player, true);
        }
    }

    public static void SnapTo(PlayerControl player, Vector2 position)
    {
        player.NetTransform.SnapTo(position);
    }

    public static void TeleportTo(PlayerControl player, Vector2 position)
    {
        player.transform.position = (Vector3)position;
        player.NetTransform.SnapTo(position);
    }

    public static void TeleportToPlayer(PlayerControl source, PlayerControl target)
    {
        source.transform.position = target.transform.position;
        source.NetTransform.SnapTo((Vector2)target.transform.position);
    }

    public static float GetDistance(Vector2 pos1, Vector2 pos2)
    {
        return Vector2.Distance(pos1, pos2);
    }

    public static PlayerControl? GetClosestPlayer(Vector2 position, float maxDistance = float.MaxValue, bool includeDead = false)
    {
        return PlayerControl.AllPlayerControls.ToArray()
            .Where(p => (includeDead || !p.Data.IsDead) && p.PlayerId != LocalPlayer().PlayerId)
            .OrderBy(p => GetDistance(position, p.GetTruePosition()))
            .FirstOrDefault(p => GetDistance(position, p.GetTruePosition()) <= maxDistance);
    }

    public static PlayerControl? GetClosestLivingPlayer(Vector2 position, float maxDistance = float.MaxValue)
    {
        return GetClosestPlayer(position, maxDistance, false);
    }

    public static Il2CppSystem.Collections.Generic.List<PlayerControl> GetClosestPlayers(Vector2 position, float radius, bool includeDead)
    {
        var list = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!includeDead && player.Data.IsDead) continue;
            if (Vector2.Distance(position, player.GetTruePosition()) <= radius)
            {
                list.Add(player);
            }
        }
        return list;
    }

    public static bool IsImpostor(PlayerControl player)
    {
        return player.Data.Role.IsImpostor;
    }

    public static bool IsCrewmate(PlayerControl player)
    {
        return !player.Data.Role.IsImpostor;
    }

    public static bool IsDead(PlayerControl player)
    {
        return player.Data.IsDead;
    }

    public static bool IsGhost(PlayerControl player)
    {
        return player.Data.IsDead;
    }

    public static T? GetRole<T>(PlayerControl player) where T : RoleBehaviour
    {
        return player.GetRole<T>();
    }

    public static bool IsRole<T>(PlayerControl player) where T : RoleBehaviour
    {
        return player.IsRole<T>();
    }

    public static void SetNameColor(PlayerControl player, Color color)
    {
        if (player.cosmetics.nameText != null)
        {
            player.cosmetics.nameText.color = color;
        }
    }

    public static void ResetNameColor(PlayerControl player)
    {
        if (player.cosmetics.nameText != null)
        {
            player.cosmetics.nameText.color = Color.white;
        }
    }

    public static void SetVisibility(PlayerControl player, bool visible)
    {
        player.Visible = visible;
    }

    public static void Vanish(PlayerControl player)
    {
        player.Visible = false;
    }

    public static void SetScale(PlayerControl player, float scale)
    {
        player.transform.localScale = new Vector3(scale, scale, 1f);
    }

    public static void ResetScale(PlayerControl player)
    {
        player.transform.localScale = Vector3.one;
    }

    //commented for now, since uhh im tired right now and dont wanna deal with the errors

    // public static void TriggerWin(GameOverReason reason)
    // {
    //     if (AmHost())
    //     {
    //         ShipStatus.Instance.EndGame(reason, false);
    //     }
    // }

    // public static void EndGame()
    // {
    //     if (AmHost())
    //     {
    //         ShipStatus.Instance.EndGame(GameOverReason.Impostors, false);
    //     }
    // }

    public static void StartMeeting(PlayerControl reporter, PlayerControl? body = null)
    {
        reporter.RpcStartMeeting(body?.Data);
    }

    public static void Exile(PlayerControl player)
    {
        if (AmHost())
        {
            player.Exiled();
        }
    }

    public static void CompleteAllTasks(PlayerControl player)
    {
        foreach (var task in player.myTasks)
        {
            player.RpcCompleteTask(task.Id);
        }
    }

    public static void ClearTasks(PlayerControl player)
    {
        player.myTasks.Clear();
    }

    public static void EnterVent(PlayerControl player, int ventId)
    {
        var vent = ShipStatus.Instance.AllVents.FirstOrDefault(v => v.Id == ventId);
        if (vent != null)
        {
            player.MyPhysics.RpcEnterVent(ventId);
        }
    }

    public static void ExitVent(PlayerControl player, int ventId)
    {
        player.MyPhysics.RpcExitVent(ventId);
    }

    public static Vent? GetClosestVent(Vector2 position)
    {
        return ShipStatus.Instance.AllVents.ToArray()
            .OrderBy(v => GetDistance(position, (Vector2)v.transform.position))
            .FirstOrDefault();
    }

    public static void SnapToClosestVent(PlayerControl player)
    {
        var vent = GetClosestVent(player.GetTruePosition());
        if (vent != null)
        {
            TeleportTo(player, (Vector2)vent.transform.position);
        }
    }

    public static void SetSpeed(PlayerControl player, float speed)
    {
        player.MyPhysics.Speed = speed;
    }

    public static void ResetSpeed(PlayerControl player)
    {
        player.MyPhysics.Speed = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.PlayerSpeedMod);
    }

    public static void SetCooldown(PlayerControl player, float seconds)
    {
        player.killTimer = seconds;
    }

    public static void ResetCooldowns(PlayerControl player)
    {
        player.killTimer = 0f;
    }

    public static int GetRemainingTasks(PlayerControl player)
    {
        return player.myTasks.ToArray().Count(t => !t.IsComplete);
    }

    public static int GetTotalTasks(PlayerControl player)
    {
        return player.myTasks.Count;
    }

    public static List<PlayerControl> GetAllImpostors()
    {
        return PlayerControl.AllPlayerControls.ToArray().Where(p => p.Data.Role.IsImpostor).ToList();
    }

    public static List<PlayerControl> GetAllCrewmates()
    {
        return PlayerControl.AllPlayerControls.ToArray().Where(p => !p.Data.Role.IsImpostor).ToList();
    }

    public static PlayerControl? GetRandomPlayer(bool includeDead = false)
    {
        var players = GetAllPlayers(includeDead);
        if (players.Count == 0) return null;
        return players[UnityEngine.Random.Range(0, players.Count)];
    }

    public static PlayerControl? GetRandomAlivePlayer()
    {
        return GetRandomPlayer(false);
    }

    public static void FlashColor(PlayerControl player, Color color, float duration)
    {
        if (player.GetBodySprite() != null)
        {
            player.GetBodySprite().material.SetColor("_VisorColor", color);
        }
    }

    public static void SetOutlineColor(PlayerControl player, Color color)
    {
        if (player.GetBodySprite() != null)
        {
            player.GetBodySprite().material.SetColor("_OutlineColor", color);
        }
    }

    public static void DelayedKill(PlayerControl source, PlayerControl target, float delay)
    {
        Coroutines.Start(CoDelayedKill(source, target, delay));
    }

    private static System.Collections.IEnumerator CoDelayedKill(PlayerControl source, PlayerControl target, float delay)
    {
        yield return new WaitForSeconds(delay);
        Kill(source, target);
    }

    public static void CompleteTaskById(PlayerControl player, byte taskId)
    {
        player.RpcCompleteTask(taskId);
    }

    public static void CompleteRandomTask(PlayerControl player)
    {
        var tasks = player.myTasks.ToArray().Where(t => !t.IsComplete).ToList();
        if (tasks.Count > 0)
        {
            var task = tasks[UnityEngine.Random.Range(0, tasks.Count)];
            player.RpcCompleteTask(task.Id);
        }
    }

    public static bool HasTasksLeft(PlayerControl player)
    {
        return player.myTasks.ToArray().Any(t => !t.IsComplete);
    }

    public static List<PlayerTask> GetTasks(PlayerControl player)
    {
        return player.myTasks.ToArray().ToList();
    }

    public static SpriteRenderer GetBodySprite(this PlayerControl player)
    {
        return player.cosmetics.currentBodySprite.BodySprite;
    }

    public static TMPro.TextMeshPro GetNameText(this PlayerControl player)
    {
        return player.cosmetics.nameText;
    }
}
