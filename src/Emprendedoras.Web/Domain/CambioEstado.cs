using System.ComponentModel.DataAnnotations;

namespace Emprendedoras.Web.Domain;

// Registro de un cambio de estado: quién, cuándo y de qué estado a cuál (HU-05, RF-04).
public class CambioEstado
{
    public int Id { get; set; }

    public int SolicitudId { get; set; }

    public EstadoSolicitud EstadoAnterior { get; set; }

    public EstadoSolicitud EstadoNuevo { get; set; }

    public DateTime FechaUtc { get; set; }

    // Nombre de usuario del personal que hizo el cambio.
    [Required, StringLength(256)]
    public string Usuario { get; set; } = string.Empty;
}
