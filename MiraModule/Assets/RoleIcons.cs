using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraModule.Assets;

public static class RoleIcons
{
    // THIS FILE SHOULD ONLY HOLD ROLE ICONS

    private const string IconsPath = "MiraModule.Resources.RoleIcons";

    // Neutrals
    public static LoadableAsset<Sprite> Sentinel { get; } = new LoadableResourceAsset($"{IconsPath}.Sentinel.png", 200);
    public static LoadableAsset<Sprite> Abyss { get; } = new LoadableResourceAsset($"{IconsPath}.Abyss.png", 200);
    public static LoadableAsset<Sprite> Shifter { get; } = new LoadableResourceAsset($"{IconsPath}.TempRoleIcon.png", 200);
    public static LoadableAsset<Sprite> Fakeposter { get; } = new LoadableResourceAsset($"{IconsPath}.TempRoleIcon.png", 200);
    public static LoadableAsset<Sprite> Baiter { get; } = new LoadableResourceAsset($"{IconsPath}.Baiter.png", 200);

    // Impostors
    public static LoadableAsset<Sprite> Eraser { get; } = new LoadableResourceAsset($"{IconsPath}.TempRoleIcon.png", 200);
    public static LoadableAsset<Sprite> Distorter { get; } = new LoadableResourceAsset($"{IconsPath}.TempRoleIcon.png", 200);
    public static LoadableAsset<Sprite> Arbiter { get; } = new LoadableResourceAsset($"{IconsPath}.TempRoleIcon.png", 200);
    public static LoadableAsset<Sprite> Ninja { get; } = new LoadableResourceAsset($"{IconsPath}.TempRoleIcon.png", 200);
    public static LoadableAsset<Sprite> Poisoner { get; } = new LoadableResourceAsset($"{IconsPath}.Poisoner.png", 200);
    public static LoadableAsset<Sprite> Revenant { get; } = new LoadableResourceAsset($"{IconsPath}.TempRoleIcon.png", 200);

    // Crewmates
    public static LoadableAsset<Sprite> Dictator { get; } = new LoadableResourceAsset($"{IconsPath}.TempRoleIcon.png", 200);
    public static LoadableAsset<Sprite> Sentinel { get; } = new LoadableResourceAsset($"{ShortPath}.RoleIcons.Sentinel.png", 200);
    public static LoadableAsset<Sprite> Abyss { get; } = new LoadableResourceAsset($"{ShortPath}.RoleIcons.Abyss.png", 200);
    public static LoadableAsset<Sprite> Shifter { get; } = new LoadableResourceAsset($"{ShortPath}.RoleIcons.Shifter.png", 200);
    public static LoadableAsset<Sprite> Fakeposter { get; } = new LoadableResourceAsset($"{ShortPath}.RoleIcons.Fakeposter.png", 200);


    // Impostors
    public static LoadableAsset<Sprite> Eraser { get; } = new LoadableResourceAsset($"{ShortPath}.RoleIcons.Eraser.png", 200);
    public static LoadableAsset<Sprite> Distorter { get; } = new LoadableResourceAsset($"{ShortPath}.RoleIcons.Distorter.png", 200);
    public static LoadableAsset<Sprite> Arbiter { get; } = new LoadableResourceAsset($"{ShortPath}.RoleIcons.Arbiter.png", 200);
    public static LoadableAsset<Sprite> Ninja { get; } = new LoadableResourceAsset($"{ShortPath}.RoleIcons.Ninja.png", 200);

    // Crewmates
}
