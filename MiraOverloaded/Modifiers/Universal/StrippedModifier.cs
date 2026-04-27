using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Options.Modifiers.Universal;
using MiraModule.Roles.Neutral;
using TownOfUs.Modifiers.Game;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace MiraModule.Modifiers.Universal;

public sealed class StrippedModifier : UniversalGameModifier
{
    public override string LocaleKey => "Stripped";
    public override string ModifierName => "Stripped";
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(255, 255, 255, 255);
    public override LoadableAsset<Sprite>? ModifierIcon => ModifierIcons.Stripped;

    public override string GetDescription() =>
        "Lose all abilities";

    public override int GetAssignmentChance() => 0;

    public override int GetAmountPerGame() => 0;

    public override void OnActivate()
    {
        var currentRole = Player.Data.Role;

        if (currentRole is not ICustomRole customRole)
        {
            Player.Die(DeathReason.Kill, true);
            return;
        }

        bool isNeutral = customRole.Team == ModdedRoleTeams.Custom;
        bool isBenign = isNeutral && customRole.GetRoleAlignment() == RoleAlignment.NeutralBenign;
        bool isImp = customRole.Team == ModdedRoleTeams.Impostor;

        if (isBenign)
        {
            switch (OptionGroupSingleton<StrippedOptions>.Instance.NeutralBenign)
            {
                case NeutralBenignBehavier.BecomeCrewmate:
                    Player.RpcSetRole(RoleTypes.Crewmate);
                    break;
                case NeutralBenignBehavier.BecomeFakeposter:
                    Player.RpcSetRole((RoleTypes)RoleId.Get<FakeposterRole>());
                    break;
                case NeutralBenignBehavier.BecomeAmni:
                    Player.RpcSetRole((RoleTypes)RoleId.Get<AmnesiacRole>());
                    break;
                default:
                    Player.Die(DeathReason.Kill, true);
                    break;

            }
        }
        else if (isNeutral) // ONLY a neutral. Not a benign role
        {
            switch (OptionGroupSingleton<StrippedOptions>.Instance.NeutralOther)
            {
                case NeutralBehavier.BecomeFakeposter:
                    Player.RpcSetRole((RoleTypes)RoleId.Get<FakeposterRole>());
                    break;
                case NeutralBehavier.BecomeAmni:
                    Player.RpcSetRole((RoleTypes)RoleId.Get<AmnesiacRole>());
                    break;
                default:
                    Player.Die(DeathReason.Kill, true);
                    break;
            }
        }
        else if (isImp)
        {
            Player.RpcSetRole(RoleTypes.Impostor);
        }
        else
        {
            Player.RpcSetRole(RoleTypes.Crewmate);
        }
    }
}
