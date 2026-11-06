using Emprendedoras.Web.Data;
using Emprendedoras.Web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Emprendedoras.Web.Pages.Admin;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class SolicitudModel(ApplicationDbContext db, GestionEstados gestion) : PageModel
{
    [TempData]
    public string? Mensaje { get; set; }

    public Solicitud? Solicitud { get; private set; }

    public string? Error { get; private set; }

    public async Task<IActionResult> OnGetAsync(string numero)
    {
        Solicitud = await CargarAsync(numero);
        return Solicitud is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(string numero, EstadoSolicitud estadoEsperado, EstadoSolicitud nuevo)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var resultado = await gestion.CambiarAsync(numero, estadoEsperado, nuevo, User.Identity!.Name!);
        if (resultado == GestionEstados.Resultado.NoExiste)
        {
            return NotFound();
        }
        if (resultado == GestionEstados.Resultado.Aplicado)
        {
            Mensaje = $"El estado cambió a {nuevo.Texto()}.";
            return RedirectToPage(new { numero });
        }

        Error = resultado == GestionEstados.Resultado.TransicionInvalida
            ? $"No se puede pasar de {estadoEsperado.Texto()} a {nuevo.Texto()}."
            : "Otra persona cambió el estado de esta solicitud. Revise el estado actual antes de cambiarlo.";
        Solicitud = await CargarAsync(numero);
        return Page();
    }

    private Task<Solicitud?> CargarAsync(string numero) =>
        db.Solicitudes.AsNoTracking().Include(s => s.Producto).Include(s => s.Cambios)
            .FirstOrDefaultAsync(s => s.Numero == numero);
}
