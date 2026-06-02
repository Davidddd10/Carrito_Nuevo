using System.Drawing;
using RutaCarritoESP32.Domain.Models;

namespace RutaCarritoESP32.Application.Geometry;

public sealed class PathPlanner
{
    private readonly float _pixelsPerCm;
    private readonly float _minPointSpacingPx;
    private readonly float _simplificationTolerancePx;
    private readonly float _cornerAngleToleranceDeg;
    private readonly float _minSegmentLengthPx;
    private readonly int _maxSegments;

    public PathPlanner(
        float pixelsPerCm = 10f,
        float minPointSpacingPx = 4f,
        float simplificationTolerancePx = 8f,
        float cornerAngleToleranceDeg = 14f,
        float minSegmentLengthPx = 10f,
        int maxSegments = 95)
    {
        _pixelsPerCm = pixelsPerCm;
        _minPointSpacingPx = minPointSpacingPx;
        _simplificationTolerancePx = simplificationTolerancePx;
        _cornerAngleToleranceDeg = cornerAngleToleranceDeg;
        _minSegmentLengthPx = minSegmentLengthPx;
        _maxSegments = maxSegments;
    }

    public PathPlan Build(IReadOnlyList<PointF> rawPath)
    {
        if (rawPath.Count == 0)
        {
            return new PathPlan(Array.Empty<PointF>(), Array.Empty<RouteSegment>(), 0f);
        }

        List<PointF> linearPath = BuildLinearPath(rawPath);
        List<RouteSegment> segments = BuildSegments(linearPath);
        float distanceCm = ComputePathLength(linearPath) / _pixelsPerCm;
        return new PathPlan(linearPath, segments, distanceCm);
    }

    private List<PointF> BuildLinearPath(IReadOnlyList<PointF> input)
    {
        if (input.Count < 2) return input.ToList();

        List<PointF> filtered = FilterBySpacing(input, _minPointSpacingPx);
        if (filtered.Count < 2) return filtered;

        List<PointF> simplified = SimplifyPolyline(filtered, _simplificationTolerancePx);
        return RemoveNearCollinearCorners(simplified, _cornerAngleToleranceDeg, _minSegmentLengthPx);
    }

    private List<RouteSegment> BuildSegments(IReadOnlyList<PointF> points)
    {
        if (points.Count < 2) return new List<RouteSegment>();

        List<(float distanceCm, float turnDeg)> floatSegments = new();
        float headingDeg = Heading(points[0], points[1]);

        for (int i = 1; i < points.Count; i++)
        {
            PointF p1 = points[i - 1];
            PointF p2 = points[i];

            float distancePx = Distance(p1, p2);
            if (distancePx < _minSegmentLengthPx * 0.5f) continue;

            float distanceCm = distancePx / _pixelsPerCm;
            float targetHeadingDeg = Heading(p1, p2);
            float turnDeg = i == 1 ? 0f : NormalizeAngle(targetHeadingDeg - headingDeg);
            headingDeg = targetHeadingDeg;

            floatSegments.Add((distanceCm, turnDeg));
        }

        return Quantize(floatSegments, _maxSegments);
    }

    private static List<PointF> FilterBySpacing(IReadOnlyList<PointF> points, float minSpacing)
    {
        if (points.Count == 0) return new List<PointF>();

        List<PointF> result = new() { points[0] };
        for (int i = 0; i < points.Count; i++)
        {
            PointF current = points[i];
            if (Distance(result[^1], current) >= minSpacing)
            {
                result.Add(current);
            }
        }

        if (Distance(result[^1], points[^1]) > 0.01f)
        {
            result.Add(points[^1]);
        }

        return result;
    }

    private static List<PointF> SimplifyPolyline(IReadOnlyList<PointF> points, float tolerance)
    {
        if (points.Count < 2) return points.ToList();

        bool[] keep = new bool[points.Count];
        keep[0] = true;
        keep[^1] = true;

        SimplifySection(points, 0, points.Count - 1, tolerance, keep);

        List<PointF> result = new();
        for (int i = 0; i < points.Count; i++)
        {
            if (keep[i])
            {
                result.Add(points[i]);
            }
        }

        return result;
    }

