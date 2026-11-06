using Microsoft.Playwright;
using Xunit.Abstractions;

namespace Emprendedoras.Tests;

[Trait("Categoria", "Navegador")]
public class TecladoTests(ServidorNavegador servidor, ITestOutputHelper salida) : IClassFixture<ServidorNavegador>
{
    private const int MaximoTabulaciones = 40;

    private int tabulaciones;

    private readonly List<string> sinFocoVisible = [];

    private async Task TabularHastaAsync(IPage pagina, string condicion, string descripcion)
    {
        for (var i = 0; i < MaximoTabulaciones; i++)
        {
            await pagina.Keyboard.PressAsync("Tab");
            tabulaciones++;
            var foco = await pagina.EvaluateAsync<string[]>("""
                (() => {
                    const e = document.activeElement;
                    const s = getComputedStyle(e);
                    const visible = (s.outlineStyle !== 'none' && parseFloat(s.outlineWidth) > 0) || s.boxShadow !== 'none';
                    return [e.outerHTML.slice(0, 80), String(visible)];
                })()
                """);
            if (foco[1] != "true")
            {
                sinFocoVisible.Add(foco[0]);
            }
            if (await pagina.EvaluateAsync<bool>($"(() => {{ const e = document.activeElement; return {condicion}; }})()"))
            {
                return;
            }
        }
        Assert.Fail($"No se llegó con Tab a {descripcion}.");
    }

    [Fact]
    public async Task Se_completa_una_solicitud_solo_con_teclado()
    {
        var pagina = await servidor.NuevaPaginaAsync(360, 740);
        await pagina.GotoAsync("/");

        await TabularHastaAsync(pagina, "e.textContent.includes('Solicitar') && e.getAttribute('href') === '/Solicitudes/Nueva/1'", "el botón Solicitar del primer producto");
        await pagina.Keyboard.PressAsync("Enter");
        await pagina.WaitForURLAsync("**/Solicitudes/Nueva/1");

        await TabularHastaAsync(pagina, "e.id === 'Datos_Nombre'", "el campo Nombre");
        await pagina.Keyboard.TypeAsync("Persona Sintética");
        await TabularHastaAsync(pagina, "e.id === 'Datos_Correo'", "el campo Correo");
        await pagina.Keyboard.TypeAsync("teclado@ejemplo.com");
        await TabularHastaAsync(pagina, "e.id === 'Datos_Cantidad'", "el campo Cantidad");
        await pagina.Keyboard.PressAsync("Control+A");
        await pagina.Keyboard.TypeAsync("2");
        await TabularHastaAsync(pagina, "e.id === 'Datos_Consentimiento'", "la casilla de consentimiento");
        await pagina.Keyboard.PressAsync("Space");
        await TabularHastaAsync(pagina, "e.type === 'submit'", "el botón Enviar solicitud");
        await pagina.Keyboard.PressAsync("Enter");

        await pagina.WaitForURLAsync("**/Solicitudes/Confirmacion");
        Assert.StartsWith("SOL-", (await pagina.TextContentAsync("#numero-solicitud"))!.Trim());
        salida.WriteLine($"Solicitud completada con {tabulaciones} pulsaciones de Tab.");
        foreach (var elemento in sinFocoVisible)
        {
            salida.WriteLine($"Sin foco visible: {elemento}");
        }
        Assert.Empty(sinFocoVisible);
    }

    [Fact]
    public async Task El_primer_Tab_lleva_al_enlace_para_saltar_al_contenido()
    {
        var pagina = await servidor.NuevaPaginaAsync(360, 740);
        await pagina.GotoAsync("/");

        await pagina.Keyboard.PressAsync("Tab");

        Assert.Equal("Saltar al contenido", await pagina.EvaluateAsync<string>("document.activeElement.textContent.trim()"));
        Assert.True(await pagina.EvaluateAsync<bool>("document.activeElement.getBoundingClientRect().left >= 0"));
    }
}
