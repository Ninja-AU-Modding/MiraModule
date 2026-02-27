using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraModule.Assets;
using MiraModule.Buttons.Impostor;
using MiraModule.Options.Roles.Impostor;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraModule.Roles.Impostor;

public sealed class EraserRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    /// <summary>
    /// Maps victim PlayerId → EraserPlayerId for pending erases (applied at next meeting).
    /// </summary>
    public static readonly Dictionary<byte, byte> PendingErases = new();

    /// <summary>
    /// All PlayerId's that have been fully erased (post-meeting), used to scale cooldown.
    /// </summary>
    public static readonly List<byte> ErasedPlayerIds = new();

    public string LocaleKey => "Eraser";
    public string RoleName => TouLocale.Get($"MiraRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"MiraRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"MiraRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"MiraRole{LocaleKey}WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"MiraRole{LocaleKey}Erase", "Erase"),
                    TouLocale.GetParsed($"MiraRole{LocaleKey}EraseWikiDescription"),
                    ImpostorAssets.EraserEraseSprite),
            };
        }
    }

    public Color RoleColor => MiraModuleColors.Eraser;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = RoleIcons.Eraser,
        GhostRole = RoleTypes.ImpostorGhost,
        UseVanillaKillButton = true,
        CanUseVent = OptionGroupSingleton<EraserOptions>.Instance.CanVent,
        CanUseSabotage = true,
    };

    /// <summary>
    /// How many erases this specific Eraser has queued or confirmed — used for cooldown scaling.
    /// </summary>
    public int TotalErasesDoneByMe =>
        PendingErases.Values.Count(id => id == Player.PlayerId) +
        ErasedPlayerIds.Count; // shared global total; we track globally since one eraser per game is typical

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
    }
}
