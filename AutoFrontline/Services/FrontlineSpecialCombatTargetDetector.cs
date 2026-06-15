using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>ModelCharaId 指定のフロントライン特殊戦闘オブジェクト（リス・ドローン等）の検出。</summary>
internal static class FrontlineSpecialCombatTargetDetector
{
    private static readonly (uint ModelCharaId, string DebugName)[] Targets =
    [
        (FrontlineConstants.IcedotomeIrisModelCharaId, "Icedotome Iris"),
        (FrontlineConstants.AssaultDroneModelCharaId, "Assault Drone"),
        (FrontlineConstants.AssaultSystemModelCharaId, "Assault System"),
    ];

    public static bool HasNearby(float radiusMeters) =>
        TryGetClosest(radiusMeters, out _, out _, out _);

    public static bool TryGetClosest(
        float radiusMeters,
        out IGameObject closest,
        out float distanceMeters,
        out string debugName)
    {
        closest = null!;
        distanceMeters = float.MaxValue;
        debugName = string.Empty;

        if (!Player.Available || Player.Object == null)
            return false;

        var selfPosition = Player.Object.Position;
        var radiusSq = radiusMeters >= float.MaxValue / 2f
            ? float.MaxValue
            : radiusMeters * radiusMeters;

        foreach (var obj in Svc.Objects)
        {
            if (obj == null || obj.Address == nint.Zero || !TryMatch(obj, out var name))
                continue;

            var distanceSq = Vector3.DistanceSquared(selfPosition, obj.Position);
            if (distanceSq > radiusSq || distanceSq >= distanceMeters * distanceMeters)
                continue;

            distanceMeters = MathF.Sqrt(distanceSq);
            closest = obj;
            debugName = name;
        }

        return closest != null;
    }

    public static bool Matches(IGameObject obj) => TryMatch(obj, out _);

    private static bool TryMatch(IGameObject obj, out string debugName)
    {
        foreach (var (modelCharaId, name) in Targets)
        {
            if (!MatchesModelCharaId(obj, modelCharaId))
                continue;

            debugName = name;
            return true;
        }

        debugName = string.Empty;
        return false;
    }

    private static bool MatchesModelCharaId(IGameObject obj, uint modelCharaId)
    {
        if (obj is ICharacter character)
            return (uint)character.ModelId == modelCharaId;

        return obj.DataId == modelCharaId;
    }
}
