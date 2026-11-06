using System.Net;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using Emprendedoras.Web.Data;
using Emprendedoras.Web.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Emprendedoras.Tests;

public class SolicitudTests(AplicacionDePrueba aplicacion) : IClassFixture<AplicacionDePrueba>
{
    private const int ProductoDisponible = 1;
    private const int ProductoAgotado = 3;
    private const string TokenCampo = "__RequestVerificationToken";

    private readonly HttpClient cliente = aplicacion.CreateClient();

    private static Task<IDocument> LeerAsync(string html) =>
        BrowsingContext.New().OpenAsync(req => req.Content(html));

    private async Task<string> ObtenerTokenAsync()
    {
        var html = await cliente.GetStringAsync($"/Solicitudes/Nueva/{ProductoDisponible}");
        var documento = await LeerAsync(html);
        return documento.QuerySelector($"input[name={TokenCampo}]")!.GetAttribute("value")!;
    }

    private async Task<HttpResponseMessage> EnviarAsync(int productoId, Dictionary<string, string>? cambios = null, bool conToken = true)
    {
        var campos = new Dictionary<string, string>
        {
            ["Datos.Nombre"] = "Persona de Prueba",
            ["Datos.Correo"] = "Prueba@Ejemplo.com",
            ["Datos.Telefono"] = "8888-0000",
            ["Datos.Cantidad"] = "2",
            ["Datos.Consentimiento"] = "true"
        };
        foreach (var (campo, valor) in cambios ?? [])
        {
            campos[campo] = valor;
        }
        if (conToken)
        {
            campos[TokenCampo] = await ObtenerTokenAsync();
        }
        return await cliente.PostAsync($"/Solicitudes/Nueva/{productoId}", new FormUrlEncodedContent(campos));
    }

    private async Task<int> ContarSolicitudesAsync()
    {
        using var alcance = aplicacion.Services.CreateScope();
        return await alcance.ServiceProvider.GetRequiredService<ApplicationDbContext>().Solicitudes.CountAsync();
    }

    private async Task<Solicitud> BuscarAsync(string numero)
    {
        using var alcance = aplicacion.Services.CreateScope();
        return await alcance.ServiceProvider.GetRequiredService<ApplicationDbContext>().Solicitudes.SingleAsync(s => s.Numero == numero);
    }

    private static async Task<string> NumeroConfirmadoAsync(HttpResponseMessage respuesta)
    {
        respuesta.EnsureSuccessStatusCode();
        var documento = await LeerAsync(await respuesta.Content.ReadAsStringAsync());
        return documento.QuerySelector("#numero-solicitud")!.TextContent.Trim();
    }

    [Fact]
    public async Task CP_HU02_01_SolicitudValidaSeGuardaComoRecibidaYMuestraSuNumero()
    {
        var numero = await NumeroConfirmadoAsync(await EnviarAsync(ProductoDisponible));

        var solicitud = await BuscarAsync(numero);
        Assert.Equal(EstadoSolicitud.Recibida, solicitud.Estado);
        Assert.Equal(ProductoDisponible, solicitud.ProductoId);
        Assert.Equal(2, solicitud.Cantidad);
        Assert.Equal(DatosSinteticos.Productos[0].Precio, solicitud.PrecioUnitario);
        Assert.Equal("prueba@ejemplo.com", solicitud.Correo);
        Assert.True(solicitud.FechaCreacionUtc > DateTime.UtcNow.AddMinutes(-5));
        Assert.Equal(solicitud.FechaCreacionUtc, solicitud.FechaConsentimientoUtc);
    }

    [Fact]
    public async Task CP_HU02_01_El_catalogo_enlaza_el_formulario_de_un_producto_disponible_sin_iniciar_sesion()
    {
        var documento = await LeerAsync(await cliente.GetStringAsync("/"));
        var enlace = documento.QuerySelector($"a[href='/Solicitudes/Nueva/{ProductoDisponible}']");
        Assert.NotNull(enlace);

        var respuesta = await cliente.GetAsync(enlace!.GetAttribute("href"));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.DoesNotContain("/Identity/Account/Login", respuesta.RequestMessage!.RequestUri!.ToString());
    }

    [Fact]
    public async Task CP_HU02_02_SinConsentimientoLaSolicitudSeRechaza()
    {
        var antes = await ContarSolicitudesAsync();

        var respuesta = await EnviarAsync(ProductoDisponible, new() { ["Datos.Consentimiento"] = "false" });

        var documento = await LeerAsync(await respuesta.Content.ReadAsStringAsync());
        Assert.Contains("Debe aceptar el aviso de privacidad", documento.QuerySelector("#error-consentimiento")!.TextContent);
        Assert.Equal(antes, await ContarSolicitudesAsync());
    }

