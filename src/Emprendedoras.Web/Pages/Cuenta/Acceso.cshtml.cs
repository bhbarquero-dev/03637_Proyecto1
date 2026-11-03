using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Emprendedoras.Web.Pages.Cuenta;

// Inicio de sesión del personal (HU-04). No existe registro público de cuentas.
public class AccesoModel(SignInManager<IdentityUser> acceso) : PageModel
{
    [BindProperty]
    public DatosAcceso Datos { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public bool Rechazado { get; private set; }

    public class DatosAcceso
    {
        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "Escriba su correo electrónico.")]
        [EmailAddress(ErrorMessage = "Escriba un correo válido, por ejemplo nombre@ejemplo.com.")]
        [StringLength(254, ErrorMessage = "El correo puede tener hasta 254 caracteres.")]
        public string Correo { get; set; } = string.Empty;

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "Escriba su contraseña.")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; } = string.Empty;
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

        var resultado = await acceso.PasswordSignInAsync(Datos.Correo.Trim(), Datos.Contrasena, isPersistent: false, lockoutOnFailure: true);
        if (resultado.Succeeded)
        {
            // LocalRedirect impide que ReturnUrl lleve a otro sitio.
            return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : "/Admin");
        }

        // El mismo mensaje para correo desconocido, contraseña equivocada y cuenta bloqueada:
        // no se revela qué cuentas existen.
        Rechazado = true;
        return Page();
    }

    public string? AriaInvalido(string campo) =>
        ModelState[$"{nameof(Datos)}.{campo}"]?.Errors.Count > 0 ? "true" : null;
}
