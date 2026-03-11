using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraModule.Assets;

public static class RoleIcons
{
    // THIS FILE SHOULD ONLY HOLD ROLE ICONS

    private const string ShortPath = "MiraModule.Resources";

    // Neutrals
    public static LoadableAsset<Sprite> Sentinel { get; } = new LoadableResourceAsset($"{ShortPath}.RoleIcons.Sentinel.png", 200);
    public static LoadableAsset<Sprite> Abyss { get; } = new LoadableResourceAsset($"{ShortPath}.RoleIcons.Abyss.png", 200);
    public static LoadableAsset<Sprite> Shifter { get; } = new LoadableResourceAsset($"{ShortPath}.RoleIcons.TempRoleIcon.png", 200);
    public static LoadableAsset<Sprite> Fakeposter { get; } = new LoadableResourceAsset($"{ShortPath}.RoleIcons.TempRoleIcon.png", 200);

    // Impostors
    public static LoadableAsset<Sprite> Eraser { get; } = new LoadableResourceAsset($"{ShortPath}.RoleIcons.TempRoleIcon.png", 200);

    // Crewmates
    public static LoadableAsset<Sprite> Dictator { get; } = new LoadableResourceAsset($"{ShortPath}.RoleIcons.TempRoleIcon.png", 200);
}
