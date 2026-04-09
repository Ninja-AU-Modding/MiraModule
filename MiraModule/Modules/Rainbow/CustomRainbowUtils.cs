using UnityEngine;

namespace MiraModule.Modules.Rainbow;

public static class CustomRainbowUtils
{
    public static Color GetColor(float speed, Color32[] colors)
    {
        if (colors.Length == 0)
        {
            return Color.white;
        }

        if (colors.Length == 1)
        {
            return colors[0];
        }

        var t = Mathf.Repeat(Time.time * speed, 1f);
        var scaled = t * colors.Length;
        var i = Mathf.FloorToInt(scaled);
        var frac = scaled - i;
        var a = colors[i % colors.Length];
        var b = colors[(i + 1) % colors.Length];

        return Color.Lerp(a, b, frac);
    }

    public static Color GetShadow(Color color)
    {
        return new Color(color.r - 0.3f, color.g - 0.3f, color.b - 0.3f);
    }
}
