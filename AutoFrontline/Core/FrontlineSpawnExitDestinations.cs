using System.Collections.Generic;
using System.Numerics;

namespace AutoFrontline;

/// <summary>Per-field fixed moveto while inside spawn exclusion. Unset fields use group movement.</summary>
internal static class FrontlineSpawnExitDestinations
{
    private static readonly Dictionary<uint, Vector3> FixedExitByTerritory = new()
    {
        [431] = new Vector3(0, 10, 50), // Seal Rock
        [554] = new Vector3(0, 0, 0), // the Fields of Glory
        [888] = new Vector3(0, 0, 1), // Onsal Hakair
        [1273] = new Vector3(5, 5, 30), // Borderland Ruins
        [1313] = new Vector3(-10, -15, 0), // Training
    };

    public static bool TryGetFixedExitDestination(uint territoryId, out Vector3 destination) =>
        FixedExitByTerritory.TryGetValue(territoryId, out destination);
}
