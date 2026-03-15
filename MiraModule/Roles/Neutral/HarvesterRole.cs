using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.LocalSettings;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Buttons.Neutral;
using MiraModule.Options.Roles.Neutral;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace MiraModule.Roles.Neutral;

public sealed class HarvesterRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    private static readonly string[] KillAbilityKeywords =
    [
        "kill",
        "assassinate",
        "shoot",
        "murder",
        "execute",
        "stab",
        "slash"
    ];

    private CustomButtonWikiDescription? _stolenAbility;
    private string? _stolenAbilityName;
    private LoadableAsset<Sprite>? _stolenAbilityIcon;

    public string LocaleKey => "Harvester";
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
                new(TouLocale.GetParsed($"MiraRole{LocaleKey}Harvest", "Harvest"),
                    TouLocale.GetParsed($"MiraRole{LocaleKey}HarvestWikiDescription"),
                    NeutAssets.HarvesterHarvestSprite),
            };
        }
    }

    public Color RoleColor => MiraModuleColors.Harvester;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<HarvesterOptions>.Instance.CanVent,
        IntroSound = TouAudio.GlitchSound,
        Icon = RoleIcons.Harvester,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool HasImpostorVision => OptionGroupSingleton<HarvesterOptions>.Instance.ImpostorVision;

    public bool HasStolenAbility => _stolenAbility != null;
    public string? CurrentAbilityName => _stolenAbilityName;
    public LoadableAsset<Sprite>? CurrentAbilityIcon => _stolenAbilityIcon;

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            OffsetButtons();
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(MiraModuleColors.Harvester);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        ClearStolenAbility();
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
        }
    }

    public void OffsetButtons()
    {
        var canVent = OptionGroupSingleton<HarvesterOptions>.Instance.CanVent ||
                      LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.OffsetButtonsToggle.Value;
        var ability = CustomButtonSingleton<HarvesterAbilityButton>.Instance;
        var kill = CustomButtonSingleton<HarvesterKillButton>.Instance;
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(ability, !canVent));
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(kill, !canVent));
    }

    public void TryHarvestAbility(PlayerControl victim)
    {
        if (victim == null || victim.Data == null || victim.Data.Role == null)
        {
            return;
        }

        if (!Player.AmOwner)
        {
            return;
        }

        var role = victim.Data.Role;
        if (role is not ICustomRole customRole || role is not IWikiDiscoverable wiki)
        {
            return;
        }

        if (customRole.Team != ModdedRoleTeams.Crewmate)
        {
            return;
        }

        if (IsKillingAlignment(customRole.GetRoleAlignment()))
        {
            return;
        }

        var abilityIndices = GetCompatibleAbilityIndices(wiki);
        if (abilityIndices.Count == 0)
        {
            return;
        }

        var idx = abilityIndices[UnityEngine.Random.Range(0, abilityIndices.Count)];
        RpcSetStolenAbility(Player, victim.PlayerId, (byte)idx);
    }

    public void UseStolenAbility(PlayerControl? target)
    {
        if (!Player.AmOwner || !HasStolenAbility)
        {
            return;
        }

        var targetId = target != null ? target.PlayerId : byte.MaxValue;
        RpcUseStolenAbility(Player, targetId);
    }

    public bool WinConditionMet()
    {
        var harvesterCount = CustomRoleUtils.GetActiveRolesOfType<HarvesterRole>().Count(x => !x.Player.HasDied());
        if (MiscUtils.KillersAliveCount > harvesterCount)
        {
            return false;
        }

        return harvesterCount >= Helpers.GetAlivePlayers().Count - harvesterCount;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }

    [MethodRpc((uint)MiraModuleRpc.HarvesterSetAbility)]
    public static void RpcSetStolenAbility(PlayerControl sender, byte victimId, byte abilityIndex)
    {
        if (sender.Data.Role is not HarvesterRole harvester)
        {
            return;
        }

        var victim = MiscUtils.PlayerById(victimId);
        if (victim == null || victim.Data?.Role is not IWikiDiscoverable wiki)
        {
            harvester.ClearStolenAbility();
            return;
        }

        var abilities = wiki.Abilities;
        if (abilityIndex >= abilities.Count)
        {
            harvester.ClearStolenAbility();
            return;
        }

        harvester.SetStolenAbility(abilities[abilityIndex]);
    }

    [MethodRpc((uint)MiraModuleRpc.HarvesterUseAbility)]
    public static void RpcUseStolenAbility(PlayerControl sender, byte targetId)
    {
        if (sender.Data.Role is not HarvesterRole harvester)
        {
            return;
        }

        if (!harvester.HasStolenAbility)
        {
            return;
        }

        var target = targetId == byte.MaxValue ? null : MiscUtils.PlayerById(targetId);
        harvester.PlayFakeAbilityEffect(sender, target);
        harvester.ClearStolenAbility();
    }

    private void SetStolenAbility(CustomButtonWikiDescription ability)
    {
        _stolenAbility = ability;
        _stolenAbilityName = ResolveAbilityName(ability);
        _stolenAbilityIcon = ResolveAbilityIcon(ability);
    }

    private void ClearStolenAbility()
    {
        _stolenAbility = null;
        _stolenAbilityName = null;
        _stolenAbilityIcon = null;
    }

    private static bool IsKillingAlignment(RoleAlignment alignment)
    {
        var name = alignment.ToString();
        return name.Contains("Killing", StringComparison.OrdinalIgnoreCase);
    }

    private static List<int> GetCompatibleAbilityIndices(IWikiDiscoverable wiki)
    {
        var indices = new List<int>();
        var abilities = wiki.Abilities;
        for (var i = 0; i < abilities.Count; i++)
        {
            var ability = abilities[i];
            var name = ResolveAbilityName(ability) ?? string.Empty;
            if (IsKillAbilityName(name))
            {
                continue;
            }

            indices.Add(i);
        }

        return indices;
    }

    private static bool IsKillAbilityName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        foreach (var keyword in KillAbilityKeywords)
        {
            if (name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private void PlayFakeAbilityEffect(PlayerControl sender, PlayerControl? target)
    {
        var displayName = CurrentAbilityName ?? TouLocale.GetParsed("MiraRoleHarvesterHarvest", "Harvest");
        var icon = CurrentAbilityIcon?.LoadAsset();

        if (HudManager.InstanceExists)
        {
            var text = $"<b>{displayName}</b>";
            var notif = Helpers.CreateAndShowNotification(
                text,
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: icon);
            notif.AdjustNotification();
        }

        if (target != null && icon != null)
        {
            Coroutines.Start(CoShowAbilityVfx(target, icon));
        }

        if (sender.AmOwner)
        {
            var colorHex = ColorUtility.ToHtmlStringRGB(RoleColor);
            var targetName = target != null ? target.Data.PlayerName : "no target";
            MiscUtils.AddFakeChat(
                sender.Data,
                $"<color=#{colorHex}>Harvester</color>",
                $"Used <b>{displayName}</b> on {targetName}.",
                showHeadsup: true);
        }
    }

    private static System.Collections.IEnumerator CoShowAbilityVfx(PlayerControl target, Sprite sprite)
    {
        var go = new GameObject("HarvesterFakeAbilityVfx");
        go.transform.SetParent(target.transform, false);
        go.transform.localPosition = new Vector3(0f, 1.2f, -1f);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = Color.white;

        yield return new WaitForSeconds(1.5f);
        UnityEngine.Object.Destroy(go);
    }

    private static string? ResolveAbilityName(object ability)
    {
        var type = ability.GetType();
        var nameProp = type.GetProperty("Name") ??
                       type.GetProperty("AbilityName") ??
                       type.GetProperty("ButtonName") ??
                       type.GetProperty("Label");

        if (nameProp != null && nameProp.PropertyType == typeof(string))
        {
            return nameProp.GetValue(ability) as string;
        }

        var firstString = type.GetProperties()
            .FirstOrDefault(p => p.PropertyType == typeof(string));

        return firstString?.GetValue(ability) as string;
    }

    private static LoadableAsset<Sprite>? ResolveAbilityIcon(object ability)
    {
        var type = ability.GetType();
        var iconProp = type.GetProperties()
            .FirstOrDefault(p => typeof(LoadableAsset<Sprite>).IsAssignableFrom(p.PropertyType));

        if (iconProp != null)
        {
            return iconProp.GetValue(ability) as LoadableAsset<Sprite>;
        }

        return null;
    }
}
