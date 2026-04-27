using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraOverloaded.Assets;

public static class ModifierIcons
{
    private const string ShortPath = "MiraOverloaded.Resources.Modifiers";

    public static LoadableAsset<Sprite> Explosive { get; } =
        new LoadableResourceAsset($"{ShortPath}.TempIcon.png", 200);

    public static LoadableAsset<Sprite> ChaosToken { get; } =
        new LoadableResourceAsset($"{ShortPath}.TempIcon.png", 200);

    public static LoadableAsset<Sprite> Stripped { get; } =
        new LoadableResourceAsset($"{ShortPath}.TempIcon.png", 200);
    public static LoadableAsset<Sprite> Agent { get; } =
        new LoadableResourceAsset($"{ShortPath}.TempIcon.png", 200);
}
