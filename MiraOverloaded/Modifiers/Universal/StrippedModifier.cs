using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using MiraOverloaded.Assets;
using MiraOverloaded.Options.Modifiers.Universal;
using MiraOverloaded.Roles.Neutral;
using TownOfUs.Modifiers.Game;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraOverloaded.Modifiers.Universal;

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
                    Player.RpcChangeRole((ushort)RoleTypes.Crewmate);
                    break;
                case NeutralBenignBehavier.BecomeFakePostor:
                    Player.RpcChangeRole((ushort)RoleId.Get<FakePostorRole>());
                    break;
                case NeutralBenignBehavier.BecomeAmni:
                    Player.RpcChangeRole((ushort)RoleId.Get<AmnesiacRole>());
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
                case NeutralBehavier.BecomeFakePostor:
                    Player.RpcChangeRole((ushort)RoleId.Get<FakePostorRole>());
                    break;
                case NeutralBehavier.BecomeAmni:
                    Player.RpcChangeRole((ushort)RoleId.Get<AmnesiacRole>());
                    break;
                default:
                    Player.Die(DeathReason.Kill, true);
                    break;
            }
        }
        else if (isImp)
        {
            Player.RpcChangeRole((ushort)RoleTypes.Impostor);
        }
        else
        {
            Player.RpcChangeRole((ushort)RoleTypes.Crewmate);
        }
    }
}
