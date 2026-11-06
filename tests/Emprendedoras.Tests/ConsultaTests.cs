using AngleSharp;
using AngleSharp.Dom;
using Emprendedoras.Web.Data;
using Emprendedoras.Web.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Emprendedoras.Tests;

public class ConsultaTests(AplicacionDePrueba aplicacion) : IClassFixture<AplicacionDePrueba>
{
    private const string Ruta = "/Solicitudes/Consultar";
    private const string TokenCampo = "__RequestVerificationToken";

    private readonly HttpClient cliente = aplicacion.CreateClient();

    private static Task<IDocument> LeerAsync(string html) =>
        BrowsingContext.New().OpenAsync(req => req.Content(html));

    private async Task<Solicitud> RegistrarAsync(string correo)
    {
        using var alcance = aplicacion.Services.CreateScope();
        var db = alcance.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var producto = await db.Productos.AsNoTracking().FirstAsync(p => p.Id == 1);
        return await alcance.ServiceProvider.GetRequiredService<RegistroSolicitudes>()
            .RegistrarAsync(producto, "Persona Secreta", correo, "8888-0000", 3);
    }

    private async Task<(string Html, IDocument Documento)> ConsultarAsync(string numero, string correo)
    {
        var formulario = await LeerAsync(await cliente.GetStringAsync(Ruta));
        var token = formulario.QuerySelector($"input[name={TokenCampo}]")!.GetAttribute("value")!;
        var respuesta = await cliente.PostAsync(Ruta, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Datos.Numero"] = numero,
            ["Datos.Correo"] = correo,
            [TokenCampo] = token
        }));
        respuesta.EnsureSuccessStatusCode();
        var html = await respuesta.Content.ReadAsStringAsync();
        return (html, await LeerAsync(html));
    }

    [Fact]
    public async Task CP_HU03_01_DatosCorrectosMuestranElEstadoYLaFechaDeRecepcion()
    {
        var solicitud = await RegistrarAsync("compradora1@ejemplo.com");

        var (_, documento) = await ConsultarAsync(solicitud.Numero, "compradora1@ejemplo.com");

        var resultado = documento.QuerySelector("#resultado-consulta")!;
        Assert.Equal("Recibida", documento.QuerySelector("#estado-solicitud")!.TextContent.Trim());
        Assert.Contains(solicitud.Numero, resultado.TextContent);
        Assert.Contains(DatosSinteticos.Productos[0].Nombre, resultado.TextContent);
        Assert.Contains(HoraCostaRica.Formatear(solicitud.FechaCreacionUtc), resultado.TextContent);
    }

    [Fact]
    public async Task La_consulta_no_distingue_mayusculas_ni_espacios()
    {
        var solicitud = await RegistrarAsync("compradora2@ejemplo.com");

        var (_, documento) = await ConsultarAsync($" {solicitud.Numero.ToLowerInvariant()} ", "COMPRADORA2@ejemplo.com");

        Assert.NotNull(documento.QuerySelector("#resultado-consulta"));
    }

    [Fact]
    public async Task CP_HU03_02_CorreoQueNoCorrespondeMuestraElMensajeGenerico()
    {
        var solicitud = await RegistrarAsync("compradora3@ejemplo.com");

        var (html, documento) = await ConsultarAsync(solicitud.Numero, "otra@ejemplo.com");

        Assert.NotNull(documento.QuerySelector("#sin-resultado"));
        Assert.Null(documento.QuerySelector("#resultado-consulta"));
        Assert.DoesNotContain(DatosSinteticos.Productos[0].Nombre, html);
    }

    [Fact]
    public async Task CP_HU03_03_NumeroInexistenteMuestraElMismoMensajeGenerico()
    {
        var solicitud = await RegistrarAsync("compradora4@ejemplo.com");

        var (_, correoEquivocado) = await ConsultarAsync(solicitud.Numero, "otra@ejemplo.com");
        var (_, numeroInexistente) = await ConsultarAsync("SOL-1999-9999", "compradora4@ejemplo.com");

        Assert.Equal(
            correoEquivocado.QuerySelector("#sin-resultado")!.TextContent.Trim(),
            numeroInexistente.QuerySelector("#sin-resultado")!.TextContent.Trim());
    }

    [Fact]
    public async Task La_consulta_nunca_muestra_datos_personales_ni_de_otra_solicitud()
    {
        var mia = await RegistrarAsync("compradora5@ejemplo.com");
        var otra = await RegistrarAsync("compradora6@ejemplo.com");

        var (_, documento) = await ConsultarAsync(mia.Numero, "compradora5@ejemplo.com");

        var resultado = documento.QuerySelector("main")!.TextContent;
        Assert.Contains(mia.Numero, resultado);
        Assert.DoesNotContain(otra.Numero, resultado);
        Assert.DoesNotContain("Persona Secreta", resultado);
        Assert.DoesNotContain("8888-0000", resultado);
    }

    [Theory]
    [InlineData("", "a@ejemplo.com", "#error-numero")]
    [InlineData("SOL-2026-0001", "", "#error-correo")]
    [InlineData("SOL-2026-0001", "no-es-correo", "#error-correo")]
    public async Task Datos_incompletos_muestran_el_error_en_su_campo(string numero, string correo, string selectorError)
    {
        var (_, documento) = await ConsultarAsync(numero, correo);

        Assert.False(string.IsNullOrWhiteSpace(documento.QuerySelector(selectorError)!.TextContent));
        Assert.Null(documento.QuerySelector("#sin-resultado"));
    }

    [Fact]
    public async Task El_menu_enlaza_la_consulta()
    {
        var documento = await LeerAsync(await cliente.GetStringAsync("/"));

        Assert.NotNull(documento.QuerySelector($"nav a[href='{Ruta}']"));
    }

    [Fact]
    public void El_estado_en_entrega_se_muestra_con_espacio()
    {
        Assert.Equal("En entrega", EstadoSolicitud.EnEntrega.Texto());
        Assert.Equal("Recibida", EstadoSolicitud.Recibida.Texto());
    }
}
