using System.Linq;
using System.Numerics;
using AutoFrontline.Services;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoFrontline.UI;

public static unsafe class DebugTab
{
    public static void Draw()
    {
        DrawTerritorySection();
        DrawSelfSection();
        DrawTargetSection();
        DrawMovementSection();
        DrawStuckRecoverySection();
        DrawRotationSection();
        DrawStatusSection();
    }

    private static void DrawTerritorySection()
    {
        AflImGui.SectionHeader("Territory");

        var territoryId = Svc.ClientState.TerritoryType;
        var isFrontline = FrontlineFields.IsFrontline(territoryId);
        var fieldName = FrontlineFields.GetDisplayName(territoryId);
        if (string.IsNullOrEmpty(fieldName))
            fieldName = TerritoryName.GetTerritoryName(territoryId);

        var table = new DebugTable("##AflDbgTerritory");
        if (!table.Begin())
            return;

        table.Row("Field", $"[{territoryId}] {fieldName}",
            isFrontline ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed);
        table.Row("Alliance", $"{FollowTargetService.LastAllianceMemberCount}");

        if (!string.IsNullOrEmpty(AllianceMemberCollector.LastCollectionSource))
            table.Row("Source", AllianceMemberCollector.LastCollectionSource);

        table.End();
    }

    private static void DrawSelfSection()
    {
        AflImGui.SectionHeader("Self");

        var table = new DebugTable("##AflDbgSelf");
        if (!table.Begin())
            return;

        table.Row("Job", Player.Job.GetData().Name.ToString());
        table.Row("In combat", TrackedPlayerSync.LastSelfInCombat ? "Yes" : "No",
            TrackedPlayerSync.LastSelfInCombat ? ImGuiColors.DalamudRed : ImGuiColors.HealerGreen);
        table.Row("Mounted", Player.Mounted ? "Yes" : "No");
        table.Row("Casting", Player.IsCasting ? "Yes" : "No");
        table.Row("Mounting", Player.Mounting ? "Yes" : "No");
        table.Row("Mount choice", MountCatalog.GetDisplayName(C.MountSelectionId));
        table.Row($"Nearby enemies ({C.DismountEnemyDistanceMeters}m)", $"{TrackedPlayerSync.LastNearbyEnemyCount}");
        table.Row($"Special combat nearby ({C.DismountEnemyDistanceMeters}m)",
            TrackedPlayerSync.LastIcedotomeIrisNearby ? "Yes" : "No");
        table.End();
    }

    private static void DrawTargetSection()
    {
        AflImGui.SectionHeader("Target");

        var table = new DebugTable("##AflDbgTarget");
        if (!table.Begin())
            return;

        table.Row("追従モード", GetFollowModeLabel(), GetFollowModeColor());

        var tracked = FollowTargetService.LastPickedMemberName;
        table.Row("Tracked", string.IsNullOrEmpty(tracked) ? "—" : tracked);
        table.Row("Latest commander",
            string.IsNullOrEmpty(AllianceCommanderTracker.LatestCommanderName)
                ? "—"
                : AllianceCommanderTracker.LatestCommanderName);
        table.Row("Commander follow", GetCommanderFollowState());
        table.Row("Job", GetTrackedJobName());
        table.Row("Distance", $"{TrackedPlayerSync.LastDistanceToTracked:F1} m");
        if (!string.IsNullOrEmpty(FollowTargetService.LastProximityEnemyName))
            table.Row("Proximity enemy", FollowTargetService.LastProximityEnemyName);

        var enemy = ClosestEnemyPlayerTargeting.LastClosestEnemy;
        table.Row("Combat target",
            ClosestEnemyPlayerTargeting.LastTargetIsSpecialCombatObject
                ? ClosestEnemyPlayerTargeting.LastSpecialCombatTargetName
                : enemy?.Name.ToString() ?? "—");
        table.Row("PvP LB", GetPvpLimitBreakDebugText());
        if (ClosestEnemyPlayerTargeting.LastTargetIsSpecialCombatObject || enemy != null)
            table.Row("Combat distance", $"{ClosestEnemyPlayerTargeting.LastClosestEnemyDistance:F1} m");

        if (!string.IsNullOrEmpty(FollowTargetService.LastDensestMemberName))
        {
            table.Row("Densest (50m)",
                $"{FollowTargetService.LastDensestMemberName} ({FollowTargetService.LastDensestNeighborCount})");
            table.Row("Densest job", TryGetJobDisplayNameByPlayerName(FollowTargetService.LastDensestMemberName));
        }

        table.End();
    }

