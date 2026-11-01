using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Emprendedoras.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages().AddMvcOptions(options =>
{
    // Mensajes de conversión de datos en español (WCAG 3.3.1).
    var mensajes = options.ModelBindingMessageProvider;
    mensajes.SetAttemptedValueIsInvalidAccessor((_, campo) => $"El valor de {campo} no es válido.");
    mensajes.SetValueIsInvalidAccessor(_ => "El valor no es válido.");
    mensajes.SetValueMustBeANumberAccessor(campo => $"{campo} debe ser un número.");
    mensajes.SetValueMustNotBeNullAccessor(campo => $"{campo} es obligatorio.");
});
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<RegistroSolicitudes>();

var app = builder.Build();

// Create or update the SQLite database on startup (app.db is not versioned).
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

// Permite que las pruebas de integración usen WebApplicationFactory<Program>.
public partial class Program;
