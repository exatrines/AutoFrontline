using System.Collections.Generic;

namespace AutoFrontline;

public sealed class Configuration
{
    private const int CurrentConfigVersion = 2;

    public int ConfigVersion = CurrentConfigVersion;

    /// <summary>v1 互換用（ConfigVersion &lt; 2 の移行時のみ参照）。</summary>
    public bool Enabled;

    public PluginMode Mode = PluginMode.Manual;

    public int AutoMaxCount = 1;

    /// <summary>0 = マウントルーレット。それ以外は <see cref="Mount"/> の RowId。</summary>
    public uint MountSelectionId;

    /// <summary>集団移動モード: 密集中心プレイヤー判定・moveto 間隔（秒）。</summary>
    public float GroupMovementRefreshIntervalSeconds = 1f;

    /// <summary>敵対モード: 最寄り敵・最寄り味方・中点判定・moveto 間隔（秒）。</summary>
    public float HostileModeRefreshIntervalSeconds = 1f;

    /// <summary>敵対モード: 先頭（敵に最も近い）〜最遠味方間の立ち位置（0=先頭、1=最遠、既定0.5=中央）。</summary>
    public float HostileModePositionRatio = 0.5f;

    /// <summary>移動先までこの距離以上ならマウントを試行（m）。</summary>
    public int MountDistanceMeters = 30;

    /// <summary>この半径内に敵プレイヤーがいればマウントを降ろす（m）。</summary>
    public int DismountEnemyDistanceMeters = 20;

    /// <summary>ContentsFinderConfirm（デイリーフロントライン）表示時に自動で参加する。</summary>
    public bool AutoEnterEnabled = true;

    /// <summary>FrontlineRecord 表示時に自動でコンテンツ退出する。</summary>
    public bool AutoLeaveEnabled = true;

    /// <summary>ジョブ別 Auto Limit Break（キー: PvpLimitBreakCatalog の Entry Id）。</summary>
    public Dictionary<string, bool> AutoLimitBreakByEntryId = new();

    /// <summary>Experimental: アライアンスチャット発言者を軍師として追従する。</summary>
    public bool CommanderFollowEnabled;

    /// <summary>Experimental: 自分 50m 内の密集中心、不在時は最寄り味方へ集団移動する。</summary>
    public bool ExperimentalGroupMoveEnabled;

    /// <summary>Experimental: 集団行動スタック時のデジョン発動までの停滞時間（秒）。</summary>
    public float DejonStallSeconds = FrontlineConstants.DejonStallSecondsDefault;

    public void MigrateIfNeeded()
    {
        if (ConfigVersion >= CurrentConfigVersion)
        {
            ClampDejonStallSeconds();
            return;
        }

        Mode = Enabled ? PluginMode.Manual : PluginMode.Disable;
        ConfigVersion = CurrentConfigVersion;
        ClampDejonStallSeconds();
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
