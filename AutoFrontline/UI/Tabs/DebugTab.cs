using AutoFrontline.Services;
using ECommons.GameHelpers;

namespace AutoFrontline.UI;

public static class DebugTab
{
    public static void Draw()
    {
        MirageUi.Header("Debug");
        DrawSpawnSection();
        DrawMovementSection();
        DrawMountSection();
        DrawReturnSection();
    }

    private static void DrawMovementSection()
    {
        MirageUi.SubHeader(I18n.Get("debug.movement"));

        MirageUi.Text(I18n.Format("debug.follow_mode", FollowTargetService.CurrentFollowModeLabel));

        if (!string.IsNullOrEmpty(FollowTargetService.TrackedMemberName))
            MirageUi.Text(I18n.Format("debug.target", FollowTargetService.TrackedMemberName));
        else
            MirageUi.Text(I18n.Get("debug.target.none"), color: MirageUi.Color.Secondary);

        DrawExcludedFollowTargetsSection();

        if (MovementCommands.LastIssuedMoveTo is { } target)
        {
            MirageUi.Text(I18n.Format("debug.last_moveto", GameCoords.FormatDisplay(target)));
            MirageUi.Text(
                $"/vnav moveto {GameCoords.FormatCommand(target)}",
                color: MirageUi.Color.Secondary);
        }
        else
        {
            MirageUi.Text(I18n.Get("debug.last_moveto.none"), color: MirageUi.Color.Secondary);
        }
    }

    private static void DrawReturnSection()
    {
        MirageUi.SubHeader(I18n.Get("debug.return"));

        if (NaviStuckDejonAutomation.IsMonitoringStall)
        {
            MirageUi.Text(
                I18n.Format(
                    "debug.stall",
                    NaviStuckDejonAutomation.StallElapsedSeconds,
                    NaviStuckDejonAutomation.StallThresholdSeconds));
        }
        else if (NaviStuckDejonAutomation.IsStallTimerActive)
        {
            MirageUi.Text(
                I18n.Format(
                    "debug.stall.resetting",
                    NaviStuckDejonAutomation.StallElapsedSeconds,
                    NaviStuckDejonAutomation.StallThresholdSeconds),
                color: MirageUi.Color.Secondary);
        }
        else
        {
            MirageUi.Text(I18n.Get("debug.stall.none"), color: MirageUi.Color.Secondary);
        }

        MirageUi.Text(I18n.Format("debug.phase", NaviStuckDejonAutomation.StallPhaseLabel));

        if (NaviStuckDejonAutomation.StallMonitorBlockReason is { Length: > 0 } reason)
            MirageUi.Text(I18n.Format("debug.monitor_blocked", reason), color: MirageUi.Color.Secondary);
    }

    private static void DrawSpawnSection()
    {
        MirageUi.SubHeader(I18n.Get("debug.spawn"));

        MirageUi.Text(I18n.Format("debug.exclusion_radius", C.SpawnExclusionRadiusMeters));

        if (FrontlineEntryZone.EntryPosition is not { } entry)
        {
            MirageUi.Text(I18n.Get("debug.spawn_center.none"), color: MirageUi.Color.Secondary);
            MirageUi.Text(I18n.Get("debug.in_exclusion.none"), color: MirageUi.Color.Secondary);
            MirageUi.Text(I18n.Get("debug.distance_spawn.none"), color: MirageUi.Color.Secondary);
        }
        else
        {
            MirageUi.Text(I18n.Format("debug.spawn_center", GameCoords.FormatDisplay(entry)));

            var inExclusion = FrontlineEntryZone.IsPlayerInExclusion();
            MirageUi.Text(
                I18n.Format("debug.in_exclusion", inExclusion ? I18n.Get("debug.yes") : I18n.Get("debug.no")));

            if (Player.Available && Player.Object != null)
            {
                var distance = FrontlineEntryZone.DistanceToEntry(Player.Object.Position);
                MirageUi.Text(I18n.Format("debug.distance_spawn", distance));
            }
            else
            {
                MirageUi.Text(I18n.Get("debug.distance_spawn.none"), color: MirageUi.Color.Secondary);
            }
        }

        if (InitialMovementMode.HasFixedExitForCurrentTerritory)
        {
            MirageUi.Text(I18n.Get("debug.fixed_exit.configured"));
            if (InitialMovementMode.FixedExitDestination is { } exit)
            {
                MirageUi.Text(
                    GameCoords.FormatDisplay(exit),
                    color: MirageUi.Color.Secondary);
            }
        }
        else
        {
            MirageUi.Text(
                I18n.Get("debug.fixed_exit.none"),
                color: MirageUi.Color.Secondary);
        }

        MirageUi.Text(
            I18n.Format(
                "debug.left_exclusion",
                InitialMovementMode.HasLeftSpawnExclusion ? I18n.Get("debug.yes") : I18n.Get("debug.no")));
        MirageUi.Text(
            I18n.Format(
                "debug.initial_movement",
                InitialMovementMode.IsActive ? I18n.Get("debug.active") : I18n.Get("debug.inactive")));
    }

