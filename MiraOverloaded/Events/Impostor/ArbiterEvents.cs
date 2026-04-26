using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraOverloaded.Roles.Impostor;

namespace MiraOverloaded.Events.Impostor;

public static class ArbiterEvents
{
    [RegisterEvent]
    public static void AfterMurderEvent(AfterMurderEvent @event)
    {
        if (@event.Source.Data.Role is not ArbiterRole arbiter)
        {
            return;
        }

        if (!arbiter.RewardActive)
        {
            return;
        }

        arbiter.ApplyKillCooldownBonus();
    }

    [RegisterEvent(400)]
    public static void WrapUpEvent(EjectionEvent @event)
    {
        var exiledPlayer = @event.ExileController.initData.networkedPlayer?.Object;
        if (exiledPlayer == null)
        {
            return;
        }

        foreach (var plr in PlayerControl.AllPlayerControls.ToArray())
        {
            if (plr.Data?.Role is not ArbiterRole arbiter)
            {
                continue;
            }

            if (arbiter.Player.HasDied() || arbiter.TargetId == byte.MaxValue)
            {
                continue;
            }

            if (exiledPlayer.PlayerId != arbiter.TargetId)
            {
                continue;
            }

            arbiter.ActivateReward();
        }
    }
}
