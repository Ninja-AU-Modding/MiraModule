using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using MiraAPI.Colors;

namespace MiraOverloaded.Patches.Gradient;

[HarmonyPatch]
public static class CustomColorRegistrationPatch
{
    [HarmonyTargetMethod]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by Harmony via reflection")]
    private static System.Reflection.MethodInfo? TargetMethod()
    {
        return AccessTools.Method("MiraAPI.Colors.PaletteManager:RegisterAllColors");
    }

    public static void Prefix()
    {
        MiraOverloadedColorRegistry.EnsureInitialized();

        var field = AccessTools.Field(typeof(PaletteManager), "CustomColors");
        if (field?.GetValue(null) is not List<CustomColor> list)
        {
            return;
        }

        var existing = new HashSet<StringNames>();
        foreach (var color in list)
        {
            existing.Add(color.Name);
        }

        foreach (var color in MiraOverloadedColorRegistry.CustomColors.Where(color => !existing.Contains(color.Name)))
        {
            list.Add(color);
        }
    }
}
