using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Emprendedoras.Web.Pages.Cuenta;

// Cierra la sesión solo por POST, para que un enlace externo no pueda cerrarla.
public class SalirModel(SignInManager<IdentityUser> acceso) : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync()
    {
        await acceso.SignOutAsync();
        return RedirectToPage("/Index");
    }
}
