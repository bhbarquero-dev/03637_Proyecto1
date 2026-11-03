using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Emprendedoras.Tests;

// Levanta la aplicación con una base SQLite temporal propia de cada ejecución de pruebas.
public class AplicacionDePrueba : WebApplicationFactory<Program>
{
    private readonly string rutaBaseDatos = Path.Combine(Path.GetTempPath(), $"emprendedoras-pruebas-{Guid.NewGuid():N}.db");

    // Generada en cada ejecución: no hay ninguna contraseña en el repositorio.
    public string ContrasenaAdministracion { get; } = $"Prueba-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"DataSource={rutaBaseDatos};Pooling=False");
        builder.UseSetting("Administracion:Contrasena", ContrasenaAdministracion);
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
