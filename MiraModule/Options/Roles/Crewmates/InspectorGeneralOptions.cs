using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraModule.Roles.Crewmate;

namespace MiraModule.Options.Roles.Crewmates;

public sealed class InspectorGeneralOptions : AbstractOptionGroup<InspectorGeneralRole>
{
    public override string GroupName => TouLocale.Get("MiraRoleInspectorGeneral", "Inspector General");

    [ModdedNumberOption("MiraOptionInspectorGeneralKillCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    [ModdedNumberOption("MiraOptionInspectorGeneralConvertCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ConvertCooldown { get; set; } = 30f;

    [ModdedNumberOption("MiraOptionInspectorGeneralMaxConversions", 1f, 5f, 1f, MiraNumberSuffixes.None)]
    public float MaxConversions { get; set; } = 2f;

    [ModdedNumberOption("MiraOptionCommandSpecialistKillCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CommandSpecialistKillCooldown { get; set; } = 25f;
}
