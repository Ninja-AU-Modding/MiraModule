using System.Globalization;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Networking;
using MiraAPI.Patches.Freeplay;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraModule.Assets;
using MiraModule.Buttons.Impostor;
using MiraModule.Options.Roles.Impostor;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TMPro;
using TownOfUs;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraModule.Roles.Impostor;

public sealed class DistorterRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    [HideFromIl2Cpp] public byte PullTargetId { get; private set; } = byte.MaxValue;
    [HideFromIl2Cpp] public bool PullActive { get; private set; }

    [HideFromIl2Cpp] public bool PostInvisibilityActive { get; private set; }
    [HideFromIl2Cpp] public float InvisibilityEndTime { get; private set; }

    [HideFromIl2Cpp] public bool PendingInvisibilityFromAutoKill { get; private set; }

    private float _speedCache = 1f;
    private bool _speedBoostApplied;
    private TextMeshPro? _invisText;

    public string LocaleKey => "Distorter";
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
                new(
                    TouLocale.GetParsed($"MiraRole{LocaleKey}Distort", "Distort"),
                    TouLocale.GetParsed($"MiraRole{LocaleKey}DistortWikiDescription"),
                    ImpostorAssets.DistorterDistortSprite),
            ];
        }
    }

    public Color RoleColor => MiraModuleColors.Distorter;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = RoleIcons.Distorter,
        GhostRole = RoleTypes.ImpostorGhost,
        UseVanillaKillButton = true,
        CanUseSabotage = true,
        CanUseVent = OptionGroupSingleton<DistorterOptions>.Instance.CanVent,
        FreeplayFolder = TaskAdderPatches.ImpostorName,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        ForceCleanupStates();

        if (Player.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(CustomButtonSingleton<DistorterDistortButton>.Instance,
                !OptionGroupSingleton<DistorterOptions>.Instance.CanVent));
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(MiraModuleColors.Distorter);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        ForceCleanupStates();

        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
        }
    }

    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not DistorterRole || Player.HasDied())
        {
            return;
        }

        if (MeetingHud.Instance != null || ExileController.Instance != null)
        {
            ForceCleanupStates();
            return;
        }

        if (PullActive)
        {
            if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
            {
                TickPull();
            }
        }

        if (PostInvisibilityActive)
        {
            TickPostInvisibility();
        }
    }

    public bool IsAbilityBusy => PullActive;

    public bool CanStartDistortOn(PlayerControl? target)
    {
        if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
        {
            return false;
        }

        if (target.PlayerId == Player.PlayerId)
        {
            return false;
        }

        if (IsAbilityBusy || PostInvisibilityActive || Player.inVent)
        {
            return false;
        }

        return true;
    }

    public void StartPull(byte targetId)
    {
        var target = PlayerById(targetId);
        if (!CanStartDistortOn(target))
        {
            return;
        }

        PullTargetId = targetId;
        PullActive = true;
        PendingInvisibilityFromAutoKill = false;
    }

    [MethodRpc((uint)MiraModuleRpc.DistorterStart)]
    public static void RpcStartDistort(PlayerControl source, byte targetId)
    {
        if (source.Data.Role is not DistorterRole role)
        {
            return;
        }

        role.StartPull(targetId);
    }

    public void OnAutoKillResolved()
    {
        if (!PendingInvisibilityFromAutoKill)
        {
            return;
        }

        PendingInvisibilityFromAutoKill = false;
        StartPostInvisibility();
    }

    public void ForceCleanupStates()
    {
        PullActive = false;
        PullTargetId = byte.MaxValue;
        PendingInvisibilityFromAutoKill = false;

        EndPostInvisibility();
        SetPlayerVisibility(Player, true);
        RemoveInvisibilityCountdown();
    }

    private void TickPull()
    {
        var target = PlayerById(PullTargetId);
        if (target == null || target.Data == null || target.Data.Disconnected || target.HasDied())
        {
            PullActive = false;
            PullTargetId = byte.MaxValue;
            PendingInvisibilityFromAutoKill = false;
            return;
        }

        var sourcePos = (Vector2)Player.transform.position;
        var targetPos = (Vector2)target.transform.position;
        var pullSpeed = Mathf.Max(0.1f, OptionGroupSingleton<DistorterOptions>.Instance.PullSpeed);

        // Keep the victim visible but unable to counter-move while being reeled in.
        if (target.MyPhysics != null)
        {
            if (target.MyPhysics.body != null)
            {
                target.MyPhysics.body.velocity = Vector2.zero;
            }

            target.MyPhysics.SetNormalizedVelocity(Vector2.zero);
        }

        var toSource = sourcePos - targetPos;
        var distToSource = toSource.magnitude;
        const float killDistance = 0.42f;
        if (distToSource <= killDistance)
        {
            ResolvePullKill(target);
            return;
        }

        var direction = toSource / Mathf.Max(distToSource, 0.0001f);
        var maxStep = pullSpeed * Time.fixedDeltaTime;
        var step = Mathf.Clamp(maxStep, 0.01f, Mathf.Min(1.1f, distToSource - killDistance));
        var nextPos = targetPos + direction * step;

        target.NetTransform.SnapTo(nextPos);

        if (Vector2.Distance(nextPos, sourcePos) > killDistance)
        {
            return;
        }

        ResolvePullKill(target);
    }

    private void ResolvePullKill(PlayerControl target)
    {
        PullActive = false;
        PullTargetId = byte.MaxValue;

        if (!target.HasDied() && AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            PendingInvisibilityFromAutoKill = true;
            Player.RpcCustomMurder(target);
        }
        else if (target.HasDied())
        {
            StartPostInvisibility();
        }
    }

    private void StartPostInvisibility()
    {
        PostInvisibilityActive = true;
        InvisibilityEndTime = Time.time + OptionGroupSingleton<DistorterOptions>.Instance.PostInvisibilityDuration;
        SetPlayerVisibility(Player, false);
        ApplySpeedBoost();
        UpdateInvisibilityCountdown();
    }

    private void TickPostInvisibility()
    {
        SetPlayerVisibility(Player, false);
        UpdateInvisibilityCountdown();

        if (Time.time >= InvisibilityEndTime)
        {
            EndPostInvisibility();
        }
    }

    private void EndPostInvisibility()
    {
        if (!PostInvisibilityActive)
        {
            RestoreSpeedBoost();
            RemoveInvisibilityCountdown();
            return;
        }

        PostInvisibilityActive = false;
        InvisibilityEndTime = 0f;
        SetPlayerVisibility(Player, true);
        RestoreSpeedBoost();
        RemoveInvisibilityCountdown();
    }

    private void ApplySpeedBoost()
    {
        if (_speedBoostApplied || Player?.MyPhysics == null)
        {
            return;
        }

        _speedCache = Player.MyPhysics.Speed;
        Player.MyPhysics.Speed = _speedCache * OptionGroupSingleton<DistorterOptions>.Instance.InvisibleSpeedMultiplier;
        _speedBoostApplied = true;
    }

    private void RestoreSpeedBoost()
    {
        if (!_speedBoostApplied || Player?.MyPhysics == null)
        {
            return;
        }

        Player.MyPhysics.Speed = _speedCache;
        _speedBoostApplied = false;
    }

    private void UpdateInvisibilityCountdown()
    {
        if (!Player.AmOwner || !HudManager.InstanceExists)
        {
            return;
        }

        if (_invisText == null)
        {
            var go = new GameObject("DistorterInvisText");
            go.transform.SetParent(HudManager.Instance.transform, false);
            go.transform.localPosition = new Vector3(0f, -2.35f, -20f);

            _invisText = go.AddComponent<TextMeshPro>();
            _invisText.fontSize = 2.5f;
            _invisText.alignment = TextAlignmentOptions.Center;
            _invisText.color = MiraModuleColors.Distorter;
        }

        var remaining = Math.Max(0f, InvisibilityEndTime - Time.time);
        _invisText.text = string.Format(CultureInfo.InvariantCulture, "Invisible: {0:0.0}s", remaining);
        _invisText.enabled = true;
    }

    private void RemoveInvisibilityCountdown()
    {
        if (_invisText != null)
        {
            Object.Destroy(_invisText.gameObject);
            _invisText = null;
        }
    }

    private static PlayerControl? PlayerById(byte playerId)
    {
        return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.PlayerId == playerId);
    }

    private static void SetPlayerVisibility(PlayerControl player, bool visible)
    {
        if (player == null || player.Data == null || player.Data.Disconnected)
        {
            return;
        }

        player.Visible = visible;
    }
}



