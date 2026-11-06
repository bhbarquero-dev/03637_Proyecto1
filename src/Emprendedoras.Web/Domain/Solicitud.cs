using System.ComponentModel.DataAnnotations;

namespace Emprendedoras.Web.Domain;

public class Solicitud
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string Numero { get; set; } = string.Empty;

    public int Anio { get; set; }

    public int Secuencia { get; set; }

    public int ProductoId { get; set; }

    public Producto? Producto { get; set; }

    public int Cantidad { get; set; }

    public int PrecioUnitario { get; set; }

    [Required, StringLength(100)]
    public string NombrePersona { get; set; } = string.Empty;

    [Required, StringLength(254)]
    public string Correo { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefono { get; set; }

    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Recibida;

    public DateTime FechaCreacionUtc { get; set; }

    public DateTime FechaConsentimientoUtc { get; set; }

    public List<CambioEstado> Cambios { get; set; } = [];

    public static string FormatearNumero(int anio, int secuencia) => $"SOL-{anio:D4}-{secuencia:D4}";
}
