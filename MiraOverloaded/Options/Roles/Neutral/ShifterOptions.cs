using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraOverloaded.Roles.Neutral;
using TownOfUs.Modules.Localization;

namespace MiraOverloaded.Options.Roles.Neutral;

public sealed class ShifterOptions : AbstractOptionGroup<ShifterRole>
{
    public override string GroupName => TouLocale.Get("MiraRoleShifter", "Shifter");

    [ModdedNumberOption("MiraOptionShifterShiftCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ShiftCooldown { get; set; } = 30f;

    [ModdedToggleOption("MiraOptionShifterShiftDeadPlayers")]
    public bool CanShiftDeadPlayers { get; set; } = false;
}
