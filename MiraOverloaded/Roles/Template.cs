//using System;
//using System.Collections.Generic;
//using AmongUs.GameOptions;
//using AsmResolver.DotNet.Resources;
//using Il2CppInterop.Runtime.Attributes;
//using MiraAPI.GameOptions;
//using MiraAPI.Hud;
//using MiraAPI.Patches.Freeplay;
//using MiraAPI.Patches.Stubs;
//using MiraAPI.Roles;
//using MiraOverloaded.Assets;
//using MiraOverloaded.Buttons.Crewmates;
//using MiraOverloaded.Options.Roles.Crewmates;
//using Reactor.Networking.Attributes;
//using Reactor.Utilities;
//using TownOfUs.Roles.Crewmate;
//using UnityEngine;

//namespace MiraOverloaded.Roles.Crewmate;

//public sealed class TemplateRole(IntPtr cppPtr)
//    : TemplateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
//{

//    public string LocaleKey => "Template";
//    public string RoleName => TouLocale.Get($"MiraRole{LocaleKey}");
//    public string RoleDescription => TouLocale.GetParsed($"MiraRole{LocaleKey}IntroBlurb");
//    public string RoleLongDescription => TouLocale.GetParsed($"MiraRole{LocaleKey}TabDescription");

//    public string GetAdvancedDescription()
//    {
//        return TouLocale.GetParsed($"MiraRole{LocaleKey}WikiDescription") +
//               MiscUtils.AppendOptionsText(GetType());
//    }

//    [HideFromIl2Cpp]
//    public List<CustomButtonWikiDescription> Abilities =>
//    [
//        new(
//            TouLocale.GetParsed($"MiraRole{LocaleKey}Template", "Template"),
//            TouLocale.GetParsed($"MiraRole{LocaleKey}TemplateWikiDescription"),
//            CrewAssets.TemplateSprite),
//    ];

//    public Color RoleColor => MiraOverloadedColors.ROLECOLOR;
//    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate/ModdedRoleTeams.Custom/ModdedRoleTeams.Impostor;
//    public RoleAlignment RoleAlignment => RoleAlignment.ALIGNMENT;

//    public CustomRoleConfiguration Configuration => new(this)
//    {
//        Icon = RoleIcons.ROLEICON (defined in RoleIcons),
//        GhostRole = RoleTypes.CrewmateGhost/RoleTypes.ImpostorGhost,
//        FreeplayFolder = TaskAdderPatches.CrewmateName/NeutralName/ImpostorName,
//    };

//    public override void Initialize(PlayerControl player)
//    {
//        RoleBehaviourStubs.Initialize(this, player);
//    }

//    public override void Deinitialize(PlayerControl targetPlayer)
//    {
//        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
//    }
//}

//Template for roles.
