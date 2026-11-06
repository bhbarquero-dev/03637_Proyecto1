using System.Net;
using AngleSharp;
using AngleSharp.Dom;
using Emprendedoras.Web.Data;
using Emprendedoras.Web.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Emprendedoras.Tests;

public class CambioEstadoTests(AplicacionDePrueba aplicacion) : IClassFixture<AplicacionDePrueba>
{
    private const string TokenCampo = "__RequestVerificationToken";

    private static Task<IDocument> LeerAsync(string html) =>
        BrowsingContext.New().OpenAsync(req => req.Content(html));

    private async Task<Solicitud> RegistrarAsync(string correo)
    {
        using var alcance = aplicacion.Services.CreateScope();
        var db = alcance.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var producto = await db.Productos.AsNoTracking().FirstAsync(p => p.Id == 1);
        return await alcance.ServiceProvider.GetRequiredService<RegistroSolicitudes>()
            .RegistrarAsync(producto, "Persona Secreta", correo, null, 1);
    }

    private async Task<Solicitud> LeerSolicitudAsync(string numero)
    {
        using var alcance = aplicacion.Services.CreateScope();
        return await alcance.ServiceProvider.GetRequiredService<ApplicationDbContext>().Solicitudes
            .AsNoTracking().Include(s => s.Cambios).FirstAsync(s => s.Numero == numero);
    }

