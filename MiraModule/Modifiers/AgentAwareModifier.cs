using MiraAPI.Modifiers.Types;
using MiraModule.Patches;
using UnityEngine;

namespace MiraModule.Modifiers;

public sealed class AgentAwareModifier : GameModifier
{
    public override string ModifierName => "Agent Aware";

    public override Color FreeplayFileColor => new Color32(220, 220, 220, 255);

    public override bool HideOnUi => false;

    public override int GetAmountPerGame() => 0;

    public override int GetAssignmentChance() => 0;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }

    public override void OnActivate()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        HudManager.Instance.Chat.gameObject.SetActive(true);
        AgentChatPatches.ApplyAgentSprites();
    }

    public override void OnDeactivate()
    {
        HudManager.Instance.Chat.gameObject.SetActive(false);
    }
    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        if (!Player.AmOwner)
        {
            return;
        }

        var inactive = HudManager.Instance.Chat.chatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>();
        var active = HudManager.Instance.Chat.chatButton.transform.Find("Active").GetComponent<SpriteRenderer>();
        var selected = HudManager.Instance.Chat.chatButton.transform.Find("Selected").GetComponent<SpriteRenderer>();

        inactive.sprite = TouChatAssets.NormalChatIdle.LoadAsset();
        active.sprite = TouChatAssets.NormalChatHover.LoadAsset();
        selected.sprite = TouChatAssets.NormalChatOpen.LoadAsset();

        inactive.color = Color.white;
        active.color = Color.white;
        selected.color = Color.white;
    }
}

