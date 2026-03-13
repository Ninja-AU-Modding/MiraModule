using UnityEngine;

namespace MiraModule;

public static class MiraModuleGradientColors
{
    public static bool TryGetSecondary(int colorId, out Color32 secondaryMain, out Color32 secondaryShadow)
    {
        secondaryMain = default;
        secondaryShadow = default;

        if (colorId < 0 || colorId >= Palette.ColorNames.Length)
        {
            return false;
        }

        var name = Palette.ColorNames[colorId];
        return MiraModuleColorRegistry.TryGetSecondary(name, out secondaryMain, out secondaryShadow);
    }

    public static bool TryGetRainbowName(int colorId, out MiraModuleRainbowDef def)
    {
        def = default;

        if (colorId < 0 || colorId >= Palette.ColorNames.Length)
        {
            return false;
        }

        var name = Palette.ColorNames[colorId];
        return MiraModuleColorRegistry.TryGetRainbow(name, out def);
    }
}
