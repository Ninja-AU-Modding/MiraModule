using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Roles;
using MiraModule.Roles.Neutral;
using TownOfUs.Utilities;

namespace MiraModule.Patches;

/// <summary>
/// Applies all queued Shifter role-swaps at the END of a meeting (VotingComplete).
/// The swap happens regardless of whether either player is dead.
/// </summary>
[HarmonyPatch(typeof(GameManager))]
public static class ShifterMeetingPatch
{
    [RegisterEvent]
    public static void VotingCompleteHandler(VotingCompleteEvent @event)
    {
        if (ShifterRole.PendingShifts.Count == 0) return;
        if (!AmongUsClient.Instance.AmHost) return;

        var toProcess = ShifterRole.PendingShifts.ToList();
        ShifterRole.PendingShifts.Clear();

        foreach (var (shifterId, targetId) in toProcess)
        {
            var shifter = MiscUtils.PlayerById(shifterId);
            var target = MiscUtils.PlayerById(targetId);

            if (shifter == null || target == null)
            {
                Warning($"[Shifter] Skipping shift: shifter={shifterId} target={targetId} — player not found");
                continue;
            }

            // Capture roles BEFORE any changes
            var shifterRole = shifter.Data.Role.Role;
            var targetRole = target.Data.Role.Role;

            Info($"[Shifter] Swapping roles: {shifter.Data.PlayerName} ({shifterRole}) ↔ {target.Data.PlayerName} ({targetRole})");

            // Swap: shifter gets target's role, target gets Shifter's role (canOverrideRole=true)
            shifter.RpcSetRole(targetRole, true);
            target.RpcSetRole(shifterRole, true);
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.StartGame))]
    [HarmonyPostfix]
    public static void GameManager_StartGame_Postfix()
    {
        ShifterRole.PendingShifts.Clear();
    }
}
