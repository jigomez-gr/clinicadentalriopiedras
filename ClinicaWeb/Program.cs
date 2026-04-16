using ClinicaData.Configuracion;
using ClinicaData.Contrato;
using ClinicaData.Implementacion;
using ClinicaData.Repositorio;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// --- BLOQUE DE RED PARA VPS (DOKPLOY) ---
if (!builder.Environment.IsDevelopment())
{
    // Mantenemos tu puerto configurado
    builder.WebHost.UseUrls("http://*:4431");
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
// ----------------------------------------

builder.Services.AddControllersWithViews();

builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.AddScoped<ICitaRepositorio, CitaRepositorio>();
builder.Services.AddScoped<IDoctorRepositorio, DoctorRepositorio>();
builder.Services.AddScoped<IEspecialidadRepositorio, EspecialidadRepositorio>();
builder.Services.AddScoped<IRolUsuarioRepositorio, RolUsuarioRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IAnalisisVapiRepositorio, AnalisisVapiRepositorio>();
builder.Services.AddScoped<IZadarmaSmsRespuestaRepositorio, ZadarmaSmsRespuestaRepositorio>();

// --- CONFIGURACIÓN DE LÍMITES PARA ARCHIVOS PESADOS ---
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 104857600;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = 104857600;
    options.MultipartBodyLengthLimit = 104857600;
    options.MemoryBufferThreshold = 104857600;
});

// --- CONFIGURACIÓN DE COOKIES MODIFICADA PARA PERMITIR ACCESO EXTERNO ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Acceso/Login";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        option.AccessDeniedPath = "/Acceso/Denegado";

        // MODIFICACIÓN: Esto evita que la cookie bloquee la navegación si el navegador 
        // considera que la petición es "cross-site" (como desde Telegram)
        option.Cookie.SameSite = SameSiteMode.Lax;
        option.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

var app = builder.Build();

// IMPORTANTE: UseForwardedHeaders debe ir de los primeros para que el sistema sepa que es HTTPS
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // HSTS ayuda a la seguridad en producción
    app.UseHsts();
}

// Comentamos Redirection si Dokploy ya maneja el SSL internamente para evitar bucles, 
// pero es vital que UseForwardedHeaders esté arriba.
// app.UseHttpsRedirection(); 

app.UseStaticFiles();
app.UseRouting();

// EL ORDEN AQUÍ ES CRÍTICO
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}");

// Inyección de la cadena de conexión en tu clase estática
var connectionString = builder.Configuration.GetConnectionString("CadenaSQL");
ClinicaWeb.Controllers.Conexion.CN = connectionString;

app.Run();