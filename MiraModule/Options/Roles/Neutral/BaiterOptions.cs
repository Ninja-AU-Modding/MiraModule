using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraModule.Roles.Neutral;

namespace MiraModule.Options.Roles.Neutral;

public sealed class BaiterOptions : AbstractOptionGroup<BaiterRole>
{
    public override string GroupName => TouLocale.Get("MiraRoleBaiter", "Baiter");

    [ModdedNumberOption("MiraOptionBaiterSpawnCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SpawnCooldown { get; set; } = 25f;

    [ModdedNumberOption("MiraOptionBaiterBaitsNeeded", 1f, 15f, 1f, MiraNumberSuffixes.None)]
    public float BaitsNeeded { get; set; } = 3f;

    [ModdedToggleOption("MiraOptionBaiterCanVent")]
    public bool CanVent { get; set; }

    [ModdedToggleOption("MiraOptionBaiterImpostorVision")]
    public bool ImpostorVision { get; set; } = true;

    [ModdedToggleOption("MiraOptionBaiterHasAssassin")]
    public bool HasAssassin { get; set; }
}
