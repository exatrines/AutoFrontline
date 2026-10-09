# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Added

- Added `CONTRIBUTING.md`, synced from the shared contributing guide

### Changed

- README layout: centered plugin icon, English | Japanese switch, pink and white Release / Changelog / License badges, and a Contributing section
- Moved the Japanese README to `docs/README.ja.md`
- Restored the full GNU AGPL v3 license text

## [1.6.0.0] - 2026-10-09

### Added

- Configurable UI language: follow the client, or lock English / Japanese
- Plugin page (title-bar Heart or sidebar icon): Discord, Support, and GitHub
- New plugin icon (AF)
- English and Japanese README
- Plugin icon and i18n files ship from `Data/`

### Changed

- Window, sidebar, and tab names stay English; plugin page copy stays English
- Recommended job: use Black Mage or any ranged DPS; melee jobs may not work well enough
- Latest MirageUI (TwoColumn, dropdowns, plugin page). Config footer links removed

### Removed

- GitHub Actions Release workflow; releases are cut locally

## [1.5.0.0] - 2026-07-03

### Added

- Bundled plugin icon (`Assets/AutoFrontlineIcon.png`) in the settings sidebar

### Changed

- Settings UI migrated to **MirageUI** (TwoColumn). Sidebar: General / Settings / Experimental / Debug, with GitHub / OFUSE / Ko-fi footer links
- Settings / Experimental / Debug use MirageUI controls (CheckboxGroup, Combo, Slider, and similar)
- Experimental: commander follow and combat mode use CheckboxGroup with help text and indented child settings (disabled when off)

### Removed

- vnavmesh debug tab and `VNavmeshIpc`
- Legacy UI helpers (`AflImGui`, `ConfigFooter`)

## [1.4.1.0] - 2026-06-27

### Changed

- Group movement: when only one ally is inside the search radius, prefer a cluster of **2 or more allies within 30m** whose center is closest. If none, follow the nearest singleton in radius
- When required plugins are missing, only **Mode** is locked to Disable. Other settings stay editable (full-window grey-out removed)

## [1.4.0.0] - 2026-06-27

### Added

- **Group search radius** in Settings (25–100m, default 75m)
- Follow-target exclusion: after **5 consecutive** stationary picks, or the same target picked **N times in a row** (N is 1–20, default 10), add them to an exclusion list. Exclusion duration 0–20 seconds (default 10, 0 = off)
- `HostileModeEnabled`: combat mode and commander follow as Experimental tab toggles

### Changed

- Code laid out by domain (`Core` / `Services/*` / `UI/Tabs`, and similar)
- Follow split into `FollowModeSelector` / `FollowMoveResolver` / `GroupMoveSelector`
- Always use cluster logic (legacy Densest group move removed; ConfigVersion 3 migration)
- First spawn exit: fixed-coordinate fields moveto on re-entry; unconfigured fields use group movement. Return is enabled after leaving the spawn zone once
- Return (formerly Dejon) settings moved to Settings → Movement. English label is Return
- Special combat objects: Icedome Lis in Shatter only, Strike Drones in Seize only
- `/vnav stop` on death only in Frontline while automation is on
- `PlayerMovementGate` suppresses moveto only while casting
- Debug: AutoFrontline / vnavmesh sub-tabs, excluded follow-target list (name, reason, remaining time), Spawn / Mount / Return info
- Mount whenever outside enemy and special-object range

### Removed

- Mount distance setting
- Unchanged-position check no longer suppresses moveto (used only for re-pick and stationary exclusion)

## [1.3.1.1] - 2026-06-22

### Added

- Experimental: **Experimental Group Move** (cluster center within 50m of self. If fewer than 2 allies within 50m, the nearest 2 are candidates. Cluster radius 30m. Prefer the closer cluster of equal size. Combat mode is skipped while this is on. Mount only with no enemy within 20m; dismount when one is present. Destination distance is ignored)

### Changed

- When a follow target's position is unchanged and you are **20m or more** away, keep moving instead of suppressing moveto

### Removed

- `/vnav stop` when switching movement modes

## [1.3.1.0] - 2026-06-12

### Changed

- Field of Glory (Shatter): first spawn-exit destination fixed at `(0, 0, 0)`

## [1.3.0.9] - 2026-06-12

### Changed

- The Fields of Glory (Secure): first spawn-exit destination fixed at `(-10, -15, 0)`

## [1.3.0.8] - 2026-06-12

### Changed

- Onsal Hakair (Danshig Naadam): first spawn-exit destination fixed at `(0, 0, 1)`

## [1.3.0.7] - 2026-06-11

