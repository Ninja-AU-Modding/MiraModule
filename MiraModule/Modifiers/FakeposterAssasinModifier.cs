using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraModule.Options.Roles.Neutral;
using MiraModule.Roles.Neutral;
using TownOfUs.Modifiers.Game;
using UnityEngine;

namespace MiraModule.Modifiers;

public sealed class FakeposterAssassinModifier : AssassinModifier
{
    public override string ModifierName => "Assassin";
    public override Color FreeplayFileColor => MiraModuleColors.Fakeposter;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.Role == (RoleTypes)RoleId.Get<FakeposterRole>();
    }

    public override int GetAmountPerGame() =>
        OptionGroupSingleton<FakeposterOptions>.Instance.HasAssassin ? 15 : 0;

    public override int GetAssignmentChance() =>
        OptionGroupSingleton<FakeposterOptions>.Instance.HasAssassin ? 100 : 0;
}
