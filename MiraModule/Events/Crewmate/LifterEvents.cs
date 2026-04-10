using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraModule.Roles.Crewmate;

namespace MiraModule.Events;

public static class LifterEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            LifterRole.ClearAllVotes();
        }
    }

    [RegisterEvent]
    public static void HandleVotesEventHandler(HandleVoteEvent @event)
    {
        var owner = @event.VoteData.Owner;
        if (owner?.Data?.Role is not LifterRole lifter) return;

        var extraVotes = lifter.ExtraVotes;
        if (extraVotes <= 0) return;

        @event.VoteData.SetRemainingVotes(0);

        var totalVotes = 1 + extraVotes;
        for (var i = 0; i < totalVotes; i++)
        {
            @event.VoteData.VoteForPlayer(@event.TargetId);
        }

        @event.Cancel();
    }
}
