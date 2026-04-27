using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraOverloaded.Modifiers.Universal;
using UnityEngine;

namespace MiraOverloaded.Options.Modifiers.Universal;

public enum NeutralBenignBehavier: uint
{
    BecomeCrewmate = 0,
    BecomeFakeposter = 1,
    Die = 2,
    BecomeAmni = 3,

}

public enum NeutralBehavier: uint
{
    BecomeFakeposter = 1,
    BecomeAmni = 2,
    Die = 3
}
public sealed class StrippedOptions : AbstractOptionGroup<ExplosiveModifier>
{
    public override string GroupName => "Stripped";
    public override uint GroupPriority => 52;
    public override Color GroupColor => new Color32(255, 255, 255, 255);

    public override Func<bool> GroupVisible =>
        () => true;

    [ModdedEnumOption("Neutral Benigns", typeof(NeutralBenignBehavier))]
    public NeutralBenignBehavier NeutralBenign { get; set; } = NeutralBenignBehavier.BecomeCrewmate;

    [ModdedEnumOption("Other Neutrals", typeof(NeutralBehavier))]
    public NeutralBehavier NeutralOther { get; set; } = NeutralBehavier.BecomeFakeposter;
}
