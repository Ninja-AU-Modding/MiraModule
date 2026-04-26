using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using System.Diagnostics.CodeAnalysis;

namespace MiraOverloaded.Patches.Gradient;

[HarmonyPatch]
public static class CustomColorOrderPatch
{
    [HarmonyTargetMethod]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by Harmony via reflection")]
    private static MethodInfo? TargetMethod()
    {
        return AccessTools.Method("MiraAPI.Colors.PaletteManager:RegisterAllColors");
    }

    public static void Postfix()
    {
        var ordered = MiraOverloadedColorRegistry.CustomColors;
        if (ordered.Count == 0)
        {
            return;
        }

        var names = new List<StringNames>(Palette.ColorNames);
        var player = new List<Color32>(Palette.PlayerColors);
        var shadow = new List<Color32>(Palette.ShadowColors);
        var text = new List<Color32>(Palette.TextColors);
        var textOutline = new List<Color32>(Palette.TextOutlineColors);

        var orderIndices = new List<int>(ordered.Count);
        var customSet = new HashSet<int>();

        foreach (var color in ordered)
        {
            var idx = names.IndexOf(color.Name);
            if (idx < 0)
            {
                continue;
            }

            if (customSet.Add(idx))
            {
                orderIndices.Add(idx);
            }
        }

        if (orderIndices.Count == 0)
        {
            return;
        }

        var newNames = new List<StringNames>(names.Count);
        var newPlayer = new List<Color32>(player.Count);
        var newShadow = new List<Color32>(shadow.Count);
        var newText = new List<Color32>(text.Count);
        var newTextOutline = new List<Color32>(textOutline.Count);

        for (var i = 0; i < names.Count; i++)
        {
            if (customSet.Contains(i))
            {
                continue;
            }

            newNames.Add(names[i]);
            newPlayer.Add(player[i]);
            newShadow.Add(shadow[i]);
            newText.Add(text[i]);
            newTextOutline.Add(textOutline[i]);
        }

        foreach (var idx in orderIndices)
        {
            newNames.Add(names[idx]);
            newPlayer.Add(player[idx]);
            newShadow.Add(shadow[idx]);
            newText.Add(text[idx]);
            newTextOutline.Add(textOutline[idx]);
        }

        Palette.ColorNames = newNames.ToArray();
        Palette.PlayerColors = newPlayer.ToArray();
        Palette.ShadowColors = newShadow.ToArray();
        Palette.TextColors = newText.ToArray();
        Palette.TextOutlineColors = newTextOutline.ToArray();
    }
}
