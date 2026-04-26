using MiraAPI.Utilities;
using TownOfUs;
using UnityEngine;

namespace MiraOverloaded;

public static class MiraOverloadedColors
{
    // Crew Colors
    public static Color Chameleon => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(81, 180, 154, 255);
    public static Color Lifter => new Color32(92, 176, 255, 255);
    public static Color InspectorGeneral => new Color32(72, 190, 164, 255);
    public static Color CommandSpecialist => new Color32(112, 200, 255, 255);

    // Impostor Colors
    public static Color Eraser => new Color32(200, 60, 60, 255); // Deep crimson
    public static Color Distorter => new Color32(176, 65, 214, 255);
    public static Color Arbiter => new Color32(200, 60, 60, 255);
    public static Color Ninja => new Color32(200, 60, 60, 255);

    // Neutral Colors
    public static Color Sentinel => new Color32(143, 162, 141, 255);
    public static Color Abyss => new Color32(75, 45, 120, 255); // Purple
    public static Color Shifter => new Color32(12, 11, 6, 255); // Greyish Black
    public static Color Baiter => new Color(0f, 0.45f, 1f, 1f); // Vivid blue
    public static Color Fakeposter => new Color32(170, 170, 170, 255); 
    public static Color HiveMind => new Color32(221, 178, 68, 255); // Golden Yellow

    // Modifier Colors
    public static Color ChaosTokens => new Color32(221, 178, 68, 255);
}
