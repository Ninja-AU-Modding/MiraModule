using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraOverloaded.Utilities;
using MiraOverloaded.Options;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using MiraOverloaded.Modules;
using static MiraOverloaded.Assets.Assets;
using BepInEx.Unity.IL2CPP;

namespace MiraOverloaded.Patches;

[HarmonyPatch]
public static class ModCompatabilityPatches
{
    static readonly MiraOverloadedOptions opts = OptionGroupSingleton<MiraOverloadedOptions>.Instance;

    [HarmonyPatch(typeof(CustomRoleUtils), nameof(CustomRoleUtils.CanSpawnOnCurrentMode))]
    public static class DisableRolesPatch
    {
        private static readonly HashSet<string> Blocked = new()
        {
            "TownOfExtra.Roles.Impostor.Power.EraserRole",
            "TownOfExtra.Roles.Crewmate.Power.Chief",
        };

        [HarmonyPostfix]
        public static void Postfix(RoleBehaviour role, ref bool __result)
        {
            if (__result && Blocked.Contains(role.GetType().FullName) && opts.DisableRoles)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class DisablePlayButtonPatch
    {
        private static bool _toeWarningShown = false;

        [HarmonyPostfix]
        public static void Postfix(MainMenuManager __instance)
        {
            var button = __instance.playButton;
            if (button == null) return;

            bool hasChaosTokens = IL2CPPChainloader.Instance.Plugins.ContainsKey("chipseq.chaostokens");
            bool hasTownOfExtra = IL2CPPChainloader.Instance.Plugins.ContainsKey("me.mehzxzz.townOfExtra");

            if (hasChaosTokens)
            {
                foreach (var sr in button.GetComponentsInChildren<SpriteRenderer>(true))
                    sr.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                foreach (var tmp in button.GetComponentsInChildren<TextMeshPro>(true))
                    tmp.color = new Color(0.6f, 0.6f, 0.6f, 1f);

                int persistentCount = button.OnClick.GetPersistentEventCount();
                for (int i = 0; i < persistentCount; i++)
                {
                    button.OnClick.SetPersistentListenerState(i, UnityEventCallState.Off);
                }

                button.OnClick.RemoveAllListeners();
                button.OnClick.AddListener((System.Action)(() =>
                {
                    ModPopupUtility.Show(
                        MiraOverloadedLocale.GetString("MiraCompatabilityChaosTokensWarning"),
                        new Vector2(10f, 5f),
                        SabotagedAuModdingLogo.LoadAsset(),
                        0.25f,
                        PopupImagePosition.LeftOfText,
                        PopupTextAlignment.Left
                    );
                }));
            }
            else if (hasTownOfExtra && opts.DisableRoles)
            {
                button.OnClick.AddListener((System.Action)(() =>
                {
                    if (_toeWarningShown) return;

                    _toeWarningShown = true;
                    ModPopupUtility.Show(
                        MiraOverloadedLocale.GetString("MiraCompatabilityTownOfExtraWarning"),
                        new Vector2(8f, 5f),
                        Banner.LoadAsset(),
                        0.25f,
                        PopupImagePosition.AboveText,
                        PopupTextAlignment.Center
                    );
                }));
            }
        }
    }
}