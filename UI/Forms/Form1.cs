using System.Drawing;
using System.Drawing.Drawing2D;
using RutaCarritoESP32.Application.Geometry;
using RutaCarritoESP32.Domain.Models;
using RutaCarritoESP32.Infrastructure.Serial;
using RutaCarritoESP32.UI.Theming;

namespace RutaCarritoESP32;

public partial class Form1 : Form
{
    private const float DrawInputThresholdPx = 2.5f;

    // NOTA DE CALIBRACIÓN: Cambia este 10f si el carrito recorre más o menos distancia en la vida real.
    // Menos de 10 = El carrito recorrerá menos distancia. Más de 10 = El carrito recorrerá más distancia.
    private const float PlannerPixelsPerCm = 10f;
    // Tamaño de la cuadricula en píxeles (5 cm = 5 * PlannerPixelsPerCm px => 50px)
    private const int GridSize = 50;
    private float _zoom = 1.0f; // 1.0 = 100%

    private readonly PathPlanner _pathPlanner = new(PlannerPixelsPerCm);
    private readonly WifiRouteClient _wifiClient = new();
    private readonly System.Windows.Forms.Timer _telemetryTimer = new() { Interval = 150 };
    private readonly List<PointF> _rawPath = new();

    private PathPlan _plan = new(Array.Empty<PointF>(), Array.Empty<RouteSegment>(), 0f);
    private bool _isDrawing;
    private bool _hasCartTelemetry;
    private bool _manualMode = false;
    private readonly System.Windows.Forms.Timer _manualBlinkTimer = new() { Interval = 300 };

    public Form1()
    {
        InitializeComponent();
        DoubleBuffered = true;
        picLienzo.Paint += picLienzo_Paint;
        picLienzo.MouseEnter += picLienzo_MouseEnter;
        picLienzo.MouseWheel += picLienzo_MouseWheel;
        FormClosing += Form1_FormClosing;
        _telemetryTimer.Tick += TelemetryTimer_Tick;
        // Asociar handlers para botones de zoom (si existen en el diseñador)
        try
        {
            btnZoomIn.Click += btnZoomIn_Click;
            btnZoomOut.Click += btnZoomOut_Click;
        }
        catch { }

        // Permitir capturar la rueda del ratón a nivel de formulario
        this.MouseWheel += Form1_MouseWheel;
        // Teclas para control manual
        this.KeyPreview = true;
        this.KeyDown += Form1_KeyDown;
        InitializeManualIndicators();
    }

    private void InitializeManualIndicators()
    {
        _manualBlinkTimer.Tick += (s, e) =>
        {
            lblArrowUp.BackColor = Color.Transparent;
            lblArrowDown.BackColor = Color.Transparent;
            lblArrowLeft.BackColor = Color.Transparent;
            lblArrowRight.BackColor = Color.Transparent;
            _manualBlinkTimer.Stop();
        };
    }

    private void btnModo_Click(object? sender, EventArgs e)
    {
        _manualMode = !_manualMode;

        // Cambios visuales en la interfaz
        btnModo.Text = _manualMode ? "Modo: Manual" : "Modo: Autónomo";
        btnModo.BackColor = _manualMode ? Color.FromArgb(200, 80, 80) : Color.FromArgb(70, 70, 70);
        SetStatus(_manualMode ? "Modo: Manual" : "Modo: Autónomo", _manualMode ? Color.Orange : FluentPalette.TextSecondary);

        if (lblManualStatus != null)
            lblManualStatus.Text = _manualMode ? "Manual: Activo" : "Manual: Inactivo";

        // NUEVO: Enviar el comando al ESP32 para que actualice su pantalla LCD
        if (_wifiClient != null && _wifiClient.IsConnected)
        {
            try
            {
                // Enviamos el mensaje tal cual lo espera el loop de tu Arduino
                _wifiClient.SendManualCommand(_manualMode ? "MODE_MANUAL" : "MODE_AUTO");
            }
            catch (Exception ex)
            {
                SetStatus("Error al cambiar modo: " + ex.Message, Color.Firebrick);
            }
        }
    }

