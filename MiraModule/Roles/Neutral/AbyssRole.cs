using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraModule.Modifiers;
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

public sealed class AbyssRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    /// <summary>
    /// Tracks all players currently swallowed by any active Abyss instance.
    /// Keyed by victim PlayerId → Abyss PlayerId.
    /// </summary>
    public static readonly Dictionary<byte, byte> SwallowedPlayers = new();

    public DoomableType DoomHintType => DoomableType.Relentless;
    public string LocaleKey => "Abyss";
    public string RoleName => TouLocale.Get($"MiraRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"MiraRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"MiraRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"MiraRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"MiraRole{LocaleKey}Gulp", "Gulp"),
                    TouLocale.GetParsed($"MiraRole{LocaleKey}GulpWikiDescription"),
                    NeutAssets.AbyssGulpSprite),
            };
        }
    }

    public Color RoleColor => MiraModuleColors.Abyss;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<AbyssOptions>.Instance.CanVent,
        IntroSound = TouAudio.GlitchSound,
        Icon = RoleIcons.Abyss,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool HasImpostorVision => OptionGroupSingleton<AbyssOptions>.Instance.ImpostorVision;

    /// <summary>
    /// Returns how many players this specific Abyss has swallowed.
    /// </summary>
    public int SwallowCount => SwallowedPlayers.Values.Count(id => id == Player.PlayerId);

    public bool WinConditionMet()
    {
        var abyssCount = CustomRoleUtils.GetActiveRolesOfType<AbyssRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > abyssCount)
        {
            return false;
        }

        // Win when all other alive players are gone (swallowed + dead cover everyone else)
        return abyssCount >= Helpers.GetAlivePlayers().Count - abyssCount;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(CustomButtonSingleton<AbyssGulpButton>.Instance, !OptionGroupSingleton<AbyssOptions>.Instance.CanVent));
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(MiraModuleColors.Abyss);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
        }

        // Release any swallowed players — snap them to the Abyss's death position first,
        // then remove their modifier (which restores camera/HUD/movement/appearance).
        var myId = targetPlayer.PlayerId;
        var deathPos = targetPlayer.GetTruePosition();
        var keys = SwallowedPlayers.Where(kv => kv.Value == myId).Select(kv => kv.Key).ToList();
        foreach (var victimId in keys)
        {
            var victim = MiscUtils.PlayerById(victimId);
            if (victim == null) continue;

            // Teleport victim to where the Abyss died before releasing them
            victim.NetTransform.SnapTo(deathPos);

            if (victim.HasModifier<SwallowedModifier>())
            {
                victim.RpcRemoveModifier<SwallowedModifier>();
            }

            // Force visibility on — register in ReleasingPlayers so the patch allows it through
            SwallowedModifier.ReleasingPlayers.Add(victimId);
            victim.Visible = true;
            SwallowedModifier.ReleasingPlayers.Remove(victimId);
        }
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }
}
