using System.ComponentModel.DataAnnotations;

namespace Emprendedoras.Web.Domain;

public class CambioEstado
{
    public int Id { get; set; }

    public int SolicitudId { get; set; }

    public EstadoSolicitud EstadoAnterior { get; set; }

    public EstadoSolicitud EstadoNuevo { get; set; }

    public DateTime FechaUtc { get; set; }

    [Required, StringLength(256)]
    public string Usuario { get; set; } = string.Empty;
}
