using MiraAPI.Modifiers.Types;
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
        var buttonArray = new[]
            { TouChatAssets.LoveChatIdle.LoadAsset(), TouChatAssets.LoveChatHover.LoadAsset(), TouChatAssets.LoveChatOpen.LoadAsset()};
        HudManager.Instance.Chat.chatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite = buttonArray[0];
        HudManager.Instance.Chat.chatButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite = buttonArray[1];
        HudManager.Instance.Chat.chatButton.transform.Find("Selected").GetComponent<SpriteRenderer>().sprite = buttonArray[2];
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

        var buttonArray = new Sprite[]
        {
            TouChatAssets.NormalChatIdle.LoadAsset(), TouChatAssets.NormalChatHover.LoadAsset(),
            TouChatAssets.NormalChatOpen.LoadAsset()
        };
        HudManager.Instance.Chat.chatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite =
            buttonArray[0];
        HudManager.Instance.Chat.chatButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite =
            buttonArray[1];
        HudManager.Instance.Chat.chatButton.transform.Find("Selected").GetComponent<SpriteRenderer>().sprite =
            buttonArray[2];
    }
}