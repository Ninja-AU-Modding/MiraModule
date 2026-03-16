using AmongUs.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Extensions;
using UnityEngine;

namespace MiraModule.Modifiers.Neutral;

public sealed class HarvesterCacheModifier : BaseModifier, ICachedRole
{
    public override string ModifierName => "Harvester Cache";
    public override bool HideOnUi => true;
    public bool ShowCurrentRoleFirst => true;

    public bool Visible => Player.AmOwner || PlayerControl.LocalPlayer.HasDied();
    public CacheRoleGuess GuessMode => CacheRoleGuess.ActiveOrCachedRole;
    public RoleBehaviour CachedRole => RoleManager.Instance.GetRole((RoleTypes)CachedRoleId);

    public ushort CachedRoleId { get; }

    public HarvesterCacheModifier(ushort cachedRoleId)
    {
        CachedRoleId = cachedRoleId;
    }
}
