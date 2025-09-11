namespace SimuladorTrafico
{
    public enum EstadoSemaforo
    {
        Rojo,
        Amarillo,
        Verde
    }

    public enum DireccionVehiculo
    {
        Norte,
        Sur,
        Este,
        Oeste
    }

    public enum TipoVehiculo
    {
        Auto,
        Camion,
        Moto
    }

    public enum EstadoVehiculo
    {
        Moviendose,
        Detenido,
        EnInterseccion
    }

    public enum EstadoSemaforoPeaton
    {
        Rojo,    // No cruzar (mano roja)
        Verde    // Cruzar (muñeco caminando)
    }

    public enum DireccionPeaton
    {
        NorteSur,  // Cruza de norte a sur o viceversa
        EsteOeste  // Cruza de este a oeste o viceversa
    }

    public enum EstadoPeaton
    {
        Esperando,
        Cruzando,
        Terminado
    }

    public enum TipoConfiguracionVial
    {
        DobleVia,    // Ida y vuelta en ambas direcciones (actual)
        UnicaVia     // Solo Norte-Sur (↓) y Este-Oeste (←)
    }
}