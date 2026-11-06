namespace Emprendedoras.Web.Domain;

// Estados de una solicitud de compra (RF-04).
public enum EstadoSolicitud
{
    Recibida,
    Confirmada,
    EnEntrega,
    Entregada,
    Cancelada
}

public static class EstadoSolicitudTexto
{
    public static string Texto(this EstadoSolicitud estado) =>
        estado == EstadoSolicitud.EnEntrega ? "En entrega" : estado.ToString();

    // Transiciones válidas (HU-05): Recibida → Confirmada → En entrega → Entregada,
    // y cualquier estado salvo Entregada puede pasar a Cancelada. Entregada y Cancelada son finales.
    public static IReadOnlyList<EstadoSolicitud> Siguientes(this EstadoSolicitud estado) => estado switch
    {
        EstadoSolicitud.Recibida => [EstadoSolicitud.Confirmada, EstadoSolicitud.Cancelada],
        EstadoSolicitud.Confirmada => [EstadoSolicitud.EnEntrega, EstadoSolicitud.Cancelada],
        EstadoSolicitud.EnEntrega => [EstadoSolicitud.Entregada, EstadoSolicitud.Cancelada],
        _ => []
    };

    public static bool PuedePasarA(this EstadoSolicitud estado, EstadoSolicitud nuevo) =>
        estado.Siguientes().Contains(nuevo);
}
