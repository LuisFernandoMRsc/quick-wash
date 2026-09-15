using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickWash.Data;
using QuickWash.Models;

namespace QuickWash.Controllers;

public class HomeController : Controller
{
    private readonly QuickWashDbContext _context;

    public HomeController(QuickWashDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalMaquinas = await _context.Maquinas.CountAsync();
        var operativas = await _context.Maquinas.CountAsync(m => m.EstadoOperativo == EstadosMaquina.Operativa);
        var reservasHoy = await _context.Reservas.CountAsync(r => r.Fecha.Date == DateTime.Today && r.Estado != EstadosReserva.Cancelada);
        var enProcesoHoy = await _context.Reservas.CountAsync(r => r.Fecha.Date == DateTime.Today && r.Estado == EstadosReserva.Proceso);

        ViewBag.TotalMaquinas = totalMaquinas;
        ViewBag.Operativas = operativas;
        ViewBag.ReservasHoy = reservasHoy;
        ViewBag.EnProcesoHoy = enProcesoHoy;

        return View();
    }

    public IActionResult Reglas()
    {
        return View();
    }
}
