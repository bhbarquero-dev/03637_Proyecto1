using Emprendedoras.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace Emprendedoras.Web.Data;

public class GestionEstados(ApplicationDbContext db, TimeProvider reloj)
{
    public enum Resultado
    {
        Aplicado,
        NoExiste,
        TransicionInvalida,
        CambiadoPorOtraPersona
    }

    public async Task<Resultado> CambiarAsync(string numero, EstadoSolicitud estadoEsperado, EstadoSolicitud nuevo, string usuario)
    {
        var solicitud = await db.Solicitudes.FirstOrDefaultAsync(s => s.Numero == numero);
        if (solicitud is null)
        {
            return Resultado.NoExiste;
        }
        if (solicitud.Estado != estadoEsperado)
        {
            return Resultado.CambiadoPorOtraPersona;
        }
        if (!solicitud.Estado.PuedePasarA(nuevo))
        {
            return Resultado.TransicionInvalida;
        }

        db.CambiosEstado.Add(new CambioEstado
        {
            SolicitudId = solicitud.Id,
            EstadoAnterior = solicitud.Estado,
            EstadoNuevo = nuevo,
            FechaUtc = reloj.GetUtcNow().UtcDateTime,
            Usuario = usuario
        });
        solicitud.Estado = nuevo;
        try
        {
            await db.SaveChangesAsync();
            return Resultado.Aplicado;
        }
        catch (DbUpdateConcurrencyException)
        {
            return Resultado.CambiadoPorOtraPersona;
        }
    }
}
