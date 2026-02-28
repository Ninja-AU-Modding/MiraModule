using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraModule.Assets;

public static class CrewAssets
{
    private const string ShortPath = "MiraModule.Resources.CrewButtons";

    // Dictator Assets
    public static LoadableAsset<Sprite> DictatorCondemnSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Tempbutton.png");
    public static LoadableAsset<Sprite> DictatorEndMeetingSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Tempbutton.png");

    // Phasewalker Assets
    public static LoadableAsset<Sprite> PhasewalkerPhaseSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Tempbutton.png");
}
