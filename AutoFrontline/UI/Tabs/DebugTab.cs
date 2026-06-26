using AutoFrontline.Services;
using ECommons.GameHelpers;

namespace AutoFrontline.UI;

public static class DebugTab
{
    public static void Draw()
    {
        if (!ImGui.BeginTabBar("AflDebugSubTabs"))
            return;

        if (ImGui.BeginTabItem("AutoFrontline"))
        {
            DrawAutoFrontline();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("vnavmesh"))
        {
            MeshStatusTab.Draw();
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    private static void DrawAutoFrontline()
    {
        DrawSpawnSection();

        ImGui.Spacing();
        AflImGui.SectionHeader("Movement");

        ImGui.Text($"Follow mode: {FollowTargetService.CurrentFollowModeLabel}");

        if (!string.IsNullOrEmpty(FollowTargetService.TrackedMemberName))
            ImGui.Text($"Target player: {FollowTargetService.TrackedMemberName}");
        else
            ImGui.TextDisabled("Target player: —");

        DrawExcludedFollowTargetsSection();

        ImGui.Spacing();

        if (MovementCommands.LastIssuedMoveTo is { } target)
        {
            ImGui.Text($"Last moveto: {GameCoords.FormatDisplay(target)}");
            ImGui.TextDisabled($"/vnav moveto {GameCoords.FormatCommand(target)}");
        }
        else
        {
            ImGui.TextDisabled("Last moveto: —");
        }

        ImGui.Spacing();
        DrawMountSection();

        ImGui.Spacing();
        AflImGui.SectionHeader("Stuck recovery (Return)");

        if (NaviStuckDejonAutomation.IsMonitoringStall)
        {
            ImGui.Text(
                $"Stall timer: {NaviStuckDejonAutomation.StallElapsedSeconds:F1} / {NaviStuckDejonAutomation.StallThresholdSeconds:F1} s");
        }
        else if (NaviStuckDejonAutomation.IsStallTimerActive)
        {
            ImGui.TextDisabled(
                $"Stall timer: {NaviStuckDejonAutomation.StallElapsedSeconds:F1} / {NaviStuckDejonAutomation.StallThresholdSeconds:F1} s (resetting)");
        }
        else
        {
            ImGui.TextDisabled("Stall timer: —");
        }

        ImGui.Text($"Phase: {NaviStuckDejonAutomation.StallPhaseLabel}");

        if (NaviStuckDejonAutomation.StallMonitorBlockReason is { Length: > 0 } reason)
            ImGui.TextDisabled($"Monitor blocked: {reason}");
    }

    private static void DrawSpawnSection()
    {
        AflImGui.SectionHeader("Spawn");

        ImGui.Text($"Exclusion radius: {C.SpawnExclusionRadiusMeters} m");

        if (FrontlineEntryZone.EntryPosition is not { } entry)
        {
            ImGui.TextDisabled("Spawn center: —");
            ImGui.TextDisabled("In exclusion zone: —");
            ImGui.TextDisabled("Distance to spawn: —");
        }
        else
        {
            ImGui.Text($"Spawn center: {GameCoords.FormatDisplay(entry)}");

            var inExclusion = FrontlineEntryZone.IsPlayerInExclusion();
            ImGui.Text($"In exclusion zone: {(inExclusion ? "yes" : "no")}");

            if (Player.Available && Player.Object != null)
            {
                var distance = FrontlineEntryZone.DistanceToEntry(Player.Object.Position);
                ImGui.Text($"Distance to spawn: {distance:F1} m");
            }
            else
            {
                ImGui.TextDisabled("Distance to spawn: —");
            }
        }

        if (InitialMovementMode.HasFixedExitForCurrentTerritory)
        {
            ImGui.Text("Fixed spawn exit: configured");
            if (InitialMovementMode.FixedExitDestination is { } exit)
                ImGui.TextDisabled($"  {GameCoords.FormatDisplay(exit)}");
        }
        else
        {
            ImGui.TextDisabled("Fixed spawn exit: — (group movement only)");
        }

        ImGui.Text(
            $"Left exclusion zone: {(InitialMovementMode.HasLeftSpawnExclusion ? "yes" : "no")}");

        ImGui.Text($"Initial movement mode: {(InitialMovementMode.IsActive ? "active" : "inactive")}");
    }

    private static void DrawExcludedFollowTargetsSection()
    {
        if (C.StationaryTargetExclusionSeconds <= 0)
        {
            ImGui.TextDisabled("Excluded follow targets: — (exclusion disabled)");
            return;
        }

        var entries = StationaryTargetExclusion.GetDebugEntries();
        if (entries.Count == 0)
        {
            ImGui.TextDisabled("Excluded follow targets: —");
            return;
        }

        ImGui.Text($"Excluded follow targets ({entries.Count}):");
        foreach (var entry in entries)
        {
            ImGui.BulletText(
                $"{entry.Name} — {FormatExclusionReason(entry.Reason)} — {entry.RemainingSeconds:F1}s left");
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
        AflImGui.SectionHeader("Mount");

        ImGui.Text(
            $"Nearby enemies ({C.DismountEnemyDistanceMeters}m): {TrackedPlayerSync.LastNearbyEnemyCount}");

        if (TrackedPlayerSync.LastIcedotomeIrisNearby)
        {
            ImGui.Text(
                $"Special combat object ({C.DismountEnemyDistanceMeters}m): {TrackedPlayerSync.LastNearbySpecialCombatName}");
        }
        else
        {
            ImGui.TextDisabled(
                $"Special combat object ({C.DismountEnemyDistanceMeters}m): —");
        }

        if (Player.Mounted)
        {
            ImGui.Text("Status: mounted");
            if (!TrackedPlayerSync.LastIsSafeToMount)
                ImGui.TextDisabled($"Dismount pending: {TrackedPlayerSync.LastUnsafeMountReason}");
        }
        else if (Player.Mounting)
        {
            ImGui.Text("Status: mounting");
        }
        else if (!TrackedPlayerSync.LastIsSafeToMount)
        {
            ImGui.Text($"Dismount reason: {TrackedPlayerSync.LastUnsafeMountReason}");
        }
        else
        {
            ImGui.TextDisabled("Dismount reason: — (safe to mount)");
        }
    }
}
