using HarmonyLib;
using MiraAPI.Modifiers;
using MiraModule.Modifiers.Crewmate;
using MiraModule.Roles.Crewmate;
using MiraAPI.Roles;
using AmongUs.GameOptions;
using TownOfUs.Patches;

namespace MiraModule.Patches;

[HarmonyPatch(typeof(HudManagerPatches), nameof(HudManagerPatches.UpdateRoleNameText))]
public static class InspectorGeneralVisibilityPatch
{
    [HarmonyPostfix]
    public static void UpdateRoleNameTextPostfix()
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || local.Data == null)
        {
            return;
        }

        static bool IsIg(PlayerControl player)
        {
            return player.Data?.Role != null &&
                   player.Data.Role.Role == (RoleTypes)RoleId.Get<InspectorGeneralRole>();
        }

        static bool IsCs(PlayerControl player)
        {
            return player.Data?.Role != null &&
                   player.Data.Role.Role == (RoleTypes)RoleId.Get<CommandSpecialistRole>();
        }

        var localIsIg = IsIg(local);
        var localIsCs = IsCs(local);
        var wantsReveal = localIsIg || localIsCs;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null)
            {
                continue;
            }

            var shouldReveal = wantsReveal &&
                               ((localIsIg && (IsCs(player) || IsIg(player))) ||
                                (localIsCs && (IsIg(player) || IsCs(player))));

            if (shouldReveal)
            {
                if (!player.HasModifier<InspectorGeneralLinkRevealModifier>())
                {
                    player.AddModifier<InspectorGeneralLinkRevealModifier>();
                }
            }
            else
            {
                if (player.HasModifier<InspectorGeneralLinkRevealModifier>())
                {
                    player.RemoveModifier<InspectorGeneralLinkRevealModifier>();
                }
            }
        }
    }
}
