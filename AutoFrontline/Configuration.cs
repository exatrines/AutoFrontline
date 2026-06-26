using System.Collections.Generic;

namespace AutoFrontline;

public sealed class Configuration
{
    private const int CurrentConfigVersion = 3;

    public int ConfigVersion = CurrentConfigVersion;

    /// <summary>v1 互換用（ConfigVersion &lt; 2 の移行時のみ参照）。</summary>
    public bool Enabled;

    public PluginMode Mode = PluginMode.Manual;

    public int AutoMaxCount = 1;

    /// <summary>0 = マウントルーレット。それ以外は <see cref="Mount"/> の RowId。</summary>
    public uint MountSelectionId;

    /// <summary>追従 moveto の再判定・再発行間隔（秒）。</summary>
    public float GroupMovementRefreshIntervalSeconds = 1f;

    /// <summary>集団行動: 自分からこの半径（m）以内の味方を候補とする。</summary>
    public int GroupMoveSelfSearchRadiusMeters = FrontlineConstants.GroupMoveSelfSearchRadiusDefaultMeters;

    /// <summary>静止していた追従対象を集団選定から除外する時間（秒）。0 = 無効。</summary>
    public int StationaryTargetExclusionSeconds = FrontlineConstants.StationaryTargetExclusionSecondsDefault;

    /// <summary>同じ集団行動対象を連続で選んだ回数がこの値に達したら除外リストへ追加する。</summary>
    public int RepeatedFollowTargetExcludePickCount = FrontlineConstants.RepeatedFollowTargetExcludePickCountDefault;

    /// <summary>Experimental 戦闘モード: 再判定・moveto 間隔（秒）。</summary>
    public float HostileModeRefreshIntervalSeconds = 1f;

    /// <summary>Experimental 戦闘モード: 先頭（敵に最も近い）〜最遠味方間の立ち位置（0=先頭、1=最遠、既定0.5=中央）。</summary>
    public float HostileModePositionRatio = 0.5f;

    /// <summary>この半径内に敵プレイヤーまたは特殊戦闘オブジェクトがいればマウントを降ろす（m）。</summary>
    public int DismountEnemyDistanceMeters = 20;

    /// <summary>入室地点からこの半径（m）以内への moveto をブロック（スポーン付近への誤移動防止）。</summary>
    public int SpawnExclusionRadiusMeters = 30;

    /// <summary>ContentsFinderConfirm（デイリーフロントライン）表示時に自動で参加する。</summary>
    public bool AutoEnterEnabled = true;

    /// <summary>FrontlineRecord 表示時に自動でコンテンツ退出する。</summary>
    public bool AutoLeaveEnabled = true;

    /// <summary>ジョブ別 Auto Limit Break（キー: PvpLimitBreakCatalog の Entry Id）。</summary>
    public Dictionary<string, bool> AutoLimitBreakByEntryId = new();

    /// <summary>Experimental: アライアンスチャット発言者を軍師として追従する。</summary>
    public bool CommanderFollowEnabled;

    /// <summary>Experimental: 敵対モード（最寄り敵付近の味方列へ移動）を有効にする。</summary>
    public bool HostileModeEnabled;

    /// <summary>集団行動スタック時の Return（デジョン）発動までの停滞時間（秒）。</summary>
    public float DejonStallSeconds = FrontlineConstants.DejonStallSecondsDefault;

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
