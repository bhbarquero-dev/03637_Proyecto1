using System.Net;
using AngleSharp;
using AngleSharp.Dom;
using Emprendedoras.Web.Data;
using Emprendedoras.Web.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Emprendedoras.Tests;

// HU-04 Acceso del personal y lista de solicitudes (issue #5).
public class AccesoPersonalTests(AplicacionDePrueba aplicacion) : IClassFixture<AplicacionDePrueba>
{
    private const string RutaAcceso = "/Cuenta/Acceso";
    private const string TokenCampo = "__RequestVerificationToken";

    private static Task<IDocument> LeerAsync(string html) =>
        BrowsingContext.New().OpenAsync(req => req.Content(html));

    private HttpClient ClienteSinRedireccion() =>
        aplicacion.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static async Task<HttpResponseMessage> IniciarSesionAsync(HttpClient cliente, string correo, string contrasena, string ruta = RutaAcceso)
    {
        var formulario = await LeerAsync(await cliente.GetStringAsync(ruta));
        var token = formulario.QuerySelector($"input[name={TokenCampo}]")!.GetAttribute("value")!;
        return await cliente.PostAsync(ruta, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Datos.Correo"] = correo,
            ["Datos.Contrasena"] = contrasena,
            [TokenCampo] = token
        }));
    }

    private async Task<string> CrearUsuarioSinRolAsync(string correo, string contrasena)
    {
        using var alcance = aplicacion.Services.CreateScope();
        var usuarios = alcance.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var resultado = await usuarios.CreateAsync(new IdentityUser { UserName = correo, Email = correo }, contrasena);
        Assert.True(resultado.Succeeded);
        return correo;
    }

    [Fact]
    public async Task CP_HU04_01_SinSesionAdminRedirigeAlInicioDeSesion()
    {
        var respuesta = await ClienteSinRedireccion().GetAsync("/Admin");

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.StartsWith(RutaAcceso, respuesta.Headers.Location!.PathAndQuery);
    }

    [Fact]
    public async Task CP_HU04_02_ElAdministradorIniciaSesionYVeLaListaOrdenada()
    {
        int[] anios = [2020, 2021];
        using (var alcance = aplicacion.Services.CreateScope())
        {
            var db = alcance.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            foreach (var anio in anios)
            {
                db.Solicitudes.Add(new Solicitud
                {
                    Numero = Solicitud.FormatearNumero(anio, 1), Anio = anio, Secuencia = 1, ProductoId = 1, Cantidad = 1,
                    PrecioUnitario = 3500, NombrePersona = "Persona Secreta", Correo = "secreta@ejemplo.com",
                    FechaCreacionUtc = new DateTime(anio, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    FechaConsentimientoUtc = new DateTime(anio, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });
            }
            await db.SaveChangesAsync();
        }

        var cliente = aplicacion.CreateClient();
        var respuesta = await IniciarSesionAsync(cliente, CuentaAdministracion.CorreoPredeterminado, aplicacion.ContrasenaAdministracion);

        respuesta.EnsureSuccessStatusCode();
        Assert.Equal("/Admin", respuesta.RequestMessage!.RequestUri!.AbsolutePath);
        var html = await respuesta.Content.ReadAsStringAsync();
        var documento = await LeerAsync(html);
        var numeros = documento.QuerySelectorAll("#lista-solicitudes tbody th").Select(c => c.TextContent.Trim()).ToList();
        Assert.True(numeros.IndexOf("SOL-2021-0001") < numeros.IndexOf("SOL-2020-0001"));
        var fila = documento.QuerySelector("#lista-solicitudes tbody tr:last-child")!.TextContent;
        Assert.Contains(DatosSinteticos.Productos[0].Nombre, fila);
        Assert.Contains("Recibida", fila);
        Assert.Contains("2019", fila); // 1 ene 2020 00:00 UTC es 31 dic 2019 en Costa Rica.
        Assert.DoesNotContain("Persona Secreta", html);
        Assert.DoesNotContain("secreta@ejemplo.com", html);
    }

    [Theory]
    [InlineData("/Identity/Account/Register")]
    [InlineData("/Identity/Account/Login")]
    [InlineData("/Identity/Account/Manage")]
    public async Task CP_HU04_03_LasPaginasDeRegistroEIdentityNoEstanDisponibles(string ruta)
    {
        var respuesta = await ClienteSinRedireccion().GetAsync(ruta);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task CP_HU04_04_TrasCincoIntentosFallidosLaCuentaQuedaBloqueada()
    {
        const string contrasena = "Contrasena-Prueba-1";
        var correo = await CrearUsuarioSinRolAsync("bloqueo@emprendedoras.test", contrasena);
        var cliente = ClienteSinRedireccion();

        for (var intento = 1; intento <= 5; intento++)
        {
            var fallido = await IniciarSesionAsync(cliente, correo, "Contrasena-Equivocada-1");
            Assert.Equal(HttpStatusCode.OK, fallido.StatusCode);
        }

        // Con la cuenta bloqueada, ni la contraseña correcta permite entrar.
        var respuesta = await IniciarSesionAsync(cliente, correo, contrasena);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var documento = await LeerAsync(await respuesta.Content.ReadAsStringAsync());
        Assert.NotNull(documento.QuerySelector("#acceso-rechazado"));

        using var alcance = aplicacion.Services.CreateScope();
        var usuarios = alcance.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        Assert.True(await usuarios.IsLockedOutAsync((await usuarios.FindByNameAsync(correo))!));
    }

    [Fact]
    public async Task CP_HU04_05_UnUsuarioSinElRolNoAccedeAAdmin()
    {
        const string contrasena = "Contrasena-Prueba-2";
        var correo = await CrearUsuarioSinRolAsync("sinrol@emprendedoras.test", contrasena);
        var cliente = aplicacion.CreateClient();

        var respuesta = await IniciarSesionAsync(cliente, correo, contrasena, $"{RutaAcceso}?ReturnUrl=%2FAdmin");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        var html = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("acceso-denegado", html);
        Assert.DoesNotContain("lista-solicitudes", html);
    }

    [Fact]
    public async Task Un_correo_desconocido_y_una_contrasena_equivocada_muestran_el_mismo_mensaje()
    {
        const string contrasena = "Contrasena-Prueba-3";
        var correo = await CrearUsuarioSinRolAsync("mensaje@emprendedoras.test", contrasena);

        var conocido = await IniciarSesionAsync(ClienteSinRedireccion(), correo, "Contrasena-Equivocada-1");
        var desconocido = await IniciarSesionAsync(ClienteSinRedireccion(), "nadie@emprendedoras.test", "Contrasena-Equivocada-1");

        var mensajeConocido = (await LeerAsync(await conocido.Content.ReadAsStringAsync())).QuerySelector("#acceso-rechazado")!.TextContent;
        var mensajeDesconocido = (await LeerAsync(await desconocido.Content.ReadAsStringAsync())).QuerySelector("#acceso-rechazado")!.TextContent;
        Assert.Equal(mensajeConocido, mensajeDesconocido);
    }

    [Fact]
    public async Task ReturnUrl_externo_no_redirige_a_otro_sitio()
    {
        var cliente = ClienteSinRedireccion();

        var respuesta = await IniciarSesionAsync(cliente, CuentaAdministracion.CorreoPredeterminado, aplicacion.ContrasenaAdministracion,
            $"{RutaAcceso}?ReturnUrl=https%3A%2F%2Fejemplo.com%2F");

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal("/Admin", respuesta.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task La_barra_no_muestra_login_ni_register_y_el_pie_enlaza_el_acceso()
    {
        var documento = await LeerAsync(await aplicacion.CreateClient().GetStringAsync("/"));

        var barra = documento.QuerySelector("nav")!.TextContent;
        Assert.DoesNotContain("Login", barra);
        Assert.DoesNotContain("Register", barra);
        Assert.Equal(RutaAcceso, documento.QuerySelectorAll("footer a").First(a => a.TextContent == "Acceso del personal").GetAttribute("href"));
    }

    [Fact]
    public async Task Cerrar_sesion_vuelve_a_exigir_el_acceso()
    {
        var cliente = aplicacion.CreateClient();
        var lista = await IniciarSesionAsync(cliente, CuentaAdministracion.CorreoPredeterminado, aplicacion.ContrasenaAdministracion);
        var documento = await LeerAsync(await lista.Content.ReadAsStringAsync());
        var token = documento.QuerySelector($"form[action='/Cuenta/Salir'] input[name={TokenCampo}]")!.GetAttribute("value")!;

        var salida = await cliente.PostAsync("/Cuenta/Salir", new FormUrlEncodedContent(new Dictionary<string, string> { [TokenCampo] = token }));
        salida.EnsureSuccessStatusCode();

        var respuesta = await cliente.GetAsync("/Admin");
        Assert.Equal(RutaAcceso, respuesta.RequestMessage!.RequestUri!.AbsolutePath);
    }
}