    private static void DrawMovementSection()
    {
        AflImGui.SectionHeader("Movement");

        var table = new DebugTable("##AflDbgMove");
        if (!table.Begin())
            return;

        if (FollowTargetService.LastTrackedPlayerPosition is Vector3 trackedPos)
        {
            var posText = GameCoords.FormatDisplay(trackedPos);
            if (FollowTargetService.TrackedPositionUnchanged)
                posText += " (unchanged)";

            table.Row("Tracked pos.", posText,
                FollowTargetService.TrackedPositionUnchanged ? ImGuiColors.DalamudGrey : null);
        }

        if (FollowTargetService.CachedTrackedPlayerPosition is Vector3 cached
            && FollowTargetService.LastTrackedPlayerPosition is Vector3 live
            && !GameCoords.AreNear(cached, live, 0.1f))
        {
            table.Row("Cached pos.", GameCoords.FormatDisplay(cached));
        }

        if (FollowTargetService.LastMoveAnchorPosition is Vector3 anchor)
            table.Row("Move anchor", GameCoords.FormatDisplay(anchor));

        table.Row("Move target",
            InitialMovementMode.IsActive && InitialMovementMode.FirstExitPosition is Vector3 initialMove
                ? $"{GameCoords.FormatDisplay(initialMove)} (initial)"
                : FollowTargetService.LastMoveTarget is Vector3 move
                    ? GameCoords.FormatDisplay(move)
                    : "—");

        table.Row("Entry position",
            FrontlineEntryZone.EntryPosition is Vector3 entry ? GameCoords.FormatDisplay(entry) : "—");

        var exclusionLabel = FrontlineEntryZone.LastMoveBlocked || FrontlineEntryZone.LastTargetBlocked
            ? "blocked"
            : "ok";
        if (FrontlineEntryZone.EntryPosition is Vector3 spawn
            && FollowTargetService.LastMoveTarget is Vector3 lastMove)
        {
            var dist = FrontlineEntryZone.DistanceToEntry(lastMove);
            exclusionLabel = $"{exclusionLabel} (move target {dist:F1}m from entry)";
        }

        if (FrontlineEntryZone.LastTargetBlocked && Player.Available && Player.Object != null)
        {
            var playerDist = FrontlineEntryZone.DistanceToEntry(Player.Object.Position);
            if (playerDist is float dist)
                exclusionLabel = $"{exclusionLabel} (player {dist:F1}m from entry)";
        }

        table.Row($"Spawn exclusion ({FrontlineConstants.SpawnExclusionRadiusMeters}m)", exclusionLabel,
            FrontlineEntryZone.LastMoveBlocked || FrontlineEntryZone.LastTargetBlocked
                ? ImGuiColors.DalamudOrange
                : null);

        table.Row("Initial movement", InitialMovementMode.IsActive ? "active" : "—",
            InitialMovementMode.IsActive ? ImGuiColors.DalamudOrange : null);
        table.Row("First exit pos.",
            InitialMovementMode.FirstExitPosition is Vector3 firstExit
                ? GameCoords.FormatDisplay(firstExit)
                : "—");
        if (InitialMovementMode.FirstExitPosition == null
            && FrontlineEntryZone.EntryPosition is Vector3 spawnCenter
            && Player.Available
            && Player.Object != null)
        {
            var recordDist = GameCoords.HorizontalDistance(spawnCenter, Player.Object.Position);
            table.Row(
                $"Record ring ({FrontlineConstants.InitialMovementRecordRadiusMeters}m XY)",
                $"{recordDist:F1} m");
        }
        if (InitialMovementMode.IsActive && InitialMovementMode.DistanceToAnchor() is float anchorDist)
            table.Row("Initial move dist.", $"{anchorDist:F1} m");

        table.End();

        if (ImGui.Button("Reset initial (spawn + first exit)"))
        {
            if (InitialMovementMode.DebugResetInitialState())
                Svc.Chat.Print("[AutoFrontline] Initial state reset at current position.");
            else
                Svc.Chat.Print("[AutoFrontline] Initial reset failed (not in Frontline or player unavailable).");
        }

        ImGui.TextDisabled("Recaptures spawn center here and clears first exit. Use at the real spawn after a hot reload.");
    }

