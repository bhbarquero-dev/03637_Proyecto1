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
}
