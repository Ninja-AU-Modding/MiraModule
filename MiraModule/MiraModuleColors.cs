using MiraAPI.Utilities;
using TownOfUs;
using UnityEngine;

namespace MiraModule;

public static class MiraModuleColors
{
    // Crew Colors
    public static Color Chameleon => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(81, 180, 154, 255);
    public static Color Dictator => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(220, 233, 102, 255); // Yellow Ish

    // Impostor Colors
    public static Color Eraser => new Color32(200, 60, 60, 255); // Deep crimson
    public static Color Distorter => new Color32(176, 65, 214, 255);

    // Neutral Colors
    public static Color Sentinel => new Color32(143, 162, 141, 255);
    public static Color Abyss => new Color32(75, 45, 120, 255); // Purple
    public static Color Shifter => new Color32(12, 11, 6, 255); // Greyish Black
}
