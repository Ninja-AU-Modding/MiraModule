using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraModule.Modifiers.Neutral;
using MiraModule.Roles.Neutral;
using MiraModule.HarvesterAbilities.Buttons;
using TownOfUs.Buttons;

namespace MiraModule.Events.Neutral;

public static class HarvesterEvents
{
    private sealed class PendingHarvest
    {
        public PendingHarvest(byte targetId, ushort roleId)
        {
            TargetId = targetId;
            RoleId = roleId;
        }

        public byte TargetId { get; }
        public ushort RoleId { get; }
    }

    private static readonly Dictionary<byte, PendingHarvest> PendingHarvests = new();

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        if (@event.Source == null || @event.Target == null)
        {
            return;
        }

        if (@event.Source.Data.Role is not HarvesterRole)
        {
            return;
        }

        var targetRole = @event.Target.Data?.Role;
        if (targetRole == null)
        {
            return;
        }

        if (!HarvesterRole.IsTownOfUsRole(targetRole) || targetRole is HarvesterRole)
        {
            return;
        }

        PendingHarvests[@event.Source.PlayerId] = new PendingHarvest(@event.Target.PlayerId, (ushort)targetRole.Role);
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Source.Data.Role is not HarvesterRole)
        {
            return;
        }

        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (PendingHarvests.TryGetValue(@event.Source.PlayerId, out var pending))
        {
            PendingHarvests.Remove(@event.Source.PlayerId);
            if (pending.TargetId == @event.Target.PlayerId)
            {
                HarvesterRole.TryHarvestRole(@event.Source, @event.Target, pending.RoleId);
                return;
            }
        }

        HarvesterRole.TryHarvestRole(@event.Source, @event.Target);
    }

    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null)
        {
            return;
        }

        var button = @event.Button;
        if (button == null)
        {
            return;
        }

        if (!button.CanClick())
        {
            return;
        }

        if (button is TownOfUs.Buttons.IKillButton || button is MiraModule.HarvesterAbilities.Buttons.IKillButton)
        {
            return;
        }

        if (!player.TryGetModifier<HarvesterCacheModifier>(out _))
        {
            return;
        }

        if (AmongUsClient.Instance.AmHost)
        {
            HarvesterRole.ClearHarvestedRole(player);
        }
    }
}
