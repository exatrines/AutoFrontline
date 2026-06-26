using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace AutoFrontline.Services;

/// <summary>降下条件（敵・特殊オブジェクト）の範囲外ならマウントを試行。</summary>
public static unsafe class TrackedPlayerSync
{
    public static int LastNearbyEnemyCount { get; private set; }
    public static bool LastIcedotomeIrisNearby { get; private set; }
    public static string LastNearbySpecialCombatName { get; private set; } = string.Empty;
    public static bool LastIsSafeToMount { get; private set; }
    public static string LastUnsafeMountReason { get; private set; } = string.Empty;

    public static bool ShouldDeferMovement =>
        IsSafeToMount() && !Player.Mounted && !Player.Mounting;

    public static void Update() => SyncMount();

    private static bool IsSafeToMount()
    {
        var allies = AllianceMemberCache.GetMembers();
        var hasNearbyEnemy = NearbyEnemyDetector.HasNearbyEnemy(allies, out var enemyCount);
        LastNearbyEnemyCount = enemyCount;

        var hasSpecialCombat = FrontlineSpecialCombatTargetDetector.TryGetClosest(
            C.DismountEnemyDistanceMeters,
            out _,
            out _,
            out var specialName);
        LastIcedotomeIrisNearby = hasSpecialCombat;
        LastNearbySpecialCombatName = hasSpecialCombat ? specialName : string.Empty;
        LastUnsafeMountReason = BuildUnsafeMountReason(hasNearbyEnemy, enemyCount, hasSpecialCombat, specialName);
        LastIsSafeToMount = !hasNearbyEnemy && !hasSpecialCombat;
        return LastIsSafeToMount;
    }

    private static string BuildUnsafeMountReason(
        bool hasNearbyEnemy,
        int enemyCount,
        bool hasSpecialCombat,
        string specialName)
    {
        if (!hasNearbyEnemy && !hasSpecialCombat)
            return string.Empty;

        var radius = C.DismountEnemyDistanceMeters;
        if (hasNearbyEnemy && hasSpecialCombat)
        {
            return $"{enemyCount} enemy player(s) within {radius}m, {specialName} within {radius}m";
        }

        if (hasNearbyEnemy)
            return $"{enemyCount} enemy player(s) within {radius}m";

        return $"{specialName} within {radius}m";
    }

    private static void SyncMount()
    {
        if (!IsSafeToMount())
        {
            if (Player.Mounted
                && EzThrottler.Throttle(FrontlineConstants.ThrottleDismount, FrontlineConstants.DismountThrottleMs))
                Chat.ExecuteCommand("/mount");

            return;
        }

        TryMount();
    }

    private static void TryMount()
    {
        if (Player.Mounted || Player.Mounting)
            return;

        if (EzThrottler.Throttle(FrontlineConstants.ThrottleMount, FrontlineConstants.MountThrottleMs))
            UseConfiguredMount();
    }

    private static void UseConfiguredMount()
    {
        if (C.MountSelectionId != MountCatalog.RouletteSelectionId
            && MountCatalog.IsMountOwned(C.MountSelectionId))
        {
            UseSpecificMount(C.MountSelectionId);
            return;
        }

        UseMountRoulette();
    }

    private static void UseMountRoulette()
    {
        var actionId = MountCatalog.ResolveRouletteActionId();
        var actionManager = ActionManager.Instance();
        if (actionManager != null)
            actionManager->UseAction(ActionType.GeneralAction, actionId, 0xE0000000);

        var name = Svc.Data.GetExcelSheet<GeneralAction>().GetRowOrDefault(actionId)?.Name.ToString();
        if (!string.IsNullOrEmpty(name))
            Chat.ExecuteCommand($"/gaction \"{name}\"");
    }

    private static void UseSpecificMount(uint mountRowId)
    {
        if (Svc.Data.GetExcelSheet<Mount>().GetRowOrDefault(mountRowId) is not { } mount)
            return;

        var actionManager = ActionManager.Instance();
        if (actionManager != null)
            actionManager->UseAction(ActionType.Mount, mountRowId, 0xE0000000);
    }
}
