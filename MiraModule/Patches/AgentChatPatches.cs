using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraModule.Modifiers;
using MiraModule.Modifiers.Alliance;
using Reactor.Networking.Attributes;
using TownOfUs.Modifiers;
using TownOfUs.Options;
using UnityEngine;

namespace MiraModule.Patches;

[HarmonyPatch]
public static class AgentChatPatches
{
    private const string AnonymousLabel = "Anonymous";
    private const string AgentLabel = "Agent";

    internal static readonly Color AgentChatColor = new AgentModifier().FreeplayFileColor;

    internal static bool IsAgentChatActiveOutsideMeeting =>
        PlayerControl.LocalPlayer != null &&
        !PlayerControl.LocalPlayer.HasDied() &&
        MeetingHud.Instance == null &&
        (PlayerControl.LocalPlayer.HasModifier<AgentModifier>() ||
         PlayerControl.LocalPlayer.HasModifier<AgentAwareModifier>());

    internal static void ApplyAgentSprites()
    {
        if (!HudManager.InstanceExists || HudManager.Instance.Chat == null) return;
        var chatButton = HudManager.Instance.Chat.chatButton;
        chatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite =
            TouChatAssets.LoveChatIdle.LoadAsset();
        chatButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite =
            TouChatAssets.LoveChatHover.LoadAsset();
        chatButton.transform.Find("Selected").GetComponent<SpriteRenderer>().sprite =
            TouChatAssets.LoveChatOpen.LoadAsset();
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

        string displayName;

        if (sender.AmOwner)
        {
            displayName = sender.Data.PlayerName;
        }
        else if (local.HasModifier<AgentAwareModifier>() && senderIsAgent)
        {
            displayName = $"{AgentLabel} ({sender.Data.PlayerName})";
        }
        else
        {
            displayName = AnonymousLabel;
        }

        var title = $"<color=#{ColorUtility.ToHtmlStringRGBA(AgentChatColor)}>{displayName}</color>";
        MiscUtils.AddTeamChat(sender.Data, title, text, blackoutText: false,
            bubbleType: BubbleType.None, onLeft: !sender.AmOwner);
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

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Toggle))]
    [HarmonyPostfix]
    public static void TogglePatch(ChatController __instance)
    {
        if (!__instance.IsOpenOrOpening || !IsAgentChatActiveOutsideMeeting) return;
        ApplyAgentSprites();
    }
}