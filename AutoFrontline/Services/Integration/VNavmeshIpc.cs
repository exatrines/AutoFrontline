using System.Collections.Generic;
using System.Numerics;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

internal readonly record struct IpcField<T>(T Value, string Error)
{
    public bool Ok => Error == null;

    public static IpcField<T> From(Func<T> invoke)
    {
        try
        {
            return new IpcField<T>(invoke(), null);
        }
        catch (IpcNotReadyError)
        {
            return new IpcField<T>(default, "IPC not ready");
        }
        catch (Exception ex)
        {
            return new IpcField<T>(default, ex.Message);
        }
    }
}

internal readonly record struct IpcOptionalVector3(Vector3 Value, bool HasValue, string Error)
{
    public bool Ok => Error == null;
}

internal sealed class VNavmeshIpcSnapshot
{
    public bool PluginLoaded { get; init; }
    public bool SubscribersReady { get; init; }
    public Vector3 QueryOrigin { get; init; }
    public bool HasQueryOrigin { get; init; }

    public IpcField<bool> NavIsReady { get; init; }
    public IpcField<float> NavBuildProgress { get; init; }
    public IpcField<bool> NavPathfindInProgress { get; init; }
    public IpcField<int> NavPathfindNumQueued { get; init; }
    public IpcField<bool> NavIsAutoLoad { get; init; }

    public IpcOptionalVector3 QueryNearestPoint { get; init; }
    public IpcField<bool> QueryIsPointOnMesh { get; init; }
    public IpcOptionalVector3 QueryNearestPointReachable { get; init; }
    public IpcOptionalVector3 QueryPointOnFloor { get; init; }
    public IpcOptionalVector3 QueryFlagToPoint { get; init; }

    public IpcField<bool> PathIsRunning { get; init; }
    public IpcField<int> PathNumWaypoints { get; init; }
    public IpcField<List<Vector3>> PathListWaypoints { get; init; }
    public IpcField<bool> PathGetMovementAllowed { get; init; }
    public IpcField<bool> PathGetAlignCamera { get; init; }
    public IpcField<float> PathGetTolerance { get; init; }
}

internal static class VNavmeshIpc
{
    private const string Prefix = "vnavmesh.";
    private const float QueryHalfExtentXz = 1f;
    private const float QueryHalfExtentY = 3f;

    private static bool subscribersReady;

    private static ICallGateSubscriber<bool> navIsReady;
    private static ICallGateSubscriber<float> navBuildProgress;
    private static ICallGateSubscriber<bool> navPathfindInProgress;
    private static ICallGateSubscriber<int> navPathfindNumQueued;
    private static ICallGateSubscriber<bool> navIsAutoLoad;

    private static ICallGateSubscriber<Vector3, float, float, Vector3?> queryNearestPoint;
    private static ICallGateSubscriber<Vector3, float, bool, bool> queryIsPointOnMesh;
    private static ICallGateSubscriber<Vector3, float, float, Vector3?> queryNearestPointReachable;
    private static ICallGateSubscriber<Vector3, bool, float, Vector3?> queryPointOnFloor;
    private static ICallGateSubscriber<Vector3?> queryFlagToPoint;

    private static ICallGateSubscriber<bool> pathIsRunning;
    private static ICallGateSubscriber<int> pathNumWaypoints;
    private static ICallGateSubscriber<List<Vector3>> pathListWaypoints;
    private static ICallGateSubscriber<bool> pathGetMovementAllowed;
    private static ICallGateSubscriber<bool> pathGetAlignCamera;
    private static ICallGateSubscriber<float> pathGetTolerance;

