using AngleSharp;
using Emprendedoras.Web.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Emprendedoras.Tests;

public class AplicacionDePrueba : WebApplicationFactory<Program>
{
    private readonly string rutaBaseDatos = Path.Combine(Path.GetTempPath(), $"emprendedoras-pruebas-{Guid.NewGuid():N}.db");

    public string ContrasenaAdministracion { get; } = $"Prueba-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"DataSource={rutaBaseDatos};Pooling=False");
        builder.UseSetting("Administracion:Contrasena", ContrasenaAdministracion);
    }

    public async Task<HttpClient> ClienteAdministracionAsync()
    {
        var cliente = CreateClient();
        var html = await cliente.GetStringAsync("/Cuenta/Acceso");
        var formulario = await BrowsingContext.New().OpenAsync(req => req.Content(html));
        var token = formulario.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!;
        var respuesta = await cliente.PostAsync("/Cuenta/Acceso", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Datos.Correo"] = CuentaAdministracion.CorreoPredeterminado,
            ["Datos.Contrasena"] = ContrasenaAdministracion,
            ["__RequestVerificationToken"] = token
        }));
        respuesta.EnsureSuccessStatusCode();
        return cliente;
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
