using System;
using System.Text;
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
using TMPro;
using UnityEngine;
using Epic.OnlineServices.Stats;

namespace MiraModule.Roles.Impostor;

public sealed class ArbiterRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    [HideFromIl2Cpp] public byte TargetId { get; private set; } = byte.MaxValue;
    [HideFromIl2Cpp] public bool RewardActive { get; private set; }
    [HideFromIl2Cpp] public int InvisUses { get; private set; }
    [HideFromIl2Cpp] public int SpeedUses { get; private set; }
    [HideFromIl2Cpp] public bool InvisActive { get; private set; }
    [HideFromIl2Cpp] public float InvisEndTime { get; private set; }
    [HideFromIl2Cpp] public bool SpeedActive { get; private set; }
    [HideFromIl2Cpp] public float SpeedEndTime { get; private set; }

    private float _speedCache = 1f;
    private bool _speedBoostApplied;
    private TextMeshPro? _invisText;
    private TextMeshPro? _speedText;

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
                new(TouLocale.GetParsed($"MiraRole{LocaleKey}Invisibility", "Invisibility"),
                    TouLocale.GetParsed($"MiraRole{LocaleKey}InvisibilityWikiDescription"),
                    ImpostorAssets.ArbiterInvisSprite),
                new(TouLocale.GetParsed($"MiraRole{LocaleKey}Speed", "Speed Boost"),
                    TouLocale.GetParsed($"MiraRole{LocaleKey}SpeedWikiDescription"),
                    ImpostorAssets.ArbiterSpeedSprite),
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
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(CustomButtonSingleton<ArbiterInvisButton>.Instance,
                !OptionGroupSingleton<ArbiterOptions>.Instance.CanVent));
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(CustomButtonSingleton<ArbiterSpeedButton>.Instance,
                !OptionGroupSingleton<ArbiterOptions>.Instance.CanVent));
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(MiraModuleColors.Arbiter);
            UpdateInvisButtonUses();
            UpdateSpeedButtonUses();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        ResetState();

        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
        }
    }

    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not ArbiterRole || Player.HasDied())
        {
            EndInvisibility();
            EndSpeedBoost();
            return;
        }

        if (MeetingHud.Instance != null || ExileController.Instance != null)
        {
            EndInvisibility();
            EndSpeedBoost();
            return;
        }

        if (InvisActive)
        {
            SetPlayerVisibility(Player, false);
            if (Time.time >= InvisEndTime)
            {
                EndInvisibility();
            }
        }

        if (SpeedActive)
        {
            ApplySpeedBoost();
            if (Time.time >= SpeedEndTime)
            {
                EndSpeedBoost();
            }
        }

        UpdateDurationText();
    }

    public void ResetState()
    {
        TargetId = byte.MaxValue;
        RewardActive = false;
        InvisUses = 0;
        SpeedUses = 0;
        InvisActive = false;
        InvisEndTime = 0f;
        SpeedActive = false;
        SpeedEndTime = 0f;
        _speedBoostApplied = false;
        RemoveDurationText();
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
        AddInvisUses(GetInvisUsesGain());
        AddSpeedUses(GetSpeedUsesGain());
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

    public bool TryUseInvisibility()
    {
        if (InvisUses <= 0 || InvisActive || Player.inVent || Minigame.Instance)
        {
            return false;
        }

        InvisUses = Math.Max(0, InvisUses - 1);
        UpdateInvisButtonUses();
        InvisActive = true;
        InvisEndTime = Time.time + OptionGroupSingleton<ArbiterOptions>.Instance.InvisDuration;
        SetPlayerVisibility(Player, false);
        return true;
    }

    public bool TryUseSpeedBoost()
    {
        if (SpeedUses <= 0 || SpeedActive || Player.inVent || Minigame.Instance)
        {
            return false;
        }

        SpeedUses = Math.Max(0, SpeedUses - 1);
        UpdateSpeedButtonUses();
        SpeedActive = true;
        SpeedEndTime = Time.time + OptionGroupSingleton<ArbiterOptions>.Instance.SpeedDuration;
        ApplySpeedBoost();
        return true;
    }

    public void AddInvisUses(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        InvisUses = Math.Clamp(InvisUses + amount, 0, 99);
        UpdateInvisButtonUses();
    }

    public void AddSpeedUses(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        SpeedUses = Math.Clamp(SpeedUses + amount, 0, 99);
        UpdateSpeedButtonUses();
    }

    private static int GetInvisUsesGain()
    {
        return (int)Math.Clamp(OptionGroupSingleton<ArbiterOptions>.Instance.InvisUses, 0f, 99f);
    }

    private static int GetSpeedUsesGain()
    {
        return (int)Math.Clamp(OptionGroupSingleton<ArbiterOptions>.Instance.SpeedUses, 0f, 99f);
    }

    private void UpdateInvisButtonUses()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        var btn = CustomButtonSingleton<ArbiterInvisButton>.Instance;
        if (btn != null)
        {
            btn.SetUses(InvisUses);
        }
    }

    private void UpdateSpeedButtonUses()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        var btn = CustomButtonSingleton<ArbiterSpeedButton>.Instance;
        if (btn != null)
        {
            btn.SetUses(SpeedUses);
        }
    }

    private void ApplySpeedBoost()
    {
        if (_speedBoostApplied || Player?.MyPhysics == null)
        {
            return;
        }

        _speedCache = Player.MyPhysics.Speed;
        Player.MyPhysics.Speed = _speedCache * OptionGroupSingleton<ArbiterOptions>.Instance.SpeedMultiplier;
        _speedBoostApplied = true;
    }

    private void EndSpeedBoost()
    {
        if (!_speedBoostApplied || Player?.MyPhysics == null)
        {
            SpeedActive = false;
            return;
        }

        Player.MyPhysics.Speed = _speedCache;
        _speedBoostApplied = false;
        SpeedActive = false;
        SpeedEndTime = 0f;
    }

    private void EndInvisibility()
    {
        if (!InvisActive)
        {
            return;
        }

        InvisActive = false;
        InvisEndTime = 0f;
        SetPlayerVisibility(Player, true);
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var text = ITownOfUsRole.SetNewTabText(this);
        var name = TargetId == byte.MaxValue
            ? "None"
            : (GameData.Instance?.GetPlayerById(TargetId)?.PlayerName ?? "??");
        text.AppendLine(TownOfUsPlugin.Culture, $"Marked: {name}");
        return text;
    }

    private void UpdateDurationText()
    {
        if (!Player.AmOwner || !HudManager.InstanceExists)
        {
            return;
        }

        if (_invisText == null)
        {
            var go = new GameObject("ArbiterInvisText");
            go.transform.SetParent(HudManager.Instance.transform, false);
            go.transform.localPosition = new Vector3(0f, -2.35f, -20f);
            _invisText = go.AddComponent<TextMeshPro>();
            _invisText.fontSize = 2.5f;
            _invisText.alignment = TextAlignmentOptions.Center;
            _invisText.color = MiraModuleColors.Arbiter;
        }

        if (_speedText == null)
        {
            var go = new GameObject("ArbiterSpeedText");
            go.transform.SetParent(HudManager.Instance.transform, false);
            go.transform.localPosition = new Vector3(0f, -2.75f, -20f);
            _speedText = go.AddComponent<TextMeshPro>();
            _speedText.fontSize = 2.5f;
            _speedText.alignment = TextAlignmentOptions.Center;
            _speedText.color = MiraModuleColors.Arbiter;
        }

        if (InvisActive)
        {
            var remaining = Math.Max(0f, InvisEndTime - Time.time);
            _invisText.text = $"Invis: {remaining:0.0}s";
            _invisText.enabled = true;
        }
        else
        {
            _invisText.enabled = false;
        }

        if (SpeedActive)
        {
            var remaining = Math.Max(0f, SpeedEndTime - Time.time);
            _speedText.text = $"Speed: {remaining:0.0}s";
            _speedText.enabled = true;
        }
        else
        {
            _speedText.enabled = false;
        }
    }

    private void RemoveDurationText()
    {
        if (_invisText != null)
        {
            UnityEngine.Object.Destroy(_invisText.gameObject);
            _invisText = null;
        }

        if (_speedText != null)
        {
            UnityEngine.Object.Destroy(_speedText.gameObject);
            _speedText = null;
        }
    }

    private static void SetPlayerVisibility(PlayerControl player, bool visible)
    {
        if (player == null || player.Data == null || player.Data.Disconnected)
        {
            return;
        }

        player.Visible = visible;
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