    internal static VNavmeshIpcSnapshot Refresh()
    {
        var pluginLoaded = RequiredPlugins.IsLoaded(RequiredPlugins.VNavmesh.InternalName);
        if (!pluginLoaded)
            return new VNavmeshIpcSnapshot { PluginLoaded = false };

        EnsureSubscribers();
        if (!subscribersReady)
            return new VNavmeshIpcSnapshot { PluginLoaded = true, SubscribersReady = false };

        var hasQueryOrigin = Player.Object != null;
        var queryOrigin = hasQueryOrigin ? Player.Object!.Position : default;
        return new VNavmeshIpcSnapshot
        {
            PluginLoaded = true,
            SubscribersReady = true,
            HasQueryOrigin = hasQueryOrigin,
            QueryOrigin = queryOrigin,
            NavIsReady = Read(() => navIsReady.InvokeFunc()),
            NavBuildProgress = Read(() => navBuildProgress.InvokeFunc()),
            NavPathfindInProgress = Read(() => navPathfindInProgress.InvokeFunc()),
            NavPathfindNumQueued = Read(() => navPathfindNumQueued.InvokeFunc()),
            NavIsAutoLoad = Read(() => navIsAutoLoad.InvokeFunc()),
            QueryNearestPoint = hasQueryOrigin
                ? ReadOptionalVector3(() => queryNearestPoint.InvokeFunc(queryOrigin, QueryHalfExtentXz, QueryHalfExtentY))
                : new IpcOptionalVector3(default, false, "no local player"),
            QueryIsPointOnMesh = hasQueryOrigin
                ? Read(() => queryIsPointOnMesh.InvokeFunc(queryOrigin, QueryHalfExtentY, false))
                : new IpcField<bool>(false, "no local player"),
            QueryNearestPointReachable = hasQueryOrigin
                ? ReadOptionalVector3(() => queryNearestPointReachable.InvokeFunc(queryOrigin, QueryHalfExtentXz, QueryHalfExtentY))
                : new IpcOptionalVector3(default, false, "no local player"),
            QueryPointOnFloor = hasQueryOrigin
                ? ReadOptionalVector3(() => queryPointOnFloor.InvokeFunc(queryOrigin, false, QueryHalfExtentXz))
                : new IpcOptionalVector3(default, false, "no local player"),
            QueryFlagToPoint = ReadOptionalVector3(() => queryFlagToPoint.InvokeFunc()),
            PathIsRunning = Read(() => pathIsRunning.InvokeFunc()),
            PathNumWaypoints = Read(() => pathNumWaypoints.InvokeFunc()),
            PathListWaypoints = Read(() => pathListWaypoints.InvokeFunc()),
            PathGetMovementAllowed = Read(() => pathGetMovementAllowed.InvokeFunc()),
            PathGetAlignCamera = Read(() => pathGetAlignCamera.InvokeFunc()),
            PathGetTolerance = Read(() => pathGetTolerance.InvokeFunc()),
        };
    }

    private static void EnsureSubscribers()
    {
        if (subscribersReady)
            return;

        try
        {
            navIsReady = Svc.PluginInterface.GetIpcSubscriber<bool>($"{Prefix}Nav.IsReady");
            navBuildProgress = Svc.PluginInterface.GetIpcSubscriber<float>($"{Prefix}Nav.BuildProgress");
            navPathfindInProgress = Svc.PluginInterface.GetIpcSubscriber<bool>($"{Prefix}Nav.PathfindInProgress");
            navPathfindNumQueued = Svc.PluginInterface.GetIpcSubscriber<int>($"{Prefix}Nav.PathfindNumQueued");
            navIsAutoLoad = Svc.PluginInterface.GetIpcSubscriber<bool>($"{Prefix}Nav.IsAutoLoad");

            queryNearestPoint = Svc.PluginInterface
                .GetIpcSubscriber<Vector3, float, float, Vector3?>($"{Prefix}Query.Mesh.NearestPoint");
            queryIsPointOnMesh = Svc.PluginInterface
                .GetIpcSubscriber<Vector3, float, bool, bool>($"{Prefix}Query.Mesh.IsPointOnMesh");
            queryNearestPointReachable = Svc.PluginInterface
                .GetIpcSubscriber<Vector3, float, float, Vector3?>($"{Prefix}Query.Mesh.NearestPointReachable");
            queryPointOnFloor = Svc.PluginInterface
                .GetIpcSubscriber<Vector3, bool, float, Vector3?>($"{Prefix}Query.Mesh.PointOnFloor");
            queryFlagToPoint = Svc.PluginInterface.GetIpcSubscriber<Vector3?>($"{Prefix}Query.Mesh.FlagToPoint");

            pathIsRunning = Svc.PluginInterface.GetIpcSubscriber<bool>($"{Prefix}Path.IsRunning");
            pathNumWaypoints = Svc.PluginInterface.GetIpcSubscriber<int>($"{Prefix}Path.NumWaypoints");
            pathListWaypoints = Svc.PluginInterface.GetIpcSubscriber<List<Vector3>>($"{Prefix}Path.ListWaypoints");
            pathGetMovementAllowed = Svc.PluginInterface.GetIpcSubscriber<bool>($"{Prefix}Path.GetMovementAllowed");
            pathGetAlignCamera = Svc.PluginInterface.GetIpcSubscriber<bool>($"{Prefix}Path.GetAlignCamera");
            pathGetTolerance = Svc.PluginInterface.GetIpcSubscriber<float>($"{Prefix}Path.GetTolerance");

            subscribersReady = true;
        }
        catch (Exception ex)
        {
            ex.Log();
        }
    }

    private static IpcField<T> Read<T>(Func<T> invoke) => IpcField<T>.From(invoke);

    private static IpcOptionalVector3 ReadOptionalVector3(Func<Vector3?> invoke)
    {
        try
        {
            var value = invoke();
            return new IpcOptionalVector3(value.GetValueOrDefault(), value.HasValue, null);
        }
        catch (IpcNotReadyError)
        {
            return new IpcOptionalVector3(default, false, "IPC not ready");
        }
        catch (Exception ex)
        {
            return new IpcOptionalVector3(default, false, ex.Message);
        }
    }
}