    private static void DrawStuckRecoverySection()
    {
        AflImGui.SectionHeader("Stuck recovery (Dejon)");

        var table = new DebugTable("##AflDbgStuck");
        if (!table.Begin())
            return;

        var phase = NaviStuckDejonAutomation.StallPhaseLabel;
        table.Row("Phase", phase, GetStuckPhaseColor(phase));

        table.Row(
            "Group movement",
            NaviStuckDejonAutomation.IsGroupMovementEligible ? "yes" : "no",
            NaviStuckDejonAutomation.IsGroupMovementEligible ? ImGuiColors.HealerGreen : null);

        var blockReason = NaviStuckDejonAutomation.StallMonitorBlockReason;
        table.Row(
            "Timer armed",
            string.IsNullOrEmpty(blockReason) ? "yes" : $"no ({blockReason})",
            string.IsNullOrEmpty(blockReason) ? ImGuiColors.HealerGreen : ImGuiColors.DalamudOrange);

        table.Row("Moveto tracked", NaviStuckDejonAutomation.IsMovetoTracked ? "yes" : "no");

        var destDist = NaviStuckDejonAutomation.StallDestinationDistanceMeters;
        table.Row(
            $"Dest. distance ({FrontlineConstants.NaviStuckDejonMinDestinationDistanceMeters}m+)",
            destDist is float distance ? $"{distance:F1} m" : "—",
            destDist is float d
                && d < FrontlineConstants.NaviStuckDejonMinDestinationDistanceMeters
                ? ImGuiColors.DalamudOrange
                : null);

        table.Row(
            "Current pos.",
            NaviStuckDejonAutomation.StallCurrentPlayerPosition is Vector3 current
                ? GameCoords.FormatDisplay(current)
                : "—");

        table.Row(
            "Anchor pos.",
            NaviStuckDejonAutomation.StallPreviousPlayerPosition is Vector3 anchor
                ? GameCoords.FormatDisplay(anchor)
                : "—");

        var posDelta = NaviStuckDejonAutomation.StallPositionDeltaMeters;
        table.Row(
            $"Pos. delta ({FrontlineConstants.NaviStuckDejonPositionThresholdMeters}m)",
            posDelta is float delta ? $"{delta:F2} m" : "—",
            posDelta is float stallDelta
                && stallDelta <= FrontlineConstants.NaviStuckDejonPositionThresholdMeters
                ? ImGuiColors.DalamudOrange
                : null);

        table.Row(
            "Timer",
            NaviStuckDejonAutomation.IsStallTimerActive
                ? $"{NaviStuckDejonAutomation.StallElapsedSeconds:F1} / {NaviStuckDejonAutomation.StallThresholdSeconds:F0} s"
                : $"0.0 / {NaviStuckDejonAutomation.StallThresholdSeconds:F0} s",
            NaviStuckDejonAutomation.IsMonitoringStall ? ImGuiColors.DalamudOrange : null);

        table.End();
    }

    private static Vector4? GetStuckPhaseColor(string phase) => phase switch
    {
        "monitoring" => ImGuiColors.DalamudOrange,
        "triggering" => ImGuiColors.DalamudRed,
        "awaiting dejon confirm" => ImGuiColors.ParsedGold,
        "near destination" => ImGuiColors.DalamudOrange,
        "not group movement" => ImGuiColors.DalamudGrey,
        "idle" => ImGuiColors.DalamudGrey,
        _ => null,
    };

    private static void DrawRotationSection()
    {
        AflImGui.SectionHeader("Rotation");

        var table = new DebugTable("##AflDbgRotation");
        if (!table.Begin())
            return;

        var rsrLoaded = RequiredPlugins.IsLoaded(RequiredPlugins.RotationSolver.InternalName);
        table.Row("RSR loaded", rsrLoaded ? "Yes" : "No");

        if (!rsrLoaded)
        {
            table.End();
            return;
        }

        var snapshot = RotationSolverState.GetSnapshot();
        if (!snapshot.Resolved)
        {
            table.Row("RSR state", RotationSolverOperatingState.Unknown.ToString(), ImGuiColors.DalamudRed);
            table.Row("RSR source", string.IsNullOrEmpty(snapshot.ReadSource) ? "unresolved" : snapshot.ReadSource);
            table.End();
            return;
        }

        table.Row("RSR state", snapshot.DerivedState.ToString(), GetRsrStateColor(snapshot.DerivedState));
        table.Row("RSR UI", string.IsNullOrEmpty(snapshot.DisplayState) ? "—" : snapshot.DisplayState);
        table.Row("RSR raw",
            $"State={FormatBool(snapshot.State)}, IsManual={FormatBool(snapshot.IsManual)}, " +
            $"Targeting={FormatTargetingType(snapshot.TargetingType)}, PvP={FormatBool(snapshot.IsPvPStateEnabled)}");
        table.Row("RSR source", snapshot.ReadSource);
        table.Row("RSR ALC", string.IsNullOrEmpty(snapshot.AlcAssemblies) ? "—" : snapshot.AlcAssemblies);
        table.Row("RSR assembly", string.IsNullOrEmpty(snapshot.AssemblyName) ? "—" : snapshot.AssemblyName);
        table.Row("Manual enforce", GetManualEnforceText(snapshot.DerivedState));

        table.End();
    }

    private static string FormatBool(bool value) => value ? "true" : "false";

