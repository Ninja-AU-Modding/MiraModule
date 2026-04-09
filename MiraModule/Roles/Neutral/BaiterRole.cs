using System;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Freeplay;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraModule.Assets;
using MiraModule.Buttons.Neutral;
using MiraModule.Options.Roles.Neutral;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Roles.Neutral;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraModule.Roles.Neutral;

public sealed class BaiterRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public static readonly List<DeadBody> BaitBodies = new();
    public static readonly List<byte> DiedThisRound = new();
    public static bool BaitReportedIntroFlag;

    private static DeadBody? _cachedBodyPrefab;

    [HideFromIl2Cpp] public int BaitsSucceeded { get; private set; }

    public DoomableType DoomHintType => DoomableType.Relentless;
    public string LocaleKey => "Baiter";
    public string RoleName => TouLocale.Get($"MiraRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"MiraRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"MiraRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"MiraRole{LocaleKey}WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new(
            TouLocale.GetParsed($"MiraRole{LocaleKey}Spawn", "Spawn Bait"),
            TouLocale.GetParsed($"MiraRole{LocaleKey}SpawnWikiDescription"),
            NeutAssets.BaiterSpawnSprite),
    ];

    public Color RoleColor => MiraModuleColors.Baiter;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<BaiterOptions>.Instance.CanVent,
        IntroSound = TouAudio.OtherIntroSound,
        Icon = RoleIcons.Baiter,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        FreeplayFolder = TaskAdderPatches.NeutralName,
    };

    public bool HasImpostorVision => OptionGroupSingleton<BaiterOptions>.Instance.ImpostorVision;

    public bool WinConditionMet()
    {
        return !Player.HasDied() &&
               BaitsSucceeded >= (int)OptionGroupSingleton<BaiterOptions>.Instance.BaitsNeeded;
    }

    public override bool DidWin(GameOverReason gameOverReason) => WinConditionMet();

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(
                CustomButtonSingleton<BaiterSpawnButton>.Instance,
                !OptionGroupSingleton<BaiterOptions>.Instance.CanVent));
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(MiraModuleColors.Baiter);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        BaitBodies.Clear();
        DiedThisRound.Clear();
        BaitReportedIntroFlag = false;
        BaitsSucceeded = 0;

        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
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


    private static DeadBody? GetDeadBodyPrefab()
    {
        if (_cachedBodyPrefab != null)
        {
            return _cachedBodyPrefab;
        }

        foreach (var obj in Resources.FindObjectsOfTypeAll(Il2CppInterop.Runtime.Il2CppType.Of<DeadBody>()))
        {
            var db = obj.TryCast<DeadBody>();
            if (db == null)
            {
                continue;
            }

            if (!db.gameObject.scene.IsValid())
            {
                _cachedBodyPrefab = db;
                return _cachedBodyPrefab;
            }
        }

        return null;
    }

    private static void SpawnBaitBody(byte targetId, Vector2 pos)
    {
        var target = MiscUtils.PlayerById(targetId);
        if (ShipStatus.Instance == null || target == null)
        {
            return;
        }

        var prefab = GetDeadBodyPrefab();
        if (prefab == null)
        {
            return;
        }

        var body = Object.Instantiate(prefab);
        body.gameObject.SetActive(true);
        body.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000f);
        body.ParentId = target.PlayerId;

        foreach (var r in body.bodyRenderers)
        {
            if (r.name.Contains("bone", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            target.SetPlayerMaterialColors(r);
        }

        target.SetPlayerMaterialColors(body.bloodSplatter);
        BaitBodies.Add(body);
    }

    public void OnBaitReported(PlayerControl reporter)
    {
        BaitsSucceeded++;

        if (WinConditionMet())
        {
            RpcBaiterWin(Player);
        }
    }


    [MethodRpc((uint)MiraModuleRpc.BaiterReportBait)]
    public static void RpcReportBait(PlayerControl reporter, byte victimId)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;

        if (!reporter.IsRole<BaiterRole>())
        {
            RpcFooled(reporter);
        }

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            var br = p.GetRole<BaiterRole>();
            br?.OnBaitReported(reporter);
        }

        var victim = MiscUtils.PlayerById(victimId);
        var victimData = victim?.Data ?? reporter.Data;


        reporter.RpcStartMeeting(victimData);


        RpcDestroyBait(reporter.transform.position.x, reporter.transform.position.y);
    }


    [MethodRpc((uint)MiraModuleRpc.BaiterDestroyBait)]
    public static void RpcDestroyBait(float x, float y)
    {
        var pos = new Vector2(x, y);
        for (var i = BaitBodies.Count - 1; i >= 0; i--)
        {
            var body = BaitBodies[i];
            if (body != null && Vector2.Distance(body.transform.position, pos) < 2f)
            {
                BaitBodies.RemoveAt(i);
                Object.Destroy(body.gameObject);
            }
        }
    }


    [MethodRpc((uint)MiraModuleRpc.BaiterSpawnBait)]
    public static void RpcSpawnBait(PlayerControl source, byte targetId, float x, float y)
    {
        SpawnBaitBody(targetId, new Vector2(x, y));
    }

    [MethodRpc((uint)MiraModuleRpc.BaiterWin)]
    public static void RpcBaiterWin(PlayerControl source)
    {
        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            GameManager.Instance.LogicFlow.CheckEndCriteria();
        }
    }

    [MethodRpc((uint)MiraModuleRpc.BaiterFooled)]
    public static void RpcFooled(PlayerControl source)
    {
        BaitReportedIntroFlag = true;
    }
}
