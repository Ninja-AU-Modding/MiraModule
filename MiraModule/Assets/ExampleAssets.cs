using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraModule.Assets;

public static class ExampleAssets
{
    private const string ShortPath = "MiraModule.Resources";
    public static LoadableAsset<Sprite> Banner { get; } = new LoadableResourceAsset($"{ShortPath}.ExampleBanner.png");
}
