using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraModule.Modifiers;
using MiraModule.Modifiers.Alliance;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers;
using TownOfUs.Options;
using TownOfUs.Patches;
using TownOfUs.Utilities;
using UnityEngine;
using System;

namespace MiraModule.Patches;

[HarmonyPatch]
public static class AgentChatPatches
{
    private const string AnonymousLabel = "Anonymous";
    private const string AgentLabel = "Agent";
    private const string AgentBubblePrefix = "TOU_TeamChatBubble_Agent";

    private static Color? _agentChatColor;
    internal static Color AgentChatColor => _agentChatColor ??= new AgentModifier().FreeplayFileColor;
    private static GameObject? _agentChatButton;
    private static bool _lastAgentChatActive;
    private static bool _agentFilterActive;

    internal static bool IsAgentChatActiveOutsideMeeting =>
        PlayerControl.LocalPlayer != null &&
        !PlayerControl.LocalPlayer.HasDied() &&
        MeetingHud.Instance == null &&
        (PlayerControl.LocalPlayer.HasModifier<AgentModifier>() ||
         PlayerControl.LocalPlayer.HasModifier<AgentAwareModifier>());

    internal static void ApplyAgentSprites()
    {
        if (!HudManager.InstanceExists || HudManager.Instance.Chat == null) return;
        ApplyAgentSprites(HudManager.Instance.Chat.chatButton.transform);
    }

    internal static void ApplyAgentSprites(Transform chatButtonTransform)
    {
        var inactive = chatButtonTransform.Find("Inactive").GetComponent<SpriteRenderer>();
        var active = chatButtonTransform.Find("Active").GetComponent<SpriteRenderer>();
        var selected = chatButtonTransform.Find("Selected").GetComponent<SpriteRenderer>();

        inactive.sprite = TouChatAssets.NormalChatIdle.LoadAsset();
        active.sprite = TouChatAssets.NormalChatHover.LoadAsset();
        selected.sprite = TouChatAssets.NormalChatOpen.LoadAsset();

        inactive.color = AgentChatColor;
        active.color = AgentChatColor;
        selected.color = AgentChatColor;
    }

    private static void EnsureAgentChatClick()
    {
        if (!HudManager.InstanceExists || HudManager.Instance.Chat == null) return;
        if (!IsAgentChatActiveOutsideMeeting) return;

        var chat = HudManager.Instance.Chat;
        if (chat.chatButton == null) return;

        var pb = chat.chatButton.GetComponent<PassiveButton>();
        if (pb == null) return;

        pb.OverrideOnClickListeners(() =>
        {
            if (MeetingHud.Instance || ExileController.Instance != null) return;
            chat.Toggle();
        });
    }

    private static void EnsureAgentChatButton()
    {
        if (!HudManager.InstanceExists || HudManager.Instance.Chat == null) return;
        var chat = HudManager.Instance.Chat;
        if (chat.chatButton == null) return;

        if (_agentChatButton == null || !_agentChatButton)
        {
            _agentChatButton = UnityEngine.Object.Instantiate(chat.chatButton.gameObject, chat.chatButton.transform.parent);
            _agentChatButton.name = "AgentChatButton";

            var pbs = _agentChatButton.GetComponentsInChildren<PassiveButton>(true);
            foreach (var pb in pbs)
            {
                if (pb == null) continue;
                pb.OverrideOnClickListeners(() =>
                {
                    if (MeetingHud.Instance || ExileController.Instance != null) return;
                    chat.Toggle();
                });
            }
        }

        // Keep position/scale aligned with the original button
        _agentChatButton.transform.localPosition = chat.chatButton.transform.localPosition;
        _agentChatButton.transform.localScale = chat.chatButton.transform.localScale;
    }

    private static void ApplyAgentChatFilter(bool enable)
    {
        var chat = HudManager.Instance?.Chat;
        if (chat?.scroller?.Inner == null) return;

        for (var i = 0; i < chat.scroller.Inner.childCount; i++)
        {
            var go = chat.scroller.Inner.GetChild(i)?.gameObject;
            if (go == null) continue;

            var isPrivate = go.name.StartsWith(AgentBubblePrefix, StringComparison.OrdinalIgnoreCase);
            go.SetActive(!enable || isPrivate);
        }
    }
    private static void ApplyCamouflageBubbleCosmetics(ChatBubble bubble)
    {
        var icon = bubble.GetComponentInChildren<PoolablePlayer>(true);
        if (icon == null) return;

        PlayerMaterial.SetColors(Color.grey, icon.cosmetics.currentBodySprite.BodySprite);
        icon.cosmetics.hat.gameObject.SetActive(false);
        icon.cosmetics.skin.gameObject.SetActive(false);
        icon.cosmetics.visor.gameObject.SetActive(false);
        icon.cosmetics.currentPet.gameObject.SetActive(false);

        var nameText = icon.GetComponentInChildren<TMPro.TextMeshPro>(true);
        if (nameText != null) nameText.text = string.Empty;
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
        else if (senderIsAgent && local.HasModifier<AgentAwareModifier>())
        {
            displayName = $"{AgentLabel} ({sender.Data.PlayerName})";
        }
        else
        {
            displayName = AnonymousLabel;
        }

        var title = $"<color=#{ColorUtility.ToHtmlStringRGBA(AgentChatColor)}>{displayName}</color>";
        var anonymizeIcon = !senderIsAgent;
        AddAgentChat(sender.Data, title, text, onLeft: !sender.AmOwner, anonymizeIcon: anonymizeIcon);
    }

