using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Map;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraModule.Roles.Impostor;
using MiraModule.Options.Roles.Impostor;

namespace MiraModule.Events.Impostor;

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

    [RegisterEvent(1000)]
    public static void UpdateSystemEvent(UpdateSystemEvent @event)
    {
        if (@event.Player == null || !@event.Player.AmOwner)
        {
            return;
        }

        if (@event.Player.Data.Role is not ArbiterRole arbiter)
        {
            return;
        }

        if (!OptionGroupSingleton<ArbiterOptions>.Instance.DoorsCountAsUse &&
            IsDoorSystem(@event.SystemType))
        {
            return;
        }

        if (!arbiter.TryConsumeSabotageUse())
        {
            @event.Cancel();
        }
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

    private static bool IsDoorSystem(SystemTypes systemType)
    {
        var ship = ShipStatus.Instance;
        if (ship == null || ship.Systems == null || !ship.Systems.ContainsKey(systemType))
        {
            return false;
        }

        var sys = ship.Systems[systemType];
        if (sys == null)
        {
            return false;
        }

        return sys.GetType().Name.Contains("Doors");
    }
}
