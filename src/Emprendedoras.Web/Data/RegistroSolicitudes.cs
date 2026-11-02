using Emprendedoras.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace Emprendedoras.Web.Data;

// Registra solicitudes y les asigna un número único por año.
public class RegistroSolicitudes(ApplicationDbContext db, TimeProvider reloj)
{
    private const int MaximoIntentos = 3;

    public async Task<Solicitud> RegistrarAsync(Producto producto, string nombre, string correo, string? telefono, int cantidad)
    {
        var ahora = reloj.GetUtcNow();
        var solicitud = new Solicitud
        {
            Anio = ahora.ToOffset(HoraCostaRica.Desfase).Year,
            ProductoId = producto.Id,
            Cantidad = cantidad,
            PrecioUnitario = producto.Precio,
            NombrePersona = nombre.Trim(),
            Correo = correo.Trim().ToLowerInvariant(),
            Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim(),
            FechaCreacionUtc = ahora.UtcDateTime,
            FechaConsentimientoUtc = ahora.UtcDateTime
        };
        db.Solicitudes.Add(solicitud);

        // Dos solicitudes simultáneas pueden calcular la misma secuencia; el índice único
        // rechaza la segunda y se reintenta con la secuencia siguiente.
        for (var intento = 1; ; intento++)
        {
            var ultima = await db.Solicitudes.Where(s => s.Anio == solicitud.Anio).MaxAsync(s => (int?)s.Secuencia) ?? 0;
            solicitud.Secuencia = ultima + 1;
            solicitud.Numero = Solicitud.FormatearNumero(solicitud.Anio, solicitud.Secuencia);
            try
            {
                await db.SaveChangesAsync();
                return solicitud;
            }
            catch (DbUpdateException) when (intento < MaximoIntentos)
            {
            }
        }
    }
}