### Added

- Auto-target Strike Drones (ModelCharaId `0xC19`), same as Icedome Lis
- Auto-target Strike Systems (ModelCharaId `0x233C`), same as Icedome Lis

### Changed

- Stall duration for stuck detection is an Experimental slider **10–30 seconds** (default 15)
- Commander follow is a separate Experimental checkbox (default: off)

## [1.3.0.6] - 2026-06-12

### Added

- During group movement, if the destination is 5m or more away and the player stays within 1m for 10 seconds: `/vnav stop` → `/pvpaction デジョン` → SelectYesno Yes

## [1.3.0.5] - 2026-06-11

### Added

- `/vnav stop` when HP is 0
- Seal Rock: first spawn-exit destination fixed at map center `(0, 0, 0)` (other fields to follow)

### Changed

- `/vnav stop` before the next `moveto` when switching movement modes (group / combat / commander / initial)

## [1.3.0.4] - 2026-06-12

### Changed

- Follow priority is **combat > commander > group**
- While combat conditions hold (enemy within 30m and an ally within 30m of that enemy), alliance chat does not start commander follow
- Switch to combat immediately if combat conditions hold during commander follow
- Commander follow arrival distance **5m → 15m**
- Skip commander follow if already **within 15m** of the commander; fall back to combat or group
- Combat mode: ally check around the enemy expanded from at most 10 players to **every ally within 30m**

## [1.3.0.3] - 2026-06-12

### Added

- Auto-target Icedome Lis (ModelCharaId `0x1E0`) when closer than the nearest enemy player

## [1.3.0.2] - 2026-06-12

### Added

- Record the position at horizontal **25m** from spawn center (exclusion 20m + 5m) as the first exit destination (reset each match)
- On re-entering the spawn zone, moveto the first exit destination (ahead of normal follow)
- Debug: follow mode shows initial movement (to exit), plus a reset-initial-state button

### Changed

- Replaced consecutive moveto-refresh handling (NaviStackGuard) with **initial movement mode**

## [1.3.0.1] - 2026-06-11

### Added

- Experimental auto PvP Limit Break (`/pvpaction`; optional target; 5 second throttle)
- Automatic recovery when consecutive moveto refreshes stack (reissue toward a random position)
- Do not target inside the spawn exclusion zone

### Changed

- Smaller stability fixes and refactoring

## [1.3.0.0] - 2026-06-02

### Added

- Commander follow (`FollowCommander`) in addition to group and combat
- Track the most recent alliance-chat speaker as LatestCommander and move until 5m away
- End commander follow at 5m or when the commander HP is 0, then return to combat or group
- Debug Target shows follow mode (group / combat / commander)

## [1.2.0.2] - 2026-06-02

### Added

- Record self position on Frontline entry and do not `moveto` within **15m** (spawn exclusion zone)
- Debug Movement shows entry coordinates and exclusion state

### Fixed

- Record Entry position on frames before `Player` is available after zoning in

## [1.2.0.1] - 2026-06-02

### Changed

- Mode **Auto** renamed to **Loop** (enum value stays 2 for compatibility)
- General tab mode descriptions updated

## [1.2.0.0] - 2026-06-02

### Added

- **Mode** combo (Disable / Manual / Loop) replacing Enable
- **Loop**: Start/Stop, MaxCount, entry count. Mode is locked while Start is active
- Loop: queue Daily Challenge Frontline roulette → auto enter → auto leave after the match, up to MaxCount (count +1 on entry)
- Contents Finder: roulette row Text #6 and callback `3` (Leaf index)
- Dismount also when ModelCharaId `0x1E0` (Icedome Lis) is within **Dismount distance**
- Migrate legacy `Enabled` to ConfigVersion 2

## [1.1.0.4] - 2026-06-02

### Added

- **Auto enter** / **Auto leave** settings (Daily confirm and leave-from-results independently)

### Changed

- Auto leave uses `EventFramework.LeaveCurrentContent` (SelectYesno leave dialog removed)
- Debug status matches Auto enter / Auto leave / results screen

## [1.1.0.3] - 2026-06-01

### Changed

- `AutoFrontline.csproj` `<Version>` synced with `AssemblyVersion` (1.1.0.3)

## [1.1.0.2] - 2026-06-01

### Changed

- Icon path
- `/autofrontline` with no args toggles the settings window

## [1.1.0.1] - 2026-06-01

### Fixed

- Missing items in the v1.1.0.0 CHANGELOG

## [1.1.0.0] - 2026-06-01

### Added

