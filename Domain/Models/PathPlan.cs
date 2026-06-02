using System.Drawing;

namespace RutaCarritoESP32.Domain.Models;

public sealed class PathPlan
{
    public PathPlan(IReadOnlyList<PointF> smoothedPath, IReadOnlyList<RouteSegment> segments, float distanceCm)
    {
        SmoothedPath = smoothedPath;
        Segments = segments;
        DistanceCm = distanceCm;
    }

    public IReadOnlyList<PointF> SmoothedPath { get; }
    public IReadOnlyList<RouteSegment> Segments { get; }
    public float DistanceCm { get; }
}
