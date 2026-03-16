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
    };

    // For roles listed here, ONLY these buttons are allowed.
    // Roles not present in this map are left unchanged.
    private static readonly Dictionary<Type, HashSet<Type>> AllowedButtonsByRole = new()
    {
        // Crewmate
        { typeof(AltruistRole), new HashSet<Type> { typeof(AltruistReviveButton) } },
        { typeof(ClericRole), new HashSet<Type> { typeof(ClericBarrierButton), typeof(ClericCleanseButton) } },
        { typeof(EngineerTouRole), new HashSet<Type> { typeof(EngineerFixButton) } },
        { typeof(ForensicRole), new HashSet<Type> { typeof(DetectiveInspectButton), typeof(DetectiveExamineButton) } },
        { typeof(LookoutRole), new HashSet<Type> { typeof(WatchButton) } },
        { typeof(MedicRole), new HashSet<Type> { typeof(MedicShieldButton) } },
        { typeof(MediumRole), new HashSet<Type> { typeof(MediumMediateButton) } },
        { typeof(MysticRole), new HashSet<Type>() },
        { typeof(OracleRole), new HashSet<Type> { typeof(OracleConfessButton) } },
        { typeof(PlumberRole), new HashSet<Type> { typeof(PlumberFlushButton), typeof(PlumberBlockButton) } },
        { typeof(SeerRole), new HashSet<Type> { typeof(SeerRevealButton), typeof(SeerGazeButton), typeof(SeerIntuitButton) } },
        { typeof(SentryRole), new HashSet<Type>
            {
                typeof(SentryPlaceCameraButton),
                typeof(SentryPortableCameraButton),
                typeof(SentryPortableCameraSecondaryButton),
            }
        },
        { typeof(SonarRole), new HashSet<Type> { typeof(TrackerTrackButton) } },
        { typeof(SpyRole), new HashSet<Type>() },
        { typeof(TimeLordRole), new HashSet<Type> { typeof(TimeLordRewindButton) } },
        { typeof(TransporterRole), new HashSet<Type> { typeof(TransporterTransportButton) } },
        { typeof(TrapperRole), new HashSet<Type> { typeof(TrapperTrapButton) } },
        { typeof(VeteranRole), new HashSet<Type> { typeof(VeteranAlertButton) } },
        { typeof(WardenRole), new HashSet<Type> { typeof(WardenFortifyButton) } },

        // Impostor
        { typeof(AmbusherRole), new HashSet<Type> { typeof(AmbusherPursueButton), typeof(AmbusherAmbushButton) } },
        { typeof(BlackmailerRole), new HashSet<Type> { typeof(BlackmailerBlackmailButton) } },
        { typeof(BomberRole), new HashSet<Type> { typeof(BomberPlantButton) } },
        { typeof(EclipsalRole), new HashSet<Type> { typeof(EclipsalBlindButton) } },
        { typeof(EscapistRole), new HashSet<Type> { typeof(EscapistMarkButton), typeof(EscapistRecallButton) } },
        { typeof(GrenadierRole), new HashSet<Type> { typeof(GrenadierFlashButton) } },
        { typeof(JanitorRole), new HashSet<Type> { typeof(JanitorCleanButton) } },
        { typeof(MinerRole), new HashSet<Type> { typeof(MinerPlaceVentButton) } },
        { typeof(MorphlingRole), new HashSet<Type> { typeof(MorphlingSampleButton), typeof(MorphlingMorphButton) } },
        { typeof(ParasiteRole), new HashSet<Type> { typeof(ParasiteOvertakeButton) } },
        { typeof(PuppeteerRole), new HashSet<Type> { typeof(PuppeteerControlButton) } },
        { typeof(ScavengerRole), new HashSet<Type>() },
        { typeof(SwooperRole), new HashSet<Type> { typeof(SwooperSwoopButton) } },
        { typeof(UndertakerRole), new HashSet<Type> { typeof(UndertakerDragDropButton) } },
        { typeof(VenererRole), new HashSet<Type> { typeof(VenererAbilityButton) } },
        { typeof(WarlockRole), new HashSet<Type> { typeof(WarlockKillButton) } },

        // Neutral
        { typeof(ChefRole), new HashSet<Type> { typeof(ChefCookButton) } },
        { typeof(DoomsayerRole), new HashSet<Type> { typeof(DoomsayerObserveButton) } },
        { typeof(GlitchRole), new HashSet<Type> { typeof(GlitchMimicButton), typeof(GlitchHackButton) } },
        { typeof(JuggernautRole), new HashSet<Type>() },
        { typeof(MercenaryRole), new HashSet<Type> { typeof(MercenaryGuardButton), typeof(MercenaryBribeButton) } },
        { typeof(NeutralRole), new HashSet<Type>() },
        { typeof(PestilenceRole), new HashSet<Type>() },
        { typeof(PlaguebearerRole), new HashSet<Type> { typeof(PlaguebearerInfectButton) } },
        { typeof(SoulCollectorRole), new HashSet<Type> { typeof(SoulCollectorReapButton) } },
        { typeof(SurvivorRole), new HashSet<Type> { typeof(SurvivorVestButton) } },
        { typeof(WerewolfRole), new HashSet<Type> { typeof(WerewolfRampageButton) } },
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

    public static bool ShouldAllowButton(RoleBehaviour? role, TownOfUsButton button)
    {
        if (role == null)
        {
            return true;
        }

        if (!AllowedButtonsByRole.TryGetValue(role.GetType(), out var allowedButtons))
        {
            return true;
        }

        return allowedButtons.Contains(button.GetType());
    }
}
