using System.Diagnostics;
using Microsoft.Playwright;
using Xunit.Abstractions;

namespace Emprendedoras.Tests;

[Trait("Categoria", "Navegador")]
public class RendimientoTests(ServidorNavegador servidor, ITestOutputHelper salida) : IClassFixture<ServidorNavegador>
{
    private static readonly (string Nombre, double Bajada, double Subida, double Latencia) Fast3G = ("3G rápida", 180_000, 84_375, 562.5);

    private async Task<(double Segundos, double Kilobytes)> MedirAsync((string Nombre, double Bajada, double Subida, double Latencia) red, bool recorrerPagina)
    {
        var pagina = await servidor.NuevaPaginaAsync(360);
        var cdp = await pagina.Context.NewCDPSessionAsync(pagina);
        double bytes = 0;
        cdp.Event("Network.loadingFinished").OnEvent += (_, e) => bytes += e!.Value.GetProperty("encodedDataLength").GetDouble();
        await cdp.SendAsync("Network.enable");
        await cdp.SendAsync("Network.setCacheDisabled", new Dictionary<string, object> { ["cacheDisabled"] = true });
        await cdp.SendAsync("Network.emulateNetworkConditions", new Dictionary<string, object>
        {
            ["offline"] = false,
            ["latency"] = red.Latencia,
            ["downloadThroughput"] = red.Bajada,
            ["uploadThroughput"] = red.Subida
        });

        var reloj = Stopwatch.StartNew();
        await pagina.GotoAsync("/", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 60_000 });
        reloj.Stop();
        if (recorrerPagina)
        {
            await pagina.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
            await pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        var carga = await pagina.EvaluateAsync<double>("performance.getEntriesByType('navigation')[0].loadEventEnd");
        var resultado = (Math.Max(carga, reloj.ElapsedMilliseconds) / 1000, bytes / 1024);
        salida.WriteLine($"{red.Nombre}{(recorrerPagina ? ", página recorrida" : "")}: {resultado.Item1:F2} s, {resultado.Item2:F0} KB transferidos");
        return resultado;
    }

    [Fact]
    public async Task M04_El_catalogo_carga_en_5_segundos_o_menos_con_3G()
    {
        var (segundos, kilobytes) = await MedirAsync(Fast3G, recorrerPagina: false);

        Assert.InRange(segundos, 0, 5);
        Assert.InRange(kilobytes, 0, 1024);
    }

    [Fact]
    public async Task M03_El_catalogo_completo_pesa_1_MB_o_menos()
    {
        var (_, kilobytes) = await MedirAsync(Fast3G, recorrerPagina: true);

        Assert.InRange(kilobytes, 0, 1024);
    }
}
