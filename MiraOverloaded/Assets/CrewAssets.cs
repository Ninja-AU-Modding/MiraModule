using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraOverloaded.Assets;

public static class CrewAssets
{
    private const string ShortPath = "MiraOverloaded.Resources.CrewButtons";

    public static LoadableAsset<Sprite> LifterLiftSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Tempbutton.png");
    public static LoadableAsset<Sprite> InspectorGeneralConvertSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Tempbutton.png");
    public static LoadableAsset<Sprite> InspectorGeneralKillSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Tempbutton.png");
    public static LoadableAsset<Sprite> CommandSpecialistKillSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Tempbutton.png");

}
