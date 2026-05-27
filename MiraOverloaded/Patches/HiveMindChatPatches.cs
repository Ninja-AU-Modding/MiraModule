using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraOverloaded.Modifiers;
using MiraOverloaded.Options.Roles.Neutral;
using MiraOverloaded.Roles.Neutral;
using Reactor.Networking.Attributes;
using TownOfUs.Options;
using TownOfUs.Patches.Options;
using UnityEngine;

namespace MiraOverloaded.Patches;

public static class HiveMindChatPatches
{
    [MethodRpc((uint)MiraOverloadedRpc.HiveMindChat)]
    public static void RpcSendHiveMindChat(PlayerControl sender, string text)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null) return;

        var isHiveMindMember = local.Data?.Role is HiveMindRole || local.HasModifier<LinkedMindModifier>();
        var isDead = local.HasDied();
        var theDeadKnow = OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow;

        if (!isHiveMindMember && !(isDead && theDeadKnow)) return;

        var opts = OptionGroupSingleton<HiveMindOptions>.Instance;
        string displayName;
        if (sender.AmOwner)
            displayName = $"{sender.Data.PlayerName} (You)";
        else if (opts.HiveMindKnows)
            displayName = sender.Data.PlayerName;
        else
            displayName = "Hive Member";

        var nameHtml = ColorUtility.ToHtmlStringRGBA(MiraOverloadedColors.HiveMind);
        MiscUtils.AddTeamChat(sender.Data, $"<color=#{nameHtml}>{displayName}</color>", text,
            bubbleType: BubbleType.Other, onLeft: !sender.AmOwner);
    }

    public static void RegisterChatHandler()
    {
        var handler = new TeamChatPatches.ExtensionTeamChatHandler
        {
            Priority = 50,
            IsForced = false,
            IsChatAvailable = () =>
            {
                var local = PlayerControl.LocalPlayer;
                if (local == null || MeetingHud.Instance == null) return false;
                if (local.Data?.Role is HiveMindRole || local.HasModifier<LinkedMindModifier>()) return true;
                // Dead spectators can read hive chat when TheDeadKnow is enabled
                return local.HasDied() &&
                       OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow;
            },
            SendMessage = (sender, msg) => RpcSendHiveMindChat(sender, msg),
            GetDisplayText = () => "Hive Mind Chat",
            DisplayTextColor = MiraOverloadedColors.HiveMind
        };
        TeamChatPatches.ExtensionTeamChatRegistry.RegisterHandler(handler);
    }
}
