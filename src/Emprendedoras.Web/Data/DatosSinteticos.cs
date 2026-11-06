using Emprendedoras.Web.Domain;

namespace Emprendedoras.Web.Data;

public static class DatosSinteticos
{
    public static readonly Producto[] Productos =
    [
        new()
        {
            Id = 1, Nombre = "Jabón artesanal de avena",
            Descripcion = "Jabón hecho a mano con avena y aceite de coco, para piel sensible. Barra de 120 g.",
            Precio = 3500, Emprendedora = "Ana Solís", Disponible = true,
            ImagenUrl = "/img/productos/jabon.svg", ImagenAlt = "Barra de jabón color crema sobre un plato pequeño"
        },
        new()
        {
            Id = 2, Nombre = "Café molido de altura",
            Descripcion = "Café de altura tostado medio y molido para chorreador. Bolsa de 500 g.",
            Precio = 6000, Emprendedora = "Carmen Mora", Disponible = true,
            ImagenUrl = "/img/productos/cafe.svg", ImagenAlt = "Bolsa de café de color oscuro con un grano dibujado al frente"
        },
        new()
        {
            Id = 3, Nombre = "Bolso tejido a mano",
            Descripcion = "Bolso de hilo de algodón tejido a mano, con asa larga. Mide 30 × 25 cm.",
            Precio = 18500, Emprendedora = "Rosa Jiménez", Disponible = false,
            ImagenUrl = "/img/productos/bolso.svg", ImagenAlt = "Bolso tejido de rayas verdes y beige con asa"
        },
        new()
        {
            Id = 4, Nombre = "Mermelada de guayaba",
            Descripcion = "Mermelada artesanal de guayaba, sin preservantes. Frasco de 250 g.",
            Precio = 2800, Emprendedora = "Laura Céspedes", Disponible = true,
            ImagenUrl = "/img/productos/mermelada.svg", ImagenAlt = "Frasco de vidrio con mermelada rosada y tapa a cuadros"
        },
        new()
        {
            Id = 5, Nombre = "Collar de semillas",
            Descripcion = "Collar elaborado con semillas naturales pulidas y cordón ajustable.",
            Precio = 7500, Emprendedora = "Sofía Araya", Disponible = true,
            ImagenUrl = "/img/productos/collar.svg", ImagenAlt = "Collar de semillas cafés y rojas en forma de arco"
        },
        new()
        {
            Id = 6, Nombre = "Vela de cera de abeja",
            Descripcion = "Vela aromática de cera de abeja con mecha de algodón. Dura unas 20 horas.",
            Precio = 4200, Emprendedora = "Elena Quesada", Disponible = false,
            ImagenUrl = "/img/productos/vela.svg", ImagenAlt = "Vela amarilla encendida en un vaso de vidrio"
        },
    ];
}
