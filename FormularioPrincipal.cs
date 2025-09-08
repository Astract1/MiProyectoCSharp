using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace SimuladorTrafico
{
    public partial class FormularioPrincipal : Form
    {
        private ControladorTrafico _controlador;
        private Panel _panelSimulacion;
        private TextBox _txtLog;
        private Button _btnIniciar;
        private Button _btnDetener;
        private Button _btnLimpiar;
        private Label _lblVehiculos;
        private Label _lblEstado;
        private GroupBox _groupEstadisticas;
        private GroupBox _groupControles;

        public FormularioPrincipal()
        {
            _controlador = new ControladorTrafico();
            InitializeComponent();
            ConfigurarEventos();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // Configurar formulario principal
            Text = "🚦 Simulador de Tráfico Moderno";
            Size = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(240, 244, 248);
            Font = new Font("Segoe UI", 9);
            MinimumSize = new Size(900, 600);

            // Panel principal de simulación con double buffering
            _panelSimulacion = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(600, 600),
                BackColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.None
            };
            
            // Configurar double buffering para evitar parpadeo
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, _panelSimulacion, new object[] { true });
            
            _panelSimulacion.Paint += PanelSimulacion_Paint;

            // Agregar borde elegante al panel
            var panelBorder = new Panel
            {
                Location = new Point(18, 18),
                Size = new Size(604, 604),
                BackColor = Color.FromArgb(180, 180, 180)
            };
            panelBorder.Controls.Add(_panelSimulacion);
            _panelSimulacion.Location = new Point(2, 2);

            // 4 semáforos - uno para cada dirección
            _controlador.SemaforoNorte.Location = new Point(360, 180);    // Esquina noreste
            _controlador.SemaforoSur.Location = new Point(260, 380);     // Esquina suroeste  
            _controlador.SemaforoEste.Location = new Point(380, 330);    // Esquina sureste
            _controlador.SemaforoOeste.Location = new Point(180, 230);   // Esquina noroeste

            _panelSimulacion.Controls.Add(_controlador.SemaforoNorte);
            _panelSimulacion.Controls.Add(_controlador.SemaforoSur);
            _panelSimulacion.Controls.Add(_controlador.SemaforoEste);
            _panelSimulacion.Controls.Add(_controlador.SemaforoOeste);

            // Grupo de controles
            _groupControles = new GroupBox
            {
                Text = "🎮 Controles",
                Location = new Point(650, 20),
                Size = new Size(320, 120),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };

            _btnIniciar = CrearBoton("▶️ Iniciar Simulación", new Point(20, 30), Color.FromArgb(76, 175, 80));
            _btnDetener = CrearBoton("⏸️ Pausar", new Point(170, 30), Color.FromArgb(255, 152, 0));
            _btnLimpiar = CrearBoton("🧹 Limpiar Log", new Point(20, 70), Color.FromArgb(96, 125, 139));

            _btnDetener.Enabled = false;

            _groupControles.Controls.AddRange(new Control[] { _btnIniciar, _btnDetener, _btnLimpiar });

            // Grupo de estadísticas
            _groupEstadisticas = new GroupBox
            {
                Text = "📊 Estadísticas en Tiempo Real",
                Location = new Point(650, 160),
                Size = new Size(320, 100),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };

            _lblVehiculos = new Label
            {
                Text = "🚗 Vehículos en circulación: 0",
                Location = new Point(20, 30),
                Size = new Size(280, 25),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(33, 150, 243)
            };

            _lblEstado = new Label
            {
                Text = "⏹️ Estado: Detenido",
                Location = new Point(20, 55),
                Size = new Size(280, 25),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(244, 67, 54)
            };

            _groupEstadisticas.Controls.AddRange(new Control[] { _lblVehiculos, _lblEstado });

            // Área de log mejorada
            var lblLog = new Label
            {
                Text = "📝 Registro de Eventos",
                Location = new Point(650, 280),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };

            _txtLog = new TextBox
            {
                Location = new Point(650, 310),
                Size = new Size(320, 310),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.FromArgb(0, 255, 127),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Timer para actualización de la interfaz (menos frecuente para evitar parpadeo)
            var timerUI = new System.Windows.Forms.Timer { Interval = 250 };
            timerUI.Tick += (s, e) => ActualizarUI();
            timerUI.Start();

            // Agregar controles al formulario
            Controls.AddRange(new Control[] {
                panelBorder,
                _groupControles,
                _groupEstadisticas,
                lblLog,
                _txtLog
            });

            ResumeLayout(false);
        }

        private Button CrearBoton(string texto, Point ubicacion, Color color)
        {
            var boton = new Button
            {
                Text = texto,
                Location = ubicacion,
                Size = new Size(140, 35),
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            boton.FlatAppearance.BorderSize = 0;
            return boton;
        }

        private void ConfigurarEventos()
        {
            _btnIniciar.Click += BtnIniciar_Click;
            _btnDetener.Click += BtnDetener_Click;
            _btnLimpiar.Click += BtnLimpiar_Click;
            _controlador.LogEvent += ControladorLogEvent;

            // Evento para cerrar la aplicación correctamente
            FormClosing += (s, e) => _controlador.Dispose();
        }

        private void BtnIniciar_Click(object? sender, EventArgs e)
        {
            _controlador.IniciarSimulacion();
            _btnIniciar.Enabled = false;
            _btnDetener.Enabled = true;
            _lblEstado.Text = "▶️ Estado: En ejecución";
            _lblEstado.ForeColor = Color.FromArgb(76, 175, 80);
        }

        private void BtnDetener_Click(object? sender, EventArgs e)
        {
            _controlador.DetenerSimulacion();
            _btnIniciar.Enabled = true;
            _btnDetener.Enabled = false;
            _lblEstado.Text = "⏸️ Estado: Pausado";
            _lblEstado.ForeColor = Color.FromArgb(255, 152, 0);
        }

        private void BtnLimpiar_Click(object? sender, EventArgs e)
        {
            _txtLog.Clear();
        }

        private void ControladorLogEvent(object? sender, string mensaje)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AgregarLog(mensaje)));
            }
            else
            {
                AgregarLog(mensaje);
            }
        }

        private void AgregarLog(string mensaje)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            _txtLog.AppendText($"[{timestamp}] {mensaje}\\r\\n");
            _txtLog.SelectionStart = _txtLog.Text.Length;
            _txtLog.ScrollToCaret();

            // Limitar el log a 1000 líneas
            var lineas = _txtLog.Lines;
            if (lineas.Length > 1000)
            {
                var nuevasLineas = new string[500];
                Array.Copy(lineas, lineas.Length - 500, nuevasLineas, 0, 500);
                _txtLog.Lines = nuevasLineas;
            }
        }

        private void ActualizarUI()
        {
            _lblVehiculos.Text = $"🚗 Vehículos en circulación: {_controlador.Vehiculos.Count}";
            // Solo redibujar si hay cambios significativos
            _panelSimulacion.Invalidate();
        }

        private void PanelSimulacion_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Dibujar la escena del tráfico
            _controlador.DibujarEscena(g, _panelSimulacion.ClientRectangle);

            // Dibujar información adicional
            DibujarInformacionAdicional(g);
        }

        private void DibujarInformacionAdicional(Graphics g)
        {
            // Dibujar zonas de detección (opcional, para debug)
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                using (var pen = new Pen(Color.FromArgb(100, Color.Red), 2))
                {
                    // Zonas de detención principales (cada carril tiene una sola dirección)
                    g.DrawRectangle(pen, _controlador.ZonaDetencionNorte);  // Norte: X=315-330, Y=370-400
                    g.DrawRectangle(pen, _controlador.ZonaDetencionSur);    // Sur: X=270-285, Y=200-230
                    g.DrawRectangle(pen, _controlador.ZonaDetencionEste);   // Este: X=200-230, Y=270-285
                    g.DrawRectangle(pen, _controlador.ZonaDetencionOeste);  // Oeste: X=370-400, Y=315-330
                }
                
                using (var pen = new Pen(Color.FromArgb(100, Color.Orange), 2))
                {
                    // Zonas de detención duplicadas (mismo carril)
                    g.DrawRectangle(pen, _controlador.ZonaDetencionNorte2);  // Norte: X=315-330, Y=370-400
                    g.DrawRectangle(pen, _controlador.ZonaDetencionSur2);    // Sur: X=270-285, Y=200-230
                    g.DrawRectangle(pen, _controlador.ZonaDetencionEste2);   // Este: X=200-230, Y=270-285
                    g.DrawRectangle(pen, _controlador.ZonaDetencionOeste2);  // Oeste: X=370-400, Y=315-330
                }

                using (var pen = new Pen(Color.FromArgb(100, Color.Blue), 2))
                {
                    g.DrawRectangle(pen, _controlador.Interseccion);
                }
                
                // Mostrar estado de los 4 semáforos
                using (var font = new Font("Segoe UI", 8))
                using (var brush = new SolidBrush(Color.White))
                {
                    g.DrawString($"N: {_controlador.SemaforoNorte.Estado}", font, brush, 10, 30);
                    g.DrawString($"S: {_controlador.SemaforoSur.Estado}", font, brush, 10, 45);
                    g.DrawString($"E: {_controlador.SemaforoEste.Estado}", font, brush, 10, 60);
                    g.DrawString($"O: {_controlador.SemaforoOeste.Estado}", font, brush, 10, 75);
                }
            }

            // Título en la esquina
            using (var brush = new SolidBrush(Color.FromArgb(150, Color.White)))
            using (var font = new Font("Segoe UI", 12, FontStyle.Bold))
            {
                g.DrawString("🚦 Simulador de Tráfico", font, brush, 10, 10);
            }

            // Instrucciones
            if (_controlador.Vehiculos.Count == 0)
            {
                using (var brush = new SolidBrush(Color.FromArgb(180, Color.White)))
                using (var font = new Font("Segoe UI", 10))
                {
                    var mensaje = "Haz clic en 'Iniciar Simulación' para comenzar\\n" +
                                 "Mantén Ctrl presionado para ver zonas de detección";
                    var rect = new RectangleF(150, 250, 300, 100);
                    var formato = new StringFormat 
                    { 
                        Alignment = StringAlignment.Center, 
                        LineAlignment = StringAlignment.Center 
                    };
                    g.DrawString(mensaje, font, brush, rect, formato);
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            // Atajos de teclado
            switch (e.KeyCode)
            {
                case Keys.Space:
                    if (_btnIniciar.Enabled) BtnIniciar_Click(null, EventArgs.Empty);
                    else BtnDetener_Click(null, EventArgs.Empty);
                    break;
                case Keys.C when e.Control:
                    BtnLimpiar_Click(null, EventArgs.Empty);
                    break;
            }
        }

        // Hacer el formulario receptivo a eventos de teclado
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Space)
            {
                if (_btnIniciar.Enabled) BtnIniciar_Click(null, EventArgs.Empty);
                else BtnDetener_Click(null, EventArgs.Empty);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}