    private static void SimplifySection(IReadOnlyList<PointF> points, int start, int end, float tolerance, bool[] keep)
    {
        if (end - start < 2) return;

        float maxDistance = 0f;
        int index = -1;

        for (int i = start + 1; i < end; i++)
        {
            float distance = DistanceToSegment(points[i], points[start], points[end]);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                index = i;
            }
        }

        if (index != -1 && maxDistance > tolerance)
        {
            keep[index] = true;
            SimplifySection(points, start, index, tolerance, keep);
            SimplifySection(points, index, end, tolerance, keep);
        }
    }

    private static List<PointF> RemoveNearCollinearCorners(IReadOnlyList<PointF> points, float angleToleranceDeg, float minSegmentLengthPx)
    {
        if (points.Count < 3) return points.ToList();

        List<PointF> result = new() { points[0] };

        for (int i = 1; i < points.Count - 1; i++)
        {
            PointF previous = result[^1];
            PointF current = points[i];
            PointF next = points[i + 1];

            float previousLength = Distance(previous, current);
            float nextLength = Distance(current, next);
            if (previousLength < minSegmentLengthPx || nextLength < minSegmentLengthPx)
            {
                continue;
            }

            float incomingHeading = Heading(previous, current);
            float outgoingHeading = Heading(current, next);
            float turn = MathF.Abs(NormalizeAngle(outgoingHeading - incomingHeading));

            if (turn >= angleToleranceDeg)
            {
                result.Add(current);
            }
        }

        if (Distance(result[^1], points[^1]) > 0.01f)
        {
            result.Add(points[^1]);
        }

        return result;
    }

    private static List<RouteSegment> Quantize(IReadOnlyList<(float distanceCm, float turnDeg)> input, int maxSegments)
    {
        List<RouteSegment> quantized = new();
        float distanceCarry = 0f;
        float turnCarry = 0f;

        for (int i = 0; i < input.Count; i++)
        {
            float distanceTotal = distanceCarry + input[i].distanceCm;
            float turnTotal = turnCarry + input[i].turnDeg;

            int distanceInt = Math.Max(0, (int)MathF.Round(distanceTotal));
            int turnInt = (int)MathF.Round(turnTotal);

            distanceCarry = distanceTotal - distanceInt;
            turnCarry = turnTotal - turnInt;

            if (distanceInt == 0 && turnInt == 0) continue;
            quantized.Add(new RouteSegment(distanceInt, turnInt));

            if (quantized.Count >= maxSegments) break;
        }

        if (quantized.Count == 0 && (distanceCarry > 0.5f || MathF.Abs(turnCarry) >= 1f))
        {
            quantized.Add(new RouteSegment(Math.Max(0, (int)MathF.Round(distanceCarry)), (int)MathF.Round(turnCarry)));
        }

        return CompactSegments(quantized);
    }

    private static List<RouteSegment> CompactSegments(IReadOnlyList<RouteSegment> input)
    {
        if (input.Count == 0) return new List<RouteSegment>();

        List<RouteSegment> output = new() { input[0] };

        for (int i = 1; i < input.Count; i++)
        {
            RouteSegment next = input[i];

            RouteSegment current = output[^1];
            if (next.TurnDeg == 0)
            {
                output[^1] = new RouteSegment(current.DistanceCm + next.DistanceCm, current.TurnDeg);
                continue;
            }

            output.Add(next);
        }

        return output;
    }

    private static float ComputePathLength(IReadOnlyList<PointF> points)
    {
        if (points.Count < 2) return 0f;

        float total = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            total += Distance(points[i - 1], points[i]);
        }

        return total;
    }

    private static float Distance(PointF a, PointF b)
    {
        float dx = b.X - a.X;
        float dy = b.Y - a.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    private static float DistanceToSegment(PointF point, PointF start, PointF end)
    {
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;

        if (MathF.Abs(dx) < 0.001f && MathF.Abs(dy) < 0.001f)
        {
            return Distance(point, start);
        }

        float t = ((point.X - start.X) * dx + (point.Y - start.Y) * dy) / (dx * dx + dy * dy);
        t = Math.Clamp(t, 0f, 1f);

        PointF projection = new(start.X + dx * t, start.Y + dy * t);
        return Distance(point, projection);
    }

    private static float Heading(PointF start, PointF end)
    {
        return MathF.Atan2(end.Y - start.Y, end.X - start.X) * 180f / MathF.PI;
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}
