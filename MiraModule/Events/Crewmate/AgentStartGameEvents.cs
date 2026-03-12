using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraModule.Modifiers;
using MiraModule.Modifiers.Alliance;
using MiraModule.Options.Modifiers.Alliance;
using TownOfUs.Modifiers.Game.Alliance;


namespace MiraModule.Events;

public static class AgentStartGameEvents
{
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro) return;
        if (!AmongUsClient.Instance.AmHost) return;

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

        var toAssign = eligibleCrew.Take(numAware).ToList();

        foreach (var crewmate in toAssign)
        {
            crewmate.RpcAddModifier<AgentAwareModifier>();
        }
    }
}