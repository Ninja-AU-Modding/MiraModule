using HarmonyLib;
using MiraModule.Roles;
using MiraModule.Roles.Neutral;
using TownOfUs.Patches;

namespace MiraModule.Patches;

[HarmonyPatch]
public static class FakeposterRoleNamePatches
{
    private const string ExpectedLocaleName = "Fake-poster";
    private const string ColoredName = "<color=#AAAAAA>Fake-</color><color=#D23A58>poster</color>";

    [HarmonyPatch(typeof(HudManagerPatches), nameof(HudManagerPatches.UpdateRoleNameText))]
    [HarmonyPostfix]
    public static void Postfix()
    {
        if (MeetingHud.Instance)
            PatchMeeting();
        else
            PatchInGame();
    }

    private static void PatchMeeting()
    {
        foreach (var playerVA in MeetingHud.Instance.playerStates)
        {
            if (!IsFakeposter(MiscUtils.PlayerById(playerVA.TargetPlayerId))) continue;
            playerVA.NameText.text = playerVA.NameText.text.Replace(ExpectedLocaleName, ColoredName);
        }
    }

    private static void PatchInGame()
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!IsFakeposter(player)) continue;
            player.cosmetics.nameText.text = player.cosmetics.nameText.text.Replace(ExpectedLocaleName, ColoredName);
        }
    }

    private static bool IsFakeposter(PlayerControl? player) =>
        player?.Data?.Role is FakeposterRole fp && fp.RoleName == ExpectedLocaleName;
}