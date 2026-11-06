using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace Emprendedoras.Tests;

public class ServidorNavegador : IAsyncLifetime
{
    public AplicacionDePrueba Aplicacion { get; } = new();

    public string Direccion { get; private set; } = string.Empty;

    public IBrowser Navegador { get; private set; } = null!;

    private IPlaywright playwright = null!;

    public async Task InitializeAsync()
    {
        Aplicacion.UseKestrel(0);
        Aplicacion.StartServer();
        Direccion = Aplicacion.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses
            .First(d => d.StartsWith("http://", StringComparison.Ordinal));

        playwright = await Playwright.CreateAsync();
        Navegador = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = true });
    }

    public async Task<IPage> NuevaPaginaAsync(int ancho, int alto = 800)
    {
        var contexto = await Navegador.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = Direccion,
            ViewportSize = new ViewportSize { Width = ancho, Height = alto },
            Locale = "es-CR"
        });
        return await contexto.NewPageAsync();
    }

    public async Task IniciarSesionAsync(IPage pagina)
    {
        await pagina.GotoAsync("/Cuenta/Acceso");
        await pagina.FillAsync("#Datos_Correo", Emprendedoras.Web.Data.CuentaAdministracion.CorreoPredeterminado);
        await pagina.FillAsync("#Datos_Contrasena", Aplicacion.ContrasenaAdministracion);
        await pagina.ClickAsync("button[type=submit]");
        await pagina.WaitForURLAsync("**/Admin");
    }

    public async Task DisposeAsync()
    {
        await Navegador.DisposeAsync();
        playwright.Dispose();
        await Aplicacion.DisposeAsync();
    }
}
