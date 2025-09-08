using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SimuladorTrafico
{
    public class SemaforoPeaton : UserControl
    {
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public EstadoSemaforoPeaton Estado { get; set; } = EstadoSemaforoPeaton.Rojo;
        public DireccionPeaton Direccion { get; }
        
        private System.Windows.Forms.Timer _timerParpadeo;
        private bool _parpadeando = false;

        public SemaforoPeaton(DireccionPeaton direccion)
        {
            Direccion = direccion;
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.UserPaint | 
                     ControlStyles.DoubleBuffer | 
                     ControlStyles.ResizeRedraw, true);
            
            // Semáforo peatonal más pequeño que el de vehículos
            Size = new Size(25, 40);
            
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

            // Dibujar carcasa del semáforo peatonal
            DibujarCarcasa(g);
            
            // Dibujar símbolos peatonales
            DibujarSimbolos(g);
        }

        private void DibujarCarcasa(Graphics g)
        {
            var rect = new Rectangle(2, 2, Width - 4, Height - 4);
            
            // Sombra
            using (var brush = new SolidBrush(Color.FromArgb(80, Color.Black)))
            {
                var sombra = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width, rect.Height);
                g.FillRoundedRectangle(brush, sombra, 4);
            }

            // Carcasa principal con gradiente oscuro
            using (var brush = new LinearGradientBrush(rect,
                Color.FromArgb(60, 60, 60),
                Color.FromArgb(30, 30, 30),
                LinearGradientMode.Vertical))
            {
                g.FillRoundedRectangle(brush, rect, 4);
            }

            // Borde de la carcasa
            using (var pen = new Pen(Color.FromArgb(100, Color.Silver), 1))
            {
                g.DrawRoundedRectangle(pen, rect, 4);
            }
        }

        private void DibujarSimbolos(Graphics g)
        {
            int centroX = Width / 2;
            
            // Posiciones de los símbolos
            Point posicionRojo = new Point(centroX, 10);    // Mano roja (parte superior)
            Point posicionVerde = new Point(centroX, 25);   // Muñeco verde (parte inferior)

            // Colores según el estado
            Color colorRojo = Estado == EstadoSemaforoPeaton.Rojo ? 
                Color.FromArgb(255, 80, 80) : Color.FromArgb(80, 40, 40);
            Color colorVerde = Estado == EstadoSemaforoPeaton.Verde ? 
                Color.FromArgb(80, 255, 80) : Color.FromArgb(40, 80, 40);

            // Dibujar símbolo de ALTO (mano roja)
            DibujarManoAlto(g, posicionRojo, colorRojo, Estado == EstadoSemaforoPeaton.Rojo);
            
            // Dibujar símbolo de CAMINAR (muñeco verde)
            DibujarMunecoCaminando(g, posicionVerde, colorVerde, Estado == EstadoSemaforoPeaton.Verde);
        }

        private void DibujarManoAlto(Graphics g, Point centro, Color color, bool activo)
        {
            var rect = new Rectangle(centro.X - 6, centro.Y - 4, 12, 8);
            
            // Fondo del símbolo
            using (var brush = new SolidBrush(Color.FromArgb(20, 20, 20)))
            {
                g.FillRectangle(brush, rect);
            }

            if (activo)
            {
                // Dibujar mano de ALTO con resplandor
                using (var brush = new SolidBrush(Color.FromArgb(100, color)))
                {
                    var resplandor = new Rectangle(rect.X - 2, rect.Y - 2, rect.Width + 4, rect.Height + 4);
                    g.FillRectangle(brush, resplandor);
                }
            }

            // Dibujar mano simplificada
            using (var brush = new SolidBrush(color))
            {
                // Palma
                g.FillRectangle(brush, centro.X - 3, centro.Y - 2, 6, 4);
                // Dedos
                g.FillRectangle(brush, centro.X - 3, centro.Y - 3, 2, 2);
                g.FillRectangle(brush, centro.X - 1, centro.Y - 3, 2, 2);
                g.FillRectangle(brush, centro.X + 1, centro.Y - 3, 2, 2);
            }

            // Borde del área
            using (var pen = new Pen(Color.FromArgb(100, Color.Black), 1))
            {
                g.DrawRectangle(pen, rect);
            }
        }

        private void DibujarMunecoCaminando(Graphics g, Point centro, Color color, bool activo)
        {
            var rect = new Rectangle(centro.X - 6, centro.Y - 4, 12, 8);
            
            // Fondo del símbolo
            using (var brush = new SolidBrush(Color.FromArgb(20, 20, 20)))
            {
                g.FillRectangle(brush, rect);
            }

            if (activo)
            {
                // Dibujar resplandor
                using (var brush = new SolidBrush(Color.FromArgb(100, color)))
                {
                    var resplandor = new Rectangle(rect.X - 2, rect.Y - 2, rect.Width + 4, rect.Height + 4);
                    g.FillRectangle(brush, resplandor);
                }
            }

            // Dibujar muñeco simplificado caminando
            using (var brush = new SolidBrush(color))
            {
                // Cabeza
                g.FillEllipse(brush, centro.X - 1, centro.Y - 3, 2, 2);
                // Cuerpo
                g.FillRectangle(brush, centro.X - 1, centro.Y - 1, 2, 3);
                // Brazos en posición de caminar
                g.FillRectangle(brush, centro.X - 3, centro.Y, 2, 1);
                g.FillRectangle(brush, centro.X + 1, centro.Y, 2, 1);
                // Piernas en posición de caminar
                g.FillRectangle(brush, centro.X - 2, centro.Y + 1, 1, 2);
                g.FillRectangle(brush, centro.X + 1, centro.Y + 1, 1, 2);
            }

            // Borde del área
            using (var pen = new Pen(Color.FromArgb(100, Color.Black), 1))
            {
                g.DrawRectangle(pen, rect);
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