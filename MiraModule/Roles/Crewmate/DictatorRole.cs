using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraModule.Assets;
using MiraModule.Options.Roles.Crewmates;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace MiraModule.Roles.Crewmate;

public sealed class DictatorRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    // ── Meeting Buttons ──────────────────────────────────────────────────
    /// <summary>Button that instantly ends the meeting (no exile).</summary>
    [HideFromIl2Cpp] public PlayerVoteArea? EndMeetingButton { get; private set; }

    /// <summary>Button that, after a target is chosen, forces all votes onto that target.</summary>
    [HideFromIl2Cpp] public PlayerVoteArea? CondemnButton { get; private set; }

    // ── State ────────────────────────────────────────────────────────────
    public bool SelectingCondemnTarget { get; set; }
    public byte CondemnVictim { get; set; } = byte.MaxValue;
    public bool HasActed { get; set; }
    public int UsesRemaining { get; set; }

    // ── ITownOfUsRole ────────────────────────────────────────────────────
    public string LocaleKey => "Dictator";
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
                new(TouLocale.GetParsed($"MiraRole{LocaleKey}EndMeeting", "End Meeting"),
                    TouLocale.GetParsed($"MiraRole{LocaleKey}EndMeetingWikiDescription"),
                    RoleIcons.Dictator),
                new(TouLocale.GetParsed($"MiraRole{LocaleKey}Condemn", "Condemn"),
                    TouLocale.GetParsed($"MiraRole{LocaleKey}CondemnWikiDescription"),
                    RoleIcons.Dictator),
            ];
        }
    }

    public Color RoleColor => MiraModuleColors.Dictator;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 1,
        Icon = RoleIcons.Dictator,
    };

    // ── Tab text ─────────────────────────────────────────────────────────
    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var text = ITownOfUsRole.SetNewTabText(this);
        text.AppendLine(TownOfUsPlugin.Culture,
            $"{UsesRemaining} {TouLocale.Get("MiraOptionDictatorUsesLeft", "Uses Remaining")}.");
        return text;
    }

    // ── Lifecycle ────────────────────────────────────────────────────────
    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        UsesRemaining = (int)OptionGroupSingleton<DictatorOptions>.Instance.MaxUses;
    }

    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not DictatorRole) return;

        var meeting = MeetingHud.Instance;
        if (!Player.AmOwner || meeting == null) return;

        var canUse = UsesRemaining > 0 &&
                     !HasActed &&
                     meeting.state == MeetingHud.VoteStates.NotVoted &&
                     !SelectingCondemnTarget;

        bool inDiscussion = meeting.state == MeetingHud.VoteStates.Discussion &&
                            meeting.discussionTimer < GameOptionsManager.Instance.currentNormalGameOptions.DiscussionTime;

        // ── End Meeting Button ────────────────────────────────────────
        if (EndMeetingButton != null)
        {
            EndMeetingButton.gameObject.SetActive(canUse);
            if (EndMeetingButton.gameObject.active)
            {
                if (inDiscussion) EndMeetingButton.SetDisabled(); else EndMeetingButton.SetEnabled();
                EndMeetingButton.voteComplete = meeting.SkipVoteButton.voteComplete;
            }
        }

        // ── Condemn Button ────────────────────────────────────────────
        if (CondemnButton != null)
        {
            // Once SelectingCondemnTarget is true the button was manually hidden;
            // don't let FixedUpdate override that by showing it again.
            if (!SelectingCondemnTarget)
            {
                var condemnVisible = canUse && !HasActed && UsesRemaining > 0;
                CondemnButton.gameObject.SetActive(condemnVisible);
            }
            if (CondemnButton.gameObject.active)
            {
                if (inDiscussion) CondemnButton.SetDisabled(); else CondemnButton.SetEnabled();
                CondemnButton.voteComplete = meeting.SkipVoteButton.voteComplete;
            }
        }
    }

    // ── OnMeetingStart ───────────────────────────────────────────────────
    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        var meeting = MeetingHud.Instance;
        if (!Player.AmOwner || meeting == null || UsesRemaining <= 0) return;

        var skip = meeting.SkipVoteButton;

        // Move skip up to create room for two extra buttons beneath it
        skip.transform.localPosition += new Vector3(0f, 0.55f, 0f);

        // ── End Meeting Button (0.36 below skip) ──────────────────────
        EndMeetingButton = UnityEngine.Object.Instantiate(skip, skip.transform.parent);
        EndMeetingButton.Parent = meeting;
        EndMeetingButton.SetTargetPlayerId(252);
        EndMeetingButton.transform.localPosition = skip.transform.localPosition + new Vector3(0f, -0.36f, 0f);
        EndMeetingButton.gameObject.GetComponentInChildren<TextTranslatorTMP>().Destroy();
        EndMeetingButton.gameObject.GetComponentInChildren<TextMeshPro>().text =
            TouLocale.GetParsed("MiraRoleDictatorEndMeeting").ToUpperInvariant();
        EndMeetingButton.gameObject.name = "button_dictatorEndMeeting";

        // ── Condemn Button (0.72 below skip) ──────────────────────────
        CondemnButton = UnityEngine.Object.Instantiate(skip, skip.transform.parent);
        CondemnButton.Parent = meeting;
        CondemnButton.SetTargetPlayerId(253);
        CondemnButton.transform.localPosition = skip.transform.localPosition + new Vector3(0f, -0.72f, 0f);
        CondemnButton.gameObject.GetComponentInChildren<TextTranslatorTMP>().Destroy();
        CondemnButton.gameObject.GetComponentInChildren<TextMeshPro>().text =
            TouLocale.GetParsed("MiraRoleDictatorCondemn").ToUpperInvariant();
        CondemnButton.gameObject.name = "button_dictatorCondemn";

        // When any vote-area or skip is clicked, clear our buttons' visual state
        foreach (var plr in meeting.playerStates)
        {
            plr.gameObject.GetComponentInChildren<PassiveButton>().OnClick
                .AddListener((UnityAction)(() =>
                {
                    EndMeetingButton?.ClearButtons();
                    CondemnButton?.ClearButtons();
                }));
        }

        skip.gameObject.GetComponentInChildren<PassiveButton>().OnClick
            .AddListener((UnityAction)(() =>
            {
                EndMeetingButton?.ClearButtons();
                CondemnButton?.ClearButtons();
            }));

        // The skip VOTE AREA confirm (VoteForMe) is caught by BeforeVoteEvent.
        // We handle condemn-skip there via the SkipVoteButton check below.
        // When in targeting mode and skip's checkmark is confirmed, fire condemn-skip.
        meeting.SkipVoteButton.gameObject.GetComponentInChildren<PassiveButton>().OnClick
            .AddListener((UnityAction)(() =>
            {
                // This fires on the first click (row highlight), not the checkmark.
                // Condemn-skip is handled in BeforeVoteEvent when VoteForMe fires.
            }));
    }

    // ── Cleanup (called by events after each meeting) ────────────────────
    public void Cleanup()
    {
        if (HasActed)
        {
            UsesRemaining--;
        }

        HasActed = false;
        SelectingCondemnTarget = false;
        CondemnVictim = byte.MaxValue;
        EndMeetingButton = null;
        CondemnButton = null;
    }

    // ── RPCs ─────────────────────────────────────────────────────────────

    /// <summary>Broadcasts a Dictator announcement message to all clients' chat.</summary>
    [MethodRpc((uint)MiraModuleRpc.DictatorAnnounce)]
    public static void RpcAnnounce(PlayerControl sender, string message)
    {
        if (sender.Data.Role is not DictatorRole) return;
        var dictatorColor = $"#{MiraModuleColors.Dictator.ToHtmlStringRGBA()}";
        MiscUtils.AddFakeChat(
            sender.Data,
            $"<color={dictatorColor}>{sender.Data.PlayerName} (Dictator)</color>",
            message,
            showHeadsup: true);
    }

    /// <summary>Instantly end the meeting (skip, no exile).</summary>
    [MethodRpc((uint)MiraModuleRpc.DictatorEndMeeting)]
    public static void RpcEndMeeting(PlayerControl sender)
    {
        if (sender.Data.Role is not DictatorRole dictator) return;
        dictator.HasActed = true;

        var meeting = MeetingHud.Instance;
        if (meeting == null) return;

        // Clear all votes → meeting resolves with no one exiled (skip result)
        foreach (var plr in PlayerControl.AllPlayerControls.ToArray())
        {
            var data = plr.GetVoteData();
            data.Votes.Clear();
            data.VotesRemaining = 0;
        }

        RpcAnnounce(sender, "The Dictator has forced the meeting to end.");
        meeting.CheckForEndVoting();
    }

    /// <summary>Set condemn victim on all clients; vote tallying happens via event.</summary>
    [MethodRpc((uint)MiraModuleRpc.DictatorCondemn)]
    public static void RpcCondemn(PlayerControl sender, byte victimId)
    {
        if (sender.Data.Role is not DictatorRole dictator) return;
        dictator.HasActed = true;
        dictator.CondemnVictim = victimId;

        string message;
        if (victimId == 254) // special sentinel: skip was condemned
        {
            message = "The Dictator has forced the meeting to be skipped.";
        }
        else
        {
            var victimName = GameData.Instance.GetPlayerById(victimId)?.PlayerName ?? "??";
            message = $"The Dictator has condemned {victimName} to their death.";
        }
        RpcAnnounce(sender, message);
    }
}
