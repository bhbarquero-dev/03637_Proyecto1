using System.Globalization;

namespace Emprendedoras.Web.Domain;

// Fechas en hora de Costa Rica, que no usa horario de verano: UTC-6 todo el año.
public static class HoraCostaRica
{
    public static readonly TimeSpan Desfase = TimeSpan.FromHours(-6);

    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-CR");

    // Por ejemplo: "1 de noviembre de 2026, 11:58".
    public static string Formatear(DateTime utc) =>
        new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)).ToOffset(Desfase)
            .ToString("d 'de' MMMM 'de' yyyy, HH:mm", Cultura);
}
