using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraModule.Modifiers;
using MiraModule.Modifiers.Alliance;
using MiraModule.Patches;
using MiraModule.Options.Modifiers.Alliance;
using TownOfUs.Modifiers.Game.Alliance;
using UnityEngine;

namespace MiraModule.Events;

public static class AgentEvents
{
    [RegisterEvent]
    public static void OnRoundStartHost(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro || !AmongUsClient.Instance.AmHost) return;

        var agent = ModifierUtils.GetPlayersWithModifier<AgentModifier>().FirstOrDefault();
        if (agent == null) return;

        var numAware = (int)OptionGroupSingleton<AgentOptions>.Instance.NumAware;
        var eligibleCrew = Helpers.GetAlivePlayers()
            .Where(x => x.IsCrewmate() &&
                        !x.HasModifier<AgentAwareModifier>() &&
                        !x.HasModifier<CrewpostorModifier>() &&
                        !x.HasModifier<EgotistModifier>() &&
                        !x.HasModifier<LoverModifier>())
            .ToList();
        eligibleCrew.Shuffle();

        foreach (var crewmate in eligibleCrew.Take(numAware))
        {
            crewmate.RpcAddModifier<AgentAwareModifier>();
        }
    }

    [RegisterEvent]
    public static void OnRoundStartLocal(RoundStartEvent @event)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null) return;

        bool isAgent = local.HasModifier<AgentModifier>();
        bool isAware = local.HasModifier<AgentAwareModifier>();

        if (!isAgent && !isAware) return;

        HudManager.Instance.Chat.gameObject.SetActive(true);

        AgentChatPatches.ApplyAgentSprites();
        HudManager.Instance.Chat.SetVisible(true);
    }
}