    private void Form1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (!_manualMode) return;

        try
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    _wifiClient.SendManualCommand("FORWARD");
                    HighlightArrow(lblArrowUp);
                    break;
                case Keys.Down:
                    _wifiClient.SendManualCommand("BACK");
                    HighlightArrow(lblArrowDown);
                    break;
                case Keys.Left:
                    _wifiClient.SendManualCommand("LEFT");
                    HighlightArrow(lblArrowLeft);
                    break;
                case Keys.Right:
                    _wifiClient.SendManualCommand("RIGHT");
                    HighlightArrow(lblArrowRight);
                    break;
                case Keys.Space:
                    _wifiClient.SendManualCommand("STOP");
                    // Indicar stop con parpadeo de los cuatro
                    HighlightArrow(lblArrowUp);
                    HighlightArrow(lblArrowDown);
                    HighlightArrow(lblArrowLeft);
                    HighlightArrow(lblArrowRight);
                    break;
            }
        }
        catch (Exception ex)
        {
            // No interrumpir funcionamiento normal
            SetStatus($"Error comando manual: {ex.Message}", Color.Firebrick);
        }

    }

    private void HighlightArrow(Label lbl)
    {
        if (lbl == null) return;
        lbl.BackColor = Color.Yellow;
        lbl.Refresh();
        _manualBlinkTimer.Stop();
        _manualBlinkTimer.Start();
    }

    private void picLienzo_MouseEnter(object? sender, EventArgs e)
    {
        picLienzo.Focus();
    }

    private void Form1_MouseWheel(object? sender, MouseEventArgs e)
    {
        // Forward wheel events to picLienzo so touchpad/trackpad zoom works
        picLienzo_MouseWheel(picLienzo, e);
    }

    private void picLienzo_MouseWheel(object? sender, MouseEventArgs e)
    {
        if ((Control.ModifierKeys & Keys.Control) == 0)
        {
            return; // Requerir Ctrl + rueda para zoom
        }

        float delta = e.Delta > 0 ? 1.1f : 1f / 1.1f;
        SetZoom(_zoom * delta);
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        ApplyFluentTheme();
        LoadIpAddresses();
        UpdateDistance();
        SetStatus("Estado: Listo para dibujar", FluentPalette.TextSecondary);
    }

    private void picLienzo_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;

        _isDrawing = true;
        PointF snapped = SnapToGrid(e.Location);

        // Si el trazo nuevo empieza en el final del anterior → continuar (útil tras zoom)
        // Si empieza en cualquier otro punto → reiniciar desde cero
        if (_rawPath.Count > 0 && Distance(snapped, _rawPath[^1]) < GridSize * 0.5f)
        {
            // Punto de inicio = final del trazo anterior: continuar sin borrar
        }
        else
        {
            _rawPath.Clear();
            _rawPath.Add(snapped);
        }
        RebuildPlan();
    }

    private void picLienzo_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDrawing) return;
        // Ajustar a rejilla
        PointF gridPoint = SnapToGrid(e.Location);

        // Forzar ortogonalidad respecto al último punto (sin diagonales)
        PointF point = _rawPath.Count > 0 ? SnapTo90(gridPoint, _rawPath[^1]) : gridPoint;

        if (_rawPath.Count > 0 && Distance(_rawPath[^1], point) < DrawInputThresholdPx)
        {
            return;
        }

        _rawPath.Add(point);
        RebuildPlan();
    }

    private void picLienzo_MouseUp(object sender, MouseEventArgs e)
    {
        if (!_isDrawing) return;

        _isDrawing = false;
        RebuildPlan();
        SetStatus("Estado: Trayectoria lista", FluentPalette.Success);
    }

    private void btnConector_Click(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cmbPuertos.Text))
            {
                MessageBox.Show("Introduce una dirección IP válida para continuar.", "IP requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _wifiClient.Connect(cmbPuertos.Text);
            _telemetryTimer.Start();
            SetStatus("Estado: Wi-Fi Conectado", FluentPalette.Success);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo conectar al carrito en la IP seleccionada.\n\nDetalle: {ex.Message}", "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Estado: Error de conexión", Color.Firebrick);
        }
    }

    private void btnEnviar_Click(object sender, EventArgs e)
    {
        if (!_wifiClient.IsConnected)
        {
            MessageBox.Show("Primero conecta el Wi-Fi del carrito.", "Conexión requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_plan.Segments.Count == 0)
        {
            MessageBox.Show("Dibuja una trayectoria válida antes de enviarla.", "Ruta requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            _wifiClient.SendRoute(_plan.Segments);
            SetStatus($"Estado: Ruta enviada ({_plan.Segments.Count} segmentos)", FluentPalette.AccentPrimary);
            txtLogDetallado.Text = BuildTelemetryText(
                ">>> ENVIADO CON EXITO VIA WI-FI <<<",
                $">>> IP: {cmbPuertos.Text}:8080",
                ">>> ESPERANDO EJECUCION DEL CARRITO...");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo enviar la ruta.\n\nDetalle: {ex.Message}", "Error de envío", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Estado: Error al enviar", Color.Firebrick);
        }
    }

    private void btnLimpiar_Click(object sender, EventArgs e)
    {
        _rawPath.Clear();
        RebuildPlan();
        SetStatus("Estado: Lienzo limpio", FluentPalette.TextSecondary);
    }

    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _telemetryTimer.Stop();
        _wifiClient.Dispose();
    }

    private void TelemetryTimer_Tick(object? sender, EventArgs e)
    {
        IReadOnlyList<string> lines = _wifiClient.ReadTelemetryLines();
        if (lines.Count == 0)
        {
            return;
        }

        AppendCartTelemetry(lines);
    }

    private void picLienzo_Paint(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.Clear(picLienzo.BackColor);
        e.Graphics.ScaleTransform(_zoom, _zoom);

        // Dibujar cuadricula
        using Pen gridPen = new(Color.FromArgb(220, 220, 220), 1f / _zoom);
        int scaledWidth = (int)(picLienzo.Width / _zoom);
        int scaledHeight = (int)(picLienzo.Height / _zoom);
        for (int x = 0; x < scaledWidth; x += GridSize)
        {
            e.Graphics.DrawLine(gridPen, x, 0, x, scaledHeight);
        }
        for (int y = 0; y < scaledHeight; y += GridSize)
        {
            e.Graphics.DrawLine(gridPen, 0, y, scaledWidth, y);
        }

        if (_plan.SmoothedPath.Count > 1)
        {
            using Pen pathPen = new(FluentPalette.AccentPrimary, 3f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            e.Graphics.DrawLines(pathPen, _plan.SmoothedPath.Select(p => new Point((int)(p.X), (int)(p.Y))).ToArray());
        }

        if (_plan.SmoothedPath.Count > 0)
        {
            Point start = Point.Round(_plan.SmoothedPath.First());
            Point end = Point.Round(_plan.SmoothedPath.Last());

            using Brush startBrush = new SolidBrush(FluentPalette.Success);
            using Brush endBrush = new SolidBrush(Color.Firebrick);
            e.Graphics.FillEllipse(startBrush, start.X - 4, start.Y - 4, 8, 8);
            e.Graphics.FillEllipse(endBrush, end.X - 4, end.Y - 4, 8, 8);
        }
    }

    private void ApplyFluentTheme()
    {
        // Tipografía general
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        Text = "Ruta Carrito ESP32";

        // Paleta principal más suave
        BackColor = Color.FromArgb(245, 247, 250);
        panelSidebar.BackColor = Color.FromArgb(250, 250, 250);
        panelHeader.BackColor = Color.White;
        panelContenido.BackColor = Color.FromArgb(245, 247, 250);

        // Lienzo
        picLienzo.BackColor = Color.WhiteSmoke;
        picLienzo.Cursor = Cursors.Cross;
        picLienzo.BorderStyle = BorderStyle.FixedSingle;

        // Títulos
        lblTitulo.ForeColor = Color.FromArgb(34, 34, 34);
        lblTitulo.Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold, GraphicsUnit.Point);
        lblSubtitulo.ForeColor = Color.FromArgb(110, 110, 110);
        lblSubtitulo.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

        lblPuertos.Text = "Dirección IP del Carrito";
        lblPuertos.ForeColor = Color.FromArgb(90, 90, 90);
        lblPanelTitulo.ForeColor = Color.FromArgb(34, 34, 34);
        lblPanelTitulo.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);
        lblDistancia.ForeColor = Color.FromArgb(60, 60, 60);

        // ComboBox estilo
        cmbPuertos.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPuertos.FlatStyle = FlatStyle.Flat;
        cmbPuertos.BackColor = Color.White;
        cmbPuertos.ForeColor = Color.FromArgb(34, 34, 34);

        // Botones principales
        StyleButton(btnConector, Color.FromArgb(28, 120, 208));
        btnConector.Text = "Conectar por Wi-Fi";
        StyleButton(btnEnviar, Color.FromArgb(36, 150, 75));
        StyleButton(btnLimpiar, Color.FromArgb(110, 110, 110));

        // Botones de zoom y modo
        try
        {
            StyleButton(btnZoomIn, Color.FromArgb(45, 125, 220));
            StyleButton(btnZoomOut, Color.FromArgb(45, 125, 220));
            StyleButton(btnModo, Color.FromArgb(70, 70, 70));
        }
        catch { }

        // Consola de telemetría: aspecto terminal
        txtLogDetallado.BackColor = Color.FromArgb(20, 20, 20);
        txtLogDetallado.ForeColor = Color.FromArgb(0, 230, 120);
        txtLogDetallado.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
        txtLogDetallado.BorderStyle = BorderStyle.FixedSingle;
    }

    private void LoadIpAddresses()
    {
        cmbPuertos.Items.Clear();
        cmbPuertos.Items.Add("192.168.4.1");
        cmbPuertos.Items.Add("192.168.1.100");
        cmbPuertos.Items.Add("127.0.0.1");

        if (cmbPuertos.Items.Count > 0)
        {
            cmbPuertos.SelectedIndex = 0;
        }
    }

    private void RebuildPlan()
    {
        _plan = _pathPlanner.Build(_rawPath);
        UpdateDistance();
        picLienzo.Invalidate();
        ActualizarConsolaTelemetria();
    }

    private void ActualizarConsolaTelemetria()
    {
        txtLogDetallado.Text = BuildTelemetryText();
        _hasCartTelemetry = false;
    }

    private string BuildTelemetryText(params string[] extraLines)
    {
        System.Text.StringBuilder sb = new();
        sb.AppendLine("=== TELEMETRIA DE PLANIFICACION ===");
        sb.AppendLine("Modo: polilinea de lineas rectas");
        sb.AppendLine($"Escala: {PlannerPixelsPerCm:F2} px = 1.00 cm");
        sb.AppendLine($"Puntos capturados: {_rawPath.Count}");
        sb.AppendLine($"Vertices lineales: {_plan.SmoothedPath.Count}");
        sb.AppendLine($"Distancia total: {_plan.DistanceCm:F2} cm");
        sb.AppendLine($"Segmentos rectos: {_plan.Segments.Count}");
        sb.AppendLine("--------------------------------");
        sb.AppendLine("VERTICES:");

        for (int i = 0; i < _plan.SmoothedPath.Count; i++)
        {
            PointF point = _plan.SmoothedPath[i];
            sb.AppendLine($"V{i + 1:D2}: X={point.X,7:F1}px | Y={point.Y,7:F1}px");
        }

        sb.AppendLine("--------------------------------");
        sb.AppendLine("EJECUCION ESTIMADA DEL CARRITO:");

        float headingDeg = 0f;
        for (int i = 0; i < _plan.Segments.Count; i++)
        {
            RouteSegment segment = _plan.Segments[i];
            headingDeg = NormalizeAngle(headingDeg + segment.TurnDeg);

            PointF start = i < _plan.SmoothedPath.Count ? _plan.SmoothedPath[i] : PointF.Empty;
            PointF end = i + 1 < _plan.SmoothedPath.Count ? _plan.SmoothedPath[i + 1] : start;
            float distancePx = Distance(start, end);

            sb.AppendLine($"Seg {i + 1:D2}: Girar {segment.TurnDeg:+000;-000;000}° | Avanzar {segment.DistanceCm:D3}cm | Rumbo {headingDeg:+000;-000;000}°");
            sb.AppendLine($"         De ({start.X:F0}, {start.Y:F0}) a ({end.X:F0}, {end.Y:F0}) | {distancePx:F1}px");
        }

        sb.AppendLine("--------------------------------");
        sb.AppendLine("TRAMA SERIALIZADA:");
        sb.AppendLine(RouteCommandSerializer.Serialize(_plan.Segments));

        if (extraLines.Length > 0)
        {
            sb.AppendLine("--------------------------------");
            for (int i = 0; i < extraLines.Length; i++)
            {
                sb.AppendLine(extraLines[i]);
            }
        }

        sb.AppendLine("================================");
        return sb.ToString();
    }

    private void AppendCartTelemetry(IReadOnlyList<string> lines)
    {
        if (!_hasCartTelemetry)
        {
            txtLogDetallado.AppendText(Environment.NewLine);
            txtLogDetallado.AppendText("--------------------------------" + Environment.NewLine);
            txtLogDetallado.AppendText("TELEMETRIA DEL CARRITO:" + Environment.NewLine);
            _hasCartTelemetry = true;
        }

        for (int i = 0; i < lines.Count; i++)
        {
            txtLogDetallado.AppendText(lines[i] + Environment.NewLine);
        }

        txtLogDetallado.SelectionStart = txtLogDetallado.TextLength;
        txtLogDetallado.ScrollToCaret();
    }

    private void UpdateDistance()
    {
        lblDistancia.Text = $"Distancia: {_plan.DistanceCm:F1} cm";
    }

    private void SetStatus(string text, Color color)
    {
        lblEstado.Text = text;
        lblEstado.ForeColor = color;
    }

    private static void StyleButton(Button button, Color color)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = color;
        button.ForeColor = Color.White;
        button.Cursor = Cursors.Hand;
        button.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
    }

    private static float Distance(PointF a, PointF b)
    {
        float dx = b.X - a.X;
        float dy = b.Y - a.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    private PointF SnapToGrid(PointF p)
    {
        // Ajustar considerando el zoom (p está en coordenadas del control)
        float zx = p.X / MathF.Max(0.0001f, _zoom);
        float zy = p.Y / MathF.Max(0.0001f, _zoom);
        float x = MathF.Round(zx / GridSize) * GridSize;
        float y = MathF.Round(zy / GridSize) * GridSize;
        return new PointF(x, y);
    }

    private void btnZoomIn_Click(object? sender, EventArgs e)
    {
        SetZoom(_zoom * 1.25f);
    }

    private void btnZoomOut_Click(object? sender, EventArgs e)
    {
        SetZoom(_zoom / 1.25f);
    }

    private void SetZoom(float newZoom)
    {
        _zoom = MathF.Max(0.2f, MathF.Min(4.0f, newZoom));
        picLienzo.Invalidate();
    }

    private static PointF SnapTo90(PointF candidate, PointF reference)
    {
        float dx = MathF.Abs(candidate.X - reference.X);
        float dy = MathF.Abs(candidate.Y - reference.Y);

        if (dx >= dy)
        {
            // Mantener Y del referencia, ajustar X del candidato
            return new PointF(candidate.X, reference.Y);
        }

        // Mantener X del referencia, ajustar Y del candidato
        return new PointF(reference.X, candidate.Y);
    }
}