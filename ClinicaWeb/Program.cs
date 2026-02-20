using ClinicaData.Configuracion;
using ClinicaData.Contrato;
using ClinicaData.Implementacion;
using ClinicaData.Repositorio;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// --- BLOQUE DE RED PARA VPS (DOKPLOY) ---
// Si detecta que está en el servidor (Production), fuerza el puerto 4431
// Si estás en local (Development), usará el puerto de Visual Studio (IIS Express/Kestrel)
if (!builder.Environment.IsDevelopment())
{
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

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Acceso/Login";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        option.AccessDeniedPath = "/Acceso/Denegado";
    });

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}");

app.Run();