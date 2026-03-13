using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using MiraModule.Modifiers.Universal;
using UnityEngine;

namespace MiraModule.Options.Modifiers.Universal;

public sealed class ExplosiveOptions : AbstractOptionGroup<ExplosiveModifier>
{
    public override string GroupName => "Explosive";
    public override uint GroupPriority => 50;
    public override Color GroupColor => new Color32(255, 120, 20, 255);

    public override Func<bool> GroupVisible =>
        () => OptionGroupSingleton<ExplosiveModifierOptions>.Instance.ExplosiveAmount > 0;

    [ModdedNumberOption("Kill Distance", 5f, 20f, 1f, MiraNumberSuffixes.None, "0")]
    public float KillDistance { get; set; } = 10f;

    [ModdedNumberOption("Explosive Duration", 40f, 60f, 1f, MiraNumberSuffixes.Seconds, "0")]
    public float ExplosiveDuration { get; set; } = 50f;

    [ModdedNumberOption("Max Kills", 1f, 10f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxKills { get; set; } = 3f;
}
