namespace Emprendedoras.Web.Domain;

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
