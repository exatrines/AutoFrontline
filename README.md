# Auto FrontLine

FF14 Dalamud plugin that automates Frontline movement and combat.

## Required plugins (not bundled)

- [vnavmesh](https://github.com/awgil/ffxiv_navmesh) — `/vnav moveto <X> <Y> <Z>`
- [Rotation Solver Reborn](https://github.com/FFXIV-CombatReborn/RotationSolverReborn) — `/rotation Off` / `/rotation Auto`

## Install (custom repo)

Add this URL in Dalamud → Settings → Experimental → Custom Plugin Repositories:

```
https://raw.githubusercontent.com/exatrines/AutoFrontLine/main/pluginmaster.json
```

Then install **Auto FrontLine** from Available Plugins.

## Build

```powershell
git submodule update --init --recursive
dotnet build AutoFrontLine.sln -c Release
```

## In-game

- `/autofrontline` or `/afl` — open settings
- **General**: required plugins, Enable, intervals
- **Debug**: field, tracked player, movement state

## Release (maintainers)

1. Bump version (four-part `AssemblyVersion`, tag `v` + version):

   ```bash
   bash .github/scripts/bump-version.sh 1.0.0.0
   ```

2. Commit, tag, and push（`main` だけでは Release は走りません。タグ必須）:

   ```bash
   git add pluginmaster.json AutoFrontLine/AutoFrontLine.json AutoFrontLine/AutoFrontLine.csproj CHANGELOG.md
   git commit -m "Release 1.0.0.0"
   git tag v1.0.0.0
   git push origin main --follow-tags
   ```

3. GitHub Actions **Release** workflow builds `AutoFrontLine.zip` and publishes it to [Releases](https://github.com/exatrines/AutoFrontLine/releases).

   `v*` タグの push で自動実行。Actions タブから手動実行する場合は tag に `v1.0.0.0` を指定。

`pluginmaster.json` の `AssemblyVersion` とタグ（例: `v1.0.0.0`）は一致している必要があります。

## License

AGPL-3.0-or-later
