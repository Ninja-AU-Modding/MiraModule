using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraModule.Assets;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace MiraModule.Roles.Neutral;

public sealed class ShifterRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    /// <summary>
    /// Maps ShifterPlayerId → TargetPlayerId for pending shifts (applied at next meeting end).
    /// Persists through death — the shift always goes through.
    /// </summary>
    public static readonly Dictionary<byte, byte> PendingShifts = new();

    public string LocaleKey => "Shifter";
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
                new(TouLocale.GetParsed($"MiraRole{LocaleKey}Shift", "Shift"),
                    TouLocale.GetParsed($"MiraRole{LocaleKey}ShiftWikiDescription"),
                    NeutAssets.ShifterShiftSprite),
            };
        }
    }

    public Color RoleColor => MiraModuleColors.Shifter;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.MediumIntroSound,
        Icon = RoleIcons.Shifter,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        // Shifter wins if they successfully shifted into a winning role.
        // The win is handled externally by whatever role they obtained.
        return false;
    }
}
