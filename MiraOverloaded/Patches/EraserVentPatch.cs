using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraOverloaded.Options.Roles.Impostor;
using MiraOverloaded.Roles.Impostor;

namespace MiraOverloaded.Patches;

public static class EraserVentPatch
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

        if (localPlayer.Data?.Role is not EraserRole)
        {
            return;
        }

        if (!OptionGroupSingleton<EraserOptions>.Instance.CanVent)
        {
            @event.Cancel();
        }
    }
}
