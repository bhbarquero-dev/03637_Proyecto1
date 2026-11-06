using System.Globalization;

namespace Emprendedoras.Web.Domain;

public static class HoraCostaRica
{
    public static readonly TimeSpan Desfase = TimeSpan.FromHours(-6);

    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-CR");

    public static string Formatear(DateTime utc) =>
        new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)).ToOffset(Desfase)
            .ToString("d 'de' MMMM 'de' yyyy, HH:mm", Cultura);
}
