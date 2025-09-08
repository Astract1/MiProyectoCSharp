using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SimuladorTrafico
{
    public class Peaton
    {
        private static int _siguienteId = 1;
        
        public int Id { get; }
        public Point Posicion { get; set; }
        public DireccionPeaton Direccion { get; }
        public EstadoPeaton Estado { get; set; }
        public Point PuntoInicio { get; }
        public Point PuntoFin { get; }
        public int Velocidad { get; }
        private Random _random;

        public Peaton(DireccionPeaton direccion, Point puntoInicio, Point puntoFin)
        {
            Id = _siguienteId++;
            Direccion = direccion;
            PuntoInicio = puntoInicio;
            PuntoFin = puntoFin;
            Posicion = puntoInicio;
            Estado = EstadoPeaton.Esperando;
            Velocidad = 1; // Peatones se mueven más lento que vehículos
            _random = new Random();
        }

        public void Mover()
        {
            if (Estado != EstadoPeaton.Cruzando) return;

            // Calcular dirección hacia el punto final
            int deltaX = Math.Sign(PuntoFin.X - Posicion.X);
            int deltaY = Math.Sign(PuntoFin.Y - Posicion.Y);

            // Mover hacia el destino
            Posicion = new Point(
                Posicion.X + deltaX * Velocidad,
                Posicion.Y + deltaY * Velocidad
            );

            // Verificar si llegó al destino
            var distancia = Math.Abs(PuntoFin.X - Posicion.X) + Math.Abs(PuntoFin.Y - Posicion.Y);
            if (distancia < 5)
            {
                Estado = EstadoPeaton.Terminado;
                Posicion = PuntoFin;
            }
        }

        public void IniciarCruce()
        {
            if (Estado == EstadoPeaton.Esperando)
            {
                Estado = EstadoPeaton.Cruzando;
            }
        }

        public bool EstaEsperandoEnCruce()
        {
            return Estado == EstadoPeaton.Esperando;
        }

        public bool EstaEnCruce(Rectangle cruceArea)
        {
            var rectPeaton = new Rectangle(Posicion.X - 8, Posicion.Y - 8, 16, 16);
            return rectPeaton.IntersectsWith(cruceArea);
        }

        public void Dibujar(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Color del peatón según su estado
            Color colorPeaton = Estado switch
            {
                EstadoPeaton.Esperando => Color.FromArgb(100, 150, 255), // Azul (esperando)
                EstadoPeaton.Cruzando => Color.FromArgb(50, 255, 50),    // Verde (cruzando)
                EstadoPeaton.Terminado => Color.FromArgb(150, 150, 150), // Gris (terminado)
                _ => Color.White
            };

            // Dibujar cuerpo del peatón (círculo para la cabeza + rectángulo para el cuerpo)
            using (var brush = new SolidBrush(colorPeaton))
            {
                // Cabeza
                g.FillEllipse(brush, Posicion.X - 4, Posicion.Y - 8, 8, 8);
                
                // Cuerpo
                g.FillRectangle(brush, Posicion.X - 3, Posicion.Y - 2, 6, 12);
                
                // Brazos
                g.FillRectangle(brush, Posicion.X - 6, Posicion.Y + 1, 12, 2);
                
                // Piernas
                g.FillRectangle(brush, Posicion.X - 2, Posicion.Y + 8, 2, 6);
                g.FillRectangle(brush, Posicion.X + 1, Posicion.Y + 8, 2, 6);
            }

            // Agregar efecto de sombra
            using (var brush = new SolidBrush(Color.FromArgb(50, Color.Black)))
            {
                var sombra = new Rectangle(Posicion.X - 4, Posicion.Y + 12, 8, 3);
                g.FillEllipse(brush, sombra);
            }

            // Mostrar ID del peatón si está en modo debug
            if (System.Windows.Forms.Control.ModifierKeys.HasFlag(System.Windows.Forms.Keys.Control))
            {
                using (var font = new Font("Arial", 8))
                using (var brush = new SolidBrush(Color.White))
                {
                    g.DrawString($"P{Id}", font, brush, Posicion.X - 8, Posicion.Y - 20);
                }
            }
        }
    }
}