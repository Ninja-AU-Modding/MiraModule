using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using MiraModule.Assets;
using MiraModule.Buttons.Neutral;
using MiraModule.Options.Roles.Neutral;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraModule.Roles.Neutral;

public sealed class PelicanRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    /// <summary>
    /// Tracks all players currently swallowed by any active Pelican instance.
    /// Keyed by victim PlayerId → Pelican PlayerId.
    /// </summary>
    public static readonly Dictionary<byte, byte> SwallowedPlayers = new();

    public DoomableType DoomHintType => DoomableType.Relentless;
    public string LocaleKey => "Pelican";
    public string RoleName => TouLocale.Get($"ExampleRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"ExampleRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"ExampleRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"ExampleRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"ExampleRole{LocaleKey}Gulp", "Gulp"),
                    TouLocale.GetParsed($"ExampleRole{LocaleKey}GulpWikiDescription"),
                    ExampleNeutAssets.PelicanGulpSprite),
            };
        }
    }

    public Color RoleColor => MiraModuleColors.Pelican;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<PelicanOptions>.Instance.CanVent,
        IntroSound = TouAudio.GlitchSound,
        Icon = ExampleRoleIcons.Pelican,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool HasImpostorVision => OptionGroupSingleton<PelicanOptions>.Instance.ImpostorVision;

    /// <summary>
    /// Returns how many players this specific Pelican has swallowed.
    /// </summary>
    public int SwallowCount => SwallowedPlayers.Values.Count(id => id == Player.PlayerId);

    public bool WinConditionMet()
    {
        var pelicanCount = CustomRoleUtils.GetActiveRolesOfType<PelicanRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > pelicanCount)
        {
            return false;
        }

        // Win when all other alive players are gone (swallowed + dead cover everyone else)
        return pelicanCount >= Helpers.GetAlivePlayers().Count - pelicanCount;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(CustomButtonSingleton<PelicanGulpButton>.Instance, !OptionGroupSingleton<PelicanOptions>.Instance.CanVent));
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(MiraModuleColors.Pelican);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
        }

        // Release any swallowed players if this Pelican is dying/leaving
        var myId = targetPlayer.PlayerId;
        var keys = SwallowedPlayers.Where(kv => kv.Value == myId).Select(kv => kv.Key).ToList();
        foreach (var victimId in keys)
        {
            SwallowedPlayers.Remove(victimId);
            var victim = MiscUtils.PlayerById(victimId);
            if (victim != null && victim.HasDied())
            {
                // Revive the swallowed player so they can return
                victim.Revive();
            }
        }
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }
}
