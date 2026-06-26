namespace AutoFrontline;

/// <summary>フロントライン自動化で使う固定値とスロットルキー。</summary>
internal static class FrontlineConstants
{
    // ── Follow ──────────────────────────────────────────────────────────────

    /// <summary>集団行動: 自分から味方を探す半径スライダーの下限（m）。</summary>
    public const int GroupMoveSelfSearchRadiusMinMeters = 25;

    /// <summary>集団行動: 自分から味方を探す半径スライダーの上限（m）。</summary>
    public const int GroupMoveSelfSearchRadiusMaxMeters = 100;

    /// <summary>集団行動: 自分から味方を探す半径の既定値（m）。</summary>
    public const int GroupMoveSelfSearchRadiusDefaultMeters = 75;

    /// <summary>集団行動: 2 名以上候補時の密集度カウント半径（m）。</summary>
    public const float GroupMoveDensityRadiusMeters = 30f;

    /// <summary>敵対モードで最寄り敵・付近味方を探索する半径（m）。</summary>
    public const float EnemyProximityFollowRadiusMeters = 30f;

    /// <summary>集団移動の moveto 先に付与するランダムオフセットの最小距離（m）。</summary>
    public const float MoveOffsetMinMeters = 1f;

    /// <summary>集団移動の moveto 先に付与するランダムオフセットの最大距離（m）。</summary>
    public const float MoveOffsetMaxMeters = 3f;

    /// <summary>追従対象が静止しているとみなす距離（m）。再選定時の除外判定に使用。</summary>
    public const float PositionUnchangedThresholdMeters = 0.1f;

    /// <summary>静止追従対象を集団選定から除外する時間スライダーの下限（秒）。0 = 無効。</summary>
    public const int StationaryTargetExclusionSecondsMin = 0;

    /// <summary>静止追従対象を集団選定から除外する時間スライダーの上限（秒）。</summary>
    public const int StationaryTargetExclusionSecondsMax = 20;

    /// <summary>静止追従対象を集団選定から除外する時間の既定値（秒）。</summary>
    public const int StationaryTargetExclusionSecondsDefault = 10;

    /// <summary>同じ集団行動対象を連続で選んだ回数がこの値に達したら除外リストへ追加する（スライダー下限）。</summary>
    public const int RepeatedFollowTargetExcludePickCountMin = 1;

    /// <summary>同じ集団行動対象を連続で選んだ回数がこの値に達したら除外リストへ追加する（スライダー上限）。</summary>
    public const int RepeatedFollowTargetExcludePickCountMax = 20;

    /// <summary>同じ集団行動対象を連続で選んだ回数がこの値に達したら除外リストへ追加する（既定値）。</summary>
    public const int RepeatedFollowTargetExcludePickCountDefault = 15;

    /// <summary>追従対象が連続で静止判定された回数がこの値に達したら除外リストへ追加する。</summary>
    public const int StationaryTargetExcludePickCount = 5;

    /// <summary>軍師追従モードで「到着済み」とみなし追従を止める距離（m）。</summary>
    public const float CommanderFollowArrivalDistanceMeters = 15f;

    /// <summary>EzThrottler キー: moveto 発行。</summary>
    public const string ThrottleMove = "AflMove";

    // ── Movement ────────────────────────────────────────────────────────────

    /// <summary>マウント試行コマンドの最小間隔（ms）。</summary>
    public const int MountThrottleMs = 1500;

    /// <summary>降下試行コマンドの最小間隔（ms）。</summary>
    public const int DismountThrottleMs = 1500;

    /// <summary>マウントルーレットの GeneralAction ID（ゲームデータ取得失敗時のフォールバック）。</summary>
    public const uint MountRouletteGeneralActionId = 9;

    /// <summary>moveto 停滞時の Return 確認（SelectYesno）操作の最小間隔（ms）。</summary>
    public const int NaviStuckDejonConfirmThrottleMs = 500;

    /// <summary>moveto 停滞判定で「座標が変わった」とみなす距離（m）。</summary>
    public const float NaviStuckDejonPositionThresholdMeters = 1f;