    internal static void AddAgentChat(NetworkedPlayerInfo basePlayer, string nameText, string message, bool onLeft = true, bool anonymizeIcon = false)
    {
        if (!HudManager.InstanceExists || HudManager.Instance.Chat == null) return;
        var chat = HudManager.Instance.Chat;
        var pooledBubble = chat.GetPooledBubble();
        pooledBubble.transform.SetParent(chat.scroller.Inner);
        pooledBubble.transform.localScale = Vector3.one;
        if (onLeft)
        {
            pooledBubble.SetLeft();
        }
        else
        {
            pooledBubble.SetRight();
        }
        pooledBubble.SetCosmetics(basePlayer);
        pooledBubble.NameText.text = nameText;
        pooledBubble.NameText.ForceMeshUpdate(true, true);
        pooledBubble.votedMark.enabled = false;
        pooledBubble.Xmark.enabled = false;
        pooledBubble.TextArea.text = message;
        pooledBubble.TextArea.ForceMeshUpdate(true, true);
        pooledBubble.Background.size = new Vector2(5.52f,
            0.2f + pooledBubble.NameText.GetNotDumbRenderedHeight() + pooledBubble.TextArea.GetNotDumbRenderedHeight());
        pooledBubble.MaskArea.size = pooledBubble.Background.size - new Vector2(0, 0.03f);
        pooledBubble.Background.color = new Color(0.2f, 0.2f, 0.27f, 1f);
        pooledBubble.NameText.color = Color.white;
        pooledBubble.TextArea.color = Color.white;
        pooledBubble.gameObject.name = AgentBubblePrefix;
        if (anonymizeIcon)
        {
            ApplyAnonymousBubbleCosmetics(pooledBubble);
            ApplyCamouflageBubbleCosmetics(pooledBubble);
        }
        pooledBubble.AlignChildren();
        var pos = pooledBubble.NameText.transform.localPosition;
        pooledBubble.NameText.transform.localPosition = pos;
        chat.AlignAllBubbles();
        if (chat is { IsOpenOrOpening: false, notificationRoutine: null })
        {
            chat.notificationRoutine = chat.StartCoroutine(chat.BounceDot());
        }
    }

    private static void ApplyAnonymousBubbleCosmetics(ChatBubble bubble)
    {
        var gray = new Color(0.6f, 0.6f, 0.6f, 1f);
        var sprites = bubble.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in sprites)
        {
            if (sr == null) continue;
            var name = sr.gameObject.name;
            if (name.IndexOf("background", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("mask", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            sr.color = gray;
            if (sr.material != null && sr.material.HasProperty("_Color"))
            {
                sr.material.color = gray;
            }
        }

        var meshes = bubble.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in meshes)
        {
            if (mr == null) continue;
            var name = mr.gameObject.name;
            if (name.IndexOf("background", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("mask", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            if (mr.material != null && mr.material.HasProperty("_Color"))
            {
                mr.material.color = gray;
            }
        }
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

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudUpdatePatch()
    {
        if (!HudManager.InstanceExists || HudManager.Instance.Chat == null) return;

        var chat = HudManager.Instance.Chat;
        var isActive = IsAgentChatActiveOutsideMeeting;

        if (!isActive)
        {
            if (_agentChatButton != null && _agentChatButton)
            {
                _agentChatButton.SetActive(false);
            }

            if (chat.chatButton != null)
            {
                chat.chatButton.gameObject.SetActive(true);
            }

            if (_agentFilterActive)
            {
                ApplyAgentChatFilter(false);
                _agentFilterActive = false;
            }

            _lastAgentChatActive = false;
            return;
        }

        if (!_lastAgentChatActive)
        {
            chat.gameObject.SetActive(true);
            chat.SetVisible(true);
        }

        EnsureAgentChatButton();

        if (_agentChatButton != null && _agentChatButton)
        {
            _agentChatButton.SetActive(true);

            var agentTransform = _agentChatButton.transform;
            var buttonPbs = _agentChatButton.GetComponentsInChildren<PassiveButton>(true);
            foreach (var pb in buttonPbs)
            {
                if (pb != null)
                {
                    pb.enabled = true;
                }
            }

            var colliders = _agentChatButton.GetComponentsInChildren<Collider2D>(true);
            foreach (var col in colliders)
            {
                if (col != null)
                {
                    col.enabled = true;
                }
            }

            var sprites = _agentChatButton.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in sprites)
            {
                if (sr != null)
                {
                    sr.enabled = true;
                }
            }

            ApplyAgentSprites(agentTransform);
        }

        if (chat.chatButton != null)
        {
            chat.chatButton.gameObject.SetActive(false);
        }

        EnsureAgentChatClick();
        var shouldFilter = chat.IsOpenOrOpening;
        if (shouldFilter)
        {
            ApplyAgentChatFilter(true);
            _agentFilterActive = true;
        }
        else if (_agentFilterActive)
        {
            ApplyAgentChatFilter(false);
            _agentFilterActive = false;
        }

        _lastAgentChatActive = true;
    }
}
