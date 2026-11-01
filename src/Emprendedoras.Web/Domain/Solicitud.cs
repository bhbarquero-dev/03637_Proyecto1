using System.ComponentModel.DataAnnotations;

namespace Emprendedoras.Web.Domain;

// Solicitud de compra registrada sin cuenta (HU-02, RF-03).
public class Solicitud
{
    public int Id { get; set; }

    // Número de seguimiento: SOL-AAAA-NNNN.
    [Required, StringLength(20)]
    public string Numero { get; set; } = string.Empty;

    public int Anio { get; set; }

    public int Secuencia { get; set; }

    public int ProductoId { get; set; }

    public Producto? Producto { get; set; }

    public int Cantidad { get; set; }

    // Precio en colones al momento de la solicitud; el del producto puede cambiar después.
    public int PrecioUnitario { get; set; }

    [Required, StringLength(100)]
    public string NombrePersona { get; set; } = string.Empty;

    [Required, StringLength(254)]
    public string Correo { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefono { get; set; }

    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Recibida;

    public DateTime FechaCreacionUtc { get; set; }

    // Evidencia del consentimiento informado (Ley 8968).
    public DateTime FechaConsentimientoUtc { get; set; }

    public static string FormatearNumero(int anio, int secuencia) => $"SOL-{anio:D4}-{secuencia:D4}";
}
