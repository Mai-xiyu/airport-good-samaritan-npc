# GoodSamaritanNpc

BepInEx 6 IL2CPP mod for **Airport Security Sucks! Demo**.

It adds witness-style civilian NPC behavior without changing the original NPC combat/ragdoll/jail flow. Witness NPCs do not read the real smuggler role. They only react to suspicious observable behavior, then report either a directly visible suspicious person or a nearby area.

## Features

- Server-authoritative witness NPC logic.
- Existing NPC conversion plus optional extra witness spawns.
- Direct callout: uses the game's original spotted icon on the suspicious target.
- Area callout: uses the game's original log and NPC question indicator.
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

## Build Locally

The project needs the game's generated BepInEx interop assemblies. They are not included in this repository.

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

## Release Workflow

`.github/workflows/release.yml` creates a GitHub release on `v*` tags or manual dispatch.

Important constraint: GitHub-hosted runners do not have the game's IL2CPP interop assemblies. The workflow always publishes a source package. It also publishes `GoodSamaritanNpc.dll` when either:

- a self-hosted Windows runner has `AIRPORT_SECURITY_SUCKS_DIR` pointing at the game install, or
- a private `refs/` folder exists in the runner workspace with the required BepInEx/core and BepInEx/interop DLLs.

Do not commit game DLLs or generated interop DLLs to a public repository.
