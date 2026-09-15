using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickWash.Data;
using QuickWash.Models;
using QuickWash.Models.ViewModels;
using QuickWash.Services;

namespace QuickWash.Controllers;

public class AccountController : Controller
{
    private readonly QuickWashDbContext _context;
    private readonly IReservaService _reservaService;
    private readonly PasswordHasher<Usuario> _hasher;

    public AccountController(QuickWashDbContext context, IReservaService reservaService)
    {
        _context = context;
        _reservaService = reservaService;
        _hasher = new PasswordHasher<Usuario>();
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());

        if (usuario == null)
        {
            ModelState.AddModelError(string.Empty, "Credenciales incorrectas o usuario no registrado.");
            return View(model);
        }

        var verificationResult = _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, model.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Credenciales incorrectas.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreCompleto),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Rol),
            new("CodigoEstudiante", usuario.CodigoEstudiante ?? "")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.Recordarme,
            ExpiresUtc = model.Recordarme ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(4)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        TempData["MensajeExito"] = $"¡Bienvenido/a, {usuario.Nombre}!";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        if (usuario.Rol == Roles.Personal)
        {
            return RedirectToAction("Index", "Gestion");
        }

        return RedirectToAction("Index", "Maquinas");
    }

    [HttpGet]
    public IActionResult Registro()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegistroViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroViewModel model)
    {
        // REGLA: El correo debe terminar en @est.univalle.edu
        if (!_reservaService.ValidarCorreoEstudiantil(model.Email))
        {
            ModelState.AddModelError("Email", "El correo institucional debe pertenecer al dominio '@est.univalle.edu'.");
        }

        var existeEmail = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());
        if (existeEmail)
        {
            ModelState.AddModelError("Email", "Este correo electrónico ya se encuentra registrado.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var nuevoUsuario = new Usuario
        {
            Nombre = model.Nombre.Trim(),
            Apellido = model.Apellido.Trim(),
            Email = model.Email.Trim().ToLower(),
            CodigoEstudiante = model.CodigoEstudiante.Trim().ToUpper(),
            Rol = Roles.Estudiante,
            FechaRegistro = DateTime.UtcNow
        };

        nuevoUsuario.PasswordHash = _hasher.HashPassword(nuevoUsuario, model.Password);

        _context.Usuarios.Add(nuevoUsuario);
        await _context.SaveChangesAsync();

        TempData["MensajeExito"] = "¡Cuenta creada exitosamente! Ahora puedes iniciar sesión con tu cuenta de Univalle.";
        return RedirectToAction("Login");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["MensajeInfo"] = "Has cerrado sesión correctamente.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}
