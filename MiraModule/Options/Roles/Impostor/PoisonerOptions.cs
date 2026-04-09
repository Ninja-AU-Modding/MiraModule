using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraModule.Roles.Impostor;

namespace MiraModule.Options.Roles.Impostor;

public enum PoisonerMeetingAction : uint
{
    SavesVictims = 0,
    KillsAllVictims = 1,
    KillsFirstVictim = 2
}

public sealed class PoisonerOptions : AbstractOptionGroup<PoisonerRole>
{
    public override string GroupName => TouLocale.Get("MiraRolePoisoner", "Poisoner");

    [ModdedNumberOption("MiraOptionPoisonerTimer", 1f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float PoisonTimer { get; set; } = 10f;

    [ModdedEnumOption("MiraOptionPoisonerMeetingAction", typeof(PoisonerMeetingAction))]
    public PoisonerMeetingAction MeetingAction { get; set; } = PoisonerMeetingAction.SavesVictims;

    [ModdedNumberOption("MiraOptionPoisonerKillCooldown", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    [ModdedToggleOption("MiraOptionPoisonerCanVent")]
    public bool CanVent { get; set; } = true;
}
