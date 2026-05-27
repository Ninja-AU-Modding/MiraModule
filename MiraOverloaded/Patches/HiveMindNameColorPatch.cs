using HarmonyLib;
using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraOverloaded.Modifiers;
using MiraOverloaded.Options.Roles.Neutral;
using MiraOverloaded.Roles.Neutral;

namespace MiraOverloaded.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
[HarmonyPriority(Priority.Last)]
public static class HiveMindNameColorPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        if (!IsGameActive()) return;
        if (!OptionGroupSingleton<HiveMindOptions>.Instance.HiveMindKnows) return;
        if (!IsLocalHiveMindMember()) return;

        if (MeetingHud.Instance)
            PatchMeeting();
        else
            PatchInGame();
    }

    private static bool IsGameActive() =>
        PlayerControl.LocalPlayer != null &&
        PlayerControl.LocalPlayer.Data?.Role != null &&
        ShipStatus.Instance &&
        AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started;

    private static bool IsLocalHiveMindMember()
    {
        var local = PlayerControl.LocalPlayer;
        return local.Data?.Role is HiveMindRole || local.HasModifier<LinkedMindModifier>();
    }

    private static bool IsHiveMindMember(PlayerControl player) =>
        player.Data?.Role is HiveMindRole || player.HasModifier<LinkedMindModifier>();

    private static void PatchInGame()
    {
        var local = PlayerControl.LocalPlayer;
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player == null || player.Data?.Role == null || player == local) continue;
            if (player.cosmetics?.nameText == null) continue;
            if (IsHiveMindMember(player))
                player.cosmetics.nameText.color = MiraOverloadedColors.HiveMind;
        }
    }

    private static void PatchMeeting()
    {
        var local = PlayerControl.LocalPlayer;
        foreach (var playerVA in MeetingHud.Instance.playerStates)
        {
            if (playerVA == null || !playerVA.gameObject.activeSelf) continue;
            var player = MiscUtils.PlayerById(playerVA.TargetPlayerId);
            if (player == null || player.Data?.Role == null || player == local) continue;
            if (IsHiveMindMember(player))
                playerVA.NameText.color = MiraOverloadedColors.HiveMind;
        }
    }
}
