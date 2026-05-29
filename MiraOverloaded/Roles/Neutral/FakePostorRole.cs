using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.LocalSettings;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraOverloaded.Assets;
using MiraOverloaded.Buttons.Neutral;
using MiraOverloaded.Options.Roles.Neutral;
using Reactor.Utilities;
using System.Globalization;
using System.Text;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace MiraOverloaded.Roles.Neutral;

public sealed class FakePostorRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string LocaleKey => "FakePostor";
    public string RoleName => MiraOverloadedLocale.GetString($"MiraRole{LocaleKey}");
    public string RoleDescription => MiraOverloadedLocale.GetString($"MiraRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => MiraOverloadedLocale.GetString($"MiraRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraOverloadedLocale.GetString($"MiraRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }
    public StringBuilder SetTabText()
    {
        var nameDisplay = RoleName == MiraOverloadedLocale.GetString($"MiraRole{LocaleKey}ColoredName");

        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture,
            $"{RoleColor.ToTextColor()}Your role is <b>{nameDisplay}.</b></color>");
        sb.Append("<size=70%>");
        sb.AppendLine(RoleLongDescription);
        return sb;
    }

    public DoomableType DoomHintType => DoomableType.Death;
    public Color RoleColor => MiraOverloadedColors.FakePostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<FakePostorOptions>.Instance.CanVent,
        IntroSound = TouAudio.OtherIntroSound,
        Icon = RoleIcons.FakePostor,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool HasImpostorVision => OptionGroupSingleton<FakePostorOptions>.Instance.ImposterVision;

    public bool WinConditionMet()
    {
        if (Player.HasDied())
        {
            return false;
        }

        var aliveCount = Helpers.GetAlivePlayers().Count;
        var killersAlive = MiscUtils.KillersAliveCount;

        return aliveCount <= killersAlive && killersAlive == 1;
    }

    public void OffsetButtons()
    {
        var kill = CustomButtonSingleton<FakePostorKillButton>.Instance;
        var canVent = OptionGroupSingleton<FakePostorOptions>.Instance.CanVent || LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.OffsetButtonsToggle.Value;
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(kill, !canVent));
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(MiraOverloadedColors.FakePostor);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
        }
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
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
}