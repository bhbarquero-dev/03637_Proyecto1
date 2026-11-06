using System.ComponentModel.DataAnnotations;
using Emprendedoras.Web.Data;
using Emprendedoras.Web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Emprendedoras.Web.Pages.Solicitudes;

public class ConsultarModel(ApplicationDbContext db) : PageModel
{
    [BindProperty]
    public DatosConsulta Datos { get; set; } = new();

    public Solicitud? Solicitud { get; private set; }

    public bool SinResultado { get; private set; }

    public class DatosConsulta
    {
        [Display(Name = "Número de solicitud")]
        [Required(ErrorMessage = "Escriba el número de solicitud.")]
        [StringLength(20, ErrorMessage = "El número puede tener hasta 20 caracteres.")]
        public string Numero { get; set; } = string.Empty;

        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "Escriba su correo electrónico.")]
        [EmailAddress(ErrorMessage = "Escriba un correo válido, por ejemplo nombre@ejemplo.com.")]
        [StringLength(254, ErrorMessage = "El correo puede tener hasta 254 caracteres.")]
        public string Correo { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var numero = Datos.Numero.Trim().ToUpperInvariant();
        var correo = Datos.Correo.Trim().ToLowerInvariant();
        Solicitud = await db.Solicitudes.AsNoTracking().Include(s => s.Producto).Include(s => s.Cambios)
            .FirstOrDefaultAsync(s => s.Numero == numero && s.Correo == correo);

        SinResultado = Solicitud is null;
        return Page();
    }

    public string? AriaInvalido(string campo) =>
        ModelState[$"{nameof(Datos)}.{campo}"]?.Errors.Count > 0 ? "true" : null;
}
