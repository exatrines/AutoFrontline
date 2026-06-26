using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using AutoFrontline.Services;

namespace AutoFrontline.UI;

public static class MeshStatusTab
{
    public static void Draw()
    {
        AflImGui.DrawPluginStatus(RequiredPlugins.VNavmesh);
        ImGui.Spacing();

        var snapshot = VNavmeshIpc.Refresh();

        if (!snapshot.PluginLoaded)
        {
            ImGui.TextDisabled("vnavmesh is not loaded.");
            return;
        }

        if (!snapshot.SubscribersReady)
        {
            ImGui.TextDisabled("Failed to initialize vnavmesh IPC subscribers.");
            return;
        }

        DrawNavSection(snapshot);
        DrawQuerySection(snapshot);
        DrawPathSection(snapshot);
    }

    private static void DrawNavSection(VNavmeshIpcSnapshot snapshot)
    {
        AflImGui.SectionHeader("Nav");

        DrawBool("Nav.IsReady", snapshot.NavIsReady);
        DrawFloat("Nav.BuildProgress", snapshot.NavBuildProgress, "P1");
        DrawBool("Nav.PathfindInProgress", snapshot.NavPathfindInProgress);
        DrawInt("Nav.PathfindNumQueued", snapshot.NavPathfindNumQueued);
        DrawBool("Nav.IsAutoLoad", snapshot.NavIsAutoLoad);
    }

    private static void DrawQuerySection(VNavmeshIpcSnapshot snapshot)
    {
        AflImGui.SectionHeader("Query.Mesh");

        if (snapshot.HasQueryOrigin)
        {
            ImGui.TextDisabled(
                $"Sample point (local player): {GameCoords.FormatDisplay(snapshot.QueryOrigin)}");
            ImGui.TextDisabled("Extents: XZ=1, Y=3");
        }
        else
        {
            ImGui.TextDisabled("Sample point: no local player");
        }

        DrawOptionalVector3("Query.Mesh.NearestPoint", snapshot.QueryNearestPoint);
        DrawBool("Query.Mesh.IsPointOnMesh", snapshot.QueryIsPointOnMesh);
        DrawOptionalVector3("Query.Mesh.NearestPointReachable", snapshot.QueryNearestPointReachable);
        DrawOptionalVector3("Query.Mesh.PointOnFloor", snapshot.QueryPointOnFloor);
        DrawOptionalVector3("Query.Mesh.FlagToPoint", snapshot.QueryFlagToPoint);
    }

    private static void DrawPathSection(VNavmeshIpcSnapshot snapshot)
    {
        AflImGui.SectionHeader("Path");

        DrawBool("Path.IsRunning", snapshot.PathIsRunning);
        DrawInt("Path.NumWaypoints", snapshot.PathNumWaypoints);
        DrawBool("Path.GetMovementAllowed", snapshot.PathGetMovementAllowed);
        DrawBool("Path.GetAlignCamera", snapshot.PathGetAlignCamera);
        DrawFloat("Path.GetTolerance", snapshot.PathGetTolerance, "F2");

        DrawWaypoints("Path.ListWaypoints", snapshot.PathListWaypoints);
    }

    private static void DrawBool(string label, IpcField<bool> field)
    {
        if (!field.Ok)
        {
            ImGui.TextDisabled($"{label}: {field.Error}");
            return;
        }

        ImGui.Text($"{label}: {(field.Value ? "true" : "false")}");
    }

    private static void DrawInt(string label, IpcField<int> field)
    {
        if (!field.Ok)
        {
            ImGui.TextDisabled($"{label}: {field.Error}");
            return;
        }

        ImGui.Text($"{label}: {field.Value}");
    }

    private static void DrawFloat(string label, IpcField<float> field, string format)
    {
        if (!field.Ok)
        {
            ImGui.TextDisabled($"{label}: {field.Error}");
            return;
        }

        ImGui.Text($"{label}: {field.Value.ToString(format, CultureInfo.InvariantCulture)}");
    }

    private static void DrawOptionalVector3(string label, IpcOptionalVector3 field)
    {
        if (!field.Ok)
        {
            ImGui.TextDisabled($"{label}: {field.Error}");
            return;
        }

        if (field.HasValue)
            ImGui.Text($"{label}: {GameCoords.FormatDisplay(field.Value)}");
        else
            ImGui.TextDisabled($"{label}: null");
    }

    private static void DrawWaypoints(string label, IpcField<List<Vector3>> field)
    {
        if (!field.Ok)
        {
            ImGui.TextDisabled($"{label}: {field.Error}");
            return;
        }

        var waypoints = field.Value;
        if (waypoints == null || waypoints.Count == 0)
        {
            ImGui.TextDisabled($"{label}: (empty)");
            return;
        }

        ImGui.Text($"{label}: {waypoints.Count} waypoint(s)");
        var childHeight = Math.Min(waypoints.Count * ImGui.GetTextLineHeightWithSpacing(), 120f);
        ImGui.BeginChild($"{label}List", new Vector2(0, childHeight));
        for (var i = 0; i < waypoints.Count; i++)
            ImGui.TextDisabled($"  [{i}] {GameCoords.FormatDisplay(waypoints[i])}");

        ImGui.EndChild();
    }
}
