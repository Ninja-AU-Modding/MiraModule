using HarmonyLib;
using JetBrains.Annotations;
using MiraAPI.Roles;
using MiraModule.Roles.Neutral;
using MiraModule.Assets;
using MiraModule.Utilities;
using Reactor.Networking.Attributes;
using MiraAPI.GameOptions;
using MiraModule.Options.Roles.Neutral;

namespace MiraModule.Patches;

[HarmonyPatch]
public static class HiveMindDeathPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
    public static void Postfix(PlayerControl __instance, DeathReason reason)
    {
        var role = __instance.Data.Role;
        if (role is not ICustomRole customRole) return;
        if (customRole is not HiveMindRole) return;
        RpcHiveMindDeath(__instance);
    }

    [MethodRpc((uint)MiraModuleRpc.HiveMindDeath)]
    public static void RpcHiveMindDeath(PlayerControl sender)
    {
        if (sender.AmOwner) { return; }

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer != null)
        {
            var role = localPlayer.Data.Role;
            if (role is not ICustomRole customRole) return;
            if (customRole is not HiveMindRole) return;

            SoundManager.Instance.PlaySound(MiraModuleAudio.HiveMind.LoadAsset(), false, 1f);
            var dotheyknow = OptionGroupSingleton<HiveMindOptions>.Instance.HiveMindKnows;
            string extraMessage = "";
            if (!dotheyknow)
            {
                extraMessage = ", who was a part of the hive,";
            }
            ChaosTokensUtils.Notification($"<b><#FF0000>{sender.name}{extraMessage} has died!</color></b>");
        }
    }
}
