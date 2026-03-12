using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Options.Modifiers;
using Reactor.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Options;
using UnityEngine;

namespace MiraModule.Modifiers.Alliance;

public sealed class AgentModifier : AllianceGameModifier, IWikiDiscoverable, IAssignableTargets
{
    public override string LocaleKey => "Agent";
    public override string ModifierName => "Agent";
    public override string IntroInfo => "Aid the <color=#72B3CAFF>Crewmates</color>";

    public override string GetDescription()
    {
        return "Betray the Impostors as a Crewmate.";
    }

    public string GetAdvancedDescription()
    {
        return "The Agent is an Impostor Alliance modifier, which forces an Impostor to work for the Crewmate faction. The Agent has no idea who the Impostors are, but can inquiry players to reveal their faction. The Agent also gets a private, anonymous chat with 2 other crew members. There is a certain chance for an Agent to still be working with Impostors.";
    }

    public override string Symbol => "*";
    public override float IntroSize => 4f;
    public override bool DoesTasks => false;
    public override bool GetsPunished => false;
    public override bool CrewContinuesGame => false;
    public override ModifierFaction FactionType => ModifierFaction.ImpostorAlliance;
    public override AlliedFaction TrueFactionType => AlliedFaction.Crewmate;
    public override Color FreeplayFileColor => new Color32(114, 179, 202, 255);
    public override LoadableAsset<Sprite>? ModifierIcon => ModifierIcons.Agent;

    public int Priority { get; set; } = -1;
    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public void AssignTargets()
    {
        if (!OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment) return;

        System.Random rnd = new();

        if (rnd.Next(1, 101) > (int)OptionGroupSingleton<AgentModifierOptions>.Instance.AgentChance) return;

        var filtered = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => x.IsImpostor() && !x.HasDied() && !x.HasModifier<AllianceGameModifier>()).ToList();

        if (filtered.Count == 0) return;

        filtered[rnd.Next(0, filtered.Count)].RpcAddModifier<AgentModifier>();
    }

    public override int GetAmountPerGame() => 0;
    public override int GetAssignmentChance() => 0;

    public override void OnActivate()
    {
        base.OnActivate();
        if (!Player.HasModifier<BasicGhostModifier>())
            Player.AddModifier<BasicGhostModifier>();

        if (Player.HasModifier<ToBecomeTraitorModifier>())
            Player.RemoveModifier<ToBecomeTraitorModifier>();

        if (!Player.AmOwner)
        {
            return;
        }

        HudManager.Instance.Chat.gameObject.SetActive(true);
        var buttonArray = new[]
            { TouChatAssets.LoveChatIdle.LoadAsset(), TouChatAssets.LoveChatHover.LoadAsset(), TouChatAssets.LoveChatOpen.LoadAsset()};
        HudManager.Instance.Chat.chatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite = buttonArray[0];
        HudManager.Instance.Chat.chatButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite = buttonArray[1];
        HudManager.Instance.Chat.chatButton.transform.Find("Selected").GetComponent<SpriteRenderer>().sprite = buttonArray[2];
    }

    public override void OnDeactivate()
    {
        HudManager.Instance.Chat.gameObject.SetActive(false);
    }

    public override int CustomAmount => (int)OptionGroupSingleton<AgentModifierOptions>.Instance.AgentChance != 0 ? 1 : 0;
    public override int CustomChance => (int)OptionGroupSingleton<AgentModifierOptions>.Instance.AgentChance;

    public static bool AgentVisibilityFlag(PlayerControl player)
    {
        return player.AmOwner || (player.Data != null && !player.Data.Disconnected &&
               PlayerControl.LocalPlayer.HasModifier<AgentAwareModifier>());
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsImpostor();
    }
    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        if (!Player.AmOwner)
        {
            return;
        }

        var buttonArray = new Sprite[]
        {
            TouChatAssets.NormalChatIdle.LoadAsset(), TouChatAssets.NormalChatHover.LoadAsset(),
            TouChatAssets.NormalChatOpen.LoadAsset()
        };
        HudManager.Instance.Chat.chatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite =
            buttonArray[0];
        HudManager.Instance.Chat.chatButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite =
            buttonArray[1];
        HudManager.Instance.Chat.chatButton.transform.Find("Selected").GetComponent<SpriteRenderer>().sprite =
            buttonArray[2];
    }

    public override bool? DidWin(GameOverReason reason)
    {
        return reason is GameOverReason.CrewmatesByTask ||
               reason is GameOverReason.CrewmatesByVote ||
               reason is GameOverReason.HideAndSeek_CrewmatesByTimer;
    }
}