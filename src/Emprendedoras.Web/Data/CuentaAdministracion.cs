using Microsoft.AspNetCore.Identity;

namespace Emprendedoras.Web.Data;

public static class CuentaAdministracion
{
    public const string Rol = "Administración";

    public const string Politica = "SoloAdministracion";

    public const string CorreoPredeterminado = "administracion@emprendedoras.test";

    public static async Task CrearAsync(IServiceProvider servicios)
    {
        var roles = servicios.GetRequiredService<RoleManager<IdentityRole>>();
        var usuarios = servicios.GetRequiredService<UserManager<IdentityUser>>();
        var configuracion = servicios.GetRequiredService<IConfiguration>();
        var registro = servicios.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(CuentaAdministracion));

        if (!await roles.RoleExistsAsync(Rol))
        {
            Comprobar(await roles.CreateAsync(new IdentityRole(Rol)));
        }

        var correo = configuracion["Administracion:Correo"] ?? CorreoPredeterminado;
        if (await usuarios.FindByNameAsync(correo) is not null)
        {
            return;
        }

        var contrasena = configuracion["Administracion:Contrasena"];
        if (string.IsNullOrEmpty(contrasena))
        {
            registro.LogWarning("No se creó el administrador: falta configurar Administracion:Contrasena.");
            return;
        }

        var usuario = new IdentityUser { UserName = correo, Email = correo, EmailConfirmed = true };
        var creado = await usuarios.CreateAsync(usuario, contrasena);
        if (!creado.Succeeded)
        {
            registro.LogError("No se creó el administrador: la contraseña de Administracion:Contrasena no cumple la política ({Faltas}).",
                string.Join(" ", creado.Errors.Select(e => Explicar(e.Code))));
            return;
        }
        Comprobar(await usuarios.AddToRoleAsync(usuario, Rol));
    }

    private static string Explicar(string codigo) => codigo switch
    {
        "PasswordTooShort" => "Debe tener al menos 12 caracteres.",
        "PasswordRequiresUpper" => "Debe incluir una mayúscula.",
        "PasswordRequiresLower" => "Debe incluir una minúscula.",
        "PasswordRequiresDigit" => "Debe incluir un número.",
        "PasswordRequiresNonAlphanumeric" => "Debe incluir un símbolo.",
        _ => codigo
    };

    private static void Comprobar(IdentityResult resultado)
    {
        if (!resultado.Succeeded)
        {
            var errores = string.Join(", ", resultado.Errors.Select(e => e.Code));
            throw new InvalidOperationException($"No se pudo preparar la cuenta de administración: {errores}.");
        }
    }
}
