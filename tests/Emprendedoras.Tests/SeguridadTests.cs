namespace Emprendedoras.Tests;

public class SeguridadTests(AplicacionDePrueba aplicacion) : IClassFixture<AplicacionDePrueba>
{
    [Theory]
    [InlineData("/")]
    [InlineData("/Solicitudes/Consultar")]
    [InlineData("/Cuenta/Acceso")]
    [InlineData("/css/site.css")]
    public async Task Todas_las_respuestas_llevan_las_cabeceras_de_seguridad(string ruta)
    {
        var respuesta = await aplicacion.CreateClient().GetAsync(ruta);

        respuesta.EnsureSuccessStatusCode();
        var politica = string.Join(";", respuesta.Headers.GetValues("Content-Security-Policy"));
        Assert.Contains("default-src 'self'", politica);
        Assert.Contains("frame-ancestors 'none'", politica);
        Assert.Contains("object-src 'none'", politica);
        Assert.Equal("nosniff", respuesta.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", respuesta.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("strict-origin-when-cross-origin", respuesta.Headers.GetValues("Referrer-Policy").Single());
    }

    [Fact]
    public async Task Las_paginas_del_personal_no_se_guardan_en_cache()
    {
        var cliente = await aplicacion.ClienteAdministracionAsync();

        var respuesta = await cliente.GetAsync("/Admin");

        respuesta.EnsureSuccessStatusCode();
        Assert.True(respuesta.Headers.CacheControl!.NoStore);
    }
}
