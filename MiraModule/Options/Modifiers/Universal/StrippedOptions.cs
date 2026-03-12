using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraModule.Modifiers.Universal;
using UnityEngine;

namespace MiraModule.Options.Modifiers.Universal;

public enum NeutralBenignBehavier: uint
{
    BecomeCrewmate = 0,
    BecomeFakeposter = 1,
    Die = 2

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
}
