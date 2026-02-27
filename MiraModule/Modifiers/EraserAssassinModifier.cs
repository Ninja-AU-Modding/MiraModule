using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraModule.Options.Roles.Impostor;
using MiraModule.Roles.Impostor;
using TownOfUs.Modifiers.Game;
using UnityEngine;

namespace MiraModule.Modifiers;

/// <summary>
/// Gives the Eraser the Assassin ability during meetings (role-guessing kill).
/// MiraAPI auto-assigns this to any EraserRole player when HasAssassin is enabled.
/// </summary>
public sealed class EraserAssassinModifier : AssassinModifier
{
    public override string ModifierName => "Assassin";
    public override Color FreeplayFileColor => MiraModuleColors.Eraser;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.Role == (RoleTypes)RoleId.Get<EraserRole>();
    }

    public override int GetAmountPerGame() =>
        OptionGroupSingleton<EraserOptions>.Instance.HasAssassin ? 15 : 0;

    public override int GetAssignmentChance() =>
        OptionGroupSingleton<EraserOptions>.Instance.HasAssassin ? 100 : 0;
}
