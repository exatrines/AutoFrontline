# Auto FrontLine

FF14 Dalamud plugin that automates Frontline movement and combat.

## Required plugins (not bundled)

- [vnavmesh](https://github.com/awgil/ffxiv_navmesh) — `/vnav moveto <X> <Y> <Z>`
- [Rotation Solver Reborn](https://github.com/ArchiDog1998/RotationSolverReborn) — `/rotation auto`

## Build

```powershell
git submodule update --init --recursive
dotnet build AutoFrontLine.sln -c Release
```

## In-game

- `/autofrontline` or `/afl` — open settings
- **Main**: Enable automation
- **Debug**: Current field, tracked player, and move target