    /// <summary>Return 停滞時間スライダーの下限（秒）。</summary>
    public const float DejonStallSecondsMin = 10f;

    /// <summary>Return 停滞時間スライダーの上限（秒）。</summary>
    public const float DejonStallSecondsMax = 30f;

    /// <summary>Return 停滞時間の既定値（秒）。</summary>
    public const float DejonStallSecondsDefault = 15f;

    /// <summary>スタック判定を行う移動先までの最小距離（m）。これ未満は到着扱い。</summary>
    public const float NaviStuckDejonMinDestinationDistanceMeters = 5f;

    /// <summary>EzThrottler キー: マウント。</summary>
    public const string ThrottleMount = "AflMount";

    /// <summary>EzThrottler キー: 降下。</summary>
    public const string ThrottleDismount = "AflDismount";

    /// <summary>EzThrottler キー: Return 確認ダイアログ。</summary>
    public const string ThrottleNaviStuckDejonConfirm = "AflNaviStuckDejonConfirm";

    // ── Combat ──────────────────────────────────────────────────────────────

    /// <summary>フロントラインのアイスドトームリス（砕氷戦のみ。付近でマウント降下・攻撃対象）。ModelChara / DataId。</summary>
    public const uint IcedotomeIrisModelCharaId = 0x1E0;

    /// <summary>フロントラインの遊撃ドローン（制圧戦のみ。付近でマウント降下・攻撃対象）。ModelChara / DataId。</summary>
    public const uint AssaultDroneModelCharaId = 0xC19;

    /// <summary>フロントラインの遊撃システム（付近でマウント降下・攻撃対象）。ModelChara / DataId。</summary>
    public const uint AssaultSystemModelCharaId = 0x233C;

    /// <summary>EzThrottler キー: 敵プレイヤーターゲット切替。</summary>
    public const string ThrottleEnemyTarget = "AflEnemyTarget";

    // ── Duty ──────────────────────────────────────────────────────────────────

    /// <summary>コンテンツファインダー参加確認ダイアログ操作の最小間隔（ms）。</summary>
    public const int ContentsFinderConfirmThrottleMs = 500;

    /// <summary>コンテンツファインダーキュー操作の最小間隔（ms）。</summary>
    public const int ContentsFinderQueueThrottleMs = 250;

    /// <summary>自動参加人数（Auto Max Count）の下限。</summary>
    public const int AutoMaxCountMin = 1;

    /// <summary>自動参加人数（Auto Max Count）の上限。</summary>
    public const int AutoMaxCountMax = 99;

    /// <summary>EzThrottler キー: コンテンツファインダー参加確認。</summary>
    public const string ThrottleContentsFinderConfirm = "AflContentsFinderConfirm";

    /// <summary>EzThrottler キー: コンテンツファインダーキュー操作。</summary>
    public const string ThrottleContentsFinderQueue = "AflContentsFinderQueue";

    /// <summary>EzThrottler キー: コンテンツファインダー画面を開く操作。</summary>
    public const string ThrottleContentsFinderOpen = "AflContentsFinderOpen";

    // ── Rotation ────────────────────────────────────────────────────────────

    /// <summary>Rotation Solver を Manual に戻す /rotation manual の最小間隔（ms）。</summary>
    public const int RotationManualIntervalMs = 2000;

    /// <summary>EzThrottler キー: Rotation Solver Manual 強制。</summary>
    public const string ThrottleRotationManual = "AflRotationManual";

    // ── LimitBreak ────────────────────────────────────────────────────────────

    /// <summary>PvP リミットブレイク /pvpaction の最小間隔（ms）。</summary>
    public const int PvpLimitBreakIntervalMs = 5000;

    /// <summary>EzThrottler キー: PvP リミットブレイク。</summary>
    public const string ThrottlePvpLimitBreak = "AflPvpLimitBreak";

    // ── Process ───────────────────────────────────────────────────────────────

    /// <summary>設定可能な更新間隔の下限（ms）。</summary>
    public const int ConfigIntervalMinMs = 100;

    /// <summary>集団移動・敵対モードの更新間隔を秒から ms に変換するときの下限（ms）。</summary>
    public const int ModeRefreshMinMs = 500;
}