- Auto-target the nearest enemy player (RSR auto-target is not used)
- Auto-enter Daily Challenge: Frontline matches only
- Settings UI split into General / Settings / Debug
- DTR bar shows plugin state; click toggles ON/OFF
- Mount picker

### Changed

- Seize territory ID 1273
- Group movement: with no enemy within 30m, move toward the 50m cluster center of allies (excluding self) with a **1–3m offset**. Fallback here if an enemy is within 30m but no ally is within 30m of that enemy
- Combat: with an enemy within 30m, move among **allies within 30m of that enemy** (excluding self), up to 10, between nearest and farthest (**Hostile mode position**, 0 = nearest, 1 = farthest, **default 0.5**). Tracked target is the nearest ally
- Movement refresh: **Group movement refresh** / **Hostile mode refresh** (0.5–3.0 seconds each)
- Mount when destination is **30m or more** (0–100), dismount when an enemy is **within 20m** (0–100)
- RSR **rotation** locked to Manual (`/rotation manual` when not already Manual)
- Skip moveto while casting. Skip moveto if the tracked target moved less than 0.1m (resend on the refresh interval)

## [1.0.0.0] - 2026-05-29

### Added

- Initial release
- Track the densest ally cluster (50m) on five Frontline fields
- vnavmesh / Rotation Solver Reborn integration with required-plugin checks
- Mount, movement, and auto-leave at match end
- Settings UI (General / Debug)

[Unreleased]: https://github.com/exatrines/AutoFrontline/compare/v1.6.0.0...HEAD
[1.6.0.0]: https://github.com/exatrines/AutoFrontline/compare/v1.5.0.0...v1.6.0.0
[1.5.0.0]: https://github.com/exatrines/AutoFrontline/compare/v1.4.1.0...v1.5.0.0
[1.4.1.0]: https://github.com/exatrines/AutoFrontline/compare/v1.4.0.0...v1.4.1.0
[1.4.0.0]: https://github.com/exatrines/AutoFrontline/compare/v1.3.1.1...v1.4.0.0
[1.3.1.1]: https://github.com/exatrines/AutoFrontline/compare/v1.3.1.0...v1.3.1.1
[1.3.1.0]: https://github.com/exatrines/AutoFrontline/compare/v1.3.0.9...v1.3.1.0
[1.3.0.9]: https://github.com/exatrines/AutoFrontline/compare/v1.3.0.8...v1.3.0.9
[1.3.0.8]: https://github.com/exatrines/AutoFrontline/compare/v1.3.0.7...v1.3.0.8
[1.3.0.7]: https://github.com/exatrines/AutoFrontline/compare/v1.3.0.6...v1.3.0.7
[1.3.0.6]: https://github.com/exatrines/AutoFrontline/compare/v1.3.0.5...v1.3.0.6
[1.3.0.5]: https://github.com/exatrines/AutoFrontline/compare/v1.3.0.4...v1.3.0.5
[1.3.0.4]: https://github.com/exatrines/AutoFrontline/compare/v1.3.0.3...v1.3.0.4
[1.3.0.3]: https://github.com/exatrines/AutoFrontline/compare/v1.3.0.2...v1.3.0.3
[1.3.0.2]: https://github.com/exatrines/AutoFrontline/compare/v1.3.0.1...v1.3.0.2
[1.3.0.1]: https://github.com/exatrines/AutoFrontline/compare/v1.3.0.0...v1.3.0.1
[1.3.0.0]: https://github.com/exatrines/AutoFrontline/compare/v1.2.0.2...v1.3.0.0
[1.2.0.2]: https://github.com/exatrines/AutoFrontline/compare/v1.2.0.1...v1.2.0.2
[1.2.0.1]: https://github.com/exatrines/AutoFrontline/compare/v1.2.0.0...v1.2.0.1
[1.2.0.0]: https://github.com/exatrines/AutoFrontline/compare/v1.1.0.4...v1.2.0.0
[1.1.0.4]: https://github.com/exatrines/AutoFrontline/compare/v1.1.0.3...v1.1.0.4
[1.1.0.3]: https://github.com/exatrines/AutoFrontline/compare/v1.1.0.2...v1.1.0.3
[1.1.0.2]: https://github.com/exatrines/AutoFrontline/compare/v1.1.0.1...v1.1.0.2
[1.1.0.1]: https://github.com/exatrines/AutoFrontline/compare/v1.1.0.0...v1.1.0.1
[1.1.0.0]: https://github.com/exatrines/AutoFrontline/compare/v1.0.0.0...v1.1.0.0
[1.0.0.0]: https://github.com/exatrines/AutoFrontline/releases/tag/v1.0.0.0
