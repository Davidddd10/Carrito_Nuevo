namespace RutaCarritoESP32
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelSidebar = new Panel();
            lblPanelTitulo = new Label();
            lblPuertos = new Label();
            cmbPuertos = new ComboBox();
            btnConector = new Button();
            btnEnviar = new Button();
            btnLimpiar = new Button();
            btnZoomIn = new Button();
            btnZoomOut = new Button();
            btnModo = new Button();
            lblManualStatus = new Label();
            lblArrowUp = new Label();
            lblArrowLeft = new Label();
            lblArrowDown = new Label();
            lblArrowRight = new Label();
            lblDistancia = new Label();
            lblEstado = new Label();
            txtLogDetallado = new TextBox();
            panelHeader = new Panel();
            lblSubtitulo = new Label();
            lblTitulo = new Label();
            panelContenido = new Panel();
            picLienzo = new PictureBox();
            panelSidebar.SuspendLayout();
            panelHeader.SuspendLayout();
            panelContenido.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLienzo).BeginInit();
            SuspendLayout();
            // 
            // panelSidebar
            // 
            panelSidebar.BackColor = Color.White;
            panelSidebar.Controls.Add(lblPanelTitulo);
            panelSidebar.Controls.Add(lblPuertos);
            panelSidebar.Controls.Add(cmbPuertos);
            panelSidebar.Controls.Add(btnConector);
            panelSidebar.Controls.Add(btnEnviar);
            panelSidebar.Controls.Add(btnLimpiar);
            panelSidebar.Controls.Add(btnZoomIn);
            panelSidebar.Controls.Add(btnZoomOut);
            panelSidebar.Controls.Add(btnModo);
            panelSidebar.Controls.Add(lblManualStatus);
            panelSidebar.Controls.Add(lblArrowUp);
            panelSidebar.Controls.Add(lblArrowLeft);
            panelSidebar.Controls.Add(lblArrowDown);
            panelSidebar.Controls.Add(lblArrowRight);
            panelSidebar.Controls.Add(lblDistancia);
            panelSidebar.Controls.Add(lblEstado);
            panelSidebar.Controls.Add(txtLogDetallado);
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.Location = new Point(0, 0);
            panelSidebar.Name = "panelSidebar";
            panelSidebar.Size = new Size(320, 760);
            panelSidebar.TabIndex = 0;
            // 
            // lblPanelTitulo
            // 
            lblPanelTitulo.AutoSize = true;
            lblPanelTitulo.Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold, GraphicsUnit.Point);
            lblPanelTitulo.Location = new Point(20, 20);
            lblPanelTitulo.Name = "lblPanelTitulo";
            lblPanelTitulo.Size = new Size(167, 30);
            lblPanelTitulo.TabIndex = 0;
            lblPanelTitulo.Text = "Panel de Control";
            // 
            // lblPuertos
            // 
            lblPuertos.AutoSize = true;
            lblPuertos.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblPuertos.ForeColor = Color.FromArgb(100, 100, 100);
            lblPuertos.Location = new Point(20, 65);
            lblPuertos.Name = "lblPuertos";
            lblPuertos.Size = new Size(164, 20);
            lblPuertos.TabIndex = 1;
            lblPuertos.Text = "Dirección IP del Carrito";
            // 
            // cmbPuertos
            // 
            cmbPuertos.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            cmbPuertos.FormattingEnabled = true;
            cmbPuertos.Location = new Point(20, 90);
            cmbPuertos.Name = "cmbPuertos";
            cmbPuertos.Size = new Size(280, 31);
            cmbPuertos.TabIndex = 2;
            // 
            // btnConector
            // 
            btnConector.BackColor = Color.FromArgb(13, 110, 253);
            btnConector.Cursor = Cursors.Hand;
            btnConector.FlatAppearance.BorderSize = 0;
            btnConector.FlatStyle = FlatStyle.Flat;
            btnConector.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnConector.ForeColor = Color.White;
            btnConector.Location = new Point(20, 130);
            btnConector.Name = "btnConector";
            btnConector.Size = new Size(280, 42);
            btnConector.TabIndex = 3;
            btnConector.Text = "Conectar Wi-Fi";
            btnConector.UseVisualStyleBackColor = false;
            btnConector.Click += btnConector_Click;
            // 
            // btnEnviar
            // 
            btnEnviar.BackColor = Color.FromArgb(25, 135, 84);
            btnEnviar.Cursor = Cursors.Hand;
            btnEnviar.FlatAppearance.BorderSize = 0;
            btnEnviar.FlatStyle = FlatStyle.Flat;
            btnEnviar.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnEnviar.ForeColor = Color.White;
            btnEnviar.Location = new Point(20, 185);
            btnEnviar.Name = "btnEnviar";
            btnEnviar.Size = new Size(280, 42);
            btnEnviar.TabIndex = 4;
            btnEnviar.Text = "Enviar Ruta";
            btnEnviar.UseVisualStyleBackColor = false;
            btnEnviar.Click += btnEnviar_Click;
            // 
            // btnLimpiar
            // 
            btnLimpiar.BackColor = Color.FromArgb(108, 117, 125);
            btnLimpiar.Cursor = Cursors.Hand;
            btnLimpiar.FlatAppearance.BorderSize = 0;
            btnLimpiar.FlatStyle = FlatStyle.Flat;
            btnLimpiar.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnLimpiar.ForeColor = Color.White;
            btnLimpiar.Location = new Point(20, 235);
            btnLimpiar.Name = "btnLimpiar";
            btnLimpiar.Size = new Size(280, 42);
            btnLimpiar.TabIndex = 5;
            btnLimpiar.Text = "Limpiar Lienzo";
            btnLimpiar.UseVisualStyleBackColor = false;
            btnLimpiar.Click += btnLimpiar_Click;
            // 
            // btnZoomIn
            // 
            btnZoomIn.BackColor = Color.FromArgb(230, 240, 255);
            btnZoomIn.Cursor = Cursors.Hand;
            btnZoomIn.FlatAppearance.BorderSize = 0;
            btnZoomIn.FlatStyle = FlatStyle.Flat;
            btnZoomIn.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnZoomIn.ForeColor = Color.FromArgb(13, 110, 253);
            btnZoomIn.Location = new Point(20, 290);
            btnZoomIn.Name = "btnZoomIn";
            btnZoomIn.Size = new Size(135, 42);
            btnZoomIn.TabIndex = 100;
            btnZoomIn.Text = "Zoom +";
            btnZoomIn.UseVisualStyleBackColor = false;
            btnZoomIn.Click += btnZoomIn_Click;
            // 
            // btnZoomOut
            // 
            btnZoomOut.BackColor = Color.FromArgb(230, 240, 255);
            btnZoomOut.Cursor = Cursors.Hand;
            btnZoomOut.FlatAppearance.BorderSize = 0;
            btnZoomOut.FlatStyle = FlatStyle.Flat;
            btnZoomOut.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnZoomOut.ForeColor = Color.FromArgb(13, 110, 253);
            btnZoomOut.Location = new Point(165, 290);
            btnZoomOut.Name = "btnZoomOut";
            btnZoomOut.Size = new Size(135, 42);
            btnZoomOut.TabIndex = 101;
            btnZoomOut.Text = "Zoom -";
            btnZoomOut.UseVisualStyleBackColor = false;
            btnZoomOut.Click += btnZoomOut_Click;
            // 
            // btnModo
            // 
            btnModo.BackColor = Color.FromArgb(50, 50, 50);
            btnModo.Cursor = Cursors.Hand;
            btnModo.FlatAppearance.BorderSize = 0;
            btnModo.FlatStyle = FlatStyle.Flat;
            btnModo.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnModo.ForeColor = Color.White;
            btnModo.Location = new Point(20, 345);
            btnModo.Name = "btnModo";
            btnModo.Size = new Size(280, 42);
            btnModo.TabIndex = 102;
            btnModo.Text = "Modo: Autónomo";
            btnModo.UseVisualStyleBackColor = false;
            btnModo.Click += btnModo_Click;
            // 
            // lblManualStatus
            // 
            lblManualStatus.AutoSize = true;
            lblManualStatus.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblManualStatus.ForeColor = Color.FromArgb(150, 150, 150);
            lblManualStatus.Location = new Point(20, 395);
            lblManualStatus.Name = "lblManualStatus";
            lblManualStatus.Size = new Size(116, 20);
            lblManualStatus.TabIndex = 103;
            lblManualStatus.Text = "Manual: Inactivo";
            // 
            // lblArrowUp
            // 
            lblArrowUp.BackColor = Color.WhiteSmoke;
            lblArrowUp.BorderStyle = BorderStyle.FixedSingle;
            lblArrowUp.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            lblArrowUp.Location = new Point(135, 425);
            lblArrowUp.Name = "lblArrowUp";
            lblArrowUp.Size = new Size(50, 45);
            lblArrowUp.TabIndex = 104;
            lblArrowUp.Text = "↑";
            lblArrowUp.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblArrowLeft
            // 
            lblArrowLeft.BackColor = Color.WhiteSmoke;
            lblArrowLeft.BorderStyle = BorderStyle.FixedSingle;
            lblArrowLeft.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            lblArrowLeft.Location = new Point(80, 475);
            lblArrowLeft.Name = "lblArrowLeft";
            lblArrowLeft.Size = new Size(50, 45);
            lblArrowLeft.TabIndex = 105;
            lblArrowLeft.Text = "←";
            lblArrowLeft.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblArrowDown
            // 
            lblArrowDown.BackColor = Color.WhiteSmoke;
            lblArrowDown.BorderStyle = BorderStyle.FixedSingle;
            lblArrowDown.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            lblArrowDown.Location = new Point(135, 475);
            lblArrowDown.Name = "lblArrowDown";
            lblArrowDown.Size = new Size(50, 45);
            lblArrowDown.TabIndex = 106;
            lblArrowDown.Text = "↓";
            lblArrowDown.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblArrowRight
            // 
            lblArrowRight.BackColor = Color.WhiteSmoke;
            lblArrowRight.BorderStyle = BorderStyle.FixedSingle;
            lblArrowRight.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            lblArrowRight.Location = new Point(190, 475);
            lblArrowRight.Name = "lblArrowRight";
            lblArrowRight.Size = new Size(50, 45);
            lblArrowRight.TabIndex = 107;
            lblArrowRight.Text = "→";
            lblArrowRight.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblDistancia
            // 
            lblDistancia.AutoSize = true;
            lblDistancia.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            lblDistancia.Location = new Point(20, 540);
            lblDistancia.Name = "lblDistancia";
            lblDistancia.Size = new Size(130, 23);
            lblDistancia.TabIndex = 6;
            lblDistancia.Text = "Distancia: 0 cm";
            // 
            // lblEstado
            // 
            lblEstado.AutoSize = true;
            lblEstado.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            lblEstado.ForeColor = Color.FromArgb(25, 135, 84);
            lblEstado.Location = new Point(20, 565);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(185, 21);
            lblEstado.TabIndex = 7;
            lblEstado.Text = "Estado: Listo para dibujar";
            // 
            // txtLogDetallado
            // 
            txtLogDetallado.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtLogDetallado.BackColor = Color.FromArgb(30, 30, 30);
            txtLogDetallado.BorderStyle = BorderStyle.None;
            txtLogDetallado.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtLogDetallado.ForeColor = Color.FromArgb(0, 240, 120);
            txtLogDetallado.Location = new Point(20, 600);
            txtLogDetallado.Multiline = true;
            txtLogDetallado.Name = "txtLogDetallado";
            txtLogDetallado.ReadOnly = true;
            txtLogDetallado.ScrollBars = ScrollBars.Vertical;
            txtLogDetallado.Size = new Size(280, 140);
            txtLogDetallado.TabIndex = 8;
            txtLogDetallado.Text = "=== TELEMETRIA INICIADA ===";
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.White;
            panelHeader.Controls.Add(lblSubtitulo);
            panelHeader.Controls.Add(lblTitulo);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(320, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(964, 90);
            panelHeader.TabIndex = 1;
            // 
            // lblSubtitulo
            // 
            lblSubtitulo.AutoSize = true;
            lblSubtitulo.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            lblSubtitulo.ForeColor = Color.FromArgb(100, 100, 100);
            lblSubtitulo.Location = new Point(30, 50);
            lblSubtitulo.Name = "lblSubtitulo";
            lblSubtitulo.Size = new Size(428, 23);
            lblSubtitulo.TabIndex = 1;
            lblSubtitulo.Text = "Dibuja una ruta con el mouse y envíala al carrito por Wi-Fi";
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitulo.Location = new Point(25, 15);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(257, 37);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Ruta Carrito ESP32";
            // 
            // panelContenido
            // 
            panelContenido.BackColor = Color.FromArgb(245, 246, 250);
            panelContenido.Controls.Add(picLienzo);
            panelContenido.Dock = DockStyle.Fill;
            panelContenido.Location = new Point(320, 90);
            panelContenido.Name = "panelContenido";
            panelContenido.Padding = new Padding(30);
            panelContenido.Size = new Size(964, 670);
            panelContenido.TabIndex = 2;
            // 
            // picLienzo
            // 
            picLienzo.BackColor = Color.White;
            picLienzo.BorderStyle = BorderStyle.FixedSingle;
            picLienzo.Cursor = Cursors.Cross;
            picLienzo.Dock = DockStyle.Fill;
            picLienzo.Location = new Point(30, 30);
            picLienzo.Name = "picLienzo";
            picLienzo.Size = new Size(904, 610);
            picLienzo.TabIndex = 1;
            picLienzo.TabStop = false;
            picLienzo.MouseDown += picLienzo_MouseDown;
            picLienzo.MouseMove += picLienzo_MouseMove;
            picLienzo.MouseUp += picLienzo_MouseUp;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 246, 250);
            ClientSize = new Size(1284, 760);
            Controls.Add(panelContenido);
            Controls.Add(panelHeader);
            Controls.Add(panelSidebar);
            MinimumSize = new Size(1160, 780);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Ruta Carrito ESP32 - Estación de Control";
            Load += Form1_Load;
            panelSidebar.ResumeLayout(false);
            panelSidebar.PerformLayout();
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            panelContenido.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picLienzo).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelSidebar;
        private Label lblPanelTitulo;
        private Label lblPuertos;
        private PictureBox picLienzo;
        private ComboBox cmbPuertos;
        private Button btnConector;
        private Button btnLimpiar;
        private Button btnEnviar;
        private Label lblDistancia;
        private Label lblEstado;
        private TextBox txtLogDetallado;
        private Panel panelHeader;
        private Label lblSubtitulo;
        private Label lblTitulo;
        private Panel panelContenido;
        private Button btnZoomIn;
        private Button btnZoomOut;
        private Button btnModo;
        private Label lblManualStatus;
        private Label lblArrowUp;
        private Label lblArrowLeft;
        private Label lblArrowDown;
        private Label lblArrowRight;
    }
}