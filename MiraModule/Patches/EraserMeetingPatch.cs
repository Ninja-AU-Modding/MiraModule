using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraModule.Options.Roles.Impostor;
using MiraModule.Roles.Impostor;
using TownOfUs.Networking;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace MiraModule.Patches;

/// <summary>
/// Applies all queued Eraser erases at the start of a meeting.
/// The erase happens regardless of whether the Eraser or the target died beforehand.
/// </summary>
[HarmonyPatch(typeof(MeetingHud))]
public static class EraserMeetingPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.Start))]
    public static void Start_Postfix()
    {
        if (EraserRole.PendingErases.Count == 0) return;
        if (!AmongUsClient.Instance.AmHost) return;

        var opts = OptionGroupSingleton<EraserOptions>.Instance;
        var toProcess = EraserRole.PendingErases.ToList();

        foreach (var (victimId, _) in toProcess)
        {
            var victim = MiscUtils.PlayerById(victimId);
            if (victim == null)
            {
                EraserRole.PendingErases.Remove(victimId);
                continue;
            }

            var currentRole = victim.Data.Role;

            // Determine if the target is a neutral (non-impostor, non-crewmate modded role or vanilla neutral)
            bool isNeutral = false;
            if (currentRole is ICustomRole customRole)
            {
                isNeutral = customRole.Team == ModdedRoleTeams.Custom;
            }

            if (isNeutral && opts.NeutralOutcome == NeutralEraseOutcome.Die)
            {
                // Kill the neutral player outright
                var killer = MiscUtils.PlayerById(EraserRole.PendingErases[victimId]);
                (killer ?? victim).RpcSpecialMurder(
                    victim,
                    createDeadBody: true,
                    teleportMurderer: false,
                    showKillAnim: false,
                    causeOfDeath: "Erased");
            }
            else
            {
                // Wipe the role — assign crewmate
                victim.RpcSetRole(RoleTypes.Crewmate, false);
            }

            EraserRole.ErasedPlayerIds.Add(victimId);
            EraserRole.PendingErases.Remove(victimId);

            Info($"[Eraser] Erased {victim.Data.PlayerName} (id={victimId}), isNeutral={isNeutral}, outcome={opts.NeutralOutcome}");
        }
    }

    /// <summary>
    /// Clear all static state when a new game starts.
    /// </summary>
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.StartGame))]
    [HarmonyPostfix]
    public static void GameManager_StartGame_Postfix()
    {
        EraserRole.PendingErases.Clear();
        EraserRole.ErasedPlayerIds.Clear();
    }
}
