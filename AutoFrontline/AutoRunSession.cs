namespace AutoFrontline;

/// <summary>Auto モードの Start/Stop セッション状態（設定ファイルには保存しない）。</summary>
internal static class AutoRunSession
{
    public static bool Active { get; set; }
    public static int CurrentCount { get; set; }
    public static bool PendingStopAfterLeave { get; set; }
    public static bool CountedEnterThisRound { get; set; }
    public static bool WasInFrontline { get; set; }

    public static void Start()
    {
        Active = true;
        CurrentCount = 0;
        PendingStopAfterLeave = false;
        CountedEnterThisRound = false;
        WasInFrontline = false;
    }

    public static void Stop()
    {
        Active = false;
        CurrentCount = 0;
        PendingStopAfterLeave = false;
        CountedEnterThisRound = false;
        WasInFrontline = false;
    }
}
