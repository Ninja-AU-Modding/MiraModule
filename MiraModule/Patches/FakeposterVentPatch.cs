using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraModule.Options.Roles.Neutral;
using MiraModule.Roles.Neutral;

namespace MiraModule.Patches;

public static class FakeposterVentPatch
{
    [RegisterEvent]
    public static void PlayerCanUseEventHandler(PlayerCanUseEvent @event)
    {
        if (!@event.IsVent)
        {
            return;
        }

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null)
        {
            return;
        }

        if (localPlayer.Data?.Role is not FakeposterRole)
        {
            return;
        }

        if (!OptionGroupSingleton<FakeposterOptions>.Instance.CanVent)
        {
            @event.Cancel();
        }
    }
}
