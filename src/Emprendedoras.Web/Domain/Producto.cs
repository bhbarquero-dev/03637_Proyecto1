using System.ComponentModel.DataAnnotations;

namespace Emprendedoras.Web.Domain;

// Producto del catálogo público (HU-01, RF-01).
public class Producto
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    // Precio en colones, sin decimales.
    [Range(0, 10_000_000)]
    public int Precio { get; set; }

    [Required, StringLength(100)]
    public string Emprendedora { get; set; } = string.Empty;

    public bool Disponible { get; set; }

    // Ruta relativa a wwwroot, p. ej. "/img/productos/jabon.svg".
    [Required, StringLength(200)]
    public string ImagenUrl { get; set; } = string.Empty;

    // Texto alternativo de la imagen (WCAG 1.1.1).
    [Required, StringLength(200)]
    public string ImagenAlt { get; set; } = string.Empty;
}
