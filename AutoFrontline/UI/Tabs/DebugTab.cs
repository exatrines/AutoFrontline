using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using AutoFrontline.Services;
using ECommons.GameHelpers;

namespace AutoFrontline.UI;

public static class DebugTab
{
    public static void Draw()
    {
        DrawSpawnSection();
        DrawMovementSection();
        DrawMountSection();
        DrawReturnSection();
    }

    private static void DrawMovementSection()
    {
        MirageUi.SubHeader("Movement");

        MirageUi.Text($"Follow mode: {FollowTargetService.CurrentFollowModeLabel}");

        if (!string.IsNullOrEmpty(FollowTargetService.TrackedMemberName))
            MirageUi.Text($"Target player: {FollowTargetService.TrackedMemberName}");
        else
            MirageUi.Text("Target player: —", color: MirageUi.Color.Secondary);

        DrawExcludedFollowTargetsSection();

        if (MovementCommands.LastIssuedMoveTo is { } target)
        {
            MirageUi.Text($"Last moveto: {GameCoords.FormatDisplay(target)}");
            MirageUi.Text(
                $"/vnav moveto {GameCoords.FormatCommand(target)}",
                color: MirageUi.Color.Secondary);
        }
        else
        {
            MirageUi.Text("Last moveto: —", color: MirageUi.Color.Secondary);
        }
    }

    private static void DrawReturnSection()
    {
        MirageUi.SubHeader("Stuck recovery (Return)");

        if (NaviStuckDejonAutomation.IsMonitoringStall)
        {
            MirageUi.Text(
                $"Stall timer: {NaviStuckDejonAutomation.StallElapsedSeconds:F1} / {NaviStuckDejonAutomation.StallThresholdSeconds:F1} s");
        }
        else if (NaviStuckDejonAutomation.IsStallTimerActive)
        {
            MirageUi.Text(
                $"Stall timer: {NaviStuckDejonAutomation.StallElapsedSeconds:F1} / {NaviStuckDejonAutomation.StallThresholdSeconds:F1} s (resetting)",
                color: MirageUi.Color.Secondary);
        }
        else
        {
            MirageUi.Text("Stall timer: —", color: MirageUi.Color.Secondary);
        }

        MirageUi.Text($"Phase: {NaviStuckDejonAutomation.StallPhaseLabel}");

        if (NaviStuckDejonAutomation.StallMonitorBlockReason is { Length: > 0 } reason)
            MirageUi.Text($"Monitor blocked: {reason}", color: MirageUi.Color.Secondary);
    }

    private static void DrawSpawnSection()
    {
        MirageUi.SubHeader("Spawn");

        MirageUi.Text($"Exclusion radius: {C.SpawnExclusionRadiusMeters} m");

        if (FrontlineEntryZone.EntryPosition is not { } entry)
        {
            MirageUi.Text("Spawn center: —", color: MirageUi.Color.Secondary);
            MirageUi.Text("In exclusion zone: —", color: MirageUi.Color.Secondary);
            MirageUi.Text("Distance to spawn: —", color: MirageUi.Color.Secondary);
        }
        else
        {
            MirageUi.Text($"Spawn center: {GameCoords.FormatDisplay(entry)}");

            var inExclusion = FrontlineEntryZone.IsPlayerInExclusion();
            MirageUi.Text($"In exclusion zone: {(inExclusion ? "yes" : "no")}");

            if (Player.Available && Player.Object != null)
            {
                var distance = FrontlineEntryZone.DistanceToEntry(Player.Object.Position);
                MirageUi.Text($"Distance to spawn: {distance:F1} m");
            }
            else
            {
                MirageUi.Text("Distance to spawn: —", color: MirageUi.Color.Secondary);
            }
        }

        if (InitialMovementMode.HasFixedExitForCurrentTerritory)
        {
            MirageUi.Text("Fixed spawn exit: configured");
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
                "Fixed spawn exit: — (group movement only)",
                color: MirageUi.Color.Secondary);
        }

        MirageUi.Text($"Left exclusion zone: {(InitialMovementMode.HasLeftSpawnExclusion ? "yes" : "no")}");
        MirageUi.Text($"Initial movement mode: {(InitialMovementMode.IsActive ? "active" : "inactive")}");
    }

    private static void DrawExcludedFollowTargetsSection()
    {
        if (C.StationaryTargetExclusionSeconds <= 0)
        {
            MirageUi.Text(
                "Excluded follow targets: — (exclusion disabled)",
                color: MirageUi.Color.Secondary);
            return;
        }

        var entries = StationaryTargetExclusion.GetDebugEntries();
        if (entries.Count == 0)
        {
            MirageUi.Text("Excluded follow targets: —", color: MirageUi.Color.Secondary);
            return;
        }

        MirageUi.Text($"Excluded follow targets ({entries.Count}):");
        foreach (var entry in entries)
        {
            MirageUi.Text(
                $"• {entry.Name} — {FormatExclusionReason(entry.Reason)} — {entry.RemainingSeconds:F1}s left",
                color: MirageUi.Color.Secondary);
        }
    }

    private static string FormatExclusionReason(FollowTargetExclusionReason reason) => reason switch
    {
        FollowTargetExclusionReason.Stationary =>
            $"stationary ({FrontlineConstants.StationaryTargetExcludePickCount})",
        FollowTargetExclusionReason.RepeatedPick =>
            $"repeated picks ({C.RepeatedFollowTargetExcludePickCount})",
        _ => reason.ToString(),
    };

    private static void DrawMountSection()
    {
        MirageUi.SubHeader("Mount");

        MirageUi.Text($"Nearby enemies ({C.DismountEnemyDistanceMeters}m): {TrackedPlayerSync.LastNearbyEnemyCount}");

        if (TrackedPlayerSync.LastIcedotomeIrisNearby)
        {
            MirageUi.Text(
                $"Special combat object ({C.DismountEnemyDistanceMeters}m): {TrackedPlayerSync.LastNearbySpecialCombatName}");
        }
        else
        {
            MirageUi.Text(
                $"Special combat object ({C.DismountEnemyDistanceMeters}m): —",
                color: MirageUi.Color.Secondary);
        }

        if (Player.Mounted)
        {
            MirageUi.Text("Status: mounted");
            if (!TrackedPlayerSync.LastIsSafeToMount)
            {
                MirageUi.Text(
                    $"Dismount pending: {TrackedPlayerSync.LastUnsafeMountReason}",
                    color: MirageUi.Color.Secondary);
            }
        }
        else if (Player.Mounting)
        {
            MirageUi.Text("Status: mounting");
        }
        else if (!TrackedPlayerSync.LastIsSafeToMount)
        {
            MirageUi.Text($"Dismount reason: {TrackedPlayerSync.LastUnsafeMountReason}");
        }
        else
        {
            MirageUi.Text(
                "Dismount reason: — (safe to mount)",
                color: MirageUi.Color.Secondary);
        }
    }
}