    [Fact]
    public async Task La_casilla_de_consentimiento_viene_sin_marcar_y_enlaza_al_aviso()
    {
        var documento = await LeerAsync(await cliente.GetStringAsync($"/Solicitudes/Nueva/{ProductoDisponible}"));

        Assert.False(documento.QuerySelector("input[type=checkbox]")!.HasAttribute("checked"));
        Assert.NotNull(documento.QuerySelector("label[for='Datos_Consentimiento'] a[href='/Privacy']"));
    }

    [Theory]
    [InlineData("Datos.Correo", "no-es-correo", "#error-correo", "correo válido")]
    [InlineData("Datos.Cantidad", "0", "#error-cantidad", "entre 1 y 20")]
    [InlineData("Datos.Cantidad", "21", "#error-cantidad", "entre 1 y 20")]
    [InlineData("Datos.Cantidad", "abc", "#error-cantidad", "no es válido")]
    [InlineData("Datos.Nombre", "", "#error-nombre", "Escriba su nombre")]
    [InlineData("Datos.Telefono", "abc", "#error-telefono", "teléfono válido")]
    public async Task CP_HU02_03_DatosInvalidosMuestranElErrorEnSuCampo(string campo, string valor, string selectorError, string mensaje)
    {
        var antes = await ContarSolicitudesAsync();

        var respuesta = await EnviarAsync(ProductoDisponible, new() { [campo] = valor });

        var documento = await LeerAsync(await respuesta.Content.ReadAsStringAsync());
        Assert.Contains(mensaje, documento.QuerySelector(selectorError)!.TextContent);
        Assert.Equal("true", documento.QuerySelector($"[name='{campo}']")!.GetAttribute("aria-invalid"));
        Assert.NotNull(documento.QuerySelector("[role=alert] li"));
        Assert.Equal(antes, await ContarSolicitudesAsync());
    }

    [Fact]
    public async Task CP_HU02_04_SolicitudDeProductoAgotadoSeRechazaEnElServidor()
    {
        var antes = await ContarSolicitudesAsync();

        var respuesta = await EnviarAsync(ProductoAgotado);

        Assert.Contains("está agotado", await respuesta.Content.ReadAsStringAsync());
        Assert.Equal(antes, await ContarSolicitudesAsync());
    }

    [Fact]
    public async Task La_pagina_de_un_producto_agotado_no_muestra_el_formulario()
    {
        var documento = await LeerAsync(await cliente.GetStringAsync($"/Solicitudes/Nueva/{ProductoAgotado}"));

        Assert.Empty(documento.QuerySelectorAll("form[method=post] button[type=submit]"));
    }

    [Fact]
    public async Task Un_producto_inexistente_responde_404()
    {
        var respuesta = await cliente.GetAsync("/Solicitudes/Nueva/9999");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task CP_HU02_05_ElNumeroTieneElFormatoEsperadoYNoSeRepite()
    {
        var primero = await NumeroConfirmadoAsync(await EnviarAsync(ProductoDisponible));
        var segundo = await NumeroConfirmadoAsync(await EnviarAsync(ProductoDisponible));

        Assert.Matches(new Regex(@"^SOL-\d{4}-\d{4}$"), primero);
        Assert.Matches(new Regex(@"^SOL-\d{4}-\d{4}$"), segundo);
        Assert.NotEqual(primero, segundo);
    }

    [Fact]
    public void El_numero_se_forma_con_anio_y_secuencia()
    {
        Assert.Equal("SOL-2026-0007", Solicitud.FormatearNumero(2026, 7));
    }

    [Fact]
    public async Task Un_envio_sin_token_antifalsificacion_se_rechaza()
    {
        var antes = await ContarSolicitudesAsync();

        var respuesta = await EnviarAsync(ProductoDisponible, conToken: false);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal(antes, await ContarSolicitudesAsync());
    }

    [Fact]
    public async Task La_confirmacion_sin_solicitud_reciente_vuelve_al_catalogo()
    {
        var respuesta = await aplicacion.CreateClient().GetAsync("/Solicitudes/Confirmacion");

        Assert.Equal("/", respuesta.RequestMessage!.RequestUri!.AbsolutePath);
    }
}
