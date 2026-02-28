using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Usables;
using MiraModule.Roles.Crewmate;

namespace MiraModule.Patches;

/// <summary>
/// All game restrictions that apply while the Phasewalker is phased:
///   - Cannot use tasks, consoles, vents, sabotage panels (PlayerCanUseEvent)
///   - Cannot report bodies (ReportBodyEvent)
///   - Cannot call emergency meeting (Harmony patch on UsableDistance guard)
/// </summary>
public static class PhasewalkerPatch
{
    // ── Block use of any IUsable while phased ─────────────────────────────
    [RegisterEvent]
    public static void OnPlayerCanUse(PlayerCanUseEvent @event)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null) return;
        if (local.Data?.Role is not PhasewalkerRole { IsPhased: true }) return;

        // Block everything: tasks, consoles, vents, sabotage panels
        @event.Cancel();
    }

    // ── Block body reporting while phased ─────────────────────────────────
    [RegisterEvent]
    public static void OnReportBody(ReportBodyEvent @event)
    {
        if (@event.Reporter.Data?.Role is PhasewalkerRole { IsPhased: true })
            @event.Cancel();
    }

    // ── Block emergency-meeting button while phased ───────────────────────
    // EmergencyMinigame.Begin is what runs when the button is pressed —
    // CanUse() is called first and drives the approach distance shown to the player.
    // Cancelling PlayerCanUseEvent above already covers the console, but the
    // Harmony patch below also blocks the Harmony tap on the button itself.
    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Begin))]
    [HarmonyPrefix]
    public static bool EmergencyMinigame_Begin_Prefix(EmergencyMinigame __instance)
    {
        var local = PlayerControl.LocalPlayer;
        if (local?.Data?.Role is PhasewalkerRole { IsPhased: true })
            return false; // suppress
        return true;
    }

    // ── Cannot enter vents while phased ──────────────────────────────────
    [RegisterEvent]
    public static void OnPlayerEnterVent(EnterVentEvent @event)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null) return;
        if (local.Data?.Role is not PhasewalkerRole { IsPhased: true }) return;
        @event.Cancel();
    }
}
