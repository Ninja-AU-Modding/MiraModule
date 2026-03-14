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
        {
            foreach (var playerVA in MeetingHud.Instance.playerStates)
            {
                var player = MiscUtils.PlayerById(playerVA.TargetPlayerId);
                if (player?.Data?.Role is not FakeposterRole fp) continue;
                if (fp.RoleName != ExpectedLocaleName) continue;

                playerVA.NameText.text = playerVA.NameText.text.Replace(ExpectedLocaleName, ColoredName);
            }
        }
        else
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player?.Data?.Role is not FakeposterRole fp) continue;
                if (fp.RoleName != ExpectedLocaleName) continue;

                player.cosmetics.nameText.text =
                    player.cosmetics.nameText.text.Replace(ExpectedLocaleName, ColoredName);
            }
        }
    }
}