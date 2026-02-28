using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraModule.Assets;

public static class ModifierIcons
{
    private const string ShortPath = "MiraModule.Resources.Modifiers";

    public static LoadableAsset<Sprite> Explosive { get; } =
        new LoadableResourceAsset($"{ShortPath}.TempIcon.png", 200);

    public static LoadableAsset<Sprite> ChaosToken { get; } =
        new LoadableResourceAsset($"{ShortPath}.TempIcon.png", 200);
}
