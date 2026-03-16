using System;
using System.Collections.Generic;
using MiraAPI.Roles;
using TownOfUs.Buttons;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Roles.Neutral;

namespace MiraModule.Rules;

public static class TouRoleRestrictions
{
    // Toggleable behavior tweaks
    public const bool FakeMedicShield = true;
    public const bool FakeWardenFortify = true;
    public const bool AltruistDoesNotDieOnRevive = true;

    // Roles that should never be assigned.
    public static readonly HashSet<ushort> DisallowedRoleIds = new()
    {
        RoleId.Get<AurialRole>(),
        RoleId.Get<DeputyRole>(),
        RoleId.Get<HaunterRole>(),
        RoleId.Get<HunterRole>(),
        RoleId.Get<JailorRole>(),
        RoleId.Get<MayorRole>(),
        RoleId.Get<MirrorcasterRole>(),
        RoleId.Get<MonarchRole>(),
        RoleId.Get<PoliticianRole>(),
        RoleId.Get<ProsecutorRole>(),
        RoleId.Get<SheriffRole>(),
        RoleId.Get<SnitchRole>(),
        RoleId.Get<SwapperRole>(),
        RoleId.Get<AmbassadorRole>(),
        RoleId.Get<HypnotistRole>(),
        RoleId.Get<SpellslingerRole>(),
        RoleId.Get<TraitorRole>(),
        RoleId.Get<AmnesiacRole>(),
        RoleId.Get<ArsonistRole>(),
        RoleId.Get<ExecutionerRole>(),
        RoleId.Get<FairyRole>(),
        RoleId.Get<JesterRole>(),
        RoleId.Get<SpectreRole>(),
        RoleId.Get<VampireRole>(),
        RoleId.Get<VigilanteRole>(),
        RoleId.Get<TimeLordRole>(),
        RoleId.Get<PlumberRole>(),
    };

    // For roles listed here, ONLY these buttons are allowed.
    // Roles not present in this map are left unchanged.
    private static readonly Dictionary<Type, HashSet<string>> AllowedButtonsByRole = new()
    {
        // Crewmate
        { typeof(AltruistRole), new HashSet<string> { nameof(AltruistReviveButton) } },
        { typeof(ClericRole), new HashSet<string> { nameof(ClericBarrierButton), nameof(ClericCleanseButton) } },
        { typeof(EngineerTouRole), new HashSet<string> { nameof(EngineerFixButton) } },
        { typeof(ForensicRole), new HashSet<string> { nameof(DetectiveInspectButton), nameof(DetectiveExamineButton) } },
        { typeof(LookoutRole), new HashSet<string> { nameof(WatchButton) } },
        { typeof(MedicRole), new HashSet<string> { nameof(MedicShieldButton) } },
        { typeof(MediumRole), new HashSet<string> { nameof(MediumMediateButton) } },
        { typeof(MysticRole), new HashSet<string>() },
        { typeof(OracleRole), new HashSet<string> { nameof(OracleConfessButton) } },
        { typeof(SeerRole), new HashSet<string> { nameof(SeerRevealButton), nameof(SeerGazeButton), nameof(SeerIntuitButton) } },
        { typeof(SentryRole), new HashSet<string>
            {
                nameof(SentryPlaceCameraButton),
                nameof(SentryPortableCameraButton),
                nameof(SentryPortableCameraSecondaryButton),
            }
        },
        { typeof(SonarRole), new HashSet<string> { nameof(TrackerTrackButton) } },
        { typeof(SpyRole), new HashSet<string>() },
        { typeof(TimeLordRole), new HashSet<string> { nameof(TimeLordRewindButton) } },
        { typeof(TransporterRole), new HashSet<string> { nameof(TransporterTransportButton) } },
        { typeof(TrapperRole), new HashSet<string> { nameof(TrapperTrapButton) } },
        { typeof(VeteranRole), new HashSet<string> { nameof(VeteranAlertButton) } },
        { typeof(WardenRole), new HashSet<string> { nameof(WardenFortifyButton) } },

        // Impostor
        { typeof(AmbusherRole), new HashSet<string> { nameof(AmbusherPursueButton), nameof(AmbusherAmbushButton) } },
        { typeof(BlackmailerRole), new HashSet<string> { nameof(BlackmailerBlackmailButton) } },
        { typeof(BomberRole), new HashSet<string> { nameof(BomberPlantButton) } },
        { typeof(EclipsalRole), new HashSet<string> { nameof(EclipsalBlindButton) } },
        { typeof(EscapistRole), new HashSet<string> { nameof(EscapistMarkButton), nameof(EscapistRecallButton) } },
        { typeof(GrenadierRole), new HashSet<string> { nameof(GrenadierFlashButton) } },
        { typeof(JanitorRole), new HashSet<string> { nameof(JanitorCleanButton) } },
        { typeof(MinerRole), new HashSet<string> { nameof(MinerPlaceVentButton) } },
        { typeof(MorphlingRole), new HashSet<string> { nameof(MorphlingSampleButton), nameof(MorphlingMorphButton) } },
        { typeof(ParasiteRole), new HashSet<string> { nameof(ParasiteOvertakeButton) } },
        { typeof(PuppeteerRole), new HashSet<string> { nameof(PuppeteerControlButton) } },
        { typeof(ScavengerRole), new HashSet<string>() },
        { typeof(SwooperRole), new HashSet<string> { nameof(SwooperSwoopButton) } },
        { typeof(UndertakerRole), new HashSet<string> { nameof(UndertakerDragDropButton) } },
        { typeof(VenererRole), new HashSet<string> { nameof(VenererAbilityButton) } },
        { typeof(WarlockRole), new HashSet<string> { nameof(WarlockKillButton) } },

        // Neutral
        { typeof(ChefRole), new HashSet<string> { nameof(ChefCookButton) } },
        { typeof(DoomsayerRole), new HashSet<string> { nameof(DoomsayerObserveButton) } },
        { typeof(GlitchRole), new HashSet<string> { nameof(GlitchMimicButton), nameof(GlitchHackButton) } },
        { typeof(JuggernautRole), new HashSet<string>() },
        { typeof(MercenaryRole), new HashSet<string> { nameof(MercenaryGuardButton), nameof(MercenaryBribeButton) } },
        { typeof(NeutralRole), new HashSet<string>() },
        { typeof(PestilenceRole), new HashSet<string>() },
        { typeof(PlaguebearerRole), new HashSet<string> { nameof(PlaguebearerInfectButton) } },
        { typeof(SoulCollectorRole), new HashSet<string> { nameof(SoulCollectorReapButton) } },
        { typeof(SurvivorRole), new HashSet<string> { nameof(SurvivorVestButton) } },
        { typeof(WerewolfRole), new HashSet<string> { nameof(WerewolfRampageButton) } },
    };

    public static bool IsRoleDisallowed(ushort roleId)
    {
        return DisallowedRoleIds.Contains(roleId);
    }

    public static void RemoveDisallowedRoles(List<ushort> roles)
    {
        if (roles.Count == 0)
        {
            return;
        }

        roles.RemoveAll(IsRoleDisallowed);
    }

    public static bool ShouldAllowButton(RoleBehaviour? role, object button)
    {
        var effectiveRole = role ?? PlayerControl.LocalPlayer?.Data?.Role;
        if (effectiveRole == null)
        {
            return true;
        }

        if (!AllowedButtonsByRole.TryGetValue(effectiveRole.GetType(), out var allowedButtons))
        {
            return true;
        }

        return allowedButtons.Contains(button.GetType().Name);
    }
}
