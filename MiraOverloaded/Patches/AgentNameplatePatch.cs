using HarmonyLib;
using InnerNet;
using MiraOverloaded.Utilities;

namespace MiraOverloaded.Patches;


[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
[HarmonyPriority(Priority.Last)]
public static class AgentNameplatePatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        if (!IsGameActive()) return;

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

    private static void PatchMeeting()
    {
        foreach (var playerVA in MeetingHud.Instance.playerStates)
        {
            if (!playerVA.gameObject.active) continue;
            var player = MiscUtils.PlayerById(playerVA.TargetPlayerId);
            if (player?.Data?.Role == null) continue;
            playerVA.NameText.text = playerVA.NameText.text.UpdateAgentSymbols(player);
        }
    }

    private static void PatchInGame()
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player?.Data?.Role == null) continue;
            if (player.cosmetics?.nameText == null) continue;
            player.cosmetics.nameText.text = player.cosmetics.nameText.text.UpdateAgentSymbols(player);
        }
    }
}