    private static string FormatTargetingType(string targetingType) =>
        string.IsNullOrEmpty(targetingType) ? "—" : targetingType;

    private static Vector4? GetRsrStateColor(RotationSolverOperatingState state) =>
        state switch
        {
            RotationSolverOperatingState.Manual => ImGuiColors.HealerGreen,
            RotationSolverOperatingState.Off => ImGuiColors.DalamudGrey,
            RotationSolverOperatingState.Unknown => ImGuiColors.DalamudRed,
            _ => ImGuiColors.DalamudOrange,
        };

    private static string GetManualEnforceText(RotationSolverOperatingState derivedState)
    {
        if (!AutomationContext.CanRunInFrontlineMatch)
            return "—";

        return derivedState == RotationSolverOperatingState.Manual ? "skip" : "would send";
    }

    private static void DrawStatusSection()
    {
        AflImGui.SectionHeader("Status");
        ImGui.TextDisabled($"Mode: {C.Mode}");
        if (C.Mode == PluginMode.Loop)
        {
            ImGui.TextDisabled($"Loop: {(AutoRunSession.Active ? "active" : "idle")} ({AutoRunSession.CurrentCount}/{C.AutoMaxCount})");
            ImGui.TextDisabled($"Phase: {FrontlineAutoRunOrchestrator.LastPhase}");
            ImGui.TextDisabled($"CF queue: {ContentsFinderQueueAutomation.LastStatus}");
        }

        DrawStatusBullet("Movement", TrackedPlayerSync.ShouldDeferMovement, "Waiting to mount before move");
        DrawStatusBullet("Vnav blocked", !PlayerMovementGate.CanIssueVnavMoveTo, "Casting or mounting");
        DrawStatusBullet(
            "Auto leave",
            RequiredPlugins.ShouldAutoLeave && FrontlineLeaveAutomation.IsRecordScreenVisible,
            "Leaving from record screen");
        DrawStatusBullet(
            "Record screen",
            FrontlineLeaveAutomation.IsRecordScreenVisible,
            "Frontline record screen");
        DrawStatusBullet(
            "Auto enter",
            RequiredPlugins.ShouldAutoEnter
            && GenericHelpers.TryGetAddonByName<AtkUnitBase>("ContentsFinderConfirm", out var enterAddon)
            && enterAddon->IsVisible,
            "Contents Finder confirm open");
    }

    private static void DrawStatusBullet(string label, bool active, string message)
    {
        ImGui.Bullet();
        ImGui.SameLine();
        if (active)
            ImGui.Text($"{label}: {message}");
        else
            ImGui.TextDisabled($"{label}: —");
    }

    private static string GetPvpLimitBreakDebugText()
    {
        if (!FollowTargetService.IsHostileMode)
            return "—";

        if (!PvpLimitBreakCatalog.TryGetEnabledActionForJob(Player.Job, out var actionName))
            return "Disabled";

        var status = PvpLimitBreakAutomation.LastAttempted ? "sent" : "armed";
        return $"{actionName} ({status})";
    }

    private static string GetFollowModeLabel()
    {
        if (InitialMovementMode.IsActive)
            return $"初期移動（脱出先へ） / {FollowTargetService.CurrentFollowModeLabel}";

        return FollowTargetService.CurrentFollowModeLabel;
    }

    private static Vector4? GetFollowModeColor()
    {
        if (InitialMovementMode.IsActive)
            return ImGuiColors.DalamudOrange;

        if (FollowTargetService.IsCommanderMode)
            return ImGuiColors.ParsedGold;
        if (FollowTargetService.IsHostileMode)
            return ImGuiColors.DalamudRed;
        if (FollowTargetService.IsGroupMovementMode)
            return ImGuiColors.HealerGreen;
        return null;
    }

    private static string GetCommanderFollowState()
    {
        if (AllianceCommanderTracker.LatestCommanderContentId == 0)
            return "—";

        return AllianceCommanderTracker.IsFollowPending ? "Pending" : "Completed";
    }

    private static string GetTrackedJobName()
    {
        if (FollowTargetService.TryGetTrackedGameObject() is IPlayerCharacter pc)
            return GetJobDisplayName((Job)pc.ClassJob.RowId);

        return TryGetJobDisplayNameByPlayerName(FollowTargetService.LastPickedMemberName);
    }

    private static string GetJobDisplayName(Job job) => job.GetData().Name.ToString();

    private static string TryGetJobDisplayNameByPlayerName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "—";

        var pc = Svc.Objects.OfType<IPlayerCharacter>()
            .FirstOrDefault(p => p.Name.ToString() == name);

        return pc == null ? "—" : GetJobDisplayName((Job)pc.ClassJob.RowId);
    }
}
