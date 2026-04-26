using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraModule.Roles.Impostor;

namespace MiraModule.Events.Impostor;

public static class DistorterEvents
{
    [RegisterEvent(-1)]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        if (@event.Source.Data.Role is not DistorterRole distorter)
        {
            return;
        }

        if (distorter.PostInvisibilityActive)
        {
            @event.Cancel();
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Source.Data.Role is not DistorterRole distorter)
        {
            return;
        }

        distorter.OnMurderResolved();
    }
}
