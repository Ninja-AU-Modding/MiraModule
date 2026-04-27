using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraOverloaded.Options.Roles.Neutral;
using MiraOverloaded.Patches;
using MiraOverloaded.Roles.Neutral;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Game.Alliance;
using UnityEngine;

namespace MiraOverloaded.Events;

public static class HiveMindEvents
{
    [RegisterEvent]
    public static void OnRoundStartHost(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro || !AmongUsClient.Instance.AmHost) return;

        var hiveMind = CustomRoleUtils.GetActiveRolesOfType<HiveMindRole>().FirstOrDefault();
        if (hiveMind == null) return;

        var otherMinds = (int)OptionGroupSingleton<HiveMindOptions>.Instance.OtherConnectedMinds;
        var eligible = Helpers.GetAlivePlayers();
        foreach (var player in eligible)
        {
            if (player == null) continue;

            var role = player.Data.Role;
            if (role != null) { 
                eligible.Remove(player);
                continue;
            }

            if (role is not ICustomRole customRole)
            {
                eligible.Remove(player);
                continue;
            }

            if (customRole.Team == ModdedRoleTeams.Impostor)
            {
                eligible.Remove(player);
                continue;
            }

            if (
                player.HasModifier<EgotistModifier>() ||
                player.HasModifier<CrewpostorModifier>() ||
                customRole is HiveMindRole
            )
            {
                eligible.Remove(player);
                continue;
            }
        }

        for (var i = 0; i < otherMinds; i++)
        {
            var person = eligible.Random();
            if (person == null) continue;
            eligible.Remove(person);
            person.RpcSetRole((RoleTypes)RoleId.Get<HiveMindRole>());
        }
    }
}
