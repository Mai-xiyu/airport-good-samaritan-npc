# GoodSamaritanNpc

BepInEx 6 IL2CPP mod for **Airport Security Sucks! Demo**.

It adds witness-style civilian NPC behavior without changing the original NPC combat/ragdoll/jail flow. Witness NPCs do not read the real smuggler role. They only react to suspicious observable behavior, then report either a directly visible suspicious person or a nearby area.

## Features

- Server-authoritative witness NPC logic.
- Existing NPC conversion plus optional extra witness spawns.
- Direct callout: uses the game's original spotted icon plus player indicator fallback so TSA/agent targets can also be marked.
- Area callout: uses the game's original log and NPC question indicator.
- Native in-game mod menu inside the original `Settings` UI, with a `Mods` tab and an F8 shortcut.
- Shared config-menu API for other BepInEx mods.
- Optional playable witness players. Only clients that also have the mod installed can be randomly assigned by a modded host.
- Playable witnesses are forced onto the TSA/agent faction for win/loss handling.
- Modded clients additionally show a local exclamation marker and play a short local alert sound.
- TSA attacks against witness NPCs still use the original ordinary-NPC attack punishment path.
- Localized report text.

## Supported Languages

Config key: `Language`

Supported values:

- `Auto`
- `zh-Hans`
- `en`
- `ja`
- `ko`
- `fr`
- `de`
- `es`
- `ru`
- `pt`
- `tr`
- `uk`

`Auto` uses Simplified Chinese on Simplified Chinese systems, otherwise English.

## Install

1. Install BepInEx 6 IL2CPP for the game.
2. Copy `GoodSamaritanNpc.dll` into:

   ```text
   Airport Security Sucks! Demo/BepInEx/plugins/
   ```

3. Start the game once.
4. Edit:

   ```text
   Airport Security Sucks! Demo/BepInEx/config/com.airport.good_samaritan.cfg
   ```

The in-game `Settings` screen gets a native `Mods` tab. Press `F8` to open `Settings` directly on that tab.

## NPC Suspicion Presets

- `Easy`: lower false positives, area reports only, no direct player pointing.
- `Normal`: core behavior, including contraband, hidden contraband, reveal actions, civilian attacks, and contraband pickup.
- `Hard`: strict behavior, also reports suspicious jumping and likely queue cutting.
- `Custom`: manual toggles from the in-game menu or config file.

## Build Locally

The project includes the BepInEx and generated IL2CPP interop reference DLLs under `lib/BepInEx`, so GitHub Actions and a clean local checkout can build the plugin directly.

Default build path:

```powershell
dotnet build .\GoodSamaritanNpc.csproj -c Release
```

If your game is installed elsewhere:

```powershell
dotnet build .\GoodSamaritanNpc.csproj -c Release /p:GameDir="D:\Path\To\Airport Security Sucks! Demo"
```

Or set:

```powershell
$env:AIRPORT_SECURITY_SUCKS_DIR = "D:\Path\To\Airport Security Sucks! Demo"
dotnet build .\GoodSamaritanNpc.csproj -c Release
```

On successful local build, the DLL is copied to `BepInEx/plugins` when the plugin directory exists.

To force a different reference folder without changing the install target:

```powershell
dotnet build .\GoodSamaritanNpc.csproj -c Release /p:ReferenceRoot="D:\Path\To\ReferenceRoot"
```

## Release Workflow

`.github/workflows/release.yml` creates a GitHub release on `v*` tags or manual dispatch.

Release assets:

- `GoodSamaritanNpc.dll`
- `GoodSamaritanNpc-vX.Y.Z.zip`

The workflow builds against `lib/BepInEx` and uploads the compiled plugin, not only source code.

## Mod Menu API

Other mods can register a page if they reference this plugin assembly:

```csharp
GoodSamaritanModMenuApi.RegisterPage(
    "my_mod",
    "My Mod",
    builder =>
    {
        builder.AddSection("My Mod");
        builder.AddToggle("Enabled", true, enabled => { /* save config */ });
        builder.AddFloatSlider("Spawn chance", 0.25f, 0f, 1f, value => { /* save config */ });
    });
```

Pages are built with Unity uGUI controls inside the game's original `Settings` canvas, not IMGUI.

## Playable Witnesses

Playable witness assignment uses a mod-to-mod handshake over an existing Mirror command. A modded client periodically sends a special no-op voice command payload. A modded host intercepts that payload and marks the player as eligible. Unmodded clients never send it, and unmodded hosts do not run the assignment logic.

Relevant config keys:

- `EnablePlayableWitnessPlayers`
- `MaxPlayableWitnessPlayers`
- `PlayableWitnessChance`
