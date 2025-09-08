using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SimuladorTrafico
{
    public class ControladorTrafico
    {
        public List<Vehiculo> Vehiculos { get; private set; }
        public Semaforo SemaforoNorte { get; private set; }
        public Semaforo SemaforoSur { get; private set; }
        public Semaforo SemaforoEste { get; private set; }
        public Semaforo SemaforoOeste { get; private set; }
        
        private System.Windows.Forms.Timer _timerSimulacion;
        private System.Windows.Forms.Timer _timerSemaforos;
        private System.Windows.Forms.Timer _timerGeneracion;
        private Random _random;
        private int _contadorTiempo = 0;
        private int _faseActual = 0; // 0=Norte-Sur Verde, 1=Norte-Sur Amarillo, 2=Este-Oeste Verde, 3=Este-Oeste Amarillo
        private int _contadorFase = 0;
        
        public Rectangle Interseccion { get; private set; }
        public Rectangle ZonaDetencionNorte { get; private set; }
        public Rectangle ZonaDetencionSur { get; private set; }
        public Rectangle ZonaDetencionEste { get; private set; }
        public Rectangle ZonaDetencionOeste { get; private set; }
        
        // Zonas de detención para el segundo carril
        public Rectangle ZonaDetencionNorte2 { get; private set; }
        public Rectangle ZonaDetencionSur2 { get; private set; }
        public Rectangle ZonaDetencionEste2 { get; private set; }
        public Rectangle ZonaDetencionOeste2 { get; private set; }
        
        public event EventHandler<string>? LogEvent;

        public ControladorTrafico()
        {
            Vehiculos = new List<Vehiculo>();
            _random = new Random();
            
            // Configurar áreas
            Interseccion = new Rectangle(250, 250, 100, 100);
            
            // Zonas de detención ANTES del primer paso peatonal - UN CARRIL POR DIRECCIÓN
            // Intersección: (250,250) a (350,350)
            // Norte (X=275): viene desde Y=580, zona después de Y=350
            ZonaDetencionNorte = new Rectangle(315, 370, 15, 30);   
            // Sur (X=325): viene desde Y=20, zona antes de Y=250
            ZonaDetencionSur = new Rectangle(270, 200, 15, 30);     
            // Este (Y=275): viene desde X=20, zona antes de X=250
            ZonaDetencionEste = new Rectangle(200, 270, 30, 15);    
            // Oeste (Y=325): viene desde X=580, zona después de X=350
            ZonaDetencionOeste = new Rectangle(370, 315, 30, 15);   
            
            // Zonas de detención duplicadas para compatibilidad (mismo carril)
            ZonaDetencionNorte2 = new Rectangle(315, 370, 15, 30);  // Norte carril (mismo que Norte)
            ZonaDetencionSur2 = new Rectangle(270, 200, 15, 30);    // Sur carril (mismo que Sur)
            ZonaDetencionEste2 = new Rectangle(200, 270, 30, 15);   // Este carril (mismo que Este)
            ZonaDetencionOeste2 = new Rectangle(370, 315, 30, 15);  // Oeste carril (mismo que Oeste)   
            
            // Crear 4 semáforos
            SemaforoNorte = new Semaforo();
            SemaforoSur = new Semaforo();
            SemaforoEste = new Semaforo();
            SemaforoOeste = new Semaforo();
            
            // Estados iniciales - Norte-Sur verde, Este-Oeste rojo
            SemaforoNorte.Estado = EstadoSemaforo.Verde;
            SemaforoSur.Estado = EstadoSemaforo.Verde;
            SemaforoEste.Estado = EstadoSemaforo.Rojo;
            SemaforoOeste.Estado = EstadoSemaforo.Rojo;
            
            // Timers simples
            _timerSimulacion = new System.Windows.Forms.Timer { Interval = 100 };
            _timerSimulacion.Tick += (s, e) => ActualizarSimulacion();
            
            _timerSemaforos = new System.Windows.Forms.Timer { Interval = 3000 }; // 3 segundos por fase
            _timerSemaforos.Tick += (s, e) => CambiarSemaforos();
            
            _timerGeneracion = new System.Windows.Forms.Timer { Interval = 2000 };
            _timerGeneracion.Tick += (s, e) => GenerarVehiculo();
        }

        public void IniciarSimulacion()
        {
            _timerSimulacion.Start();
            _timerSemaforos.Start();
            _timerGeneracion.Start();
            LogEvent?.Invoke(this, "🚦 Simulación iniciada");
        }

        public void DetenerSimulacion()
        {
            _timerSimulacion.Stop();
            _timerSemaforos.Stop();
            _timerGeneracion.Stop();
            LogEvent?.Invoke(this, "⏹️ Simulación detenida");
        }

        private void ActualizarSimulacion()
        {
            for (int i = Vehiculos.Count - 1; i >= 0; i--)
            {
                var vehiculo = Vehiculos[i];
                
                // Marcar si el vehículo entra a la intersección por primera vez
                if (!vehiculo.YaEntroAInterseccion && vehiculo.EstaEnInterseccion(Interseccion))
                {
                    vehiculo.YaEntroAInterseccion = true;
                }
                
                bool debeDetenerse = false;
                
                // Verificar si debe detenerse por semáforo
                if (DebeDetenersePorSemaforo(vehiculo))
                {
                    debeDetenerse = true;
                    vehiculo.Estado = EstadoVehiculo.Detenido;
                }
                // Verificar colisiones
                else if (HayVehiculoAdelante(vehiculo))
                {
                    debeDetenerse = true;
                    vehiculo.Estado = EstadoVehiculo.Detenido;
                }
                else
                {
                    vehiculo.Estado = EstadoVehiculo.Moviendose;
                }
                
                // SOLO mover si NO debe detenerse
                if (!debeDetenerse)
                {
                    vehiculo.Mover();
                }
                
                // Eliminar vehículos que salieron
                if (vehiculo.Posicion.X < -50 || vehiculo.Posicion.X > 650 || 
                    vehiculo.Posicion.Y < -50 || vehiculo.Posicion.Y > 650)
                {
                    Vehiculos.RemoveAt(i);
                }
            }
        }

        private bool YaCruzoInterseccion(Vehiculo vehiculo)
        {
            // Verificar si el vehículo ya pasó completamente la intersección
            return vehiculo.Direccion switch
            {
                DireccionVehiculo.Norte => vehiculo.Posicion.Y < Interseccion.Top - 20, // Ya pasó la intersección hacia arriba
                DireccionVehiculo.Sur => vehiculo.Posicion.Y > Interseccion.Bottom + 20, // Ya pasó la intersección hacia abajo
                DireccionVehiculo.Este => vehiculo.Posicion.X > Interseccion.Right + 20, // Ya pasó la intersección hacia derecha
                DireccionVehiculo.Oeste => vehiculo.Posicion.X < Interseccion.Left - 20, // Ya pasó la intersección hacia izquierda
                _ => false
            };
        }

        private bool EstaEnCualquierZonaDetencion(Vehiculo vehiculo, DireccionVehiculo direccion)
        {
            var rectVehiculo = new Rectangle(vehiculo.Posicion.X - 15, vehiculo.Posicion.Y - 15, 30, 30);
            
            return direccion switch
            {
                // Vehículos que van hacia el Norte (desde Y=580 hacia Y=0)
                DireccionVehiculo.Norte => 
                    rectVehiculo.IntersectsWith(ZonaDetencionNorte) || 
                    rectVehiculo.IntersectsWith(ZonaDetencionNorte2) ||
                    EstaAcercandose(vehiculo, ZonaDetencionNorte) ||
                    EstaAcercandose(vehiculo, ZonaDetencionNorte2),
                    
                // Vehículos que van hacia el Sur (desde Y=20 hacia Y=600)
                DireccionVehiculo.Sur => 
                    rectVehiculo.IntersectsWith(ZonaDetencionSur) || 
                    rectVehiculo.IntersectsWith(ZonaDetencionSur2) ||
                    EstaAcercandose(vehiculo, ZonaDetencionSur) ||
                    EstaAcercandose(vehiculo, ZonaDetencionSur2),
                    
                // Vehículos que van hacia el Este (desde X=20 hacia X=600)
                DireccionVehiculo.Este => 
                    rectVehiculo.IntersectsWith(ZonaDetencionEste) || 
                    rectVehiculo.IntersectsWith(ZonaDetencionEste2) ||
                    EstaAcercandose(vehiculo, ZonaDetencionEste) ||
                    EstaAcercandose(vehiculo, ZonaDetencionEste2),
                    
                // Vehículos que van hacia el Oeste (desde X=580 hacia X=0)
                DireccionVehiculo.Oeste => 
                    rectVehiculo.IntersectsWith(ZonaDetencionOeste) || 
                    rectVehiculo.IntersectsWith(ZonaDetencionOeste2) ||
                    EstaAcercandose(vehiculo, ZonaDetencionOeste) ||
                    EstaAcercandose(vehiculo, ZonaDetencionOeste2),
                    
                _ => false
            };
        }

        private bool DebeDetenersePorSemaforo(Vehiculo vehiculo)
        {
            // Si ya entró a la intersección una vez, puede continuar sin importar el semáforo
            if (vehiculo.YaEntroAInterseccion)
                return false;
                
            // Si está en la intersección, puede continuar
            if (vehiculo.EstaEnInterseccion(Interseccion))
                return false;
                
            // Si ya cruzó la intersección completamente, no debe detenerse nunca más
            if (YaCruzoInterseccion(vehiculo))
                return false;
            
            EstadoSemaforo estado = vehiculo.Direccion switch
            {
                DireccionVehiculo.Norte => SemaforoNorte.Estado,
                DireccionVehiculo.Sur => SemaforoSur.Estado,
                DireccionVehiculo.Este => SemaforoEste.Estado,
                DireccionVehiculo.Oeste => SemaforoOeste.Estado,
                _ => EstadoSemaforo.Rojo
            };
            
            // Si el semáforo está verde, no detenerse
            if (estado == EstadoSemaforo.Verde)
                return false;
            
            // Verificar si está en cualquiera de las zonas de detención para su dirección
            bool enZonaDetencion = EstaEnCualquierZonaDetencion(vehiculo, vehiculo.Direccion);
            
            // Solo detenerse si el semáforo está rojo/amarillo Y está en alguna zona
            bool debeDetenerse = (estado == EstadoSemaforo.Rojo || estado == EstadoSemaforo.Amarillo) && enZonaDetencion;
            
            // Solo log una vez cuando empieza a detenerse
            if (debeDetenerse && vehiculo.Estado != EstadoVehiculo.Detenido)
            {
                LogEvent?.Invoke(this, $"🛑 Vehículo {vehiculo.Direccion} se detiene en semáforo {estado}");
            }
            
            return debeDetenerse;
        }

        private bool EstaAcercandose(Vehiculo vehiculo, Rectangle zona)
        {
            int distancia = 40;
            
            return vehiculo.Direccion switch
            {
                // Norte: viene desde abajo (Y=580), zona en Y=200-230, detectar cuando está en Y=230-270
                DireccionVehiculo.Norte => vehiculo.Posicion.Y > zona.Bottom && vehiculo.Posicion.Y < zona.Bottom + distancia,
                
                // Sur: viene desde arriba (Y=20), zona en Y=370-400, detectar cuando está en Y=330-370
                DireccionVehiculo.Sur => vehiculo.Posicion.Y < zona.Top && vehiculo.Posicion.Y > zona.Top - distancia,
                
                // Este: viene desde izquierda (X=20), zona en X=200-230, detectar cuando está en X=160-200
                DireccionVehiculo.Este => vehiculo.Posicion.X < zona.Left && vehiculo.Posicion.X > zona.Left - distancia,
                
                // Oeste: viene desde derecha (X=580), zona en X=370-400, detectar cuando está en X=410-450
                DireccionVehiculo.Oeste => vehiculo.Posicion.X > zona.Right && vehiculo.Posicion.X < zona.Right + distancia,
                
                _ => false
            };
        }

        private bool HayVehiculoAdelante(Vehiculo vehiculo)
        {
            foreach (var otro in Vehiculos)
            {
                if (otro.Id == vehiculo.Id) continue;
                
                // Solo verificar vehículos en la misma dirección y carril
                if (vehiculo.Direccion != otro.Direccion) continue;
                
                // Verificar si están en el mismo carril (cada carril tiene una sola dirección)
                bool mismoCarril = vehiculo.Direccion switch
                {
                    DireccionVehiculo.Norte => 
                        // Carril Norte (X=275)
                        Math.Abs(vehiculo.Posicion.X - 275) < 15 && Math.Abs(otro.Posicion.X - 275) < 15,
                        
                    DireccionVehiculo.Sur => 
                        // Carril Sur (X=325)
                        Math.Abs(vehiculo.Posicion.X - 325) < 15 && Math.Abs(otro.Posicion.X - 325) < 15,
                        
                    DireccionVehiculo.Este => 
                        // Carril Este (Y=275)
                        Math.Abs(vehiculo.Posicion.Y - 275) < 15 && Math.Abs(otro.Posicion.Y - 275) < 15,
                        
                    DireccionVehiculo.Oeste => 
                        // Carril Oeste (Y=325)
                        Math.Abs(vehiculo.Posicion.Y - 325) < 15 && Math.Abs(otro.Posicion.Y - 325) < 15,
                        
                    _ => false
                };
                
                if (!mismoCarril) continue;
                
                // Verificar si el otro vehículo está adelante
                bool estaAdelante = vehiculo.Direccion switch
                {
                    DireccionVehiculo.Norte => otro.Posicion.Y < vehiculo.Posicion.Y,
                    DireccionVehiculo.Sur => otro.Posicion.Y > vehiculo.Posicion.Y,
                    DireccionVehiculo.Este => otro.Posicion.X > vehiculo.Posicion.X,
                    DireccionVehiculo.Oeste => otro.Posicion.X < vehiculo.Posicion.X,
                    _ => false
                };
                
                if (estaAdelante)
                {
                    var distancia = Math.Abs(vehiculo.Posicion.X - otro.Posicion.X) + 
                                   Math.Abs(vehiculo.Posicion.Y - otro.Posicion.Y);
                    if (distancia < 50) return true;
                }
            }
            return false;
        }

        private bool HayVehiculosEnInterseccion()
        {
            return Vehiculos.Any(v => v.EstaEnInterseccion(Interseccion));
        }

        private void CambiarSemaforos()
        {
            _contadorFase++;
            
            switch (_faseActual)
            {
                case 0: // Norte-Sur Verde (6 segundos)
                    if (_contadorFase >= 2) // 6 segundos
                    {
                        SemaforoNorte.Estado = EstadoSemaforo.Amarillo;
                        SemaforoSur.Estado = EstadoSemaforo.Amarillo;
                        SemaforoEste.Estado = EstadoSemaforo.Rojo;
                        SemaforoOeste.Estado = EstadoSemaforo.Rojo;
                        LogEvent?.Invoke(this, "🚦 Norte-Sur: AMARILLO | Este-Oeste: ROJO");
                        _faseActual = 1;
                        _contadorFase = 0;
                    }
                    break;
                    
                case 1: // Norte-Sur Amarillo (3 segundos + espera)
                    if (_contadorFase >= 1) // 3 segundos
                    {
                        if (!HayVehiculosEnInterseccion())
                        {
                            SemaforoNorte.Estado = EstadoSemaforo.Rojo;
                            SemaforoSur.Estado = EstadoSemaforo.Rojo;
                            SemaforoEste.Estado = EstadoSemaforo.Verde;
                            SemaforoOeste.Estado = EstadoSemaforo.Verde;
                            LogEvent?.Invoke(this, "🚦 Norte-Sur: ROJO | Este-Oeste: VERDE");
                            _faseActual = 2;
                            _contadorFase = 0;
                        }
                        else
                        {
                            LogEvent?.Invoke(this, "⏳ Esperando que se libere la intersección...");
                        }
                    }
                    break;
                    
                case 2: // Este-Oeste Verde (6 segundos)
                    if (_contadorFase >= 2) // 6 segundos
                    {
                        SemaforoNorte.Estado = EstadoSemaforo.Rojo;
                        SemaforoSur.Estado = EstadoSemaforo.Rojo;
                        SemaforoEste.Estado = EstadoSemaforo.Amarillo;
                        SemaforoOeste.Estado = EstadoSemaforo.Amarillo;
                        LogEvent?.Invoke(this, "🚦 Norte-Sur: ROJO | Este-Oeste: AMARILLO");
                        _faseActual = 3;
                        _contadorFase = 0;
                    }
                    break;
                    
                case 3: // Este-Oeste Amarillo (3 segundos + espera)
                    if (_contadorFase >= 1) // 3 segundos
                    {
                        if (!HayVehiculosEnInterseccion())
                        {
                            SemaforoNorte.Estado = EstadoSemaforo.Verde;
                            SemaforoSur.Estado = EstadoSemaforo.Verde;
                            SemaforoEste.Estado = EstadoSemaforo.Rojo;
                            SemaforoOeste.Estado = EstadoSemaforo.Rojo;
                            LogEvent?.Invoke(this, "🚦 Norte-Sur: VERDE | Este-Oeste: ROJO");
                            _faseActual = 0;
                            _contadorFase = 0;
                        }
                        else
                        {
                            LogEvent?.Invoke(this, "⏳ Esperando que se libere la intersección...");
                        }
                    }
                    break;
            }
            
            // Forzar redibujado de los 4 semáforos
            SemaforoNorte.Invalidate();
            SemaforoSur.Invalidate();
            SemaforoEste.Invalidate();
            SemaforoOeste.Invalidate();
        }

        private void GenerarVehiculo()
        {
            if (Vehiculos.Count >= 8) return;

            // Generar vehículos en ambos carriles pero con direcciones correctas
            var tipo = TipoVehiculo.Auto;
            Point posicion;
            DireccionVehiculo direccion;
            
            // Elegir aleatoriamente entre los 4 carriles
            int carril = _random.Next(4);
            
            switch (carril)
            {
                case 0: // Carril Norte (X=275) - solo hacia el Norte
                    direccion = DireccionVehiculo.Norte;
                    posicion = new Point(275, 580);
                    break;
                    
                case 1: // Carril Sur (X=325) - solo hacia el Sur  
                    direccion = DireccionVehiculo.Sur;
                    posicion = new Point(325, 20);
                    break;
                    
                case 2: // Carril Este (Y=275) - solo hacia el Este
                    direccion = DireccionVehiculo.Este;
                    posicion = new Point(20, 275);
                    break;
                    
                case 3: // Carril Oeste (Y=325) - solo hacia el Oeste
                    direccion = DireccionVehiculo.Oeste;
                    posicion = new Point(580, 325);
                    break;
                    
                default:
                    direccion = DireccionVehiculo.Norte;
                    posicion = new Point(275, 580);
                    break;
            }

            var vehiculo = new Vehiculo(direccion, posicion, tipo);
            Vehiculos.Add(vehiculo);
            
            // Determinar en qué carril se generó
            string nombreCarril = carril switch
            {
                0 => "Norte (X=275)",
                1 => "Sur (X=325)", 
                2 => "Este (Y=275)",
                3 => "Oeste (Y=325)",
                _ => "desconocido"
            };
            
            LogEvent?.Invoke(this, $"🚗 Nuevo vehículo {direccion} generado en carril {nombreCarril} ({posicion.X},{posicion.Y})");
        }

        public void DibujarEscena(Graphics g, Rectangle area)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Dibujar carreteras
            DibujarCarreteras(g, area);
            
            // Dibujar líneas de carretera
            DibujarLineasCarretera(g, area);
            
            // Dibujar cruces peatonales
            DibujarCrucesPeatonales(g, area);
            
            // Dibujar vehículos
            foreach (var vehiculo in Vehiculos)
            {
                vehiculo.Dibujar(g);
            }
        }

        private void DibujarCarreteras(Graphics g, Rectangle area)
        {
            // Fondo de asfalto
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(area,
                Color.FromArgb(45, 45, 45),
                Color.FromArgb(35, 35, 35),
                45f))
            {
                g.FillRectangle(brush, area);
            }

            // Carretera Norte-Sur
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Rectangle(250, 0, 100, area.Height),
                Color.FromArgb(55, 55, 55),
                Color.FromArgb(40, 40, 40),
                System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
            {
                g.FillRectangle(brush, 250, 0, 100, area.Height);
            }

            // Carretera Este-Oeste
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Rectangle(0, 250, area.Width, 100),
                Color.FromArgb(55, 55, 55),
                Color.FromArgb(40, 40, 40),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, 0, 250, area.Width, 100);
            }

            // Intersección
            using (var brush = new System.Drawing.SolidBrush(Color.FromArgb(50, 50, 50)))
            {
                g.FillRectangle(brush, Interseccion);
            }
        }

        private void DibujarLineasCarretera(Graphics g, Rectangle area)
        {
            // Línea central Norte-Sur (discontinua)
            using (var pen = new Pen(Color.Yellow, 2))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                for (int y = 0; y < area.Height; y += 30)
                {
                    if (y < 240 || y > 360)
                    {
                        g.DrawLine(pen, 300, y, 300, Math.Min(y + 20, area.Height));
                    }
                }
            }

            // Línea central Este-Oeste (discontinua)
            using (var pen = new Pen(Color.Yellow, 2))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                for (int x = 0; x < area.Width; x += 30)
                {
                    if (x < 240 || x > 360)
                    {
                        g.DrawLine(pen, x, 300, Math.Min(x + 20, area.Width), 300);
                    }
                }
            }

            // Bordes de carretera
            using (var pen = new Pen(Color.White, 3))
            {
                g.DrawLine(pen, 250, 0, 250, area.Height);
                g.DrawLine(pen, 350, 0, 350, area.Height);
                g.DrawLine(pen, 0, 250, area.Width, 250);
                g.DrawLine(pen, 0, 350, area.Width, 350);
            }
        }

        private void DibujarCrucesPeatonales(Graphics g, Rectangle area)
        {
            using (var brush = new System.Drawing.SolidBrush(Color.FromArgb(200, Color.Yellow)))
            {
                // Cruce peatonal Norte
                for (int i = 0; i < 5; i++)
                {
                    g.FillRectangle(brush, 270 + i * 12, 240, 8, 20);
                }
                
                // Cruce peatonal Sur
                for (int i = 0; i < 5; i++)
                {
                    g.FillRectangle(brush, 270 + i * 12, 340, 8, 20);
                }

                // Cruce peatonal Oeste
                for (int i = 0; i < 5; i++)
                {
                    g.FillRectangle(brush, 240, 270 + i * 12, 20, 8);
                }
                
                // Cruce peatonal Este
                for (int i = 0; i < 5; i++)
                {
                    g.FillRectangle(brush, 340, 270 + i * 12, 20, 8);
                }
            }
        }

        public void Dispose()
        {
            _timerSimulacion?.Dispose();
            _timerSemaforos?.Dispose();
            _timerGeneracion?.Dispose();
        }
    }
}