using Emprendedoras.Web.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Emprendedoras.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Producto> Productos => Set<Producto>();

    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Producto>().HasData(DatosSinteticos.Productos);

        builder.Entity<Solicitud>(solicitud =>
        {
            solicitud.HasIndex(s => s.Numero).IsUnique();
            solicitud.HasIndex(s => new { s.Anio, s.Secuencia }).IsUnique();
            solicitud.Property(s => s.Estado).HasConversion<string>().HasMaxLength(20);
            solicitud.HasOne(s => s.Producto).WithMany().HasForeignKey(s => s.ProductoId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
