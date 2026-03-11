using System;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Freeplay;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraModule.Assets;
using MiraModule.Buttons.Impostor;
using MiraModule.Options.Roles.Impostor;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraModule.Roles.Impostor;

public sealed class ArbiterRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    [HideFromIl2Cpp] public byte TargetId { get; private set; } = byte.MaxValue;
    [HideFromIl2Cpp] public bool RewardActive { get; private set; }
    [HideFromIl2Cpp] public int SabotageUses { get; private set; }

    private string? _sabotageLabelCache;
    private bool _sabotageLabelReady;

    public string LocaleKey => "Arbiter";
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
            return
            [
                new(TouLocale.GetParsed($"MiraRole{LocaleKey}Mark", "Mark"),
                    TouLocale.GetParsed($"MiraRole{LocaleKey}MarkWikiDescription"),
                    ImpostorAssets.ArbiterMarkSprite),
                new(TouLocale.GetParsed($"MiraRole{LocaleKey}Sabotage", "Sabotage"),
                    TouLocale.GetParsed($"MiraRole{LocaleKey}SabotageWikiDescription"),
                    ImpostorAssets.ArbiterSabotageSprite),
            ];
        }
    }

    public Color RoleColor => MiraModuleColors.Arbiter;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = RoleIcons.Arbiter,
        GhostRole = RoleTypes.ImpostorGhost,
        UseVanillaKillButton = true,
        CanUseSabotage = true,
        CanUseVent = OptionGroupSingleton<ArbiterOptions>.Instance.CanVent,
        FreeplayFolder = TaskAdderPatches.ImpostorName,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        ResetState();

        if (Player.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(CustomButtonSingleton<ArbiterMarkButton>.Instance,
                !OptionGroupSingleton<ArbiterOptions>.Instance.CanVent));
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(MiraModuleColors.Arbiter);
            CacheSabotageLabel();
            UpdateSabotageLabel();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        ResetState();

        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
            RestoreSabotageLabel();
        }
    }

    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not ArbiterRole || !Player.AmOwner)
        {
            return;
        }

        EnsureSabotageLabel();
        UpdateSabotageButtonState();
    }

    public void ResetState()
    {
        TargetId = byte.MaxValue;
        RewardActive = false;
        SabotageUses = GetBaseSabotageUses();
        _sabotageLabelReady = false;
    }

    public void SetTarget(byte targetId)
    {
        TargetId = targetId;
    }

    public void ActivateReward()
    {
        if (RewardActive || Player == null || Player.HasDied())
        {
            return;
        }

        RewardActive = true;
        AddSabotageUses(GetBaseSabotageUses());
        ApplyKillCooldownBonus();
    }

    public void ApplyKillCooldownBonus()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        var baseCooldown = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
        var mult = OptionGroupSingleton<ArbiterOptions>.Instance.KillCooldownMultiplier;
        var reduced = Math.Clamp(baseCooldown * mult, 1f, baseCooldown);
        Player.SetKillTimer(reduced);
    }

    public bool TryConsumeSabotageUse()
    {
        if (SabotageUses <= 0)
        {
            UpdateSabotageLabel();
            return false;
        }

        SabotageUses = Math.Max(0, SabotageUses - 1);
        UpdateSabotageLabel();
        return true;
    }

    public void AddSabotageUses(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        SabotageUses = Math.Clamp(SabotageUses + amount, 0, 99);
        UpdateSabotageLabel();
    }

    private int GetBaseSabotageUses()
    {
        return (int)Math.Clamp(OptionGroupSingleton<ArbiterOptions>.Instance.BaseSabotageUses, 0f, 99f);
    }

    private void CacheSabotageLabel()
    {
        if (_sabotageLabelCache != null || !HudManager.InstanceExists)
        {
            return;
        }

        _sabotageLabelCache = HudManager.Instance.SabotageButton?.buttonLabelText?.text;
    }

    private void RestoreSabotageLabel()
    {
        if (!HudManager.InstanceExists || _sabotageLabelCache == null)
        {
            return;
        }

        var label = HudManager.Instance.SabotageButton?.buttonLabelText;
        if (label != null)
        {
            label.text = _sabotageLabelCache;
        }
    }

    private void UpdateSabotageLabel()
    {
        if (!Player.AmOwner || !HudManager.InstanceExists)
        {
            return;
        }

        var label = HudManager.Instance.SabotageButton?.buttonLabelText;
        if (label == null)
        {
            return;
        }

        var baseText = TouLocale.GetParsed("MiraRoleArbiterSabotage", "Sabotage").ToUpperInvariant();
        var usesText = SabotageUses == 1 ? "USE" : "USES";
        label.text = $"{baseText} ({SabotageUses} {usesText})";
        label.SetOutlineColor(MiraModuleColors.Arbiter);
        _sabotageLabelReady = true;
    }

    private void EnsureSabotageLabel()
    {
        if (_sabotageLabelReady)
        {
            return;
        }

        CacheSabotageLabel();
        UpdateSabotageLabel();
    }

    private void UpdateSabotageButtonState()
    {
        if (!HudManager.InstanceExists)
        {
            return;
        }

        var btn = HudManager.Instance.SabotageButton;
        if (btn == null)
        {
            return;
        }

        if (SabotageUses <= 0)
        {
            btn.SetDisabled();
        }
    }

    [MethodRpc((uint)MiraModuleRpc.ArbiterSetTarget)]
    public static void RpcSetTarget(PlayerControl source, byte targetId)
    {
        if (source.Data.Role is not ArbiterRole role)
        {
            return;
        }

        role.SetTarget(targetId);
    }
}
