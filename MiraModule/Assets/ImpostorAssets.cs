using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraModule.Assets;

public static class ImpostorAssets
{
    private const string ShortPath = "MiraModule.Resources.ImpButtons";

    // Eraser Assets
    public static LoadableAsset<Sprite> EraserEraseSprite { get; } = new LoadableResourceAsset($"{ShortPath}.EraserErase.png");

    // Distorter Assets (temp)
    public static LoadableAsset<Sprite> DistorterDistortSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Tempbutton.png");

    public static LoadableAsset<Sprite> ArbiterMarkSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Tempbutton.png");
    public static LoadableAsset<Sprite> ArbiterInvisSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Tempbutton.png");
    public static LoadableAsset<Sprite> ArbiterSpeedSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Tempbutton.png");
}
