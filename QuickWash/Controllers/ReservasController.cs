using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuickWash.Data;
using QuickWash.Models;
using QuickWash.Models.ViewModels;
using QuickWash.Services;

namespace QuickWash.Controllers;

[Authorize(Roles = Roles.Estudiante)]
public class ReservasController : Controller
{
    private readonly QuickWashDbContext _context;
    private readonly IReservaService _reservaService;

    public ReservasController(QuickWashDbContext context, IReservaService reservaService)
    {
        _context = context;
        _reservaService = reservaService;
    }

    private int ObtenerUsuarioIdActual()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var id) ? id : 0;
    }

    [HttpGet]
    public async Task<IActionResult> MisReservas()
    {
        var estudianteId = ObtenerUsuarioIdActual();
        var reservas = await _reservaService.ObtenerReservasEstudianteAsync(estudianteId);
        return View(reservas);
    }

    [HttpGet]
    public async Task<IActionResult> Crear(int? maquinaId = null, string? fecha = null, string? hora = null)
    {
        var model = new NuevaReservaViewModel
        {
            Fecha = DateTime.Today,
            HoraInicio = new TimeSpan(8, 0, 0),
            HoraFin = new TimeSpan(9, 0, 0),
            CantidadPrendas = 10
        };

        if (maquinaId.HasValue)
        {
            model.MaquinaId = maquinaId.Value;
        }

        if (!string.IsNullOrEmpty(fecha) && DateTime.TryParse(fecha, out var f))
        {
            model.Fecha = f;
        }

        if (!string.IsNullOrEmpty(hora) && TimeSpan.TryParse(hora, out var h))
        {
            model.HoraInicio = h;
            model.HoraFin = h.Add(TimeSpan.FromHours(1));
        }

        await CargarComboMaquinas();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(NuevaReservaViewModel model)
    {
        var estudianteId = ObtenerUsuarioIdActual();

        if (!ModelState.IsValid)
        {
            await CargarComboMaquinas();
            return View(model);
        }

        var (exito, mensaje, reserva) = await _reservaService.CrearReservaAsync(estudianteId, model);

        if (!exito)
        {
            ModelState.AddModelError(string.Empty, mensaje);
            await CargarComboMaquinas();
            return View(model);
        }

        TempData["MensajeExito"] = mensaje;
        return RedirectToAction(nameof(MisReservas));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int reservaId)
    {
        var estudianteId = ObtenerUsuarioIdActual();
        var (exito, mensaje) = await _reservaService.CancelarReservaEstudianteAsync(estudianteId, reservaId);

        if (exito)
        {
            TempData["MensajeExito"] = mensaje;
        }
        else
        {
            TempData["MensajeError"] = mensaje;
        }

        return RedirectToAction(nameof(MisReservas));
    }

    private async Task CargarComboMaquinas()
    {
        var maquinas = await _context.Maquinas
            .Where(m => m.EstadoOperativo == EstadosMaquina.Operativa)
            .OrderBy(m => m.Numero)
            .Select(m => new
            {
                m.Id,
                Texto = $"Lavadora #{m.Numero:D2} - {m.Nombre} (Capacidad: {m.CapacidadKg} kg)"
            })
            .ToListAsync();

        ViewBag.MaquinasDisponibles = new SelectList(maquinas, "Id", "Texto");
    }
}
