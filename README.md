# GoodSamaritanNpc

BepInEx 6 IL2CPP mod for **Airport Security Sucks!**.

Current version: `1.4.1`.

This branch targets the full Steam release of the game. The bundled reference assemblies under `lib/BepInEx` are generated from the full release, not the old demo.

It adds witness-style civilian NPC behavior without changing the original NPC combat/ragdoll/jail flow. Witness NPCs do not read the real smuggler role. They only react to suspicious observable behavior, then report either a directly visible suspicious person or a nearby area.

## Features

- Server-authoritative witness NPC logic.
- Existing NPC conversion plus optional extra witness spawns.
- Full-release hijacking mode support for plane passenger NPCs spawned by `HijackingNpcs`.
- Direct callout: uses the game's original spotted icon plus player indicator fallback so TSA/agent targets can also be marked.
- Area callout: uses the game's original log and NPC question indicator.
- Optional playable witness players. Only clients that also have the mod installed can be randomly assigned by a modded host.
- Playable witnesses are forced onto the TSA/agent faction for win/loss handling.
- Optional playable undercover players. They stay on the smuggler win/loss side and are disabled by default.
- TSA/agent and playable-witness actions are ignored by default for ordinary suspicion checks, reducing false reports.
- Modded clients additionally show a local exclamation marker, blue ally outlines, yellow report outlines/areas, and play a short local alert sound.
- TSA attacks against witness NPCs still use the original ordinary-NPC attack punishment path.
- Undercover attacks against NPCs bypass the original self-jail side effect while still allowing witness reports.
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
   Airport Security Sucks!/BepInEx/plugins/
   ```

3. Start the game once.
4. Edit:

   ```text
   Airport Security Sucks!/BepInEx/config/com.airport.good_samaritan.cfg
   ```

Configuration is file-based only. This mod does not add an in-game config GUI.

No menu, GUI, or shared config API is included. Edit the generated cfg under `BepInEx/config`.

## NPC Suspicion Presets

- `Easy`: lower false positives, area reports only, no direct player pointing.
- `Normal`: core behavior, including contraband, hidden contraband, reveal actions, civilian attacks, and contraband pickup.
- `Hard`: strict behavior, also reports suspicious jumping and likely queue cutting.
- `Custom`: manual toggles from the config file.

False-positive control:

- `IgnoreTsaSuspicion=true`: TSA/agent players do not trigger ordinary suspicion checks.
- `ReportTsaCivilianAttacks=false`: TSA/agent attacks against civilians still use the game's original punishment, but witnesses do not report them unless this is enabled.

## Client Highlights

These are local-only visual additions for modded clients. They do not replace the game's original red smuggler outline.

- Blue outline: TSA/agent players, playable witnesses, and known witness NPCs.
- Yellow outline: witness-reported suspicious players.
- Yellow floor rectangle: area report when a witness cannot directly identify a target.
- Undercover players and hijackers in the game's hijacking mode do not see local witness-side or report highlight enhancements.

Relevant config keys:

- `ShowTeamHighlights`
- `ShowReportHighlightsToAllModdedClients`
- `AreaHighlightSeconds`
- `HighlightSeconds`

## Source Layout

- `src/GoodSamaritanNpc/Plugin`: BepInEx plugin entrypoint.
- `src/GoodSamaritanNpc/Config`: cfg-only BepInEx configuration.
- `src/GoodSamaritanNpc/Core`: manager lifecycle, playable roles, NPC witness population, and witness state.
- `src/GoodSamaritanNpc/Detection`: suspicion event types and detection scans.
- `src/GoodSamaritanNpc/Reporting`: report dispatch, cooldowns, original-game feedback calls, and visibility checks.
- `src/GoodSamaritanNpc/Feedback`: local markers, outline highlights, and area highlights.
- `src/GoodSamaritanNpc/Localization`: localized report text.
- `src/GoodSamaritanNpc/Patches`: Harmony patches for game events and mod-to-mod capability handshakes.
- `src/GoodSamaritanNpc/World`: area-name resolution and local scene helpers.

## Build Locally

The project includes the BepInEx and generated IL2CPP interop reference DLLs under `lib/BepInEx`, so GitHub Actions and a clean local checkout can build the plugin directly.

Default build path:

```powershell
dotnet build .\GoodSamaritanNpc.csproj -c Release
```

If your game is installed elsewhere:

```powershell
dotnet build .\GoodSamaritanNpc.csproj -c Release /p:GameDir="D:\Path\To\Airport Security Sucks!"
```

Or set:

```powershell
$env:AIRPORT_SECURITY_SUCKS_DIR = "D:\Path\To\Airport Security Sucks!"
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

## Playable Witnesses

Playable witness assignment uses a mod-to-mod handshake over an existing Mirror command. A modded client periodically sends a special no-op voice command payload. A modded host intercepts that payload and marks the player as eligible. Unmodded clients never send it, and unmodded hosts do not run the assignment logic.

Relevant config keys:

- `EnablePlayableWitnessPlayers`
- `MaxPlayableWitnessPlayers`
- `PlayableWitnessChance`

## Playable Undercover

Undercover assignment uses the same mod-to-mod handshake and is disabled by default. When enabled, the host randomly assigns eligible modded players up to the configured limit.

Undercover players are forced to `isAgent=false`, so the game's existing win/loss logic treats them as smuggler-side. Their NPC-hit jail side effect is skipped, but the NPC hit/tackle path and witness report path remain active.

Relevant config keys:

- `EnablePlayableUndercoverPlayers`
- `MaxPlayableUndercoverPlayers`
- `PlayableUndercoverChance`
