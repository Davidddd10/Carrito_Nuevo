using System.Net.Sockets;
using System.Text;
using RutaCarritoESP32.Domain.Models;

namespace RutaCarritoESP32.Infrastructure.Serial;

public sealed class WifiRouteClient : IDisposable
{
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private readonly StringBuilder _incomingBuffer = new();

    public bool IsConnected => _tcpClient is { Connected: true };

    public void Connect(string ipAddress, int port = 8080)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            throw new ArgumentException("La dirección IP es obligatoria.", nameof(ipAddress));
        }
        Disconnect();

        _tcpClient = new TcpClient();
        // Conexión con un timeout corto de 3 segundos para no congelar la UI
        var result = _tcpClient.BeginConnect(ipAddress, port, null, null);
        var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

        if (!success)
        {
            _tcpClient.Close();
            throw new TimeoutException("Tiempo de espera agotado buscando al carrito en la red Wi-Fi.");
        }

        _tcpClient.EndConnect(result);
        _stream = _tcpClient.GetStream();
        _stream.ReadTimeout = 100;
        _incomingBuffer.Clear();
    }

    public void SendRoute(IReadOnlyList<RouteSegment> segments)
    {
        if (!IsConnected || _stream == null)
        {
            throw new InvalidOperationException("No hay conexión Wi-Fi activa con el carrito.");
        }

        if (segments.Count == 0)
        {
            throw new InvalidOperationException("No hay segmentos válidos para enviar.");
        }

        string frame = RouteCommandSerializer.Serialize(segments) + "\n";
        byte[] data = Encoding.UTF8.GetBytes(frame);
        _stream.Write(data, 0, data.Length);
        _stream.Flush();
    }

    public void SendManualCommand(string command)
    {
        if (!IsConnected || _stream == null)
        {
            throw new InvalidOperationException("No hay conexión Wi-Fi activa con el carrito.");
        }

        string frame = command + "\n";
        byte[] data = Encoding.UTF8.GetBytes(frame);
        _stream.Write(data, 0, data.Length);
        _stream.Flush();
    }

    public IReadOnlyList<string> ReadTelemetryLines()
    {
        if (!IsConnected || _stream == null)
        {
            return Array.Empty<string>();
        }

        List<string> lines = new();
        byte[] buffer = new byte[512];

        while (_stream.DataAvailable)
        {
            int bytesRead = _stream.Read(buffer, 0, buffer.Length);
            if (bytesRead <= 0)
            {
                break;
            }

            _incomingBuffer.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
        }

        while (true)
        {
            string pending = _incomingBuffer.ToString();
            int newlineIndex = pending.IndexOf('\n');
            if (newlineIndex < 0)
            {
                break;
            }

            string line = pending[..newlineIndex].TrimEnd('\r');
            _incomingBuffer.Remove(0, newlineIndex + 1);

            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.Add(line);
            }
        }

        return lines;
    }

    public void Disconnect()
    {
        _stream?.Close();
        _stream?.Dispose();
        _stream = null;

        _tcpClient?.Close();
        _tcpClient?.Dispose();
        _tcpClient = null;
        _incomingBuffer.Clear();
    }

    public void Dispose()
    {
        Disconnect();
    }
}