    private static async Task<(HttpResponseMessage Respuesta, IDocument Documento)> CambiarAsync(
        HttpClient cliente, string numero, EstadoSolicitud esperado, EstadoSolicitud nuevo)
    {
        var ruta = $"/Admin/Solicitud/{numero}";
        var pagina = await LeerAsync(await cliente.GetStringAsync(ruta));
        var token = pagina.QuerySelector($"input[name={TokenCampo}]")!.GetAttribute("value")!;
        var respuesta = await cliente.PostAsync(ruta, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["estadoEsperado"] = esperado.ToString(),
            ["nuevo"] = nuevo.ToString(),
            [TokenCampo] = token
        }));
        return (respuesta, await LeerAsync(await respuesta.Content.ReadAsStringAsync()));
    }

    public static TheoryData<EstadoSolicitud, EstadoSolicitud, bool> Transiciones()
    {
        HashSet<(EstadoSolicitud, EstadoSolicitud)> validas =
        [
            (EstadoSolicitud.Recibida, EstadoSolicitud.Confirmada),
            (EstadoSolicitud.Confirmada, EstadoSolicitud.EnEntrega),
            (EstadoSolicitud.EnEntrega, EstadoSolicitud.Entregada),
            (EstadoSolicitud.Recibida, EstadoSolicitud.Cancelada),
            (EstadoSolicitud.Confirmada, EstadoSolicitud.Cancelada),
            (EstadoSolicitud.EnEntrega, EstadoSolicitud.Cancelada)
        ];
        var datos = new TheoryData<EstadoSolicitud, EstadoSolicitud, bool>();
        foreach (var desde in Enum.GetValues<EstadoSolicitud>())
        {
            foreach (var hacia in Enum.GetValues<EstadoSolicitud>())
            {
                datos.Add(desde, hacia, validas.Contains((desde, hacia)));
            }
        }
        return datos;
    }

    [Theory]
    [MemberData(nameof(Transiciones))]
    public void Solo_se_permiten_las_transiciones_del_criterio(EstadoSolicitud desde, EstadoSolicitud hacia, bool valida)
    {
        Assert.Equal(valida, desde.PuedePasarA(hacia));
    }

    [Fact]
    public async Task CP_HU05_01_LasTransicionesValidasSeAplicanYSeRegistran()
    {
        var solicitud = await RegistrarAsync("flujo@ejemplo.com");
        var cliente = await aplicacion.ClienteAdministracionAsync();
        EstadoSolicitud[] flujo = [EstadoSolicitud.Recibida, EstadoSolicitud.Confirmada, EstadoSolicitud.EnEntrega, EstadoSolicitud.Entregada];

        for (var i = 1; i < flujo.Length; i++)
        {
            var (respuesta, documento) = await CambiarAsync(cliente, solicitud.Numero, flujo[i - 1], flujo[i]);

            respuesta.EnsureSuccessStatusCode();
            Assert.Equal(flujo[i].Texto(), documento.QuerySelector("#estado-solicitud")!.TextContent.Trim());
            Assert.Contains(flujo[i].Texto(), documento.QuerySelector("#mensaje-estado")!.TextContent);
        }

        var guardada = await LeerSolicitudAsync(solicitud.Numero);
        Assert.Equal(EstadoSolicitud.Entregada, guardada.Estado);
        Assert.Equal(3, guardada.Cambios.Count);
    }

    [Fact]
    public async Task CP_HU05_02_UnaTransicionInvalidaSeRechazaEnElServidor()
    {
        var solicitud = await RegistrarAsync("invalida@ejemplo.com");
        var cliente = await aplicacion.ClienteAdministracionAsync();

        var (respuesta, documento) = await CambiarAsync(cliente, solicitud.Numero, EstadoSolicitud.Recibida, EstadoSolicitud.Entregada);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("No se puede pasar de Recibida a Entregada.", documento.QuerySelector("#error-estado")!.TextContent.Trim());
        var guardada = await LeerSolicitudAsync(solicitud.Numero);
        Assert.Equal(EstadoSolicitud.Recibida, guardada.Estado);
        Assert.Empty(guardada.Cambios);
    }

    [Fact]
    public async Task CP_HU05_03_ElHistorialGuardaUsuarioFechaYEstados()
    {
        var solicitud = await RegistrarAsync("historial@ejemplo.com");
        var cliente = await aplicacion.ClienteAdministracionAsync();
        var antes = DateTime.UtcNow.AddSeconds(-1);

        var (_, documento) = await CambiarAsync(cliente, solicitud.Numero, EstadoSolicitud.Recibida, EstadoSolicitud.Confirmada);

        var cambio = Assert.Single((await LeerSolicitudAsync(solicitud.Numero)).Cambios);
        Assert.Equal(CuentaAdministracion.CorreoPredeterminado, cambio.Usuario);
        Assert.Equal(EstadoSolicitud.Recibida, cambio.EstadoAnterior);
        Assert.Equal(EstadoSolicitud.Confirmada, cambio.EstadoNuevo);
        Assert.InRange(cambio.FechaUtc, antes, DateTime.UtcNow.AddSeconds(1));

        var entrada = documento.QuerySelector("#historial-estados .cambio-estado")!.TextContent;
        Assert.Contains("Confirmada", entrada);
        Assert.Contains("antes: Recibida", entrada);
        Assert.Contains(HoraCostaRica.Formatear(cambio.FechaUtc), entrada);
        Assert.Contains(CuentaAdministracion.CorreoPredeterminado, entrada);
    }

    [Fact]
    public async Task CP_HU05_04_UnaSolicitudEntregadaNoSePuedeCancelar()
    {
        var solicitud = await RegistrarAsync("entregada@ejemplo.com");
        var cliente = await aplicacion.ClienteAdministracionAsync();
        await CambiarAsync(cliente, solicitud.Numero, EstadoSolicitud.Recibida, EstadoSolicitud.Confirmada);
        await CambiarAsync(cliente, solicitud.Numero, EstadoSolicitud.Confirmada, EstadoSolicitud.EnEntrega);
        var (_, entregada) = await CambiarAsync(cliente, solicitud.Numero, EstadoSolicitud.EnEntrega, EstadoSolicitud.Entregada);
        Assert.NotNull(entregada.QuerySelector("#estado-final"));
        Assert.Null(entregada.QuerySelector("#formulario-estado"));

        var (_, documento) = await CambiarAsync(cliente, solicitud.Numero, EstadoSolicitud.Entregada, EstadoSolicitud.Cancelada);

        Assert.Equal("No se puede pasar de Entregada a Cancelada.", documento.QuerySelector("#error-estado")!.TextContent.Trim());
        Assert.Equal(EstadoSolicitud.Entregada, (await LeerSolicitudAsync(solicitud.Numero)).Estado);
    }

    [Fact]
    public async Task Un_estado_esperado_desactualizado_no_aplica_el_cambio()
    {
        var solicitud = await RegistrarAsync("concurrencia@ejemplo.com");
        var cliente = await aplicacion.ClienteAdministracionAsync();
        await CambiarAsync(cliente, solicitud.Numero, EstadoSolicitud.Recibida, EstadoSolicitud.Confirmada);

        var (_, documento) = await CambiarAsync(cliente, solicitud.Numero, EstadoSolicitud.Recibida, EstadoSolicitud.Cancelada);

        Assert.Contains("Otra persona cambió el estado", documento.QuerySelector("#error-estado")!.TextContent);
        var guardada = await LeerSolicitudAsync(solicitud.Numero);
        Assert.Equal(EstadoSolicitud.Confirmada, guardada.Estado);
        Assert.Single(guardada.Cambios);
    }

    [Fact]
    public async Task La_consulta_publica_muestra_el_historial_sin_el_usuario_del_personal()
    {
        var solicitud = await RegistrarAsync("publica@ejemplo.com");
        var admin = await aplicacion.ClienteAdministracionAsync();
        await CambiarAsync(admin, solicitud.Numero, EstadoSolicitud.Recibida, EstadoSolicitud.Confirmada);

        var cliente = aplicacion.CreateClient();
        var formulario = await LeerAsync(await cliente.GetStringAsync("/Solicitudes/Consultar"));
        var token = formulario.QuerySelector($"input[name={TokenCampo}]")!.GetAttribute("value")!;
        var respuesta = await cliente.PostAsync("/Solicitudes/Consultar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Datos.Numero"] = solicitud.Numero,
            ["Datos.Correo"] = "publica@ejemplo.com",
            [TokenCampo] = token
        }));
        var html = await respuesta.Content.ReadAsStringAsync();
        var historial = (await LeerAsync(html)).QuerySelector("#historial-estados")!.TextContent;

        Assert.Contains("Recibida", historial);
        Assert.Contains("Confirmada", historial);
        Assert.DoesNotContain(CuentaAdministracion.CorreoPredeterminado, html);
    }

    [Fact]
    public async Task Sin_sesion_no_se_puede_ver_ni_cambiar_una_solicitud()
    {
        var solicitud = await RegistrarAsync("sinsesion@ejemplo.com");
        var cliente = aplicacion.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var detalle = await cliente.GetAsync($"/Admin/Solicitud/{solicitud.Numero}");
        var cambio = await cliente.PostAsync($"/Admin/Solicitud/{solicitud.Numero}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["estadoEsperado"] = "Recibida",
            ["nuevo"] = "Cancelada"
        }));

        Assert.Equal(HttpStatusCode.Redirect, detalle.StatusCode);
        Assert.NotEqual(HttpStatusCode.OK, cambio.StatusCode);
        Assert.Equal(EstadoSolicitud.Recibida, (await LeerSolicitudAsync(solicitud.Numero)).Estado);
    }

    [Fact]
    public async Task Un_numero_inexistente_responde_404()
    {
        var cliente = await aplicacion.ClienteAdministracionAsync();

        var respuesta = await cliente.GetAsync("/Admin/Solicitud/SOL-1999-9999");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }
}
