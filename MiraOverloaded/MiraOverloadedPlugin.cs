using System.Globalization;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI;
using MiraAPI.PluginLoading;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace MiraOverloaded;

[BepInAutoPlugin("com.saboau.miraoverloaded", "Mira Overloaded")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[BepInDependency(TownOfUsPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class MiraOverloadedPlugin : BasePlugin, IMiraPlugin
{
    /// <summary>
    ///     Gets the specified Culture for string manipulations.
    /// </summary>
    public static CultureInfo Culture => TownOfUs.TownOfUsPlugin.Culture;

    /// <inheritdoc />
    public string OptionsTitleText => "Mira Overloaded";

    /// <summary>
    ///     Determines if the current build is a dev build or not.
    /// </summary>
    public static bool IsDevBuild => true;

    /// <inheritdoc />
    public ConfigFile GetConfigFile()
    {
        return Config;
    }

    public Harmony Harmony { get; } = new(Id);

    public override void Load()
    {
        ReactorCredits.Register("Mira Overloaded", Version, IsDevBuild, ReactorCredits.AlwaysShow);
        
        // Initialize localization AFTER mods are loaded to ensure maximum compatibility
        IL2CPPChainloader.Instance.Finished += Modules.ExtensionLocale.SearchInternalLocale;
        
        Harmony.PatchAll();

        // Debug gradient bundle contents.
        Assets.MiraOverloadedGradientAssets.DumpBundleInfo();

        // (Rainbow animation handled per-renderer; no global updater needed.)
    }
}
