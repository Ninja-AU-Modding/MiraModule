using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraOverloaded.Modifiers.Alliance;
using TownOfUs.Options;

namespace MiraOverloaded.Utilities;

public static class PlayerRoleTextExtensions
{
    public static string UpdateAgentSymbols(this string name, PlayerControl player, bool hidden = false)
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var isDead = PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !hidden;

        if (player.IsImpostor() && player.TryGetModifier<AgentModifier>(out var agentMod) &&
            (AgentModifier.AgentVisibilityFlag(player) || isDead))
        {
            name += $"<color=#FFFFFF> (<color=#72B3CAFF>{agentMod.ModifierName}</color>)</color>";
        }

        return name;
    }
}
