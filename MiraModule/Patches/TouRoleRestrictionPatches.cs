using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using MiraAPI.Roles;
using MiraModule.Rules;
using TownOfUs.Buttons;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Events.Crewmate;
using TownOfUs.Patches;

namespace MiraModule.Patches;

public static class TouRoleRestrictionPatches
{
    [HarmonyPatch(typeof(TouRoleManagerPatches), "AssignRolesToPlayers")]
    private static class AssignRolesToPlayersPatch
    {
        private static void Prefix(List<ushort> roles)
        {
            TouRoleRestrictions.RemoveDisallowedRoles(roles);
        }
    }

    [HarmonyPatch(typeof(TownOfUsRoleButton<>), "Enabled")]
    private static class RoleButtonEnabledPatch
    {
        private static bool Prefix(object __instance, RoleBehaviour? role, ref bool __result)
        {
            if (__instance is not TownOfUsButton button)
            {
                return true;
            }

            if (!TouRoleRestrictions.ShouldAllowButton(role, button))
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
        private static bool Prefix(object __instance, RoleBehaviour? role, ref bool __result)
        {
            if (__instance is not TownOfUsButton button)
            {
                return true;
            }

            if (!TouRoleRestrictions.ShouldAllowButton(role, button))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(AltruistReviveButton), nameof(AltruistReviveButton.CoKillOnStart))]
    private static class AltruistNoDeathOnStartPatch
    {
        private static bool Prefix(ref IEnumerator __result)
        {
            if (!TouRoleRestrictions.AltruistDoesNotDieOnRevive)
            {
                return true;
            }

            __result = Empty();
            return false;
        }
    }

    [HarmonyPatch(typeof(AltruistReviveButton), nameof(AltruistReviveButton.CoSacrifite))]
    private static class AltruistNoDeathOnEndPatch
    {
        private static bool Prefix(ref IEnumerator __result)
        {
            if (!TouRoleRestrictions.AltruistDoesNotDieOnRevive)
            {
                return true;
            }

            __result = Empty();
            return false;
        }
    }

    [HarmonyPatch(typeof(MedicEvents), "CheckForMedicShield")]
    private static class FakeMedicShieldPatch
    {
        private static bool Prefix(ref bool __result)
        {
            if (!TouRoleRestrictions.FakeMedicShield)
            {
                return true;
            }

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(WardenEvents), "CheckForWardenFortify")]
    private static class FakeWardenFortifyPatch
    {
        private static bool Prefix()
        {
            return !TouRoleRestrictions.FakeWardenFortify;
        }
    }

    private static IEnumerator Empty()
    {
        yield break;
    }
}
