using MiraAPI.Colors;
using UnityEngine;

namespace MiraOverloaded;

public enum MiraOverloadedColorBlend
{
    Solid,
    Gradient,
}

public readonly struct MiraOverloadedColorDef
{
    public MiraOverloadedColorDef(string name, MiraOverloadedColorBlend blend, CustomColorBrightness brightness, params Color32[] colors)
    {
        Name = name;
        Blend = blend;
        Brightness = brightness;
        Colors = colors;
    }

    public string Name { get; }
    public MiraOverloadedColorBlend Blend { get; }
    public CustomColorBrightness Brightness { get; }
    public Color32[] Colors { get; }
}

public readonly struct MiraOverloadedRainbowDef
{
    public MiraOverloadedRainbowDef(string name, float speed, params Color32[] colors)
    {
        Name = name;
        Speed = speed;
        Colors = colors;
    }

    public string Name { get; }
    public float Speed { get; }
    public Color32[] Colors { get; }
}

public static class MiraOverloadedPlayerColors
{

    public static readonly MiraOverloadedColorDef[] ColorDefs =
    [
        new MiraOverloadedColorDef(
            "Sunset-Split",
            MiraOverloadedColorBlend.Gradient,
            CustomColorBrightness.Lighter,
            new Color32(120, 40, 200, 255),
            new Color32(255, 172, 28, 255)),

        new MiraOverloadedColorDef(
            "Void-Shift",
            MiraOverloadedColorBlend.Gradient,
            CustomColorBrightness.Lighter,
            new Color32(10, 10, 10, 255),
            new Color32(90, 0, 140, 255)),

        new MiraOverloadedColorDef(
            "Toxic-Split",
            MiraOverloadedColorBlend.Gradient,
            CustomColorBrightness.Lighter,
            new Color32(20, 120, 20, 255),
            new Color32(120, 255, 60, 255)),

        new MiraOverloadedColorDef(
            "Ocean-Split",
            MiraOverloadedColorBlend.Gradient,
            CustomColorBrightness.Lighter,
            new Color32(0, 60, 140, 255),
            new Color32(0, 200, 255, 255)),

        new MiraOverloadedColorDef(
            "Dead-Rainbow",
            MiraOverloadedColorBlend.Solid,
            CustomColorBrightness.Lighter,
            new Color32(20, 20, 20, 255)),

        new MiraOverloadedColorDef(
            "Blood-Pulse",
            MiraOverloadedColorBlend.Solid,
            CustomColorBrightness.Lighter,
            new Color32(20, 0, 0, 255)),

        new MiraOverloadedColorDef(
            "Void-Rainbow",
            MiraOverloadedColorBlend.Solid,
            CustomColorBrightness.Lighter,
            new Color32(10, 10, 10, 255)),

        new MiraOverloadedColorDef(
            "Toxic-Rainbow",
            MiraOverloadedColorBlend.Solid,
            CustomColorBrightness.Lighter,
            new Color32(30, 120, 20, 255))
    ];


    public static readonly MiraOverloadedRainbowDef[] RainbowDefs =
    [
        new MiraOverloadedRainbowDef(
            "Dead-Rainbow",
            0.5f,
            new Color32(20, 20, 20, 255),
            new Color32(45, 45, 45, 255),
            new Color32(70, 70, 70, 255),
            new Color32(90, 20, 20, 255),
            new Color32(120, 0, 0, 255),
            new Color32(60, 0, 0, 255)),

        new MiraOverloadedRainbowDef(
            "Blood-Pulse",
            0.45f,
            new Color32(20, 0, 0, 255),
            new Color32(60, 0, 0, 255),
            new Color32(120, 0, 0, 255),
            new Color32(180, 20, 20, 255),
            new Color32(120, 0, 0, 255),
            new Color32(60, 0, 0, 255)),

        new MiraOverloadedRainbowDef(
            "Void-Rainbow",
            0.55f,
            new Color32(10, 10, 10, 255),
            new Color32(40, 0, 80, 255),
            new Color32(80, 0, 120, 255),
            new Color32(120, 0, 180, 255),
            new Color32(60, 0, 120, 255),
            new Color32(20, 0, 60, 255)),

        new MiraOverloadedRainbowDef(
            "Toxic-Rainbow",
            0.6f,
            new Color32(30, 120, 20, 255),
            new Color32(80, 200, 40, 255),
            new Color32(150, 255, 60, 255),
            new Color32(80, 200, 40, 255),
            new Color32(30, 120, 20, 255))
    ];
}
