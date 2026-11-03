using Emprendedoras.Web.Data;
using Emprendedoras.Web.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Emprendedoras.Web.Pages.Admin;

// Lista de solicitudes para el personal (HU-04). Program.cs protege toda la carpeta /Admin.
public class IndexModel(ApplicationDbContext db) : PageModel
{
    public IReadOnlyList<Solicitud> Solicitudes { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Solicitudes = await db.Solicitudes.AsNoTracking().Include(s => s.Producto)
            .OrderByDescending(s => s.FechaCreacionUtc).ThenByDescending(s => s.Id)
            .ToListAsync();
    }
}
