using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraModule.Roles.Impostor;
using TownOfUs.Modules.Localization;

namespace MiraModule.Options.Roles.Impostor;

/// <summary>
/// Describes what happens to a Neutral player who gets erased.
/// </summary>
public enum NeutralEraseOutcome
{
    BecomeCrewmate = 0,
    Die = 1,
}

public sealed class EraserOptions : AbstractOptionGroup<EraserRole>
{
    public override string GroupName => TouLocale.Get("MiraRoleEraser", "Eraser");

    [ModdedNumberOption("MiraOptionEraserEraseCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float EraseCooldown { get; set; } = 30f;

    [ModdedNumberOption("MiraOptionEraserCooldownIncrease", 0f, 60f, 5f, MiraNumberSuffixes.Seconds)]
    public float CooldownIncrease { get; set; } = 10f;

    [ModdedToggleOption("MiraOptionEraserCanEraseImpostors")]
    public bool CanEraseImpostors { get; set; } = false;

    [ModdedEnumOption("MiraOptionEraserNeutralOutcome", typeof(NeutralEraseOutcome))]
    public NeutralEraseOutcome NeutralOutcome { get; set; } = NeutralEraseOutcome.BecomeCrewmate;

    [ModdedToggleOption("MiraOptionEraserCanVent")]
    public bool CanVent { get; set; } = false;

    [ModdedToggleOption("MiraOptionEraserHasAssassin")]
    public bool HasAssassin { get; set; } = true;
}
