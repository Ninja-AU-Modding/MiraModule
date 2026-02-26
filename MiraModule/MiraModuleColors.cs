using MiraAPI.Utilities;
using TownOfUs;
using UnityEngine;

namespace MiraModule;

public static class MiraModuleColors
{
    // Crew Colors
    public static Color Chameleon => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(81, 180, 154, 255);
    
    // Neutral Colors
    public static Color Sentinel => new Color32(143, 162, 141, 255);
    public static Color Hacker => new Color32(0, 255, 127, 255); // Bright green/cyan hacker color
    public static Color Abyss => new Color32(100, 180, 220, 255); // Soft ocean blue
}
