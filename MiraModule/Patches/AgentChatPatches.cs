using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraModule.Modifiers;
using MiraModule.Modifiers.Alliance;
using Reactor.Networking.Attributes;
using TownOfUs.Modifiers;
using TownOfUs.Options;
using TownOfUs.Patches.Options;
using UnityEngine;

namespace MiraModule.Patches;

[HarmonyPatch]
public static class AgentChatPatches
{
    // Lower number = higher priority. Impostor chat is 30.
    private const int ChatPriority = 25;
    private const string AnonymousLabel = "Anonymous";
    private const string AgentLabel = "Agent";

    private static readonly Color AgentChatColor = new AgentModifier().FreeplayFileColor;

    private static TeamChatPatches.ExtensionTeamChatHandler? _handler;

    public static void RegisterAgentChat()
    {
        if (_handler != null) return;

        _handler = new TeamChatPatches.ExtensionTeamChatHandler
        {
            Priority = ChatPriority,
            IsForced = true,
            IsChatAvailable = () =>
                (MeetingHud.Instance != null) &&
                !PlayerControl.LocalPlayer.HasDied() &&
                (PlayerControl.LocalPlayer.HasModifier<AgentModifier>() ||
                 PlayerControl.LocalPlayer.HasModifier<AgentAwareModifier>()),
            SendMessage = (sender, msg) => RpcSendAgentChat(sender, msg),
            GetDisplayText = () => "Agent Chat",
            DisplayTextColor = AgentChatColor,
        };

        TeamChatPatches.ExtensionTeamChatRegistry.RegisterHandler(_handler);
    }

    [MethodRpc((uint)MiraModuleRpc.SendAgentChat)]
    public static void RpcSendAgentChat(PlayerControl sender, string text)
    {
        var local = PlayerControl.LocalPlayer;
        var isDead = DeathHandlerModifier.IsFullyDead(local);
        var theDeadKnow = OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow;

        var canSee = local.HasModifier<AgentModifier>() ||
                     local.HasModifier<AgentAwareModifier>() ||
                     (isDead && theDeadKnow);

        if (!canSee) return;

        var senderIsAgent = sender.HasModifier<AgentModifier>();
        var senderIsAgentAware = sender.HasModifier<AgentAwareModifier>();

        string displayName;

        if (sender.AmOwner)
        {
            // You always see your own messages with your real name
            displayName = sender.Data.PlayerName;
        }
        else if (local.HasModifier<AgentAwareModifier>() && senderIsAgent)
        {
            // AgentAware can identify the Agent
            displayName = AgentLabel;
        }
        else
        {
            // Everyone else is anonymous to everyone else
            displayName = AnonymousLabel;
        }

        var title = $"<color=#{ColorUtility.ToHtmlStringRGBA(AgentChatColor)}>{displayName}</color>";
        var inMeeting = MeetingHud.Instance != null;
        var blackout = inMeeting; // private-like during meetings
        var bubbleType = inMeeting ? TownOfUs.Utilities.BubbleType.Other : TownOfUs.Utilities.BubbleType.None;
        // Show outside meetings (like Lover), keep private styling in meetings.
        MiscUtils.AddTeamChat(sender.Data, title, text, blackoutText: blackout, bubbleType: bubbleType, onLeft: !sender.AmOwner);
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    [HarmonyPrefix]
    public static bool SendChatPatch(ChatController __instance)
    {
        if (MeetingHud.Instance || ExileController.Instance != null || PlayerControl.LocalPlayer.Data.IsDead)
            return true;

        var text = __instance.freeChatField.Text.WithoutRichText();
        if (text.Length < 1 || text.Length > 100) return true;

        var local = PlayerControl.LocalPlayer;
        if (!local.HasModifier<AgentModifier>() && !local.HasModifier<AgentAwareModifier>()) return true;

        RpcSendAgentChat(local, text);
        __instance.freeChatField.Clear();
        __instance.quickChatMenu.Clear();
        __instance.quickChatField.Clear();
        __instance.UpdateChatMode();
        return false;
    }
}
