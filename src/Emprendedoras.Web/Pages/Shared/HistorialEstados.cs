using Emprendedoras.Web.Domain;

namespace Emprendedoras.Web.Pages.Shared;

// Datos del historial de estados (HU-05). El usuario que hizo cada cambio solo se muestra al personal.
public record HistorialEstados(Solicitud Solicitud, bool MostrarUsuario);
