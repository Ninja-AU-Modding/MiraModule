using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraOverloaded.Modifiers.Universal;
using MiraOverloaded.Options.Roles.Impostor;
using MiraOverloaded.Roles.Impostor;

namespace MiraOverloaded.Patches;

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

        // Snapshot the list so we can safely remove while iterating
        var toProcess = EraserRole.PendingErases.ToList();
        EraserRole.PendingErases.Clear();

        foreach (var (victimId, _) in toProcess)
        {
            var victim = MiscUtils.PlayerById(victimId);
            if (victim == null) continue;

            var currentRole = victim.Data.Role;

            // Check if the target is a neutral modded role
            bool isNeutral = currentRole is ICustomRole customRole &&
                             customRole.Team == ModdedRoleTeams.Custom;

            victim.RpcAddModifier<StrippedModifier>();

            EraserRole.ErasedPlayerIds.Add(victimId);
            Info($"[Eraser] Erased {victim.Data.PlayerName} (id={victimId}), isNeutral={isNeutral}");
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
