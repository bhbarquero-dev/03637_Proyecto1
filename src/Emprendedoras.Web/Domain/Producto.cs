using System.ComponentModel.DataAnnotations;

namespace Emprendedoras.Web.Domain;

public class Producto
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Range(0, 10_000_000)]
    public int Precio { get; set; }

    [Required, StringLength(100)]
    public string Emprendedora { get; set; } = string.Empty;

    public bool Disponible { get; set; }

    [Required, StringLength(200)]
    public string ImagenUrl { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string ImagenAlt { get; set; } = string.Empty;
}
