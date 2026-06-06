namespace AutoFrontline;

/// <summary>フロントライン自動化で使う固定値とスロットルキー。</summary>
internal static class FrontlineConstants
{
    public const float DensityRadiusMeters = 50f;
    public const float EnemyProximityFollowRadiusMeters = 30f;
    public const int EnemyProximityFrontlineAllyCount = 10;
    public const float MoveOffsetMinMeters = 1f;
    public const float MoveOffsetMaxMeters = 3f;
    public const float PositionUnchangedThresholdMeters = 0.1f;
    public const float SpawnExclusionRadiusMeters = 15f;

    public const int MountThrottleMs = 1500;
    public const int DismountThrottleMs = 1500;

    public const uint MountRouletteGeneralActionId = 9;

    /// <summary>フロントラインのアイスドトームリス（付近でマウント降下）。</summary>
    public const uint IcedotomeIrisModelCharaId = 0x1E0;
    public const int ContentsFinderConfirmThrottleMs = 500;
    public const int ContentsFinderQueueThrottleMs = 250;

    public const int AutoMaxCountMin = 1;
    public const int AutoMaxCountMax = 99;
    public const int RotationManualIntervalMs = 2000;

    public const int ConfigIntervalMinMs = 100;
    public const int ModeRefreshMinMs = 500;

    public const string ThrottleMove = "AflMove";
    public const string ThrottleContentsFinderConfirm = "AflContentsFinderConfirm";
    public const string ThrottleContentsFinderQueue = "AflContentsFinderQueue";
    public const string ThrottleContentsFinderOpen = "AflContentsFinderOpen";
    public const string ThrottleRotationManual = "AflRotationManual";
    public const string ThrottleEnemyTarget = "AflEnemyTarget";
    public const string ThrottleMount = "AflMount";
    public const string ThrottleDismount = "AflDismount";
}
