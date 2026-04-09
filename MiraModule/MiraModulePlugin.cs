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

namespace MiraModule;

[BepInAutoPlugin("com.naum.miramodule", "Mira Module")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[BepInDependency(TownOfUsPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class MiraModulePlugin : BasePlugin, IMiraPlugin
{
    /// <summary>
    ///     Gets the specified Culture for string manipulations.
    /// </summary>
    public static CultureInfo Culture => TownOfUs.TownOfUsPlugin.Culture;

    /// <inheritdoc />
    public string OptionsTitleText => "Mira Module";

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
        ReactorCredits.Register("Mira Module", Version, IsDevBuild, ReactorCredits.AlwaysShow);
        
        // Initialize localization AFTER mods are loaded to ensure maximum compatibility
        IL2CPPChainloader.Instance.Finished += Modules.ExtensionLocale.SearchInternalLocale;
        
        Harmony.PatchAll();

        // Debug gradient bundle contents.
        Assets.MiraModuleGradientAssets.DumpBundleInfo();

        // (Rainbow animation handled per-renderer; no global updater needed.)
    }
}
