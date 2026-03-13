using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraModule.Assets;

public static class NeutAssets
{
    private const string ShortPath = "MiraModule.Resources.NeutButtons";
    public static LoadableAsset<Sprite> SentinelVentSprite { get; } = new LoadableResourceAsset($"{ShortPath}.SentinelVentButton.png");
    public static LoadableAsset<Sprite> SentinelExplodeSprite { get; } = new LoadableResourceAsset($"{ShortPath}.SentinelExplodeButton.png");
    public static LoadableAsset<Sprite> SentinelKillSprite { get; } = new LoadableResourceAsset($"{ShortPath}.SentinelKillButton.png");
    public static LoadableAsset<Sprite> AbyssGulpSprite { get; } = new LoadableResourceAsset($"{ShortPath}.AbyssGulpButton.png");
    public static LoadableAsset<Sprite> ShifterShiftSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Tempbutton.png");
}
