using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickWash.Models;
using QuickWash.Services;

namespace QuickWash.Controllers;

[Authorize(Roles = Roles.Personal)]
public class GestionController : Controller
{
    private readonly IReservaService _reservaService;

    public GestionController(IReservaService reservaService)
    {
        _reservaService = reservaService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? fecha, string? estado)
    {
        var fechaFiltro = fecha ?? DateTime.Today;
        var reservas = await _reservaService.ObtenerTodasLasReservasAsync(fechaFiltro, estado);

        ViewBag.FechaActual = fechaFiltro.ToString("yyyy-MM-dd");
        ViewBag.EstadoActual = estado ?? "Todos";
        ViewBag.EstadosPermitidos = EstadosReserva.Todos;

        return View(reservas);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int reservaId, string nuevoEstado, string? returnUrl = null)
    {
        var (exito, mensaje) = await _reservaService.ModificarEstadoPersonalAsync(reservaId, nuevoEstado);

        if (exito)
        {
            TempData["MensajeExito"] = mensaje;
        }
        else
        {
            TempData["MensajeError"] = mensaje;
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }
}
