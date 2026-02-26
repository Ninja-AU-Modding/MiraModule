using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraModule.Assets;

public static class ExampleNeutAssets
{
    // Sentinel Assets
    private const string ShortPath = "MiraModule.Resources.NeutButtons";
    public static LoadableAsset<Sprite> SentinelVentSprite { get; } = new LoadableResourceAsset($"{ShortPath}.SentinelVentButton.png");
    public static LoadableAsset<Sprite> SentinelExplodeSprite { get; } = new LoadableResourceAsset($"{ShortPath}.SentinelExplodeButton.png");
    public static LoadableAsset<Sprite> SentinelKillSprite { get; } = new LoadableResourceAsset($"{ShortPath}.SentinelKillButton.png");

    // Pelican Assets
    public static LoadableAsset<Sprite> PelicanGulpSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Tempbutton.png");
}
