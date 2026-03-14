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

#pragma warning disable S125
public static class DictatorEvents
{
    // MiraAPI's internal skip-vote ID (see VotingUtils.SkipVoteId)
    private const byte SkipVoteId = 253;

    // Sentinel stored on DictatorRole.CondemnVictim to mean "the skip was condemned"
    private const byte CondemnSkipId = 254;

    // ── Button confirm checkmark interception ────────────────────────────
    // BeforeVoteEvent fires when the GREEN CHECKMARK is clicked on any row.
    [RegisterEvent(1000)]
    public static void BeforeLocalVoteEvent(BeforeVoteEvent @event)
    {
        if (PlayerControl.LocalPlayer.HasDied()) { @event.Cancel(); return; }
        if (PlayerControl.LocalPlayer.Data.Role is not DictatorRole dictator) return;

        var state = @event.VoteArea.Parent.state;
        if (state is MeetingHud.VoteStates.Proceeding or MeetingHud.VoteStates.Results)
        {
            @event.Cancel();
            return;
        }

        // ── Checkmark confirmed on End Meeting row ─────────────────────
        if (@event.VoteArea == dictator.EndMeetingButton)
        {
            DictatorRole.RpcEndMeeting(PlayerControl.LocalPlayer);
            @event.Cancel();
            return;
        }

        // ── Checkmark confirmed on Condemn row → enter targeting mode ──
        // Hides the button. The player now clicks any player row or skip to condemn.
        if (@event.VoteArea == dictator.CondemnButton)
        {
            dictator.SelectingCondemnTarget = true;
            dictator.CondemnButton?.gameObject.SetActive(false);
            @event.Cancel();
            return;
        }

        // ── In targeting mode: checkmark on SKIP = force skip ──────────
        if (dictator.SelectingCondemnTarget &&
            @event.VoteArea == MeetingHud.Instance.SkipVoteButton)
        {
            DictatorRole.RpcCondemn(PlayerControl.LocalPlayer, CondemnSkipId);
            dictator.SelectingCondemnTarget = false;
            @event.Cancel();
            return;
        }

        // ── In targeting mode: checkmark on a PLAYER row = condemn them ─
        // The player highlighted that row with the first click (vanilla select);
        // now confirming it is the condemn action.
        if (dictator.SelectingCondemnTarget)
        {
            DictatorRole.RpcCondemn(PlayerControl.LocalPlayer, @event.VoteArea.TargetPlayerId);
            dictator.SelectingCondemnTarget = false;
            @event.Cancel();
            return;
        }
    }

    // ── MeetingSelectEvent: fires on the FIRST click (row highlight) ─────
    // Controls which rows the dictator is allowed to highlight.
    [RegisterEvent(1000)]
    public static void OnMeetingSelect(MeetingSelectEvent @event)
    {
        if (PlayerControl.LocalPlayer.Data.Role is not DictatorRole dictator) return;

        // Not in targeting mode: only allow selecting our own special buttons
        // (EndMeeting = id 252, Condemn = id 253). Block selecting any real player.
        if (!dictator.SelectingCondemnTarget)
        {
            var isOurButton = @event.TargetId == 252 || @event.TargetId == 253;
            if (!isOurButton)
                @event.AllowSelect = false;
        }
        // In targeting mode: allow selecting any real player or skip (for the confirm checkmark).
        // Our special buttons are already hidden, so no need to block them.
    }

    // ── Vote tallying: force all votes onto condemned player (or skip) ───
    [RegisterEvent]
    public static void VoteEvent(CheckForEndVotingEvent @event)
    {
        if (!@event.IsVotingComplete) return;

        var dictator = CustomRoleUtils.GetActiveRolesOfType<DictatorRole>()
            .FirstOrDefault(x => !x.Player.HasDied() && x.HasActed && x.CondemnVictim != byte.MaxValue);

        if (dictator == null) return;

        var voteTarget = dictator.CondemnVictim == CondemnSkipId ? SkipVoteId : dictator.CondemnVictim;

        foreach (var plr in PlayerControl.AllPlayerControls.ToArray())
        {
            var data = plr.GetVoteData();
            data.Votes.Clear();
            data.VotesRemaining = 0;

            if (!plr.HasDied())
                data.VoteForPlayer(voteTarget);
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

            // Only sacrifice if it was a Condemn (not End-Meeting) and exiling succeeded
            if (wasCondemn && shouldSacrifice && exiledPlayer != null && !dictator.Player.HasDied())
            {
                dictator.Player.Exiled();
            }
        }
    }
}
