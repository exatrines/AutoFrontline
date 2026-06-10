using System.Linq;
using System.Reflection;
using ECommons.Reflection;

namespace AutoFrontline.Services;

/// <summary>Rotation Solver Reborn の稼働状態（DataCenter 参照）。</summary>
internal enum RotationSolverOperatingState
{
    Unknown,
    /// <summary>RSR Auto Off（DataCenter.State == false）。</summary>
    Off,
    Manual,
    AutoBig,
    AutoManual,
    AutoOther,
    /// <summary>RSR PvP モード（DataCenter.IsPvPStateEnabled == true）。</summary>
    PvP,
}

internal readonly record struct RotationSolverDebugSnapshot(
    bool Resolved,
    string ReadSource,
    string AssemblyName,
    string AlcAssemblies,
    string DisplayState,
    bool State,
    bool IsManual,
    string TargetingType,
    bool IsPvPStateEnabled,
    RotationSolverOperatingState DerivedState)
{
    public static RotationSolverDebugSnapshot Unresolved(string reason = "") => new(
        false,
        reason,
        string.Empty,
        string.Empty,
        string.Empty,
        false,
        false,
        string.Empty,
        false,
        RotationSolverOperatingState.Unknown);
}

internal static class RotationSolverState
{
    private const string DataCenterTypeName = "RotationSolver.Basic.DataCenter";
    private const string RSCommandsTypeName = "RotationSolver.Commands.RSCommands";
    private const string DisplayStateFieldName = "_stateString";

    private const BindingFlags StaticMemberFlags =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    public static RotationSolverOperatingState Get() => GetSnapshot().DerivedState;

    public static RotationSolverDebugSnapshot GetSnapshot()
    {
        if (!RequiredPlugins.IsLoaded(RequiredPlugins.RotationSolver.InternalName))
            return RotationSolverDebugSnapshot.Unresolved("RSR not loaded");

        if (!TryResolveRotationSolverTypes(out var rsCommandsType, out var dataCenterType, out var readSource, out var alcAssemblies))
            return RotationSolverDebugSnapshot.Unresolved("RSR plugin ALC not found");

        var displayState = ReadDisplayState(rsCommandsType);
        var state = dataCenterType != null && ReadStaticBool(dataCenterType, "State");
        var isManual = dataCenterType != null && ReadStaticBool(dataCenterType, "IsManual");
        var isPvPStateEnabled = dataCenterType != null && ReadStaticBool(dataCenterType, "IsPvPStateEnabled");
        var targetingType = dataCenterType != null ? ReadTargetingType(dataCenterType) : string.Empty;
        var assemblyName = rsCommandsType?.Assembly.GetName().Name
            ?? dataCenterType?.Assembly.GetName().Name
            ?? string.Empty;

        var derivedState = !string.IsNullOrEmpty(displayState)
            ? DeriveFromDisplayState(displayState)
            : DeriveFromDataCenter(state, isManual, isPvPStateEnabled, targetingType);

        return new RotationSolverDebugSnapshot(
            true,
            readSource,
            assemblyName,
            alcAssemblies,
            displayState,
            state,
            isManual,
            targetingType,
            isPvPStateEnabled,
            derivedState);
    }

    private static bool TryResolveRotationSolverTypes(
        out Type rsCommandsType,
        out Type dataCenterType,
        out string readSource,
        out string alcAssemblies)
    {
        rsCommandsType = null;
        dataCenterType = null;
        readSource = string.Empty;
        alcAssemblies = string.Empty;

        if (DalamudReflector.TryGetDalamudPlugin(
                RequiredPlugins.RotationSolver.InternalName,
                out _,
                out var context,
                suppressErrors: true)
            && context != null)
        {
            alcAssemblies = string.Join(", ", context.Assemblies
                .Select(static assembly => assembly.GetName().Name)
                .Where(static name => !string.IsNullOrEmpty(name))
                .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase));

            foreach (var assembly in context.Assemblies)
            {
                rsCommandsType ??= assembly.GetType(RSCommandsTypeName, false);
                dataCenterType ??= assembly.GetType(DataCenterTypeName, false);
            }

            if (rsCommandsType != null || dataCenterType != null)
            {
                readSource = "plugin ALC";
                return true;
            }
        }

