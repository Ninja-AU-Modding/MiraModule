using TownOfUs.Modifiers;

namespace MiraOverloaded.Modifiers.Crewmate;

public sealed class InspectorGeneralLinkRevealModifier : BaseRevealModifier
{
    public override string ModifierName => "Command Link";
    public override ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.Nothing;
    public override bool RevealRole { get; set; } = true;
}
