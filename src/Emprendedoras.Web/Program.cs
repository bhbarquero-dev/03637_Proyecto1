using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Emprendedoras.Web.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 12;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Cuenta/Acceso";
    options.AccessDeniedPath = "/Cuenta/Denegado";
});
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(CuentaAdministracion.Politica, politica => politica.RequireRole(CuentaAdministracion.Rol));
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", CuentaAdministracion.Politica);
}).AddMvcOptions(options =>
{
    var mensajes = options.ModelBindingMessageProvider;
    mensajes.SetAttemptedValueIsInvalidAccessor((_, campo) => $"El valor de {campo} no es válido.");
    mensajes.SetValueIsInvalidAccessor(_ => "El valor no es válido.");
    mensajes.SetValueMustBeANumberAccessor(campo => $"{campo} debe ser un número.");
    mensajes.SetValueMustNotBeNullAccessor(campo => $"{campo} es obligatorio.");
});
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<RegistroSolicitudes>();
builder.Services.AddScoped<GestionEstados>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
    await CuentaAdministracion.CrearAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    var cabeceras = context.Response.Headers;
    cabeceras.ContentSecurityPolicy = "default-src 'self'; img-src 'self' data:; " +
        "style-src-attr 'unsafe-hashes' 'sha256-aqNNdDLnnrDOnTNdkJpYlAxKVJtLt9CtFLklmInuUAE='; " +
        "object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";
    cabeceras.XContentTypeOptions = "nosniff";
    cabeceras.XFrameOptions = "DENY";
    cabeceras["Referrer-Policy"] = "strict-origin-when-cross-origin";
    cabeceras["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

public partial class Program;
