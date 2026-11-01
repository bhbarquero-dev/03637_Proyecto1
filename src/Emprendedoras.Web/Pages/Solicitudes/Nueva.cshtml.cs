using System.ComponentModel.DataAnnotations;
using Emprendedoras.Web.Data;
using Emprendedoras.Web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Emprendedoras.Web.Pages.Solicitudes;

// Formulario de solicitud de compra sin cuenta (HU-02).
public class NuevaModel(ApplicationDbContext db, RegistroSolicitudes registro) : PageModel
{
    public const string ClaveNumero = "NumeroSolicitud";

    [BindProperty]
    public DatosSolicitud Datos { get; set; } = new();

    public Producto? Producto { get; private set; }

    public class DatosSolicitud
    {
        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "Escriba su nombre.")]
        [StringLength(100, ErrorMessage = "El nombre puede tener hasta 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "Escriba su correo electrónico.")]
        [EmailAddress(ErrorMessage = "Escriba un correo válido, por ejemplo nombre@ejemplo.com.")]
        [StringLength(254, ErrorMessage = "El correo puede tener hasta 254 caracteres.")]
        public string Correo { get; set; } = string.Empty;

        [Display(Name = "Teléfono")]
        [RegularExpression(@"^[0-9+()\- ]{8,20}$", ErrorMessage = "Escriba un teléfono válido, por ejemplo 8888-0000.")]
        public string? Telefono { get; set; }

        [Display(Name = "Cantidad")]
        [Required(ErrorMessage = "Indique la cantidad.")]
        [Range(1, 20, ErrorMessage = "La cantidad debe estar entre 1 y 20.")]
        public int Cantidad { get; set; } = 1;

        [Required(ErrorMessage = "Debe aceptar el aviso de privacidad para enviar la solicitud.")]
        [DebeSerVerdadero(ErrorMessage = "Debe aceptar el aviso de privacidad para enviar la solicitud.")]
        public bool Consentimiento { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int productoId)
    {
        Producto = await BuscarProductoAsync(productoId);
        return Producto is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(int productoId)
    {
        Producto = await BuscarProductoAsync(productoId);
        if (Producto is null)
        {
            return NotFound();
        }

        // La página de un producto agotado no muestra el formulario; este control cubre
        // los envíos directos y el producto que se agota mientras se llena el formulario.
        if (!Producto.Disponible || !ModelState.IsValid)
        {
            return Page();
        }

        var solicitud = await registro.RegistrarAsync(Producto, Datos.Nombre, Datos.Correo, Datos.Telefono, Datos.Cantidad);

        // El número viaja en TempData y no en la URL: la confirmación no se puede abrir
        // adivinando números, y la consulta de solicitudes exige también el correo (HU-03).
        TempData[ClaveNumero] = solicitud.Numero;
        return RedirectToPage("Confirmacion");
    }

    public string? AriaInvalido(string campo) =>
        ModelState[$"{nameof(Datos)}.{campo}"]?.Errors.Count > 0 ? "true" : null;

    private Task<Producto?> BuscarProductoAsync(int id) =>
        db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
}
