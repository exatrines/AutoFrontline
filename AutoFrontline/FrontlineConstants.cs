namespace AutoFrontline;

/// <summary>フロントライン自動化で使う固定値とスロットルキー。</summary>
internal static class FrontlineConstants
{
    /// <summary>集団移動モードで「最も密集している味方」を数える半径（m）。</summary>
    public const float DensityRadiusMeters = 50f;

    /// <summary>敵対モードで最寄り敵・付近味方を探索する半径（m）。</summary>
    public const float EnemyProximityFollowRadiusMeters = 30f;

    /// <summary>敵対モードで敵付近から取得する味方の最大人数（先頭〜末尾間でナビ位置を補間）。</summary>
    public const int EnemyProximityFrontlineAllyCount = 10;

    /// <summary>集団移動の moveto 先に付与するランダムオフセットの最小距離（m）。</summary>
    public const float MoveOffsetMinMeters = 1f;

    /// <summary>集団移動の moveto 先に付与するランダムオフセットの最大距離（m）。</summary>
    public const float MoveOffsetMaxMeters = 3f;

    /// <summary>追従対象の座標が変わっていないとみなす距離（m）。この範囲内なら moveto 再発行を抑制。</summary>
    public const float PositionUnchangedThresholdMeters = 0.1f;

    /// <summary>指揮官追従モードで「到着済み」とみなし追従を止める距離（m）。</summary>
    public const float CommanderFollowArrivalDistanceMeters = 5f;

    /// <summary>入室地点からこの半径（m）以内への moveto をブロック（スポーン付近への誤移動防止）。</summary>
    public const float SpawnExclusionRadiusMeters = 25f;

    /// <summary>初回脱出地点を記録する半径の余白（m）。記録半径 = SpawnExclusionRadius + この値。</summary>
    public const float InitialMovementRecordRadiusOffsetMeters = 20f;

    /// <summary>初回脱出地点を記録する水平距離（m）。SpawnExclusionRadius + InitialMovementRecordRadiusOffset。</summary>
    public const float InitialMovementRecordRadiusMeters =
        SpawnExclusionRadiusMeters + InitialMovementRecordRadiusOffsetMeters;

    /// <summary>マウント試行コマンドの最小間隔（ms）。</summary>
    public const int MountThrottleMs = 1500;

    /// <summary>降下試行コマンドの最小間隔（ms）。</summary>
    public const int DismountThrottleMs = 1500;

    /// <summary>マウントルーレットの GeneralAction ID（ゲームデータ取得失敗時のフォールバック）。</summary>
    public const uint MountRouletteGeneralActionId = 9;

    /// <summary>フロントラインのアイスドトームリス（付近でマウント降下）。ModelChara / DataId。</summary>
    public const uint IcedotomeIrisModelCharaId = 0x1E0;

    /// <summary>コンテンツファインダー参加確認ダイアログ操作の最小間隔（ms）。</summary>
    public const int ContentsFinderConfirmThrottleMs = 500;

    /// <summary>コンテンツファインダーキュー操作の最小間隔（ms）。</summary>
    public const int ContentsFinderQueueThrottleMs = 250;

    /// <summary>自動参加人数（Auto Max Count）の下限。</summary>
    public const int AutoMaxCountMin = 1;

    /// <summary>自動参加人数（Auto Max Count）の上限。</summary>
    public const int AutoMaxCountMax = 99;

    /// <summary>Rotation Solver を Manual に戻す /rotation manual の最小間隔（ms）。</summary>
    public const int RotationManualIntervalMs = 2000;

    /// <summary>PvP リミットブレイク /pvpaction の最小間隔（ms）。</summary>
    public const int PvpLimitBreakIntervalMs = 5000;

    /// <summary>設定可能な更新間隔の下限（ms）。</summary>
    public const int ConfigIntervalMinMs = 100;

    /// <summary>集団移動・敵対モードの更新間隔を秒から ms に変換するときの下限（ms）。</summary>
    public const int ModeRefreshMinMs = 500;

    /// <summary>EzThrottler キー: moveto 発行。</summary>
    public const string ThrottleMove = "AflMove";

    /// <summary>EzThrottler キー: コンテンツファインダー参加確認。</summary>
    public const string ThrottleContentsFinderConfirm = "AflContentsFinderConfirm";

    /// <summary>EzThrottler キー: コンテンツファインダーキュー操作。</summary>
    public const string ThrottleContentsFinderQueue = "AflContentsFinderQueue";

    /// <summary>EzThrottler キー: コンテンツファインダー画面を開く操作。</summary>
    public const string ThrottleContentsFinderOpen = "AflContentsFinderOpen";

    /// <summary>EzThrottler キー: Rotation Solver Manual 強制。</summary>
    public const string ThrottleRotationManual = "AflRotationManual";

    /// <summary>EzThrottler キー: PvP リミットブレイク。</summary>
    public const string ThrottlePvpLimitBreak = "AflPvpLimitBreak";

    /// <summary>EzThrottler キー: 敵プレイヤーターゲット切替。</summary>
    public const string ThrottleEnemyTarget = "AflEnemyTarget";

    /// <summary>EzThrottler キー: マウント。</summary>
    public const string ThrottleMount = "AflMount";

    /// <summary>EzThrottler キー: 降下。</summary>
    public const string ThrottleDismount = "AflDismount";
}
