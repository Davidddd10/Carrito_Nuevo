using System.Text;
using RutaCarritoESP32.Domain.Models;

namespace RutaCarritoESP32.Infrastructure.Serial;

public static class RouteCommandSerializer
{
    public static string Serialize(IReadOnlyList<RouteSegment> segments)
    {
        StringBuilder command = new();
        command.Append("START;");

        for (int i = 0; i < segments.Count; i++)
        {
            RouteSegment segment = segments[i];
            command.Append($"D:{segment.DistanceCm},A:{segment.TurnDeg};");
        }

        command.Append("END");
        return command.ToString();
    }
}
