using Emprendedoras.Web.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Emprendedoras.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Producto> Productos => Set<Producto>();

    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();

    public DbSet<CambioEstado> CambiosEstado => Set<CambioEstado>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Producto>().HasData(DatosSinteticos.Productos);

        builder.Entity<Solicitud>(solicitud =>
        {
            solicitud.HasIndex(s => s.Numero).IsUnique();
            solicitud.HasIndex(s => new { s.Anio, s.Secuencia }).IsUnique();
            // Token de concurrencia: si dos personas cambian el estado a la vez, la segunda se rechaza.
            solicitud.Property(s => s.Estado).HasConversion<string>().HasMaxLength(20).IsConcurrencyToken();
            solicitud.HasOne(s => s.Producto).WithMany().HasForeignKey(s => s.ProductoId).OnDelete(DeleteBehavior.Restrict);
            solicitud.HasMany(s => s.Cambios).WithOne().HasForeignKey(c => c.SolicitudId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CambioEstado>(cambio =>
        {
            cambio.Property(c => c.EstadoAnterior).HasConversion<string>().HasMaxLength(20);
            cambio.Property(c => c.EstadoNuevo).HasConversion<string>().HasMaxLength(20);
        });
    }
}
