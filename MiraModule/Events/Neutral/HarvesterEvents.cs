using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraModule.Roles.Neutral;

namespace MiraModule.Events.Neutral;

public static class HarvesterEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Source.Data.Role is not HarvesterRole harvester)
        {
            return;
        }

        if (!@event.Source.AmOwner)
        {
            return;
        }

        harvester.TryHarvestAbility(@event.Target);
    }
}
