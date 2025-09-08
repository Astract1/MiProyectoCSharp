using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SimuladorTrafico
{
    public class Vehiculo
    {
        public Point Posicion { get; set; }
        public DireccionVehiculo Direccion { get; }
        public TipoVehiculo Tipo { get; }
        public EstadoVehiculo Estado { get; set; }
        public Color ColorBase { get; }
        public float Velocidad { get; set; } = 2.5f;
        public int Ancho { get; }
        public int Alto { get; }
        public string Id { get; }
        public bool YaEntroAInterseccion { get; set; } = false;

        private static Random _random = new Random();

        public Vehiculo(DireccionVehiculo direccion, Point posicionInicial, TipoVehiculo tipo = TipoVehiculo.Auto)
        {
            Id = Guid.NewGuid().ToString("N")[..8];
            Direccion = direccion;
            Posicion = posicionInicial;
            Tipo = tipo;
            Estado = EstadoVehiculo.Moviendose;
            ColorBase = GenerarColorAtractivo();
            
            // Dimensiones según el tipo de vehículo
            switch (tipo)
            {
                case TipoVehiculo.Auto:
                    Ancho = 24;
                    Alto = 14;
                    break;
                case TipoVehiculo.Camion:
                    Ancho = 28;
                    Alto = 18;
                    Velocidad = 1.8f;
                    break;
                case TipoVehiculo.Moto:
                    Ancho = 18;
                    Alto = 10;
                    Velocidad = 3.2f;
                    break;
            }
        }

        private Color GenerarColorAtractivo()
        {
            Color[] coloresAtractivos = {
                Color.FromArgb(230, 57, 70),   // Rojo elegante
                Color.FromArgb(41, 128, 185),  // Azul océano
                Color.FromArgb(39, 174, 96),   // Verde esmeralda
                Color.FromArgb(155, 89, 182),  // Púrpura suave
                Color.FromArgb(241, 196, 15),  // Amarillo dorado
                Color.FromArgb(230, 126, 34),  // Naranja vibrante
                Color.FromArgb(52, 73, 94),    // Gris azulado
                Color.FromArgb(46, 204, 113),  // Verde menta
                Color.FromArgb(231, 76, 60),   // Rojo coral
                Color.FromArgb(52, 152, 219)   // Azul cielo
            };
            return coloresAtractivos[_random.Next(coloresAtractivos.Length)];
        }

        public void Mover()
        {
            switch (Direccion)
            {
                case DireccionVehiculo.Norte:
                    Posicion = new Point(Posicion.X, Posicion.Y - (int)Velocidad);
                    break;
                case DireccionVehiculo.Sur:
                    Posicion = new Point(Posicion.X, Posicion.Y + (int)Velocidad);
                    break;
                case DireccionVehiculo.Este:
                    Posicion = new Point(Posicion.X + (int)Velocidad, Posicion.Y);
                    break;
                case DireccionVehiculo.Oeste:
                    Posicion = new Point(Posicion.X - (int)Velocidad, Posicion.Y);
                    break;
            }
        }

        public void Dibujar(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Guardar el estado actual del gráfico
            var estadoOriginal = g.Save();
            
            // Rotar según la dirección
            var centro = new PointF(Posicion.X, Posicion.Y);
            var matriz = new Matrix();
            matriz.RotateAt(ObtenerAngulo(), centro);
            g.Transform = matriz;

            var rect = new Rectangle(Posicion.X - Ancho/2, Posicion.Y - Alto/2, Ancho, Alto);

            // Dibujar sombra
            DibujarSombra(g, rect);

            // Dibujar carrocería
            DibujarCarroceria(g, rect);

            // Dibujar ventanas
            DibujarVentanas(g, rect);

            // Dibujar ruedas
            DibujarRuedas(g, rect);

            // Dibujar luces
            DibujarLuces(g, rect);

            // Restaurar el estado del gráfico
            g.Restore(estadoOriginal);
        }

        private void DibujarSombra(Graphics g, Rectangle rect)
        {
            using (var brush = new SolidBrush(Color.FromArgb(50, Color.Black)))
            {
                var sombra = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width, rect.Height);
                g.FillRoundedRectangle(brush, sombra, 4);
            }
        }

        private void DibujarCarroceria(Graphics g, Rectangle rect)
        {
            // Gradiente para la carrocería
            using (var brush = new LinearGradientBrush(rect,
                LightenColor(ColorBase, 0.3f),
                DarkenColor(ColorBase, 0.2f),
                LinearGradientMode.Vertical))
            {
                g.FillRoundedRectangle(brush, rect, 4);
            }

            // Borde de la carrocería
            using (var pen = new Pen(DarkenColor(ColorBase, 0.4f), 1.5f))
            {
                g.DrawRoundedRectangle(pen, rect, 4);
            }
        }

        private void DibujarVentanas(Graphics g, Rectangle rect)
        {
            using (var brush = new LinearGradientBrush(rect,
                Color.FromArgb(180, 200, 220, 255),
                Color.FromArgb(100, 150, 180, 220),
                LinearGradientMode.Vertical))
            {
                // Parabrisas delantero
                var parabrisas = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height / 3);
                g.FillRoundedRectangle(brush, parabrisas, 2);

                // Ventanas laterales (solo para autos y camiones)
                if (Tipo != TipoVehiculo.Moto)
                {
                    var ventanaIzq = new Rectangle(rect.X + 1, rect.Y + rect.Height/3, rect.Width/3, rect.Height/2);
                    var ventanaDer = new Rectangle(rect.Right - rect.Width/3 - 1, rect.Y + rect.Height/3, rect.Width/3, rect.Height/2);
                    g.FillRoundedRectangle(brush, ventanaIzq, 1);
                    g.FillRoundedRectangle(brush, ventanaDer, 1);
                }
            }
        }

        private void DibujarRuedas(Graphics g, Rectangle rect)
        {
            int radioRueda = Tipo == TipoVehiculo.Moto ? 2 : 3;
            using (var brush = new SolidBrush(Color.FromArgb(40, 40, 40)))
            {
                // Posiciones de ruedas según el tipo
                Point[] posiciones = Tipo switch
                {
                    TipoVehiculo.Moto => new[]
                    {
                        new Point(rect.X + rect.Width/4, rect.Y - 1),
                        new Point(rect.X + 3*rect.Width/4, rect.Y + rect.Height + 1)
                    },
                    _ => new[]
                    {
                        new Point(rect.X + rect.Width/4, rect.Y - 2),
                        new Point(rect.X + 3*rect.Width/4, rect.Y - 2),
                        new Point(rect.X + rect.Width/4, rect.Y + rect.Height + 2),
                        new Point(rect.X + 3*rect.Width/4, rect.Y + rect.Height + 2)
                    }
                };

                foreach (var pos in posiciones)
                {
                    g.FillEllipse(brush, pos.X - radioRueda, pos.Y - radioRueda, radioRueda * 2, radioRueda * 2);
                    
                    // Detalle de la rueda
                    using (var penRueda = new Pen(Color.Silver, 0.5f))
                    {
                        g.DrawEllipse(penRueda, pos.X - radioRueda, pos.Y - radioRueda, radioRueda * 2, radioRueda * 2);
                    }
                }
            }
        }

        private void DibujarLuces(Graphics g, Rectangle rect)
        {
            int tamañoLuz = 2;
            
            // Luces delanteras (blancas)
            using (var brush = new SolidBrush(Color.White))
            {
                g.FillEllipse(brush, rect.X + 2, rect.Y - 1, tamañoLuz, tamañoLuz);
                g.FillEllipse(brush, rect.Right - 4, rect.Y - 1, tamañoLuz, tamañoLuz);
            }

            // Luces traseras (rojas) si está detenido
            if (Estado == EstadoVehiculo.Detenido)
            {
                using (var brush = new SolidBrush(Color.Red))
                {
                    g.FillEllipse(brush, rect.X + 2, rect.Bottom - 1, tamañoLuz, tamañoLuz);
                    g.FillEllipse(brush, rect.Right - 4, rect.Bottom - 1, tamañoLuz, tamañoLuz);
                }
            }
        }

        private float ObtenerAngulo()
        {
            return Direccion switch
            {
                DireccionVehiculo.Norte => 0,
                DireccionVehiculo.Sur => 180,
                DireccionVehiculo.Este => 90,
                DireccionVehiculo.Oeste => 270,
                _ => 0
            };
        }

        public bool EstaEnInterseccion(Rectangle interseccion)
        {
            var rectVehiculo = new Rectangle(Posicion.X - Ancho/2, Posicion.Y - Alto/2, Ancho, Alto);
            return rectVehiculo.IntersectsWith(interseccion);
        }

        public Rectangle ObtenerRectangulo()
        {
            return new Rectangle(Posicion.X - Ancho/2, Posicion.Y - Alto/2, Ancho, Alto);
        }
        
        private Point ObtenerProximaPosicion()
        {
            return Direccion switch
            {
                DireccionVehiculo.Norte => new Point(Posicion.X, Posicion.Y - (int)Velocidad),
                DireccionVehiculo.Sur => new Point(Posicion.X, Posicion.Y + (int)Velocidad),
                DireccionVehiculo.Este => new Point(Posicion.X + (int)Velocidad, Posicion.Y),
                DireccionVehiculo.Oeste => new Point(Posicion.X - (int)Velocidad, Posicion.Y),
                _ => Posicion
            };
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
    }

    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using (var path = new GraphicsPath())
            {
                path.AddRoundedRectangle(rect, radius);
                g.FillPath(brush, path);
            }
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, Rectangle rect, int radius)
        {
            using (var path = new GraphicsPath())
            {
                path.AddRoundedRectangle(rect, radius);
                g.DrawPath(pen, path);
            }
        }

        public static void AddRoundedRectangle(this GraphicsPath path, Rectangle rect, int radius)
        {
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
        }
 
    }
}