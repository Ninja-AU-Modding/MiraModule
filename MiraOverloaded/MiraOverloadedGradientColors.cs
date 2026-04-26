using UnityEngine;

namespace MiraOverloaded;

public static class MiraOverloadedGradientColors
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
        return MiraOverloadedColorRegistry.TryGetSecondary(name, out secondaryMain, out secondaryShadow);
    }

    public static bool TryGetRainbowName(int colorId, out MiraOverloadedRainbowDef def)
    {
        def = default;

        if (colorId < 0 || colorId >= Palette.ColorNames.Length)
        {
            return false;
        }

        var name = Palette.ColorNames[colorId];
        return MiraOverloadedColorRegistry.TryGetRainbow(name, out def);
    }
}
