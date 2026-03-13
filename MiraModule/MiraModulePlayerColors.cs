using MiraAPI.Colors;
using UnityEngine;

namespace MiraModule;

public enum MiraModuleColorBlend
{
    Solid,
    Gradient,
}

public readonly struct MiraModuleColorDef
{
    public MiraModuleColorDef(string name, MiraModuleColorBlend blend, CustomColorBrightness brightness, params Color32[] colors)
    {
        Name = name;
        Blend = blend;
        Brightness = brightness;
        Colors = colors;
    }

    public string Name { get; }
    public MiraModuleColorBlend Blend { get; }
    public CustomColorBrightness Brightness { get; }
    public Color32[] Colors { get; }
}

public readonly struct MiraModuleRainbowDef
{
    public MiraModuleRainbowDef(string name, float speed, params Color32[] colors)
    {
        Name = name;
        Speed = speed;
        Colors = colors;
    }

    public string Name { get; }
    public float Speed { get; }
    public Color32[] Colors { get; }
}

public static class MiraModulePlayerColors
{
    // Add new colors here in the exact order you want them to appear.
    // If Colors has 1 entry => solid.
    // If Colors has 2+ entries and Blend == Gradient => gradient uses first + last (smooth blend).
    // If Colors has 2+ entries and Blend == Solid => uses only the first color.
    public static readonly MiraModuleColorDef[] ColorDefs =
    [
        new MiraModuleColorDef(
            "Sunset-Split",
            MiraModuleColorBlend.Gradient,
            CustomColorBrightness.Lighter,
            new Color32(200, 60, 60, 255),   // orange (start)
            new Color32(120, 40, 200, 255)),  // purple (end)

        new MiraModuleColorDef(
            "Dead-Rainbow",
            MiraModuleColorBlend.Solid,
            CustomColorBrightness.Lighter,
            new Color32(20, 20, 20, 255))
    ];

    // Custom rainbows (name must match a color in ColorDefs).
    // Speed: higher = faster
    public static readonly MiraModuleRainbowDef[] RainbowDefs =
    [
        new MiraModuleRainbowDef(
            "Dead-Rainbow",
            0.5f,
            new Color32(20, 20, 20, 255),    // near black
            new Color32(45, 45, 45, 255),    // dark gray
            new Color32(70, 70, 70, 255),    // gray
            new Color32(90, 20, 20, 255),    // dark crimson
            new Color32(120, 0, 0, 255),     // deep blood red
            new Color32(60, 0, 0, 255))      // dried blood
];
}
