using MiraAPI.Utilities;
using TownOfUs;
using UnityEngine;

namespace MiraModule;

public static class MiraModuleColors
{
    // Crew Colors
    public static Color Chameleon => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(81, 180, 154, 255);
    
    // Impostor Colors
    public static Color Eraser => new Color32(200, 60, 60, 255); // Deep crimson

    // Neutral Colors
    public static Color Sentinel => new Color32(143, 162, 141, 255);
    public static Color Abyss => new Color32(75, 45, 120, 255); // Soft ocean blue
    public static Color Shifter => new Color32(75, 45, 120, 255); // Soft ocean blue
}
 