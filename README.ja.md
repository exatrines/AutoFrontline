# Auto Frontline

[English](README.md)

<p align="center">
  <img src="AutoFrontline/Data/plugin-icon.png" width="128" alt="Auto Frontline">
</p>

Auto Frontline は、vnavmesh と Rotation Solver Reborn でフロントラインの移動と戦闘を自動化する Dalamud プラグインです。

**Loop** ではデイリーフロントラインのキュー・入室・試合・退出を最大回数まで繰り返します。**Manual** ではコンテンツファインダーから自分で参加し、有効なら自動参加・自動退出は使えます。追従の優先度は戦闘モード、軍師、集団行動の順です。

実験的な軍師追従・戦闘モード・オートリミットブレイクは、予告なく変更または削除されることがあります。

## インストール

1. `/xlsettings` を実行し、**試験的機能** タブを開く
2. **カスタムプラグインリポジトリ** に次の URL を追加する:

```
https://raw.githubusercontent.com/exatrines/DalamudPlugins/refs/heads/main/pluginmaster.json
```

3. `/xlplugins` を実行し、**Auto Frontline** をインストールする

## 機能

- **Loop / Manual** — デイリーフロントラインを自動キュー、またはコンテンツファインダーから参加
- **集団行動** — 固まっている味方を追従。静止・連続選定の対象は除外
- **スポーン脱出** — スポーン除外圏を出る。一部フィールドは固定座標
- **スタック回復** — 集団行動が停滞したらデジョン
- **マウント** — 敵が離れていれば騎乗、近づいたら降車
- **実験的: 軍師追従** — アライアンスチャットの直近発言者を追従
- **実験的: 戦闘モード** — 最寄り敵付近の味方と移動
- **実験的: オートリミットブレイク** — 戦闘モード中にジョブ別 `/pvpaction`
- **UI 言語** — クライアントに合わせる、または英語 / 日本語を固定

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
3. Dalamud の **dev plugin** パスを `AutoFrontline/bin/Release/` にする
4. プラグインインストーラ（dev）で **Auto Frontline** を有効化する

共有 UI キットの [MirageUI](https://github.com/exatrines/MirageUI) と [ECommons](https://github.com/NightmareXIV/ECommons) は git サブモジュールです。

## ライセンス

[AGPL-3.0-or-later](https://www.gnu.org/licenses/agpl-3.0.html)
