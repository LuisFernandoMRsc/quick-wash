using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuickWash.Data;
using QuickWash.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Entity Framework Core con SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=quickwash.db";
builder.Services.AddDbContext<QuickWashDbContext>(options =>
    options.UseSqlite(connectionString));

// 2. Configurar Autenticación basada en Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccesoDenegado";
        options.LogoutPath = "/Account/Logout";
        options.Cookie.Name = "QuickWash.Auth";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(6);
        options.SlidingExpiration = true;
    });

// 3. Registrar Servicios del Negocio
builder.Services.AddScoped<IReservaService, ReservaService>();

// 4. Agregar soporte para Controladores y Vistas
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 5. Inicializar la base de datos con datos semilla
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<QuickWashDbContext>();
        await DbInitializer.InitializeAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos SQLite.");
    }
}

// 6. Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
