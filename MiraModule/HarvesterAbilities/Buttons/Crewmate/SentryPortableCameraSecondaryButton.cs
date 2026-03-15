using TownOfUs.Roles.Crewmate;

namespace MiraModule.HarvesterAbilities.Buttons.Crewmate;

public sealed class SentryPortableCameraSecondaryButton : SentryPortableCameraButtonBase
{
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;

    protected override bool ShouldBeVisible(SentryRole role)
    {
        return AllCamerasPlaced();
    }
}


