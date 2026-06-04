using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>ModelCharaId 0x1E0（アイスドトームリス）のオブジェクトが半径内にあるか。</summary>
internal static class IcedotomeIrisDetector
{
    public static bool HasNearby(float radiusMeters)
    {
        if (!Player.Available || Player.Object == null)
            return false;

        var selfPosition = Player.Object.Position;
        var radiusSq = radiusMeters * radiusMeters;

        foreach (var obj in Svc.Objects)
        {
            if (obj == null || obj.Address == nint.Zero)
                continue;

            if (Vector3.DistanceSquared(selfPosition, obj.Position) > radiusSq)
                continue;

            if (MatchesIcedotomeIris(obj))
                return true;
        }

        return false;
    }

    private static bool MatchesIcedotomeIris(IGameObject obj)
    {
        if (obj is ICharacter character)
            return (uint)character.ModelId == FrontlineConstants.IcedotomeIrisModelCharaId;

        return obj.DataId == FrontlineConstants.IcedotomeIrisModelCharaId;
    }
}
