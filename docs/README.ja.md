<p align="center">
  <img src="../AutoFrontline/Data/plugin-icon.png" alt="Auto Frontline アイコン" width="128" height="128">
</p>

<h1 align="center">Auto Frontline</h1>

<p align="center">
  <a href="../README.md">English</a> | 日本語
</p>

<p align="center">
  <a href="https://github.com/exatrines/AutoFrontline/releases/latest">
    <img src="https://img.shields.io/github/v/release/exatrines/AutoFrontline?label=Release&amp;labelColor=F280B6&amp;color=FFFFFF&amp;style=flat&amp;sort=date&amp;display_name=tag" alt="Release">
  </a>
  <a href="../CHANGELOG.md">
    <img src="https://img.shields.io/badge/Changelog-view-FFFFFF?labelColor=F280B6&amp;style=flat" alt="Changelog">
  </a>
  <a href="../LICENSE">
    <img src="https://img.shields.io/badge/License-AGPL--3.0--or--later-FFFFFF?labelColor=F280B6&amp;style=flat" alt="AGPL-3.0-or-later">
  </a>
</p>

Auto Frontline は、vnavmesh と Rotation Solver Reborn を使って、フロントラインの移動と戦闘を自動化する Dalamud プラグインです。

**Loop** では、デイリーフロントラインのキュー、入室、試合、退出を、設定した最大回数まで繰り返します。**Manual** では、コンテンツファインダーから自分で参加します。自動参加と自動退出は、有効にしていればそのまま使えます。追従の優先は、戦闘モード、軍師、集団行動の順です。

実験的な軍師追従、戦闘モード、オートリミットブレイクは、予告なく変更や削除されることがあります。

## インストール

1. `/xlsettings` を実行し、**試験的機能**タブを開く
2. **カスタムプラグインリポジトリ** に次の URL を追加する:

```
https://raw.githubusercontent.com/exatrines/DalamudPlugins/refs/heads/main/pluginmaster.json
```

3. `/xlplugins` を実行し、**Auto Frontline** をインストールする

## 機能

- **Loop / Manual** — デイリーフロントラインを自動でキューに入れるか、コンテンツファインダーから参加できます
- **集団行動** — 固まっている味方を追従します。止まっている対象や、連続して選ばれた対象は除外します
- **スポーン脱出** — スポーンの除外圏を出ます。一部のフィールドでは固定座標を使います
- **スタック回復** — 集団行動が止まっているときにデジョンを使います
- **マウント** — 敵が離れていれば騎乗し、近づいたら降車します
- **実験的: 軍師追従** — アライアンスチャットで直近に話した人を追従します
- **実験的: 戦闘モード** — いちばん近い敵の近くにいる味方と移動します
- **実験的: オートリミットブレイク** — 戦闘モード中、ジョブごとに `/pvpaction` を使います
- **UI 言語** — クライアントに合わせるか、英語 / 日本語に固定できます

必須プラグイン: [vnavmesh](https://github.com/awgil/ffxiv_navmesh)、[Rotation Solver Reborn](https://github.com/FFXIV-CombatReborn/RotationSolverReborn)。

## コマンド

| コマンド | 説明 |
| --- | --- |
| `/autofrontline` | 設定ウィンドウの表示切替 |
| `/autofrontline on` | Manual モードにする |
| `/autofrontline off` | Disable モードにする |
| `/autofrontline toggle` | Manual / Disable を切替（Loop 実行中は停止） |

## 開発者向け

1. `git submodule update --init --recursive`
2. ビルド: `dotnet build AutoFrontline.sln -c Release -p:Platform=x64`
3. Dalamud の **dev plugin** に `AutoFrontline/bin/Release/` を指定する
4. プラグインインストーラ（dev）で **Auto Frontline** を有効にする

共有 UI キットの [MirageUI](https://github.com/exatrines/MirageUI) を git サブモジュールとして同梱しています。[ECommons](https://github.com/NightmareXIV/ECommons) もサブモジュールです。

## コントリビューション

コントリビューションは大歓迎です！[貢献ガイド](../CONTRIBUTING.md)をご覧ください。
