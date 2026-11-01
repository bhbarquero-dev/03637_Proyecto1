using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Emprendedoras.Tests;

// Levanta la aplicación con una base SQLite temporal propia de cada ejecución de pruebas.
public class AplicacionDePrueba : WebApplicationFactory<Program>
{
    private readonly string rutaBaseDatos = Path.Combine(Path.GetTempPath(), $"emprendedoras-pruebas-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"DataSource={rutaBaseDatos};Pooling=False");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (File.Exists(rutaBaseDatos))
        {
            File.Delete(rutaBaseDatos);
        }
    }
}
