using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Emprendedoras.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregaProductos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Precio = table.Column<int>(type: "INTEGER", nullable: false),
                    Emprendedora = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Disponible = table.Column<bool>(type: "INTEGER", nullable: false),
                    ImagenUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ImagenAlt = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Descripcion", "Disponible", "Emprendedora", "ImagenAlt", "ImagenUrl", "Nombre", "Precio" },
                values: new object[,]
                {
                    { 1, "Jabón hecho a mano con avena y aceite de coco, para piel sensible. Barra de 120 g.", true, "Ana Solís", "Barra de jabón color crema sobre un plato pequeño", "/img/productos/jabon.svg", "Jabón artesanal de avena", 3500 },
                    { 2, "Café de altura tostado medio y molido para chorreador. Bolsa de 500 g.", true, "Carmen Mora", "Bolsa de café de color oscuro con un grano dibujado al frente", "/img/productos/cafe.svg", "Café molido de altura", 6000 },
                    { 3, "Bolso de hilo de algodón tejido a mano, con asa larga. Mide 30 × 25 cm.", false, "Rosa Jiménez", "Bolso tejido de rayas verdes y beige con asa", "/img/productos/bolso.svg", "Bolso tejido a mano", 18500 },
                    { 4, "Mermelada artesanal de guayaba, sin preservantes. Frasco de 250 g.", true, "Laura Céspedes", "Frasco de vidrio con mermelada rosada y tapa a cuadros", "/img/productos/mermelada.svg", "Mermelada de guayaba", 2800 },
                    { 5, "Collar elaborado con semillas naturales pulidas y cordón ajustable.", true, "Sofía Araya", "Collar de semillas cafés y rojas en forma de arco", "/img/productos/collar.svg", "Collar de semillas", 7500 },
                    { 6, "Vela aromática de cera de abeja con mecha de algodón. Dura unas 20 horas.", false, "Elena Quesada", "Vela amarilla encendida en un vaso de vidrio", "/img/productos/vela.svg", "Vela de cera de abeja", 4200 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Productos");
        }
    }
}
