using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using Xunit.Abstractions;

namespace Emprendedoras.Tests;

[Trait("Categoria", "Navegador")]
public class AccesibilidadTests(ServidorNavegador servidor, ITestOutputHelper salida) : IClassFixture<ServidorNavegador>
{
    private static readonly AxeRunOptions Wcag22AA = new()
    {
        RunOnly = new RunOnlyOptions
        {
            Type = "tag",
            Values = ["wcag2a", "wcag2aa", "wcag21a", "wcag21aa", "wcag22a", "wcag22aa"]
        }
    };

    private readonly List<string> hallazgos = [];

    private async Task<IPage> PaginaAsync(int ancho)
    {
        var pagina = await servidor.NuevaPaginaAsync(ancho);
        pagina.Console += (_, mensaje) =>
        {
            if (mensaje.Type == "error")
            {
                hallazgos.Add($"Consola {ancho} en {new Uri(pagina.Url).PathAndQuery}: {mensaje.Text}");
            }
        };
        return pagina;
    }

    private async Task RevisarAsync(IPage pagina, string estado)
    {
        var resultado = await pagina.RunAxe(Wcag22AA);
        foreach (var violacion in resultado.Violations)
        {
            hallazgos.Add($"{estado}: {violacion.Id} ({violacion.Impact}) {violacion.Help} → {string.Join(", ", violacion.Nodes.Select(n => n.Target.ToString()))}");
        }

        var ancho = await pagina.EvaluateAsync<int[]>("[document.documentElement.scrollWidth, window.innerWidth]");
        if (ancho[0] > ancho[1])
        {
            hallazgos.Add($"{estado}: desplazamiento horizontal ({ancho[0]} px en una ventana de {ancho[1]} px)");
        }
        salida.WriteLine($"{estado}: {resultado.Violations.Length} incumplimientos, {resultado.Passes.Length} reglas cumplidas");
    }

    private void Comprobar()
    {
        foreach (var hallazgo in hallazgos)
        {
            salida.WriteLine(hallazgo);
        }
        Assert.Empty(hallazgos);
    }

    [Theory]
    [InlineData(360)]
    [InlineData(1280)]
    public async Task Las_paginas_publicas_no_tienen_incumplimientos(int ancho)
    {
        var pagina = await PaginaAsync(ancho);

        await pagina.GotoAsync("/");
        await RevisarAsync(pagina, $"Catálogo {ancho}");
        await pagina.GotoAsync("/Privacy");
        await RevisarAsync(pagina, $"Privacidad {ancho}");

        await pagina.GotoAsync("/Solicitudes/Nueva/1");
        await RevisarAsync(pagina, $"Solicitud vacía {ancho}");
        await pagina.ClickAsync("form.formulario-solicitud button[type=submit]");
        await RevisarAsync(pagina, $"Solicitud con errores {ancho}");
        await pagina.FillAsync("#Datos_Nombre", "Persona Sintética");
        await pagina.FillAsync("#Datos_Correo", $"axe{ancho}@ejemplo.com");
        await pagina.CheckAsync("#Datos_Consentimiento");
        await pagina.ClickAsync("form.formulario-solicitud button[type=submit]");
        await pagina.WaitForURLAsync("**/Solicitudes/Confirmacion");
        var numero = (await pagina.TextContentAsync("#numero-solicitud"))!.Trim();
        await RevisarAsync(pagina, $"Confirmación {ancho}");

        await pagina.GotoAsync("/Solicitudes/Consultar");
        await RevisarAsync(pagina, $"Consulta vacía {ancho}");
        await pagina.FillAsync("#Datos_Numero", numero);
        await pagina.FillAsync("#Datos_Correo", "otra@ejemplo.com");
        await pagina.ClickAsync("form button[type=submit]");
        await pagina.WaitForSelectorAsync("#sin-resultado");
        await RevisarAsync(pagina, $"Consulta sin resultado {ancho}");
        await pagina.FillAsync("#Datos_Numero", numero);
        await pagina.FillAsync("#Datos_Correo", $"axe{ancho}@ejemplo.com");
        await pagina.ClickAsync("form button[type=submit]");
        await pagina.WaitForSelectorAsync("#resultado-consulta");
        await RevisarAsync(pagina, $"Consulta con resultado {ancho}");

        await pagina.GotoAsync("/Cuenta/Acceso");
        await RevisarAsync(pagina, $"Acceso del personal {ancho}");
        await pagina.FillAsync("#Datos_Correo", "nadie@emprendedoras.test");
        await pagina.FillAsync("#Datos_Contrasena", "Contrasena-Equivocada-1");
        await pagina.ClickAsync("form button[type=submit]");
        await pagina.WaitForSelectorAsync("#acceso-rechazado");
        await RevisarAsync(pagina, $"Acceso rechazado {ancho}");

        Comprobar();
    }

    [Theory]
    [InlineData(360)]
    [InlineData(1280)]
    public async Task Las_paginas_del_personal_no_tienen_incumplimientos(int ancho)
    {
        var compra = await PaginaAsync(ancho);
        await compra.GotoAsync("/Solicitudes/Nueva/2");
        await compra.FillAsync("#Datos_Nombre", "Persona Sintética");
        await compra.FillAsync("#Datos_Correo", $"personal{ancho}@ejemplo.com");
        await compra.CheckAsync("#Datos_Consentimiento");
        await compra.ClickAsync("form.formulario-solicitud button[type=submit]");
        await compra.WaitForURLAsync("**/Solicitudes/Confirmacion");
        var numero = (await compra.TextContentAsync("#numero-solicitud"))!.Trim();

        var pagina = await PaginaAsync(ancho);
        await servidor.IniciarSesionAsync(pagina);
        await RevisarAsync(pagina, $"Lista de solicitudes {ancho}");
        await pagina.GotoAsync($"/Admin/Solicitud/{numero}");
        await RevisarAsync(pagina, $"Detalle {ancho}");
        await pagina.ClickAsync("button[value=Confirmada]");
        await pagina.WaitForSelectorAsync("#mensaje-estado");
        await RevisarAsync(pagina, $"Detalle tras cambio {ancho}");

        Comprobar();
    }
}
