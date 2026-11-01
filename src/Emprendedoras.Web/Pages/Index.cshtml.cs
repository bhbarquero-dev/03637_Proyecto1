using System.Globalization;
using Emprendedoras.Web.Data;
using Emprendedoras.Web.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Emprendedoras.Web.Pages;

// Catálogo público de productos (HU-01).
public class IndexModel(ApplicationDbContext db) : PageModel
{
    private static readonly CultureInfo CostaRica = CultureInfo.GetCultureInfo("es-CR");

    public IReadOnlyList<Producto> Productos { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Productos = await db.Productos.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync();
    }

    // Precio en colones sin decimales, p. ej. "₡18 500".
    public static string FormatearPrecio(int precio) => "₡" + precio.ToString("N0", CostaRica);
}
