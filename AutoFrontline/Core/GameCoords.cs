using System.Globalization;
using System.Numerics;

namespace AutoFrontline;

internal static class GameCoords
{
    public static string FormatDisplay(Vector3 position) =>
        $"{FormatComponent(position.X)}, {FormatComponent(position.Y)}, {FormatComponent(position.Z)}";

    public static string FormatCommand(Vector3 position) =>
        $"{FormatComponent(position.X)} {FormatComponent(position.Y)} {FormatComponent(position.Z)}";

    private static string FormatComponent(float value) =>
        value.ToString("F2", CultureInfo.InvariantCulture);

    public static bool AreNear(Vector3 a, Vector3 b, float thresholdMeters)
    {
        var thresholdSq = thresholdMeters * thresholdMeters;
        return Vector3.DistanceSquared(a, b) < thresholdSq;
    }

    public static bool IsWithinRadius(Vector3 a, Vector3 b, float radiusMeters) =>
        Vector3.DistanceSquared(a, b) <= radiusMeters * radiusMeters;

    public static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    public static bool AreNearHorizontal(Vector3 a, Vector3 b, float thresholdMeters)
    {
        var thresholdSq = thresholdMeters * thresholdMeters;
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return dx * dx + dy * dy <= thresholdSq;
    }

    public static bool IsWithinHorizontalRadius(Vector3 center, Vector3 point, float radiusMeters)
    {
        var radiusSq = radiusMeters * radiusMeters;
        var dx = center.X - point.X;
        var dy = center.Y - point.Y;
        return dx * dx + dy * dy <= radiusSq;
    }
}
