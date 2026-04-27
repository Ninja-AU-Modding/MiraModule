using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraOverloaded.Options.Roles.Neutral;
using MiraOverloaded.Roles.Neutral;
using TownOfUs.Modifiers.Game;
using UnityEngine;

namespace MiraOverloaded.Modifiers;

/// <summary>
/// Gives the Abyss the Assassin ability during meetings (role-guessing kill).
/// MiraAPI auto-assigns this to any AbyssRole player when HasAssassin is enabled.
/// </summary>
public sealed class AbyssAssassinModifier : AssassinModifier
{
    public override string ModifierName => "Assassin";
    public override Color FreeplayFileColor => MiraOverloadedColors.Abyss;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.Role == (RoleTypes)RoleId.Get<AbyssRole>();
    }

    public override int GetAmountPerGame() =>
        OptionGroupSingleton<AbyssOptions>.Instance.HasAssassin ? 15 : 0;

    public override int GetAssignmentChance() =>
        OptionGroupSingleton<AbyssOptions>.Instance.HasAssassin ? 100 : 0;
}
