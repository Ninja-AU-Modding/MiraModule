> [!WARNING]
> This is not a standalone mod! It runs on [Town of Us Mira](https://github.com/AU-Avengers/TOU-Mira), [Reactor](https://github.com/NuclearPowered/Reactor), and [MiraAPI](https://github.com/All-Of-Us-Mods/MiraAPI)!
> The ZIP files released with every MiraOverloaded update contain these 3, but PLEASE PLEASE PLEASE check them out :D

-----------------------

<div align="center">
  <img src="https://raw.githubusercontent.com/SabotagedAU/MiraOverloaded/refs/heads/first-release/MiraOverloaded/Resources/Banner.png" alt="MiraOverloaded"/>
  <a href="https://github.com/AU-Avengers/TOU-Mira/releases/"> <img alt="Latest Release" src="https://badgen.net/github/release/SabotagedAU/MiraOverloaded?icon=github"></a>
  <a href="https://github.com/AU-Avengers/TOU-Mira/releases"> <img alt="GitHub Downloads" src="https://img.shields.io/github/downloads/SabotagedAU/MiraOverloaded/total"></a>
  <a href="https://discord.gg/rVruMxTfJK"><img alt="Mod Discord" src="https://img.shields.io/discord/1279057211339051079.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2"></a>
</div>
<br/>

The largest extension mod for [Town Of Us Mira](https://github.com/AU-Avengers/TOU-Mira), featuring 14 new roles, and over 5 new modifiers!

Currently, we have 59 roles planned in total for future releases

## Requirements

> [!NOTE]
> Pirated versions of Among Us are not, and never will be supported.

[![Latest Among Us Version](https://badgen.net/badge/icon/Latest%20Among%20Us%20Version?icon=steam&label=)](https://www.innersloth.com/games/among-us/)

[![BepInEx Il2cpp](https://github.com/SabotagedAU/MiraOverloaded/blob/first-release/bepinbadge.svg)](https://docs.bepinex.dev/master/articles/user_guide/installation/unity_il2cpp.html)

[![Reactor](https://github.com/SabotagedAU/MiraOverloaded/blob/first-release/reactorbadge.svg)](https://github.com/NuclearPowered/Reactor)

[![MiraAPI](https://github.com/SabotagedAU/MiraOverloaded/blob/first-release/miraapibadge.svg)](https://github.com/All-Of-Us-Mods/MiraAPI)

[![Town Of Us Mira](https://github.com/SabotagedAU/MiraOverloaded/blob/first-release/toubadge.svg)](https://github.com/AU-Avengers/TOU-Mira)

## Installation

1. Make sure you have all the required dependencies installed
2. Build the project using Visual Studio or Rider
3. The compiled DLL will be automatically copied to your Among Us BepInEx plugins folder (if AmongUs path is configured)
4. Launch Among Us

## Development

### Building

1. Clone this repository
2. Open `MiraOverloaded.sln` in your IDE
3. Configure your Among Us installation path in `AmongUs.props` (optional, for auto-copy)
4. Build the solution

### Project Structure

```
MiraOverloaded/
├── Assets/           # Asset loading classes
├── Buttons/          # Custom action buttons
├── Modules/          # Core module logic
├── Options/          # Configuration options
├── Patches/          # Harmony patches
├── Resources/        # Embedded resources (images, localization)
│   └── Locale/      # Localization files
└── Roles/           # Custom role implementations
```

## Contributing

Feel free to submit issues and pull requests!

## License

This project is licensed under the same license as Town of Us Mira.
