using HarmonyLib;
using MiraAPI.Roles;
using MiraAPI.Modifiers;
using MiraModule.Modifiers.Neutral;
using TownOfUs.Buttons;

namespace MiraModule.Patches;

public static class HarvesterTouButtonDisablePatch
{
    [HarmonyPatch(typeof(TownOfUsRoleButton<>), "Enabled")]
    private static class RoleButtonEnabledPatch
    {
        private static bool Prefix(RoleBehaviour? role, ref bool __result)
        {
            var player = PlayerControl.LocalPlayer;
            if (player != null && player.HasModifier<HarvesterCacheModifier>())
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(TownOfUsRoleButton<,>), "Enabled")]
    private static class RoleTargetButtonEnabledPatch
    {
        private static bool Prefix(RoleBehaviour? role, ref bool __result)
        {
            var player = PlayerControl.LocalPlayer;
            if (player != null && player.HasModifier<HarvesterCacheModifier>())
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
