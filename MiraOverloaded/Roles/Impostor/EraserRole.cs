using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraOverloaded.Assets;
using MiraOverloaded.Buttons.Impostor;
using MiraOverloaded.Options.Roles.Impostor;
using MiraAPI.Patches.Freeplay;
using AmongUs.GameOptions;
using UnityEngine;
using Reactor.Utilities;

namespace MiraOverloaded.Roles.Impostor;

public sealed class EraserRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    /// <summary>
    /// Maps victim PlayerId → EraserPlayerId for pending erases (applied at next meeting).
    /// </summary>
    internal static readonly Dictionary<byte, byte> PendingErases = new();

    /// <summary>
    /// All PlayerId's that have been fully erased (post-meeting), used to scale cooldown.
    /// </summary>
    internal static readonly List<byte> ErasedPlayerIds = new();

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

    public Color RoleColor => MiraOverloadedColors.Eraser;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = RoleIcons.Eraser,
        GhostRole = RoleTypes.ImpostorGhost,
        UseVanillaKillButton = true,
        CanUseSabotage = true,
        CanUseVent = OptionGroupSingleton<EraserOptions>.Instance.CanVent,
        FreeplayFolder = TaskAdderPatches.ImpostorName,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(CustomButtonSingleton<EraserEraseButton>.Instance, !OptionGroupSingleton<EraserOptions>.Instance.CanVent));
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(MiraOverloadedColors.Eraser);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
        }
    }
}
