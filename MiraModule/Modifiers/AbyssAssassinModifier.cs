using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraModule.Options.Roles.Neutral;
using MiraModule.Roles.Neutral;
using TownOfUs.Modifiers.Game;
using UnityEngine;

namespace MiraModule.Modifiers;

/// <summary>
/// Gives the Abyss the Assassin ability during meetings (role-guessing kill).
/// MiraAPI auto-assigns this to any AbyssRole player when HasAssassin is enabled.
/// </summary>
public sealed class AbyssAssassinModifier : AssassinModifier
{
    public override string ModifierName => "Assassin";
    public override Color FreeplayFileColor => MiraModuleColors.Abyss;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.Role == (RoleTypes)RoleId.Get<AbyssRole>();
    }

    public override int GetAmountPerGame() =>
        OptionGroupSingleton<AbyssOptions>.Instance.HasAssassin ? 15 : 0;

    public override int GetAssignmentChance() =>
        OptionGroupSingleton<AbyssOptions>.Instance.HasAssassin ? 100 : 0;
}
