using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>ModelCharaId 0x1E0（アイスドトームリス）のオブジェクト検出。</summary>
internal static class IcedotomeIrisDetector
{
    public static bool HasNearby(float radiusMeters) =>
        TryGetClosest(radiusMeters, out _, out _);

    public static bool TryGetClosest(
        float radiusMeters,
        out IGameObject closest,
        out float distanceMeters)
    {
        closest = null!;
        distanceMeters = float.MaxValue;

        if (!Player.Available || Player.Object == null)
            return false;

        var selfPosition = Player.Object.Position;
        var radiusSq = radiusMeters >= float.MaxValue / 2f
            ? float.MaxValue
            : radiusMeters * radiusMeters;

        foreach (var obj in Svc.Objects)
        {
            if (obj == null || obj.Address == nint.Zero || !MatchesIcedotomeIris(obj))
                continue;

            var distanceSq = Vector3.DistanceSquared(selfPosition, obj.Position);
            if (distanceSq > radiusSq || distanceSq >= distanceMeters * distanceMeters)
                continue;

            distanceMeters = MathF.Sqrt(distanceSq);
            closest = obj;
        }

        return closest != null;
    }

    private static bool MatchesIcedotomeIris(IGameObject obj)
    {
        if (obj is ICharacter character)
            return (uint)character.ModelId == FrontlineConstants.IcedotomeIrisModelCharaId;

        return obj.DataId == FrontlineConstants.IcedotomeIrisModelCharaId;
    }
}
