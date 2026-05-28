using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraOverloaded.Utilities;
using MiraOverloaded.Options;
using MiraOverloaded.Options.Roles.Neutral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using MiraOverloaded.Assets;
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
                        "<b><#FF0000>Sabotaged AU Modding</color></b>, given permission by <b><#87e36b>Chipseq</color></b>,\n" +
                        "has implemented <b><#DDB244>Chaos Tokens</color></b> into <b><color=#a1e3fe>M</color><color=#fffca1>i</color><color=#7975ff>r</color><color=#5049fe>a</color> <color=#a1e3fe>O</color><color=#ffea9f>v</color><color=#8d89ff>e</color><color=#665eff>r</color><color=#5049fe>l</color><color=#a1e3fe>o</color><color=#7975ff>a</color><color=#665eff>d</color><color=#5049fe>e</color><color=#3b34db>d</color></b>,\n" +
                        "which can be enabled via a modifier.\n" +
                        "\n" +
                        "Due to this functionality, the original mod is incompatible\n" +
                        "with <b><color=#a1e3fe>M</color><color=#fffca1>i</color><color=#7975ff>r</color><color=#5049fe>a</color> <color=#a1e3fe>O</color><color=#ffea9f>v</color><color=#8d89ff>e</color><color=#665eff>r</color><color=#5049fe>l</color><color=#a1e3fe>o</color><color=#7975ff>a</color><color=#665eff>d</color><color=#5049fe>e</color><color=#3b34db>d</color></b>.\n" +
                        "Please remove the mod from <b><#0000FF>BepInEx/plugins</color></b> and\n" +
                        "relaunch the game.",
                        new Vector2(10f, 5f),
                        Assets.Assets.SabotagedAuModdingLogo.LoadAsset(),
                        0.25f,
                        PopupImagePosition.LeftOfText,
                        PopupTextAlignment.Left
                    );
                }));
            }
            else if (hasTownOfExtra)
            {
                button.OnClick.AddListener((System.Action)(() =>
                {
                    if (_toeWarningShown) return;

                    _toeWarningShown = true;
                    ModPopupUtility.Show(
                        "<size=4><b><#FF0000>Warning!</color></b></size>\n" +
                        "<b>Town of Extra</b> has been detected.\n" +
                        "Some roles have been disabled for compatibility reasons.\n" +
                        "You may continue playing." +
                        "\n" +
                        "<size=1><#898989>This can be forcefully disabled in the configuration file of the mod</color></size>",
                        new Vector2(8f, 5f),
                        Assets.Assets.Banner.LoadAsset(),
                        0.25f,
                        PopupImagePosition.AboveText,
                        PopupTextAlignment.Center
                    );
                }));
            }
        }
    }
}