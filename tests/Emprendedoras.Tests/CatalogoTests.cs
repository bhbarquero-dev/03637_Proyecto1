using AngleSharp;
using AngleSharp.Dom;
using Emprendedoras.Web.Data;
using Emprendedoras.Web.Pages;

namespace Emprendedoras.Tests;

public class CatalogoTests(AplicacionDePrueba aplicacion) : IClassFixture<AplicacionDePrueba>
{
    private async Task<IDocument> AbrirCatalogoAsync()
    {
        var respuesta = await aplicacion.CreateClient().GetAsync("/");
        respuesta.EnsureSuccessStatusCode();
        var html = await respuesta.Content.ReadAsStringAsync();
        return await BrowsingContext.New().OpenAsync(req => req.Content(html));
    }

    private static IElement TarjetaDe(IDocument documento, string nombre) =>
        documento.QuerySelectorAll("article.producto")
            .Single(t => t.QuerySelector("h2")!.TextContent.Trim() == nombre);

    [Fact]
    public async Task CP_HU01_01_MuestraTodosLosProductosConSusDatos()
    {
        var documento = await AbrirCatalogoAsync();

        Assert.Equal(DatosSinteticos.Productos.Length, documento.QuerySelectorAll("article.producto").Length);
        foreach (var producto in DatosSinteticos.Productos)
        {
            var tarjeta = TarjetaDe(documento, producto.Nombre);
            Assert.Contains(producto.Descripcion, tarjeta.TextContent);
            Assert.Contains(IndexModel.FormatearPrecio(producto.Precio), tarjeta.TextContent);
            Assert.Contains(producto.Emprendedora, tarjeta.TextContent);
            Assert.Equal(producto.ImagenUrl, tarjeta.QuerySelector("img")!.GetAttribute("src"));
        }
    }

    [Fact]
    public async Task CP_HU01_02_ProductoAgotadoMuestraAgotadoYNoPermiteSolicitar()
    {
        var documento = await AbrirCatalogoAsync();

        foreach (var producto in DatosSinteticos.Productos)
        {
            var tarjeta = TarjetaDe(documento, producto.Nombre);
            var disponibilidad = tarjeta.QuerySelector(".disponibilidad")!.TextContent.Trim();
            Assert.Equal(producto.Disponible ? "Disponible" : "Agotado", disponibilidad);
            if (!producto.Disponible)
            {
                Assert.Empty(tarjeta.QuerySelectorAll("a, button, form"));
            }
        }
    }

    [Fact]
    public async Task CP_HU01_04_TodasLasImagenesTienenTextoAlternativo()
    {
        var documento = await AbrirCatalogoAsync();

        var imagenes = documento.QuerySelectorAll("main img");
        Assert.NotEmpty(imagenes);
        Assert.All(imagenes, img => Assert.False(string.IsNullOrWhiteSpace(img.GetAttribute("alt"))));
    }

    [Fact]
    public async Task La_pagina_declara_el_idioma_espanol()
    {
        var documento = await AbrirCatalogoAsync();

        Assert.Equal("es", documento.DocumentElement.GetAttribute("lang"));
    }

    [Theory]
    [InlineData(3500, "3500")]
    [InlineData(18500, "18500")]
    public void El_precio_se_muestra_en_colones_sin_decimales(int precio, string digitos)
    {
        var texto = IndexModel.FormatearPrecio(precio);

        Assert.StartsWith("₡", texto);
        Assert.DoesNotContain(",", texto);
        Assert.Equal(digitos, new string(texto.Where(char.IsDigit).ToArray()));
    }
}
