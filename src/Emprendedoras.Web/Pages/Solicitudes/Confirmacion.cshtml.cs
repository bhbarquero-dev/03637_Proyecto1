using Emprendedoras.Web.Data;
using Emprendedoras.Web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Emprendedoras.Web.Pages.Solicitudes;

// Muestra el número de seguimiento justo después de registrar la solicitud (HU-02).
public class ConfirmacionModel(ApplicationDbContext db) : PageModel
{
    public Solicitud? Solicitud { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (TempData[NuevaModel.ClaveNumero] is not string numero)
        {
            return RedirectToPage("/Index");
        }

        Solicitud = await db.Solicitudes.AsNoTracking().Include(s => s.Producto)
            .FirstOrDefaultAsync(s => s.Numero == numero);
        return Solicitud is null ? RedirectToPage("/Index") : Page();
    }
}
