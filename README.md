# Mira Overloaded

An addon module for Town of Us Mira that extends gameplay with additional roles and features.

## Description

MiraOverloaded is a plugin addon for Town of Us Mira that uses the MiraAPI framework to add new content to the game.

## Requirements

- Among Us (latest version)
- BepInEx 6.0.0-be.735 or higher
- Reactor 2.5.0-ci.371 or higher
- MiraAPI 0.3.5 or higher
- Town of Us Mira 1.5.0 or higher

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
