using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraOverloaded.Roles.Impostor;

namespace MiraOverloaded.Events.Impostor;

public static class NinjaEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Source.Data.Role is not NinjaRole ninja)
        {
            return;
        }

        ninja.TriggerAssassinateCooldown();
    }
}
