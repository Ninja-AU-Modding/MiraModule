using HarmonyLib;
using InnerNet;
using MiraModule.Utilities;

namespace MiraModule.Patches;

/// <summary>
/// Allows people with the AgentAware modifier to see the Agent
/// </summary>
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
[HarmonyPriority(Priority.Last)]
public static class AgentNameplatePatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        if (PlayerControl.LocalPlayer == null ||
            PlayerControl.LocalPlayer.Data?.Role == null ||
            !ShipStatus.Instance ||
            AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
        {
            return;
        }

        if (MeetingHud.Instance)
        {
            foreach (var playerVA in MeetingHud.Instance.playerStates)
            {
                if (!playerVA.gameObject.active) continue;

                var player = MiscUtils.PlayerById(playerVA.TargetPlayerId);
                if (player == null || player.Data?.Role == null) continue;

                playerVA.NameText.text = playerVA.NameText.text.UpdateAgentSymbols(player);
            }
        }
        else
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data?.Role == null) continue;
                if (player.cosmetics?.nameText == null) continue;

                player.cosmetics.nameText.text = player.cosmetics.nameText.text.UpdateAgentSymbols(player);
            }
        }
    }
}