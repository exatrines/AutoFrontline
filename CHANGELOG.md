# Changelog

## v1.2.0.2 - 2026-06-02 (テスト版)

- フロントライン入室時の自キャラ座標を記録し、半径 **15m** 以内を移動先とする `moveto` を出さない（スポーン除外ゾーン）
- 入室直後に `Player` が利用可能になる前のフレームでも Entry position を記録するよう修正
- Debug の Movement に入室座標・除外状態を表示

## v1.2.0.1 - 2026-06-02

- **Mode** の **Auto** を **Loop** に名称変更（enum 値は互換のため 2 のまま）
- General タブのモード説明を更新

## v1.2.0.0 - 2026-06-02

- **Enable** を廃止し、**Mode** コンボ（Disable / Manual / Loop）を追加
- **Loop** モード: Start/Stop、MaxCount、入室カウント表示。Start 中は Mode 固定
- Loop: コンテンツルーレットのデイリーチャレンジ・フロントラインへ参加申請 → 自動参加 → 試合終了後自動退出を MaxCount 回繰り返し（入室時にカウント +1）
- コンテンツファインダー: リスト行 Text #6 と callback `3`（Leaf インデックス）でルーレット選択
- マウント降下: 近傍の敵に加え、ModelCharaId `0x1E0`（アイスドトームリス）が **Dismount distance** 内にあるときも降下
- 旧設定 `Enabled` は ConfigVersion 2 へ自動移行

## v1.1.0.4 - 2026-06-02

- 設定に **Auto enter** / **Auto leave** を追加（デイリー参加確認・試合終了画面からの退出を個別に ON/OFF）
- 自動退出を `EventFramework.LeaveCurrentContent` に変更（SelectYesno の退出確認ダイアログ操作を廃止）
- Debug のステータス表示を Auto enter / Auto leave / 結果画面に合わせて更新

## v1.1.0.3 - 2026-06-01

- `AutoFrontline.csproj` の `<Version>` を `AssemblyVersion` と同期（1.1.0.3）

## v1.1.0.2 - 2026-06-01

- アイコンパスを変更
- `/autofrontline` 実行時にインターフェースの表示非表示をトグルで切り替えるように修正

## v1.1.0.1 - 2026-06-01

- v1.1.0.0 CHANGELOG の記載漏れを修正

## v1.1.0.0 - 2026-06-01

- 制圧戦のテリトリー ID を 1273 に変更
- 集団行動ルール: 自分から 30m 以内に敵がいないとき、味方（自分除く）の 50m 密集中心へ **1〜3m オフセット**で移動。敵 30m 内だが敵 30m 内に味方がいなければこちらへフォールバック
- 戦闘時ルール: 自分から 30m 以内に敵がいるとき、**その敵から 30m 以内の味方（自分除く）**から最大 10 名を対象に、先頭〜最遠の間（**Hostile mode position**、0=先頭・1=最遠、**既定 0.5**）へ移動。追跡対象は先頭味方
- 移動更新間隔: **Group movement refresh** / **Hostile mode refresh**（各 0.5〜3.0 秒）
- マウント: 移動先まで **30m 以上**（設定可、0〜100）で乗馬、敵 **20m 以内**（設定可、0〜100）で降馬。マウントを選択できるように。
- 最寄り敵プレイヤーを自動ターゲット（RSR の自動ターゲットは使用しない）
- RSR の**ローテーション**を Manual 固定（Manual 以外のとき `/rotation manual`）
- デイリーチャレンジ：フロントラインのマッチングのみ自動参加
- 設定 UI を General / Settings / Debug に分割
- 詠唱中は moveto しない。追跡対象が 0.1m 未満しか動いていなければ moveto をスキップ（更新間隔で再送）
- DTRバーにプラグインの状態を表示　クリックでON/OFF切り替えも可

## v1.0.0.0 — 2026-05-29

初回リリース。

- フロントライン5フィールドで味方密集地点（50m）を追跡
- vnavmesh / Rotation Solver Reborn 連携（必須プラグイン検証付き）
- マウント・移動・試合終了時の自動退出
- 設定 UI（General / Debug）
