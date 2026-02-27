using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraModule.Assets;

public static class ImpostorAssets
{
    private const string ShortPath = "MiraModule.Resources.NeutButtons";

    // Eraser Assets
    public static LoadableAsset<Sprite> EraserEraseSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Tempbutton.png");
}
