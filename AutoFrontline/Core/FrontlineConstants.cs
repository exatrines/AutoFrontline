namespace AutoFrontline;

internal static class FrontlineConstants
{
    // Follow
    public const int GroupMoveSelfSearchRadiusMinMeters = 25;
    public const int GroupMoveSelfSearchRadiusMaxMeters = 100;
    public const int GroupMoveSelfSearchRadiusDefaultMeters = 75;
    public const float GroupMoveDensityRadiusMeters = 30f;
    public const float EnemyProximityFollowRadiusMeters = 30f;
    public const float MoveOffsetMinMeters = 1f;
    public const float MoveOffsetMaxMeters = 3f;
    public const float PositionUnchangedThresholdMeters = 0.1f;
    public const int StationaryTargetExclusionSecondsMin = 0;
    public const int StationaryTargetExclusionSecondsMax = 20;
    public const int StationaryTargetExclusionSecondsDefault = 10;
    public const int RepeatedFollowTargetExcludePickCountMin = 1;
    public const int RepeatedFollowTargetExcludePickCountMax = 20;
    public const int RepeatedFollowTargetExcludePickCountDefault = 15;
    public const int StationaryTargetExcludePickCount = 5;
    public const float CommanderFollowArrivalDistanceMeters = 15f;
    public const string ThrottleMove = "AflMove";

    // Movement
    public const int MountThrottleMs = 1500;
    public const int DismountThrottleMs = 1500;
    public const uint MountRouletteGeneralActionId = 9;
    public const int NaviStuckDejonConfirmThrottleMs = 500;
    public const float NaviStuckDejonPositionThresholdMeters = 1f;
    public const float DejonStallSecondsMin = 10f;
    public const float DejonStallSecondsMax = 30f;
    public const float DejonStallSecondsDefault = 15f;
    public const float NaviStuckDejonMinDestinationDistanceMeters = 5f;
    public const string ThrottleMount = "AflMount";
    public const string ThrottleDismount = "AflDismount";
    public const string ThrottleNaviStuckDejonConfirm = "AflNaviStuckDejonConfirm";

    // Combat (ModelChara / DataId)
    public const uint IcedotomeIrisModelCharaId = 0x1E0;
    public const uint AssaultDroneModelCharaId = 0xC19;
    public const uint AssaultSystemModelCharaId = 0x233C;
    public const string ThrottleEnemyTarget = "AflEnemyTarget";

    // Duty
    public const int ContentsFinderConfirmThrottleMs = 500;
    public const int ContentsFinderQueueThrottleMs = 250;
    public const int AutoMaxCountMin = 1;
    public const int AutoMaxCountMax = 99;
    public const string ThrottleContentsFinderConfirm = "AflContentsFinderConfirm";
    public const string ThrottleContentsFinderQueue = "AflContentsFinderQueue";
    public const string ThrottleContentsFinderOpen = "AflContentsFinderOpen";

    // Rotation
    public const int RotationManualIntervalMs = 2000;
    public const string ThrottleRotationManual = "AflRotationManual";

    // Limit break
    public const int PvpLimitBreakIntervalMs = 5000;
    public const string ThrottlePvpLimitBreak = "AflPvpLimitBreak";

    // Process
    public const int ConfigIntervalMinMs = 100;
    public const int ModeRefreshMinMs = 500;
}
