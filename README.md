<p align="center">
  <img src="AutoFrontline/Data/plugin-icon.png" alt="Auto Frontline icon" width="128" height="128">
</p>

<h1 align="center">Auto Frontline</h1>

<p align="center">
  English | <a href="docs/README.ja.md">日本語</a>
</p>

<p align="center">
  <a href="https://github.com/exatrines/AutoFrontline/releases/latest">
    <img src="https://img.shields.io/github/v/release/exatrines/AutoFrontline?label=Release&amp;labelColor=F280B6&amp;color=FFFFFF&amp;style=flat&amp;sort=date&amp;display_name=tag" alt="Release">
  </a>
  <a href="CHANGELOG.md">
    <img src="https://img.shields.io/badge/Changelog-view-FFFFFF?labelColor=F280B6&amp;style=flat" alt="Changelog">
  </a>
  <a href="LICENSE">
    <img src="https://img.shields.io/badge/License-AGPL--3.0--or--later-FFFFFF?labelColor=F280B6&amp;style=flat" alt="AGPL-3.0-or-later">
  </a>
</p>

Auto Frontline is a Dalamud plugin that automates Frontline movement and combat with vnavmesh and Rotation Solver Reborn.

In **Loop** it queues Daily Frontline, enters, fights, and leaves up to a max count. In **Manual** you join from Contents Finder; auto-enter and auto-leave still apply when enabled. Follow prefers combat mode, then commander, then group movement.

Experimental commander follow, combat mode, and auto limit break can change or be removed without notice.

## Install

1. Run `/xlsettings` and open the **Experimental** tab
2. Add this URL under **Custom Plugin Repositories**:

```
https://raw.githubusercontent.com/exatrines/DalamudPlugins/refs/heads/main/pluginmaster.json
```

3. Run `/xlplugins` and install **Auto Frontline**

## Features

- **Loop / Manual** — queue Daily Frontline automatically, or join from Contents Finder
- **Group movement** — follow clustered allies; skip stationary and repeatedly picked targets
- **Spawn exit** — leave the spawn exclusion zone; some fields use a fixed exit
- **Stuck recovery** — use Return when group movement stalls
- **Mount** — mount outside enemy range, dismount when hostiles are close
- **Experimental: commander follow** — follow the latest alliance chat speaker
- **Experimental: combat mode** — move with allies near the closest enemy
- **Experimental: auto limit break** — `/pvpaction` in combat mode, per job
- **UI language** — follow the client, or lock English / Japanese

Required plugins: [vnavmesh](https://github.com/awgil/ffxiv_navmesh), [Rotation Solver Reborn](https://github.com/FFXIV-CombatReborn/RotationSolverReborn).

## Commands

| Command | Description |
| --- | --- |
| `/autofrontline` | Toggle settings window |
| `/autofrontline on` | Set Manual mode |
| `/autofrontline off` | Set Disable mode |
| `/autofrontline toggle` | Toggle Manual / Disable (stops Loop if running) |

## For developers

1. `git submodule update --init --recursive`
2. Build: `dotnet build AutoFrontline.sln -c Release -p:Platform=x64`
3. Point Dalamud’s **dev plugin** path at `AutoFrontline/bin/Release/`
4. Enable **Auto Frontline** in the plugin installer (dev)

[MirageUI](https://github.com/exatrines/MirageUI) is included as a git submodule for the shared UI kit. [ECommons](https://github.com/NightmareXIV/ECommons) is also a submodule.

## Contributing

Contributions are always welcome! Please see the [contribution guide](CONTRIBUTING.md).
