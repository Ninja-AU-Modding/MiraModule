using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Roles;
using AmongUs.GameOptions;
using MiraModule.Roles.Crewmate;
using TownOfUs.Events.TouEvents;

namespace MiraModule.Events;

public static class InspectorGeneralEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            InspectorGeneralRole.ClearAllState();
            CommandSpecialistRole.ClearAllState();
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var target = @event.Target;
        if (target == null || target.Data?.Role == null)
        {
            return;
        }

        if (target.Data.Role.Role == (RoleTypes)RoleId.Get<InspectorGeneralRole>())
        {
            CommandSpecialistRole.RevertAllToOriginal();
        }
    }

    [RegisterEvent(400)]
    public static void WrapUpEvent(EjectionEvent @event)
    {
        var exiledPlayer = @event.ExileController.initData.networkedPlayer?.Object;
        if (exiledPlayer == null || exiledPlayer.Data?.Role == null)
        {
            return;
        }

        if (exiledPlayer.Data.Role.Role == (RoleTypes)RoleId.Get<InspectorGeneralRole>())
        {
            CommandSpecialistRole.RevertAllToOriginal();
        }
    }

    [RegisterEvent]
    public static void ChangeRoleHandler(ChangeRoleEvent @event)
    {
        if (@event.OldRole == null)
        {
            return;
        }

        if (@event.OldRole is InspectorGeneralRole && @event.NewRole is not InspectorGeneralRole)
        {
            CommandSpecialistRole.RevertAllToOriginal();
        }
    }
}
