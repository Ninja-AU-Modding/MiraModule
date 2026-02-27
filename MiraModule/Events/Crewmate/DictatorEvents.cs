using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraModule.Options.Roles.Crewmates;
using MiraModule.Roles.Crewmate;
using TownOfUs.Utilities;

namespace MiraModule.Events.Crewmate;

public static class DictatorEvents
{
    // ── Button click interception ────────────────────────────────────────
    [RegisterEvent(1000)]
    public static void BeforeLocalVoteEvent(BeforeVoteEvent @event)
    {
        var voteArea = @event.VoteArea;

        // ignore if dead or meeting is wrapping up
        if (PlayerControl.LocalPlayer.HasDied())
        {
            @event.Cancel();
            return;
        }

        if (PlayerControl.LocalPlayer.Data.Role is not DictatorRole dictator) return;

        if (voteArea.Parent.state is MeetingHud.VoteStates.Proceeding or MeetingHud.VoteStates.Results)
        {
            @event.Cancel();
            return;
        }

        // ── End Meeting button clicked ─────────────────────────────────
        if (voteArea == dictator.EndMeetingButton)
        {
            DictatorRole.RpcEndMeeting(PlayerControl.LocalPlayer);
            @event.Cancel();
            return;
        }

        // ── Condemn button clicked (first click: enter targeting mode) ─
        if (voteArea == dictator.CondemnButton && !dictator.SelectingCondemnTarget)
        {
            dictator.SelectingCondemnTarget = true;
            @event.Cancel();
            return;
        }

        // ── Condemn: player chosen as target ──────────────────────────
        if (voteArea != dictator.CondemnButton &&
            voteArea != MeetingHud.Instance.SkipVoteButton &&
            dictator.SelectingCondemnTarget)
        {
            DictatorRole.RpcCondemn(PlayerControl.LocalPlayer, voteArea.TargetPlayerId);
            dictator.SelectingCondemnTarget = false;
            @event.Cancel();
            return;
        }

        // ── Condemn: cancelled via skip ───────────────────────────────
        if (voteArea == MeetingHud.Instance.SkipVoteButton && dictator.SelectingCondemnTarget)
        {
            dictator.SelectingCondemnTarget = false;
            dictator.CondemnVictim = byte.MaxValue;
            @event.Cancel();
        }
    }

    // ── Vote tallying: force 15 votes onto the condemned player ─────────
    [RegisterEvent]
    public static void VoteEvent(CheckForEndVotingEvent @event)
    {
        if (!@event.IsVotingComplete) return;

        var dictator = CustomRoleUtils.GetActiveRolesOfType<DictatorRole>()
            .FirstOrDefault(x => !x.Player.HasDied() && x.HasActed && x.CondemnVictim != byte.MaxValue);

        if (dictator == null) return;

        // Clear everyone's votes then stuff the condemned slot with 15
        foreach (var plr in PlayerControl.AllPlayerControls.ToArray())
        {
            var data = plr.GetVoteData();
            data.Votes.Clear();
            data.VotesRemaining = 0;
        }

        var dictData = dictator.Player.GetVoteData();
        for (var i = 0; i < 15; i++)
        {
            dictData.VoteForPlayer(dictator.CondemnVictim);
        }
    }

    // ── Post-meeting cleanup & sacrifice logic ───────────────────────────
    [RegisterEvent(400)]
    public static void WrapUpEvent(EjectionEvent @event)
    {
        var exiledPlayer = @event.ExileController.initData.networkedPlayer?.Object;

        foreach (var dictator in CustomRoleUtils.GetActiveRolesOfType<DictatorRole>())
        {
            var hadActed = dictator.HasActed;
            var wasCondemn = dictator.CondemnVictim != byte.MaxValue;

            dictator.Cleanup();

            if (!hadActed) continue;

            var opts = OptionGroupSingleton<DictatorOptions>.Instance;

            var shouldSacrifice = opts.SacrificeOnUse ||
                                  (opts.SacrificeWhenEmpty && dictator.UsesRemaining <= 0);

            // Only sacrifice if it was a Condemn (not an End-Meeting) and exiling succeeded
            if (wasCondemn && shouldSacrifice && exiledPlayer != null && !dictator.Player.HasDied())
            {
                dictator.Player.Exiled();
            }
        }
    }
}