        alcAssemblies = string.Join(", ", AppDomain.CurrentDomain.GetAssemblies()
            .Select(static assembly => assembly.GetName().Name)
            .Where(static name => name?.StartsWith("RotationSolver", StringComparison.OrdinalIgnoreCase) == true)
            .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)!);

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = assembly.GetName().Name;
            if (name is not { } assemblyName
                || !assemblyName.StartsWith("RotationSolver", StringComparison.OrdinalIgnoreCase))
                continue;

            rsCommandsType ??= assembly.GetType(RSCommandsTypeName, false);
            dataCenterType ??= assembly.GetType(DataCenterTypeName, false);
        }

        if (rsCommandsType != null || dataCenterType != null)
        {
            readSource = "AppDomain fallback";
            return true;
        }

        return false;
    }

    private static string ReadDisplayState(Type rsCommandsType)
    {
        var field = rsCommandsType.GetField(DisplayStateFieldName, StaticMemberFlags);
        return field?.GetValue(null) as string ?? string.Empty;
    }

    private static RotationSolverOperatingState DeriveFromDisplayState(string displayState)
    {
        if (string.IsNullOrWhiteSpace(displayState)
            || displayState.Equals("Off", StringComparison.OrdinalIgnoreCase))
            return RotationSolverOperatingState.Off;

        if (displayState.Equals("Manual", StringComparison.OrdinalIgnoreCase))
            return RotationSolverOperatingState.Manual;

        if (displayState.StartsWith("PvP", StringComparison.OrdinalIgnoreCase))
            return RotationSolverOperatingState.PvP;

        if (TryParseParenthesizedMode(displayState, out var mode, out var targeting))
        {
            if (mode.Equals("Auto", StringComparison.OrdinalIgnoreCase)
                || mode.Equals("TargetOnly", StringComparison.OrdinalIgnoreCase))
            {
                if (targeting.Equals("Big", StringComparison.OrdinalIgnoreCase))
                    return RotationSolverOperatingState.AutoBig;

                if (targeting.Equals("Manual", StringComparison.OrdinalIgnoreCase))
                    return RotationSolverOperatingState.AutoManual;

                return RotationSolverOperatingState.AutoOther;
            }
        }

        if (displayState.StartsWith("Auto", StringComparison.OrdinalIgnoreCase))
            return RotationSolverOperatingState.AutoOther;

        return RotationSolverOperatingState.AutoOther;
    }

    private static bool TryParseParenthesizedMode(string displayState, out string mode, out string targeting)
    {
        mode = string.Empty;
        targeting = string.Empty;

        var open = displayState.IndexOf('(');
        var close = displayState.LastIndexOf(')');
        if (open <= 0 || close <= open)
            return false;

        mode = displayState[..open].Trim();
        targeting = displayState[(open + 1)..close].Trim();
        return mode.Length > 0 && targeting.Length > 0;
    }

    private static RotationSolverOperatingState DeriveFromDataCenter(
        bool state,
        bool isManual,
        bool isPvPStateEnabled,
        string targetingType)
    {
        if (!state)
            return RotationSolverOperatingState.Off;

        if (isManual)
            return RotationSolverOperatingState.Manual;

        if (isPvPStateEnabled)
            return RotationSolverOperatingState.PvP;

        if (targetingType.Equals("Big", StringComparison.OrdinalIgnoreCase))
            return RotationSolverOperatingState.AutoBig;

        if (targetingType.Equals("Manual", StringComparison.OrdinalIgnoreCase))
            return RotationSolverOperatingState.AutoManual;

        return RotationSolverOperatingState.AutoOther;
    }

    private static string ReadTargetingType(Type dataCenterType)
    {
        var targetingProp = dataCenterType.GetProperty("TargetingType", StaticMemberFlags);
        return targetingProp?.GetValue(null)?.ToString() ?? string.Empty;
    }

    private static bool ReadStaticBool(Type type, string propertyName)
    {
        var prop = type.GetProperty(propertyName, StaticMemberFlags);
        return prop?.GetValue(null) is bool value && value;
    }
}