    private static void DrawExcludedFollowTargetsSection()
    {
        if (C.StationaryTargetExclusionSeconds <= 0)
        {
            MirageUi.Text(
                I18n.Get("debug.excluded.disabled"),
                color: MirageUi.Color.Secondary);
            return;
        }

        var entries = StationaryTargetExclusion.GetDebugEntries();
        if (entries.Count == 0)
        {
            MirageUi.Text(I18n.Get("debug.excluded.none"), color: MirageUi.Color.Secondary);
            return;
        }

        MirageUi.Text(I18n.Format("debug.excluded.count", entries.Count));
        foreach (var entry in entries)
        {
            MirageUi.Text(
                I18n.Format(
                    "debug.excluded.entry",
                    entry.Name,
                    FormatExclusionReason(entry.Reason),
                    entry.RemainingSeconds),
                color: MirageUi.Color.Secondary);
        }
    }

    private static string FormatExclusionReason(FollowTargetExclusionReason reason) => reason switch
    {
        FollowTargetExclusionReason.Stationary =>
            I18n.Format("debug.reason.stationary", FrontlineConstants.StationaryTargetExcludePickCount),
        FollowTargetExclusionReason.RepeatedPick =>
            I18n.Format("debug.reason.repeated", C.RepeatedFollowTargetExcludePickCount),
        _ => reason.ToString(),
    };

    private static void DrawMountSection()
    {
        MirageUi.SubHeader(I18n.Get("debug.mount"));

        MirageUi.Text(
            I18n.Format("debug.nearby_enemies", C.DismountEnemyDistanceMeters, TrackedPlayerSync.LastNearbyEnemyCount));

        if (TrackedPlayerSync.LastIcedotomeIrisNearby)
        {
            MirageUi.Text(
                I18n.Format(
                    "debug.special_object",
                    C.DismountEnemyDistanceMeters,
                    TrackedPlayerSync.LastNearbySpecialCombatName));
        }
        else
        {
            MirageUi.Text(
                I18n.Format("debug.special_object.none", C.DismountEnemyDistanceMeters),
                color: MirageUi.Color.Secondary);
        }

        if (Player.Mounted)
        {
            MirageUi.Text(I18n.Get("debug.status.mounted"));
            if (!TrackedPlayerSync.LastIsSafeToMount)
            {
                MirageUi.Text(
                    I18n.Format("debug.dismount_pending", TrackedPlayerSync.LastUnsafeMountReason),
                    color: MirageUi.Color.Secondary);
            }
        }
        else if (Player.Mounting)
        {
            MirageUi.Text(I18n.Get("debug.status.mounting"));
        }
        else if (!TrackedPlayerSync.LastIsSafeToMount)
        {
            MirageUi.Text(I18n.Format("debug.dismount_reason", TrackedPlayerSync.LastUnsafeMountReason));
        }
        else
        {
            MirageUi.Text(
                I18n.Get("debug.dismount_safe"),
                color: MirageUi.Color.Secondary);
        }
    }
}
