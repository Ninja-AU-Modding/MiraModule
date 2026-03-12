using System;
using System.Collections;
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
using Object = UnityEngine.Object;

namespace MiraModule.Roles.Impostor;

public sealed class NinjaRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    [HideFromIl2Cpp] public byte MarkedTargetId { get; private set; } = byte.MaxValue;
    [HideFromIl2Cpp] public bool InvisActive { get; private set; }
    [HideFromIl2Cpp] public float InvisEndTime { get; private set; }

    public string LocaleKey => "Ninja";
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
                    TouLocale.GetParsed($"MiraRole{LocaleKey}Mark", "Mark"),
                    TouLocale.GetParsed($"MiraRole{LocaleKey}MarkWikiDescription"),
                    ImpostorAssets.NinjaMarkSprite),
                new(
                    TouLocale.GetParsed($"MiraRole{LocaleKey}Assassinate", "Assassinate"),
                    TouLocale.GetParsed($"MiraRole{LocaleKey}AssassinateWikiDescription"),
                    ImpostorAssets.NinjaAssassinateSprite),
            ];
        }
    }

    public Color RoleColor => MiraModuleColors.Ninja;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = RoleIcons.Ninja,
        GhostRole = RoleTypes.ImpostorGhost,
        UseVanillaKillButton = true,
        CanUseSabotage = true,
        CanUseVent = OptionGroupSingleton<NinjaOptions>.Instance.CanVent,
        FreeplayFolder = TaskAdderPatches.ImpostorName,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        ResetState();

        if (Player.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(CustomButtonSingleton<NinjaMarkButton>.Instance,
                !OptionGroupSingleton<NinjaOptions>.Instance.CanVent));
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(CustomButtonSingleton<NinjaAssassinateButton>.Instance,
                !OptionGroupSingleton<NinjaOptions>.Instance.CanVent));
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(MiraModuleColors.Ninja);
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
        if (Player == null || Player.Data.Role is not NinjaRole || Player.HasDied())
        {
            EndInvisibility();
            return;
        }

        if (MeetingHud.Instance != null || ExileController.Instance != null)
        {
            EndInvisibility();
            return;
        }

        UpdateMarkedTarget();

        if (InvisActive)
        {
            SetPlayerVisibility(Player, false);
            if (Time.time >= InvisEndTime)
            {
                EndInvisibility();
            }
        }
    }

    public void ResetState()
    {
        MarkedTargetId = byte.MaxValue;
        InvisActive = false;
        InvisEndTime = 0f;
        SetPlayerVisibility(Player, true);
    }

    public void SetTarget(byte targetId)
    {
        MarkedTargetId = targetId;
    }

    public bool CanAssassinate()
    {
        var target = PlayerById(MarkedTargetId);
        return CanAssassinate(target);
    }

    private bool CanAssassinate(PlayerControl? target)
    {
        if (Player == null || Player.Data == null || Player.HasDied())
        {
            return false;
        }

        if (MeetingHud.Instance != null || ExileController.Instance != null || Minigame.Instance)
        {
            return false;
        }

        if (Player.inVent || target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
        {
            return false;
        }

        if (target.PlayerId == Player.PlayerId)
        {
            return false;
        }

        return MarkedTargetId != byte.MaxValue;
    }

    public bool TryAssassinate()
    {
        if (!Player.AmOwner)
        {
            return false;
        }

        var target = PlayerById(MarkedTargetId);
        if (!CanAssassinate(target) || target == null)
        {
            return false;
        }

        var sourcePos = (Vector2)Player.transform.position;
        var targetPos = (Vector2)target.transform.position;
        RpcAssassinate(Player, target.PlayerId, sourcePos.x, sourcePos.y, targetPos.x, targetPos.y);
        return true;
    }

    private void PerformAssassination(byte targetId, Vector2 sourcePos, Vector2 targetPos)
    {
        if (Player == null || Player.Data == null || Player.HasDied())
        {
            return;
        }

        var target = PlayerById(targetId);
        if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
        {
            MarkedTargetId = byte.MaxValue;
            return;
        }

        MarkedTargetId = byte.MaxValue;
        SpawnTrace(sourcePos);
        Player.NetTransform.SnapTo(targetPos);

        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            Player.RpcMurderPlayer(target, true);
        }

        SpawnLeaves(targetPos);
        SpawnTrace(targetPos);
        StartInvisibility();
    }

    private void UpdateMarkedTarget()
    {
        if (MarkedTargetId == byte.MaxValue)
        {
            return;
        }

        var target = PlayerById(MarkedTargetId);
        if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
        {
            MarkedTargetId = byte.MaxValue;
        }
    }

    private void StartInvisibility()
    {
        InvisActive = true;
        InvisEndTime = Time.time + OptionGroupSingleton<NinjaOptions>.Instance.InvisibilityDuration;
        SetPlayerVisibility(Player, false);
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

    private static void SpawnTrace(Vector2 position)
    {
        var duration = OptionGroupSingleton<NinjaOptions>.Instance.TraceDuration;
        if (duration <= 0f)
        {
            return;
        }

        var sprite = ImpostorAssets.NinjaTraceSprite.LoadAsset();
        var trace = CreateWorldSprite("NinjaTrace", sprite, position, 1f, 2);
        Coroutines.Start(CoDestroyAfter(trace, duration));
    }

    private static void SpawnLeaves(Vector2 position)
    {
        var duration = OptionGroupSingleton<NinjaOptions>.Instance.LeafDuration;
        var count = (int)Math.Clamp(OptionGroupSingleton<NinjaOptions>.Instance.LeafCount, 0f, 20f);
        if (duration <= 0f || count <= 0)
        {
            return;
        }

        var sprite = ImpostorAssets.NinjaLeavesSprite.LoadAsset();
        for (var i = 0; i < count; i++)
        {
            var offset = UnityEngine.Random.insideUnitCircle * 0.45f;
            var leafPos = position + offset;
            var leaf = CreateWorldSprite("NinjaLeaf", sprite, leafPos, 1f, 1);
            leaf.transform.eulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(0f, 360f));
            Coroutines.Start(CoDestroyAfter(leaf, duration));
        }
    }

    private static GameObject CreateWorldSprite(string name, Sprite sprite, Vector2 position, float scale, int sortingOrder)
    {
        var go = new GameObject(name);
        if (ShipStatus.Instance != null)
        {
            go.transform.SetParent(ShipStatus.Instance.transform, false);
            go.layer = ShipStatus.Instance.gameObject.layer;
        }

        go.transform.position = new Vector3(position.x, position.y, position.y / 1000f);
        go.transform.localScale = Vector3.one * scale;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingLayerName = "Objects";
        renderer.sortingOrder = sortingOrder;
        return go;
    }

    private static IEnumerator CoDestroyAfter(GameObject target, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (target != null)
        {
            Object.Destroy(target);
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

    [MethodRpc((uint)MiraModuleRpc.NinjaSetTarget)]
    public static void RpcSetTarget(PlayerControl source, byte targetId)
    {
        if (source.Data.Role is not NinjaRole role)
        {
            return;
        }

        role.SetTarget(targetId);
    }

    [MethodRpc((uint)MiraModuleRpc.NinjaAssassinate)]
    public static void RpcAssassinate(PlayerControl source, byte targetId, float sourceX, float sourceY, float targetX, float targetY)
    {
        if (source.Data.Role is not NinjaRole role)
        {
            return;
        }

        role.PerformAssassination(targetId, new Vector2(sourceX, sourceY), new Vector2(targetX, targetY));
    }
}
