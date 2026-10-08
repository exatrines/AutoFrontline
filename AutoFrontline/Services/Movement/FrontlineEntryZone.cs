using System.Numerics;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>Session spawn center and exclusion radius (not persisted).</summary>
internal static class FrontlineEntryZone
{
    public static Vector3? EntryPosition { get; private set; }

    public static void Update()
    {
        var inFrontline = FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType);

        // Player can be unavailable for a frame after zone-in; an edge-only capture misses it.
        if (inFrontline && EntryPosition == null && Player.Available && Player.Object != null)
            EntryPosition = Player.Object.Position;

        if (!inFrontline)
            EntryPosition = null;
    }

    public static bool IsWithinExclusion(Vector3 position)
    {
        if (EntryPosition is not Vector3 entry)
            return false;

        return GameCoords.IsWithinRadius(entry, position, C.SpawnExclusionRadiusMeters);
    }

    public static bool IsPlayerInExclusion()
    {
        if (!Player.Available || Player.Object == null)
            return false;

        return IsWithinExclusion(Player.Object.Position);
    }

    public static bool ShouldSkipMoveTarget(Vector3 target)
    {
        if (EntryPosition is not Vector3 _)
            return false;

        return IsWithinExclusion(target);
    }

    public static bool ShouldSkipEnemyTargeting() => IsPlayerInExclusion();

    public static bool ShouldSkipEnemyTargeting(Vector3 enemyPosition) =>
        IsPlayerInExclusion() || IsWithinExclusion(enemyPosition);

    public static float? DistanceToEntry(Vector3 position) =>
        EntryPosition is Vector3 entry ? Vector3.Distance(entry, position) : null;
}
