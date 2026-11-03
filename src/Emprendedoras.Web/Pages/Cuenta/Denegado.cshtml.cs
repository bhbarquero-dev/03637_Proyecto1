using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Emprendedoras.Web.Pages.Cuenta;

public class DenegadoModel : PageModel
{
    public void OnGet() => Response.StatusCode = StatusCodes.Status403Forbidden;
}
