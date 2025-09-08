using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SimuladorTrafico
{
    public class Semaforo : UserControl
    {
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public EstadoSemaforo Estado { get; set; } = EstadoSemaforo.Rojo;
        public string Orientacion { get; }
        
        private System.Windows.Forms.Timer _timerParpadeo;
        private bool _parpadeando = false;

        public Semaforo(string orientacion = "vertical")
        {
            Orientacion = orientacion;
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.UserPaint | 
                     ControlStyles.DoubleBuffer | 
                     ControlStyles.ResizeRedraw, true);
            
            Size = new Size(30, 90);
            
            _timerParpadeo = new System.Windows.Forms.Timer { Interval = 800 };
            _timerParpadeo.Tick += (s, e) => 
            {
                _parpadeando = !_parpadeando;
                Invalidate();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Dibujar carcasa del semáforo
            DibujarCarcasa(g);
            
            // Dibujar luces
            DibujarLuces(g);
            
            // Dibujar poste
            DibujarPoste(g);
        }

        private void DibujarCarcasa(Graphics g)
        {
            var rect = new Rectangle(5, 5, Width - 10, Height - 20);
            
            // Sombra
            using (var brush = new SolidBrush(Color.FromArgb(100, Color.Black)))
            {
                var sombra = new Rectangle(rect.X + 3, rect.Y + 3, rect.Width, rect.Height);
                g.FillRoundedRectangle(brush, sombra, 8);
            }

            // Carcasa principal con gradiente
            using (var brush = new LinearGradientBrush(rect,
                Color.FromArgb(80, 80, 80),
                Color.FromArgb(40, 40, 40),
                LinearGradientMode.Vertical))
            {
                g.FillRoundedRectangle(brush, rect, 8);
            }

            // Borde de la carcasa
            using (var pen = new Pen(Color.FromArgb(120, Color.Silver), 2))
            {
                g.DrawRoundedRectangle(pen, rect, 8);
            }

            // Detalles decorativos
            using (var pen = new Pen(Color.FromArgb(60, Color.White), 1))
            {
                g.DrawLine(pen, rect.X + 3, rect.Y + 5, rect.Right - 3, rect.Y + 5);
            }
        }

        private void DibujarLuces(Graphics g)
        {
            int radio = 12;
            int centroX = Width / 2;

            // Posiciones de las luces ajustadas para el semáforo más grande
            Point[] posiciones = {
                new Point(centroX, 20),  // Rojo
                new Point(centroX, 45),  // Amarillo
                new Point(centroX, 70)   // Verde
            };

            Color[] colores = {
                Estado == EstadoSemaforo.Rojo ? Color.FromArgb(255, 50, 50) : Color.FromArgb(80, 30, 30),
                Estado == EstadoSemaforo.Amarillo ? Color.FromArgb(255, 220, 0) : Color.FromArgb(80, 70, 20),
                Estado == EstadoSemaforo.Verde ? Color.FromArgb(50, 255, 50) : Color.FromArgb(30, 80, 30)
            };

            for (int i = 0; i < 3; i++)
            {
                DibujarLuz(g, posiciones[i], radio, colores[i], 
                          (i == 0 && Estado == EstadoSemaforo.Rojo) ||
                          (i == 1 && Estado == EstadoSemaforo.Amarillo) ||
                          (i == 2 && Estado == EstadoSemaforo.Verde));
            }
        }

        private void DibujarLuz(Graphics g, Point centro, int radio, Color color, bool activa)
        {
            var rect = new Rectangle(centro.X - radio, centro.Y - radio, radio * 2, radio * 2);

            // Sombra interior
            using (var brush = new SolidBrush(Color.FromArgb(80, Color.Black)))
            {
                var sombra = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
                g.FillEllipse(brush, sombra);
            }

            // Luz principal
            if (activa)
            {
                // Resplandor exterior más grande y brillante
                using (var brush = new SolidBrush(Color.FromArgb(120, color)))
                {
                    var resplandor = new Rectangle(rect.X - 5, rect.Y - 5, rect.Width + 10, rect.Height + 10);
                    g.FillEllipse(brush, resplandor);
                }

                // Gradiente radial para efecto 3D más intenso
                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(rect);
                    using (var brush = new PathGradientBrush(path))
                    {
                        brush.CenterColor = Color.FromArgb(255, Math.Min(255, color.R + 100), 
                                                                Math.Min(255, color.G + 100), 
                                                                Math.Min(255, color.B + 100));
                        brush.SurroundColors = new[] { color };
                        g.FillEllipse(brush, rect);
                    }
                }

                // Brillo superior más notorio
                using (var brush = new LinearGradientBrush(
                    new Rectangle(rect.X, rect.Y, rect.Width, rect.Height/2),
                    Color.FromArgb(200, Color.White),
                    Color.Transparent,
                    LinearGradientMode.Vertical))
                {
                    var brillo = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height/2 - 2);
                    g.FillEllipse(brush, brillo);
                }
            }
            else
            {
                // Luz apagada más realista
                using (var brush = new SolidBrush(color))
                {
                    g.FillEllipse(brush, rect);
                }
                
                // Agregar un toque de reflejo aunque esté apagada
                using (var brush = new LinearGradientBrush(
                    new Rectangle(rect.X, rect.Y, rect.Width, rect.Height/3),
                    Color.FromArgb(50, Color.White),
                    Color.Transparent,
                    LinearGradientMode.Vertical))
                {
                    var reflejo = new Rectangle(rect.X + 3, rect.Y + 3, rect.Width - 6, rect.Height/3 - 3);
                    g.FillEllipse(brush, reflejo);
                }
            }

            // Borde de la luz
            using (var pen = new Pen(Color.FromArgb(120, Color.Black), 1.5f))
            {
                g.DrawEllipse(pen, rect);
            }
        }

        private void DibujarPoste(Graphics g)
        {
            var posteRect = new Rectangle(Width/2 - 3, Height - 20, 6, 15);
            
            using (var brush = new LinearGradientBrush(posteRect,
                Color.FromArgb(100, 100, 100),
                Color.FromArgb(60, 60, 60),
                LinearGradientMode.Horizontal))
            {
                g.FillRectangle(brush, posteRect);
            }

            using (var pen = new Pen(Color.FromArgb(80, Color.Black), 1))
            {
                g.DrawRectangle(pen, posteRect);
            }
        }

        public void IniciarParpadeo()
        {
            _timerParpadeo.Start();
        }

        public void DetenerParpadeo()
        {
            _timerParpadeo.Stop();
            _parpadeando = false;
            Invalidate();
        }

        private Color LightenColor(Color color, float factor)
        {
            return Color.FromArgb(color.A,
                Math.Min(255, (int)(color.R + (255 - color.R) * factor)),
                Math.Min(255, (int)(color.G + (255 - color.G) * factor)),
                Math.Min(255, (int)(color.B + (255 - color.B) * factor)));
        }

        private Color DarkenColor(Color color, float factor)
        {
            return Color.FromArgb(color.A,
                Math.Max(0, (int)(color.R * (1 - factor))),
                Math.Max(0, (int)(color.G * (1 - factor))),
                Math.Max(0, (int)(color.B * (1 - factor))));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timerParpadeo?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}