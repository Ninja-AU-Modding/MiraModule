using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using System;
using UnityEngine;

namespace MiraOverloaded.Options;

public enum GameLocale
{
    en_US,
    es_ES,
}
public sealed class MiraOverloadedOptions : AbstractOptionGroup
{
    public override string GroupName => "Mira Overloaded";
    public override Color GroupColor => MiraOverloadedColors.MiraOverloaded;
    public override Type? OptionableType => null;

    [ModdedToggleOption("MiraOptionDisableRoles")]
    public bool DisableRoles { get; set; } = true;
    [ModdedEnumOption("MiraOptionGameLocale", typeof(GameLocale))]
    public GameLocale Locale { get; set; } = GameLocale.en_US;
}
