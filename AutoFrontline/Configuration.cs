using System.Collections.Generic;

namespace AutoFrontline;

public sealed class Configuration
{
    private const int CurrentConfigVersion = 3;

    public int ConfigVersion = CurrentConfigVersion;

    /// <summary>v1 compatibility. Read only when ConfigVersion &lt; 2.</summary>
    public bool Enabled;

    public PluginMode Mode = PluginMode.Manual;

    public int AutoMaxCount = 1;

    /// <summary>0 = mount roulette; otherwise Mount RowId.</summary>
    public uint MountSelectionId;

    public float GroupMovementRefreshIntervalSeconds = 1f;
    public int GroupMoveSelfSearchRadiusMeters = FrontlineConstants.GroupMoveSelfSearchRadiusDefaultMeters;
    public int StationaryTargetExclusionSeconds = FrontlineConstants.StationaryTargetExclusionSecondsDefault;
    public int RepeatedFollowTargetExcludePickCount = FrontlineConstants.RepeatedFollowTargetExcludePickCountDefault;
    public float HostileModeRefreshIntervalSeconds = 1f;
    public float HostileModePositionRatio = 0.5f;
    public int DismountEnemyDistanceMeters = 20;
    public int SpawnExclusionRadiusMeters = 30;
    public bool AutoEnterEnabled = true;
    public bool AutoLeaveEnabled = true;
    public Dictionary<string, bool> AutoLimitBreakByEntryId = new();
    public bool CommanderFollowEnabled;
    public bool HostileModeEnabled;
    public float DejonStallSeconds = FrontlineConstants.DejonStallSecondsDefault;

    /// <summary>client / en / ja. Default follows Dalamud UI language.</summary>
    public string UiLanguage = I18n.FollowClient;

    public void MigrateIfNeeded()
    {
        if (ConfigVersion >= CurrentConfigVersion)
        {
            ClampDejonStallSeconds();
            ClampSpawnExclusionRadius();
            ClampGroupMoveSelfSearchRadius();
            ClampStationaryTargetExclusionSeconds();
            ClampRepeatedFollowTargetExcludePickCount();
            return;
        }

        if (ConfigVersion < 2)
            Mode = Enabled ? PluginMode.Manual : PluginMode.Disable;

        ConfigVersion = CurrentConfigVersion;
        ClampDejonStallSeconds();
        ClampSpawnExclusionRadius();
        ClampGroupMoveSelfSearchRadius();
        ClampStationaryTargetExclusionSeconds();
        ClampRepeatedFollowTargetExcludePickCount();
        EzConfig.Save();
    }

    private void ClampDejonStallSeconds()
    {
        if (DejonStallSeconds < FrontlineConstants.DejonStallSecondsMin)
            DejonStallSeconds = FrontlineConstants.DejonStallSecondsDefault;

        DejonStallSeconds = Math.Clamp(
            DejonStallSeconds,
            FrontlineConstants.DejonStallSecondsMin,
            FrontlineConstants.DejonStallSecondsMax);
    }

    private void ClampSpawnExclusionRadius()
    {
        SpawnExclusionRadiusMeters = Math.Clamp(SpawnExclusionRadiusMeters, 0, 100);
    }

    private void ClampGroupMoveSelfSearchRadius()
    {
        GroupMoveSelfSearchRadiusMeters = Math.Clamp(
            GroupMoveSelfSearchRadiusMeters,
            FrontlineConstants.GroupMoveSelfSearchRadiusMinMeters,
            FrontlineConstants.GroupMoveSelfSearchRadiusMaxMeters);
    }

    private void ClampStationaryTargetExclusionSeconds()
    {
        StationaryTargetExclusionSeconds = Math.Clamp(
            StationaryTargetExclusionSeconds,
            FrontlineConstants.StationaryTargetExclusionSecondsMin,
            FrontlineConstants.StationaryTargetExclusionSecondsMax);
    }

    private void ClampRepeatedFollowTargetExcludePickCount()
    {
        RepeatedFollowTargetExcludePickCount = Math.Clamp(
            RepeatedFollowTargetExcludePickCount,
            FrontlineConstants.RepeatedFollowTargetExcludePickCountMin,
            FrontlineConstants.RepeatedFollowTargetExcludePickCountMax);
    }

    // Legacy config keys
    public float UpdateIntervalSeconds
    {
        get => GroupMovementRefreshIntervalSeconds;
        set => GroupMovementRefreshIntervalSeconds = value;
    }

    public float FollowIntervalSeconds
    {
        get => GroupMovementRefreshIntervalSeconds;
        set => GroupMovementRefreshIntervalSeconds = value;
    }

    public float PlayerReselectIntervalSeconds
    {
        get => GroupMovementRefreshIntervalSeconds;
        set => GroupMovementRefreshIntervalSeconds = value;
    }

    public float RecalculateIntervalSeconds
    {
        get => GroupMovementRefreshIntervalSeconds;
        set => GroupMovementRefreshIntervalSeconds = value;
    }

    public float NaviRebuildIntervalSeconds
    {
        get => GroupMovementRefreshIntervalSeconds;
        set => GroupMovementRefreshIntervalSeconds = value;
    }
}
