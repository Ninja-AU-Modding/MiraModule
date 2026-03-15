using TownOfUs.Roles.Crewmate;

namespace MiraModule.HarvesterAbilities.Buttons.Crewmate;

public sealed class SentryPortableCameraButton : SentryPortableCameraButtonBase
{
    public override BaseKeybind Keybind => Keybinds.ModifierAction;

    protected override bool ShouldBeVisible(SentryRole role)
    {
        return !AllCamerasPlaced();
    }
}
