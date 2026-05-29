using HarmonyLib;
using MiraOverloaded.Roles;
using MiraOverloaded.Roles.Neutral;
using TownOfUs.Patches;

namespace MiraOverloaded.Patches;

[HarmonyPatch]
public static class FakePostorRoleNamePatches
{
    private static readonly string RegularName = MiraOverloadedLocale.GetString("MiraRoleFakePostor");
    private static readonly string ColoredName = MiraOverloadedLocale.GetString("MiraRoleFakePostorColoredName");

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
            playerVA.NameText.text = playerVA.NameText.text.Replace(RegularName, ColoredName);
        }
    }

    private static void PatchInGame()
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!IsFakeposter(player)) continue;
            player.cosmetics.nameText.text = player.cosmetics.nameText.text.Replace(RegularName, ColoredName);
        }
    }

    private static bool IsFakeposter(PlayerControl? player) =>
        player?.Data?.Role is FakePostorRole fp && fp.RoleName == RegularName;
}