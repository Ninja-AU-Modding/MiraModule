using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraOverloaded.Assets;

public static class Assets
{
    private const string ShortPath = "MiraOverloaded.Resources";
    public static LoadableAsset<Sprite> Banner { get; } = new LoadableResourceAsset($"{ShortPath}.Banner.png");
    public static LoadableAsset<Sprite> SabotagedAuModdingLogo { get; } = new LoadableResourceAsset($"{ShortPath}.SAULogo.png");
}
