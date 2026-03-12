using MiraAPI.Modifiers.Types;
using UnityEngine;

namespace MiraModule.Modifiers;

public sealed class AgentAwareModifier : GameModifier
{
    public override string ModifierName => "Agent Aware";

    public override Color FreeplayFileColor => new Color32(220, 220, 220, 255);

    public override bool HideOnUi => false;

    public override int GetAmountPerGame() => 0;

    public override int GetAssignmentChance() => 0;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }
}